using Unity.Entities;
using Unity.CharacterController;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public struct GroundMoveState : IPlatformerCharacterState
{
    public void OnStateEnter(CharacterState previousState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref PlatformerCharacterComponent character = ref aspect.Character.ValueRW;
    }

    public void OnStateExit(CharacterState nextState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref PlatformerCharacterComponent character = ref aspect.Character.ValueRW;
        
        character.IsOnStickySurface = false;
        character.IsSprinting = false;
    }

    public void OnStatePhysicsUpdate(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var characterComponent = ref aspect.Character.ValueRW;
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var characterPosition = ref aspect.CharacterAspect.LocalTransform.ValueRW.Position;

        // First phase of default character update
        aspect.CharacterAspect.Update_Initialize(in aspect, ref context, ref baseContext, ref characterBody, baseContext.Time.DeltaTime);
        aspect.CharacterAspect.Update_ParentMovement(in aspect, ref context, ref baseContext, ref characterBody, ref characterPosition, characterBody.WasGroundedBeforeCharacterUpdate);
        aspect.CharacterAspect.Update_Grounding(in aspect, ref context, ref baseContext, ref characterBody, ref characterPosition);

        // Update desired character velocity after grounding was detected, but before doing additional processing that depends on velocity
        {
            var deltaTime = baseContext.Time.DeltaTime;
            ref var characterControl = ref aspect.CharacterControl.ValueRW;

            // Rotate move input and velocity to take into account parent rotation
            if (characterBody.ParentEntity != Entity.Null)
            {
                characterControl.MoveVector = math.rotate(characterBody.RotationFromParent, characterControl.MoveVector);
                characterBody.RelativeVelocity = math.rotate(characterBody.RotationFromParent, characterBody.RelativeVelocity);
            }

            if (characterBody.IsGrounded)
            {
                // Move on ground
                float3 targetVelocity = characterControl.MoveVector * characterComponent.GroundRunMaxSpeed;
                CharacterControlUtilities.StandardGroundMove_Interpolated(ref characterBody.RelativeVelocity, targetVelocity, characterComponent.GroundedMovementSharpness, deltaTime, characterBody.GroundingUp, characterBody.GroundHit.Normal);

                // Jump
                if (characterControl.JumpPressed)
                {
                    CharacterControlUtilities.StandardJump(ref characterBody, characterBody.GroundingUp * characterComponent.AirJumpSpeed, true, characterBody.GroundingUp);
                }
            }
            else
            {
                // Move in air
                float3 airAcceleration = characterControl.MoveVector * characterComponent.AirAcceleration;
                if (math.lengthsq(airAcceleration) > 0f)
                {
                    float3 tmpVelocity = characterBody.RelativeVelocity;
                    CharacterControlUtilities.StandardAirMove(ref characterBody.RelativeVelocity, airAcceleration, characterComponent.AirMaxSpeed, characterBody.GroundingUp, deltaTime, false);

                    // Cancel air acceleration from input if we would hit a non-grounded surface (prevents air-climbing slopes at high air accelerations)
                    if (aspect.CharacterAspect.MovementWouldHitNonGroundedObstruction(in aspect, ref context, ref baseContext, characterBody.RelativeVelocity * deltaTime, out ColliderCastHit hit))
                    {
                        characterBody.RelativeVelocity = tmpVelocity;
                    }
                }

                // Gravity
                CharacterControlUtilities.AccelerateVelocity(ref characterBody.RelativeVelocity, aspect.CustomGravity.ValueRO.Gravity, deltaTime); // TODO need to deal with this gravity...

                // Drag
                CharacterControlUtilities.ApplyDragToVelocity(ref characterBody.RelativeVelocity, deltaTime, characterComponent.AirDrag);
            }
        }

        // Second phase of default character update
        aspect.CharacterAspect.Update_PreventGroundingFromFutureSlopeChange(in aspect, ref context, ref baseContext, ref characterBody, in characterComponent.StepAndSlopeHandling);
        aspect.CharacterAspect.Update_GroundPushing(in aspect, ref context, ref baseContext, aspect.CustomGravity.ValueRO.Gravity.y); // TODO this gravity is wonky as hell
        aspect.CharacterAspect.Update_MovementAndDecollisions(in aspect, ref context, ref baseContext, ref characterBody, ref characterPosition);
        aspect.CharacterAspect.Update_MovingPlatformDetection(ref baseContext, ref characterBody);
        aspect.CharacterAspect.Update_ParentMomentum(ref baseContext, ref characterBody);
        aspect.CharacterAspect.Update_ProcessStatefulCharacterHits();
    }

    public void OnStateVariableUpdate(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        var deltaTime = baseContext.Time.DeltaTime;
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterControl = ref aspect.CharacterControl.ValueRW;
        ref var characterRotation = ref aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;
        var customGravity = aspect.CustomGravity.ValueRO;

        CharacterControlUtilities.SlerpRotationTowardsDirectionAroundUp(ref characterRotation, deltaTime, math.normalizesafe(characterControl.LookVector), MathUtilities.GetUpFromRotation(characterRotation), character.AirRotationSharpness);

        // Weapon
        PlatformerCharacterAspect.HandleWeaponSubstate(
            context.EndFrameECB,
            context.ChunkIndex,
            aspect.ActiveWeapon.ValueRO,
            context.WeaponControlLookup,
            context.InterpolationDelayLookup,
            characterControl,
            0);

        character.IsOnStickySurface = PhysicsUtilities.HasPhysicsTag(in baseContext.PhysicsWorld, characterBody.GroundHit.RigidBodyIndex, character.StickySurfaceTag);
        if (character.IsOnStickySurface)
        {
            CharacterControlUtilities.SlerpCharacterUpTowardsDirection(ref characterRotation, deltaTime, characterBody.GroundHit.Normal, character.UpOrientationAdaptationSharpness);
        }
        else
        {
            CharacterControlUtilities.SlerpCharacterUpTowardsDirection(ref characterRotation, deltaTime, math.normalizesafe(-customGravity.Gravity), character.UpOrientationAdaptationSharpness);
        }
    }

    public void GetCameraParameters(in PlatformerCharacterComponent character, out Entity cameraTarget, out bool calculateUpFromGravity)
    {
        cameraTarget = character.DefaultCameraTargetEntity;
        calculateUpFromGravity = !character.IsOnStickySurface;
    }

    public void GetMoveVectorFromPlayerInput(in PlatformerPlayerInputs inputs, quaternion lookRotation, out float3 moveVector)
    {
        PlatformerCharacterAspect.GetCommonMoveVectorFromPlayerInput(in inputs, lookRotation, out moveVector);
    }

    public bool DetectTransitions(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref KinematicCharacterBody characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref PlatformerCharacterControl characterControl = ref aspect.CharacterControl.ValueRW;
        ref PlatformerCharacterStateMachine stateMachine = ref aspect.StateMachine.ValueRW;
        
        if (characterControl.CrouchPressed)
        {
            stateMachine.TransitionToState(CharacterState.Crouched, ref context, ref baseContext, in aspect);
            return true;
        }

        if (characterControl.RollHeld)
        {
            stateMachine.TransitionToState(CharacterState.Rolling, ref context, ref baseContext, in aspect);
            return true;
        }

        if (characterControl.DashPressed)
        {
            stateMachine.TransitionToState(CharacterState.Dashing, ref context, ref baseContext, in aspect);
            return true;
        }

        if (!characterBody.IsGrounded)
        {
            stateMachine.TransitionToState(CharacterState.AirMove, ref context, ref baseContext, in aspect);
            return true;
        }

        if (characterControl.ClimbPressed)
        {
            if (ClimbingState.CanStartClimbing(ref context, ref baseContext, in aspect))
            {
                stateMachine.TransitionToState(CharacterState.Climbing, ref context, ref baseContext, in aspect);
                return true;
            }
        }

        return aspect.DetectGlobalTransitions(ref context, ref baseContext);
    }
}