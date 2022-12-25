using Rival;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class PlatformerPlayerInputsSystem : SystemBase
{
    private PlatformerInputActions.GameplayMapActions _defaultActionsMap;
    
    protected override void OnCreate()
    {
        PlatformerInputActions inputActions = new PlatformerInputActions();
        inputActions.Enable();
        inputActions.GameplayMap.Enable();
        _defaultActionsMap = inputActions.GameplayMap;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        RequireForUpdate(SystemAPI.QueryBuilder().WithAll<PlatformerPlayer, PlatformerPlayerInputs>().Build());
    }
    
    protected override void OnUpdate()
    {
        var fixedTick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;
        var defaultActionsMap = _defaultActionsMap;

        var prefabs = SystemAPI.GetSingletonBuffer<PrefabContainer>(true);
        var cameraPrefab = PrefabContainer.GetEntityWithId(prefabs, "OrbitCamera");
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        Entities
        .WithNone<ControlledCameraComponent>()
        .WithAll<GhostOwnerIsLocal, PlatformerPlayer>()
        .ForEach((
        Entity entity) =>
        {
            var camera = commandBuffer.Instantiate(cameraPrefab);
            commandBuffer.AddComponent<MainEntityCamera>(camera);
            commandBuffer.AddComponent(entity, new ControlledCameraComponent
            {
                ControlledCamera = camera
            });
        })
        .Run();

        /// Move the camera on the client because the next foreach requires the camera
        /// rotation be up-to-date.
        Entities
        .ForEach((
        Entity entity,
        in PlatformerPlayerInputs inputs,
        in ControlledCameraComponent camera,
        in PlatformerPlayer player) =>
        {
            if (SystemAPI.HasComponent<OrbitCameraControl>(camera.ControlledCamera))
            {
                var cameraControl = SystemAPI.GetComponent<OrbitCameraControl>(camera.ControlledCamera);

                cameraControl.FollowedCharacterEntity = player.ControlledCharacter;
                cameraControl.Look = defaultActionsMap.LookConst.ReadValue<Vector2>() * SystemAPI.Time.DeltaTime;
                cameraControl.Zoom = defaultActionsMap.CameraZoom.ReadValue<float>();

                SystemAPI.SetComponent(camera.ControlledCamera, cameraControl);
            }
        })
        .Run();

        Entities
        .ForEach((
        Entity entity,
        ref PlatformerPlayerInputs inputs,
        in ControlledCameraComponent camera,
        in PlatformerPlayer player) =>
        {
            inputs = default;
            inputs.Move = Vector2.ClampMagnitude(defaultActionsMap.Move.ReadValue<Vector2>(), 1f);
            inputs.SprintHeld = defaultActionsMap.Sprint.IsPressed();
            inputs.RollHeld = defaultActionsMap.Roll.IsPressed();
            inputs.JumpHeld = defaultActionsMap.Jump.IsPressed();

            if (camera.ControlledCamera != Entity.Null)
            {
                var rotation = SystemAPI.GetComponent<LocalTransform>(camera.ControlledCamera).Rotation;
                inputs.LookDirection = rotation;
            }

            if (defaultActionsMap.Jump.WasPressedThisFrame())
            {
                inputs.JumpPressed.Set(fixedTick);
            }
            if (defaultActionsMap.Dash.WasPressedThisFrame())
            {
                inputs.DashPressed.Set(fixedTick);
            }
            if (defaultActionsMap.Crouch.WasPressedThisFrame())
            {
                inputs.CrouchPressed.Set(fixedTick);
            }
            if (defaultActionsMap.Rope.WasPressedThisFrame())
            {
                inputs.RopePressed.Set(fixedTick);
            }
            if (defaultActionsMap.Climb.WasPressedThisFrame())
            {
                inputs.ClimbPressed.Set(fixedTick);
            }
            if (defaultActionsMap.FlyNoCollisions.WasPressedThisFrame())
            {
                inputs.FlyNoCollisionsPressed.Set(fixedTick);
            }
        })
        .Run();
    }
}

/// <summary>
/// Apply inputs that need to be read at a fixed rate.
/// It is necessary to handle this as part of the fixed step group, in case your framerate is lower than the fixed step rate.
/// </summary>
[UpdateInGroup(typeof(PredictedFixedStepSimulationSystemGroup), OrderFirst = true)]
[BurstCompile]
public partial struct PlatformerPlayerFixedStepControlSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<PlatformerPlayer, PlatformerPlayerInputs>().Build());
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        uint fixedTick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;
        
        foreach (var (playerInputs, player) in SystemAPI.Query<
            RefRW<PlatformerPlayerInputs>, 
            PlatformerPlayer>()
            .WithAll<Simulate>())
        {
            if (SystemAPI.HasComponent<PlatformerCharacterControl>(player.ControlledCharacter) && SystemAPI.HasComponent<PlatformerCharacterStateMachine>(player.ControlledCharacter))
            {
                var characterControl = SystemAPI.GetComponent<PlatformerCharacterControl>(player.ControlledCharacter);
                var stateMachine = SystemAPI.GetComponent<PlatformerCharacterStateMachine>(player.ControlledCharacter);

                var cameraRotation = playerInputs.ValueRW.LookDirection;

                stateMachine.GetMoveVectorFromPlayerInput(stateMachine.CurrentState, in playerInputs.ValueRO, cameraRotation, out characterControl.MoveVector);
                
                characterControl.JumpHeld = playerInputs.ValueRW.JumpHeld;
                characterControl.RollHeld = playerInputs.ValueRW.RollHeld;
                characterControl.SprintHeld = playerInputs.ValueRW.SprintHeld;

                characterControl.JumpPressed = playerInputs.ValueRW.JumpPressed.IsSet(fixedTick);
                characterControl.DashPressed = playerInputs.ValueRW.DashPressed.IsSet(fixedTick); 
                characterControl.CrouchPressed = playerInputs.ValueRW.CrouchPressed.IsSet(fixedTick); 
                characterControl.RopePressed = playerInputs.ValueRW.RopePressed.IsSet(fixedTick); 
                characterControl.ClimbPressed = playerInputs.ValueRW.ClimbPressed.IsSet(fixedTick); 
                characterControl.FlyNoCollisionsPressed = playerInputs.ValueRW.FlyNoCollisionsPressed.IsSet(fixedTick); 
                
                SystemAPI.SetComponent(player.ControlledCharacter, characterControl);
            }
        }
    }
}