using Unity.CharacterController;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using Unity.Collections;
using Vector2 = UnityEngine.Vector2;

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
                cameraControl.Look = default;

                // Here we check if ToggleCameraRotation is active and only update camera rotation if it is.
                if (defaultActionsMap.CameraRotation.IsPressed())
                {
                    cameraControl.Look = defaultActionsMap.LookDelta.ReadValue<Vector2>();
                    if (math.lengthsq(defaultActionsMap.LookConst.ReadValue<Vector2>()) > math.lengthsq(defaultActionsMap.LookDelta.ReadValue<Vector2>()))
                    {
                        cameraControl.Look = defaultActionsMap.LookConst.ReadValue<Vector2>() * SystemAPI.Time.DeltaTime;
                    }
                }
                cameraControl.Zoom = defaultActionsMap.CameraZoom.ReadValue<float>();

                SystemAPI.SetComponent(camera.ControlledCamera, cameraControl);
            }
        })
        .WithoutBurst()
        .Run();

        var mousePosition = UnityEngine.Input.mousePosition;
        var raycastParameters = UnityEngine.Camera.main.ScreenPointToRay(mousePosition);

        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        RaycastInput input = new RaycastInput()
        {
            Start = raycastParameters.origin,
            End = raycastParameters.origin + (raycastParameters.direction * 200),
            Filter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u, // all 1s, so all layers, collide with everything
                GroupIndex = 0
            }
        };

        collisionWorld.CastRay(input, out var targetHit);
        var targetPosition = targetHit.Position;

        Entities
        .WithAll<GhostOwnerIsLocal>()
        .ForEach((
        Entity entity,
        ref PlatformerPlayerInputs inputs,
        in ControlledCameraComponent camera,
        in PlatformerPlayer player) =>
        {
            inputs = default;

            var moveInput = defaultActionsMap.Move.ReadValue<Vector2>();
            var camForward = SystemAPI.GetComponent<LocalTransform>(camera.ControlledCamera).Forward();
            var camRight = SystemAPI.GetComponent<LocalTransform>(camera.ControlledCamera).Right();

            // Ignore vertical (y) component of the camera's vectors for a more traditional control scheme.
            camForward.y = 0;
            camRight.y = 0;
            camForward = math.normalizesafe(camForward);
            camRight = math.normalizesafe(camRight);

            // Apply the camera's orientation to the move input.
            var moveDirection = moveInput.y * camForward + moveInput.x * camRight;
            inputs.Move = new float2(moveDirection.x, moveDirection.z);

            inputs.SprintHeld = defaultActionsMap.Sprint.IsPressed();
            inputs.RollHeld = defaultActionsMap.Roll.IsPressed();
            inputs.JumpHeld = defaultActionsMap.Jump.IsPressed();

            inputs.Target = targetPosition;

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
            if (defaultActionsMap.Fire1.WasReleasedThisFrame())
            {
                inputs.Fire1Released.Set();
            }
            if (defaultActionsMap.Fire2.WasPressedThisFrame())
            {
                inputs.Fire2Pressed.Set();
            }
            if (defaultActionsMap.Fire2.WasReleasedThisFrame())
            {
                inputs.Fire2Released.Set();
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
    public void OnUpdate(ref SystemState state)
    {
        uint fixedTick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;

        var localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

        foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>()) { break; }
        
        foreach (var (playerInputs, player) in SystemAPI.Query<
            RefRW<PlatformerPlayerInputs>, 
            RefRO<PlatformerPlayer>>()
            .WithAll<Simulate>())
        {
            if (SystemAPI.HasComponent<PlatformerCharacterControl>(player.ValueRO.ControlledCharacter) && SystemAPI.HasComponent<PlatformerCharacterStateMachine>(player.ValueRO.ControlledCharacter))
            {
                var characterControl = SystemAPI.GetComponent<PlatformerCharacterControl>(player.ValueRO.ControlledCharacter);
                var stateMachine = SystemAPI.GetComponent<PlatformerCharacterStateMachine>(player.ValueRO.ControlledCharacter);

                var lookDirection = default(float3);
                if (localTransformLookup.TryGetComponent(player.ValueRO.ControlledCharacter, out var characterTransform))
                {
                    lookDirection = math.normalizesafe(playerInputs.ValueRW.Target - characterTransform.Position);
                }

                stateMachine.GetMoveVectorFromPlayerInput(stateMachine.CurrentState, in playerInputs.ValueRO, quaternion.LookRotationSafe(lookDirection, new float3(0, 1, 0)), out characterControl.MoveVector);

                characterControl.Target = playerInputs.ValueRW.Target;
                
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
                characterControl.Fire1Released = playerInputs.ValueRW.Fire1Released.IsSet;
                characterControl.Fire2Pressed = playerInputs.ValueRW.Fire2Pressed.IsSet;
                characterControl.Fire2Released = playerInputs.ValueRW.Fire2Released.IsSet;

                SystemAPI.SetComponent(player.ValueRO.ControlledCharacter, characterControl);
            }
        }
    }
}