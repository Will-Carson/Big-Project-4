using System;
using System.Numerics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class PlayerInputSystem : SystemBase
{
    private InputActions inputActions;

    protected override void OnCreate()
    {
        RequireForUpdate(SystemAPI.QueryBuilder().WithAll<Player, PlayerCommands>().Build());
        RequireForUpdate<NetworkTime>();
        RequireForUpdate<NetworkIdComponent>();

        // Create the input user
        inputActions = new InputActions();
        inputActions.Enable();
        inputActions.Player.Enable();
    }

    protected override void OnUpdate()
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var elapsedTime = (float)SystemAPI.Time.ElapsedTime;
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        var localNetworkId = SystemAPI.GetSingleton<NetworkIdComponent>().Value;
        var defaultActionsMap = inputActions.Player;

        Entities
        .ForEach((
        ref PlayerCommands playerCommands,
        ref Player player,
        in GhostOwnerComponent owner,
        in Entity entity) =>
        {
            if (owner.NetworkId != localNetworkId)
            {
                // Break out if this is not the local player.
                return;
            }

            var isOnNewTick = !player.lastKnownCommandsTick.IsValid || tick.IsNewerThan(player.lastKnownCommandsTick);

            playerCommands = default;

            // Movement direction
            playerCommands.moveInput = math.clamp(defaultActionsMap.Move.ReadValue<float2>(), 1f, -1f);

            // Look input must be accumulated on each update belonging to the same tick, because it is a delta and will be processed at a variable update
            if (!isOnNewTick)
            {
                playerCommands.lookInputDelta = player.lastKnownCommands.lookInputDelta;
            }

            // Mouse look with a mouse move delta value
            playerCommands.lookInputDelta += defaultActionsMap.Look.ReadValue<float2>() * 2f; // TODO 2f here is look sensitivity

            // Jump
            if (defaultActionsMap.Jump.WasPressedThisFrame())
            {
                playerCommands.jumpPressed.Set();
            }

            // Shoot
            if (defaultActionsMap.Fire.WasPressedThisFrame())
            {
                playerCommands.shootPressed.Set();
            }
            if (defaultActionsMap.Fire.WasReleasedThisFrame())
            {
                playerCommands.shootReleased.Set();
            }

            // Aim
            playerCommands.aimHeld = defaultActionsMap.Aim.IsPressed();

            player.lastKnownCommandsTick = tick;
            player.lastKnownCommands = playerCommands;
        })
        .WithoutBurst()
        .Run();
    }
}

[Serializable]
[GhostComponent]
public struct Player : IComponentData
{
    [GhostField]
    public FixedString128Bytes name;
    [GhostField]
    public Entity controlledCharacter;

    public NetworkTick lastKnownCommandsTick;
    public PlayerCommands lastKnownCommands;
}

public struct PlayerCommands : IInputComponentData
{
    public float2 moveInput;
    public float2 lookInputDelta;
    public InputEvent jumpPressed;
    public InputEvent shootPressed;
    public InputEvent shootReleased;
    public bool aimHeld;
}