using Rival;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
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
        var defaultActionsMap = _defaultActionsMap;

        var prefabs = SystemAPI.GetSingletonBuffer<PrefabContainer>(true);
        var cameraPrefab = PrefabContainer.GetEntityWithId(prefabs, "OrbitCamera");
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        Entities
        .WithNone<ControlledCameraComponent>()
        .WithAll<GhostOwnerIsLocal, PlatformerPlayer>()
        .ForEach((
        Entity entity,
        in PlatformerPlayer player) =>
        {
            // Exit early if we don't have a controlled character yet.
            if (player.ControlledCharacter == Entity.Null) return;

            var camera = commandBuffer.Instantiate(cameraPrefab);
            commandBuffer.AddComponent<MainEntityCamera>(camera);
            commandBuffer.AddComponent(entity, new ControlledCameraComponent
            {
                ControlledCamera = camera
            });
            commandBuffer.AddComponent(camera, new OrbitCameraControl { FollowedCharacterEntity = player.ControlledCharacter });
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
                cameraControl.Look = defaultActionsMap.LookDelta.ReadValue<Vector2>();
                if (math.lengthsq(defaultActionsMap.LookConst.ReadValue<Vector2>()) > math.lengthsq(defaultActionsMap.LookDelta.ReadValue<Vector2>()))
                {
                    cameraControl.Look = defaultActionsMap.LookConst.ReadValue<Vector2>() * SystemAPI.Time.DeltaTime;
                }
                cameraControl.Zoom = defaultActionsMap.CameraZoom.ReadValue<float>();

                SystemAPI.SetComponent(camera.ControlledCamera, cameraControl);
            }
        })
        .WithoutBurst() // Required because defaultActionsMap is a managed object.
        .Run();

        Entities
        .WithAll<GhostOwnerIsLocal>()
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
                inputs.JumpPressed.Set();
            }
            if (defaultActionsMap.Dash.WasPressedThisFrame())
            {
                inputs.DashPressed.Set();
            }
            if (defaultActionsMap.Crouch.WasPressedThisFrame())
            {
                inputs.CrouchPressed.Set();
            }
            if (defaultActionsMap.Rope.WasPressedThisFrame())
            {
                inputs.RopePressed.Set();
            }
            if (defaultActionsMap.Climb.WasPressedThisFrame())
            {
                inputs.ClimbPressed.Set();
            }
            if (defaultActionsMap.FlyNoCollisions.WasPressedThisFrame())
            {
                inputs.FlyNoCollisionsPressed.Set();
            }

            if (defaultActionsMap.Fire1.WasPressedThisFrame())
            {
                inputs.Fire1Pressed.Set();
            }
            if (defaultActionsMap.Fire2.WasPressedThisFrame())
            {
                inputs.Fire2Pressed.Set();
            }
        })
        .WithoutBurst() // Required because defaultActionsMap is a managed object.
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

                var lookDirection = playerInputs.ValueRW.LookDirection;

                stateMachine.GetMoveVectorFromPlayerInput(stateMachine.CurrentState, in playerInputs.ValueRO, lookDirection, out characterControl.MoveVector);
                
                characterControl.JumpHeld = playerInputs.ValueRW.JumpHeld;
                characterControl.RollHeld = playerInputs.ValueRW.RollHeld;
                characterControl.SprintHeld = playerInputs.ValueRW.SprintHeld;

                characterControl.JumpPressed = playerInputs.ValueRW.JumpPressed.IsSet;
                characterControl.DashPressed = playerInputs.ValueRW.DashPressed.IsSet; 
                characterControl.CrouchPressed = playerInputs.ValueRW.CrouchPressed.IsSet; 
                characterControl.RopePressed = playerInputs.ValueRW.RopePressed.IsSet; 
                characterControl.ClimbPressed = playerInputs.ValueRW.ClimbPressed.IsSet; 
                characterControl.FlyNoCollisionsPressed = playerInputs.ValueRW.FlyNoCollisionsPressed.IsSet;

                characterControl.Fire1Pressed = playerInputs.ValueRW.Fire1Pressed.IsSet;
                characterControl.Fire2Pressed = playerInputs.ValueRW.Fire2Pressed.IsSet;

                SystemAPI.SetComponent(player.ControlledCharacter, characterControl);
            }
        }
    }
}