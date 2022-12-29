/// All modifications to the Platformer Rival sample are held here, to better facilitate upgrades.
using Rival;
using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[Serializable]
public struct PlatformerCharacterStateMachine : IComponentData
{
    public CharacterState CurrentState;
    public CharacterState PreviousState;

    public GroundMoveState GroundMoveState;
    public CrouchedState CrouchedState;
    public AirMoveState AirMoveState;
    public WallRunState WallRunState;
    public RollingState RollingState;
    public ClimbingState ClimbingState;
    public DashingState DashingState;
    public SwimmingState SwimmingState;
    public LedgeGrabState LedgeGrabState;
    public LedgeStandingUpState LedgeStandingUpState;
    public FlyingNoCollisionsState FlyingNoCollisionsState;
    public RopeSwingState RopeSwingState;
    public FiringState CastingState;
    public StunnedState StunnedState;
    public DeadState DeadState;

    public void TransitionToState(CharacterState newState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        PreviousState = CurrentState;
        CurrentState = newState;

        OnStateExit(PreviousState, CurrentState, ref context, ref baseContext, in aspect);
        OnStateEnter(CurrentState, PreviousState, ref context, ref baseContext, in aspect);
    }

    public void OnStateEnter(CharacterState state, CharacterState previousState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        switch (state)
        {
            case CharacterState.GroundMove:
                GroundMoveState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Crouched:
                CrouchedState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.AirMove:
                AirMoveState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.WallRun:
                WallRunState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Rolling:
                RollingState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.LedgeGrab:
                LedgeGrabState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.LedgeStandingUp:
                LedgeStandingUpState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Dashing:
                DashingState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Swimming:
                SwimmingState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Climbing:
                ClimbingState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.FlyingNoCollisions:
                FlyingNoCollisionsState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.RopeSwing:
                RopeSwingState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Casting:
                CastingState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Stunned:
                StunnedState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Dead:
                DeadState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
        }
    }

    public void OnStateExit(CharacterState state, CharacterState newState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        switch (state)
        {
            case CharacterState.GroundMove:
                GroundMoveState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Crouched:
                CrouchedState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.AirMove:
                AirMoveState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.WallRun:
                WallRunState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Rolling:
                RollingState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.LedgeGrab:
                LedgeGrabState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.LedgeStandingUp:
                LedgeStandingUpState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Dashing:
                DashingState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Swimming:
                SwimmingState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Climbing:
                ClimbingState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.FlyingNoCollisions:
                FlyingNoCollisionsState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.RopeSwing:
                RopeSwingState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Casting:
                CastingState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Stunned:
                StunnedState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Dead:
                DeadState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
        }
    }

    public void OnStatePhysicsUpdate(CharacterState state, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        switch (state)
        {
            case CharacterState.GroundMove:
                GroundMoveState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Crouched:
                CrouchedState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.AirMove:
                AirMoveState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.WallRun:
                WallRunState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Rolling:
                RollingState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.LedgeGrab:
                LedgeGrabState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.LedgeStandingUp:
                LedgeStandingUpState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Dashing:
                DashingState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Swimming:
                SwimmingState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Climbing:
                ClimbingState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.FlyingNoCollisions:
                FlyingNoCollisionsState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.RopeSwing:
                RopeSwingState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Casting:
                CastingState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Stunned:
                StunnedState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Dead:
                DeadState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
        }
    }

    public void OnStateVariableUpdate(CharacterState state, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        switch (state)
        {
            case CharacterState.GroundMove:
                GroundMoveState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Crouched:
                CrouchedState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.AirMove:
                AirMoveState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.WallRun:
                WallRunState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Rolling:
                RollingState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.LedgeGrab:
                LedgeGrabState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.LedgeStandingUp:
                LedgeStandingUpState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Dashing:
                DashingState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Swimming:
                SwimmingState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Climbing:
                ClimbingState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.FlyingNoCollisions:
                FlyingNoCollisionsState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.RopeSwing:
                RopeSwingState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Casting:
                CastingState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Stunned:
                StunnedState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Dead:
                DeadState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
        }
    }

    public void GetCameraParameters(CharacterState state, in PlatformerCharacterComponent character, out Entity cameraTarget, out bool calculateUpFromGravity)
    {
        cameraTarget = default;
        calculateUpFromGravity = default;

        switch (state)
        {
            case CharacterState.GroundMove:
                GroundMoveState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.Crouched:
                CrouchedState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.AirMove:
                AirMoveState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.WallRun:
                WallRunState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.Rolling:
                RollingState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.LedgeGrab:
                LedgeGrabState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.LedgeStandingUp:
                LedgeStandingUpState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.Dashing:
                DashingState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.Swimming:
                SwimmingState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.Climbing:
                ClimbingState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.FlyingNoCollisions:
                FlyingNoCollisionsState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.RopeSwing:
                RopeSwingState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.Casting:
                CastingState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.Stunned:
                StunnedState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.Dead:
                DeadState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
        }
    }

    public void GetMoveVectorFromPlayerInput(CharacterState state, in PlatformerPlayerInputs inputs, quaternion cameraRotation, out float3 moveVector)
    {
        moveVector = default;

        switch (state)
        {
            case CharacterState.GroundMove:
                GroundMoveState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.Crouched:
                CrouchedState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.AirMove:
                AirMoveState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.WallRun:
                WallRunState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.Rolling:
                RollingState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.LedgeGrab:
                LedgeGrabState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.LedgeStandingUp:
                LedgeStandingUpState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.Dashing:
                DashingState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.Swimming:
                SwimmingState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.Climbing:
                ClimbingState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.FlyingNoCollisions:
                FlyingNoCollisionsState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.RopeSwing:
                RopeSwingState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.Casting:
                RopeSwingState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.Stunned:
                RopeSwingState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.Dead:
                RopeSwingState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
        }
    }
}

public struct FiringState : IPlatformerCharacterState
{
    Entity weaponEntity;
    float remainingCastTime;

    public void OnStateEnter(CharacterState previousState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        ref var character = ref aspect.Character.ValueRW;
        aspect.SetCapsuleGeometry(character.StandingGeometry.ToCapsuleGeometry());
    }

    public void OnStateExit(CharacterState nextState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        // Clean up state data
        weaponEntity = Entity.Null;
        remainingCastTime = 0;

        ref var character = ref aspect.Character.ValueRW;

        character.IsOnStickySurface = false;
        character.IsSprinting = false;
    }

    public void OnStatePhysicsUpdate(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        var deltaTime = baseContext.Time.DeltaTime;
        var elapsedTime = (float)baseContext.Time.ElapsedTime;
        ref var characterBody = ref aspect.CharacterAspect.CharacterBody.ValueRW;
        ref var character = ref aspect.Character.ValueRW;
        ref var characterControl = ref aspect.CharacterControl.ValueRW;
        ref var characterPosition = ref aspect.CharacterAspect.LocalTransform.ValueRW.Position;
        ref var characterRotation = ref aspect.CharacterAspect.LocalTransform.ValueRW.Rotation;

        aspect.HandlePhysicsUpdatePhase1(ref context, ref baseContext, true, true);

        remainingCastTime -= deltaTime;

        if (characterBody.IsGrounded)
        {
            // Rotate character towards target
            CharacterControlUtilities.SlerpRotationTowardsDirectionAroundUp(ref characterRotation, deltaTime, math.normalizesafe(characterControl.MoveVector), MathUtilities.GetUpFromRotation(characterRotation), character.AirRotationSharpness);

            // Once the cast time <= 0, cast the ability
            if (remainingCastTime <= 0)
            {
                //var abilityEntity = Entity.Null;
                //context.EndFrameECB.SetComponent(context.ChunkIndex, abilityEntity, LocalTransform.FromPositionRotation(characterPosition, characterRotation));
            }
        }

        aspect.HandlePhysicsUpdatePhase2(ref context, ref baseContext, true, true, true, true, true);

        DetectTransitions(ref context, ref baseContext, in aspect);
    }

    public void OnStateVariableUpdate(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {

    }

    public void GetCameraParameters(in PlatformerCharacterComponent character, out Entity cameraTarget, out bool calculateUpFromGravity)
    {
        cameraTarget = character.DefaultCameraTargetEntity;
        calculateUpFromGravity = !character.IsOnStickySurface;
    }

    public void GetMoveVectorFromPlayerInput(in PlatformerPlayerInputs inputs, quaternion cameraRotation, out float3 moveVector)
    {
        PlatformerCharacterAspect.GetCommonMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
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

public struct StunnedState : IPlatformerCharacterState
{
    public void GetCameraParameters(in PlatformerCharacterComponent character, out Entity cameraTarget, out bool calculateUpFromGravity)
    {
        cameraTarget = Entity.Null;
        calculateUpFromGravity = false;
    }

    public void GetMoveVectorFromPlayerInput(in PlatformerPlayerInputs inputs, quaternion cameraRotation, out float3 moveVector)
    {
        moveVector = new float3();
    }

    public void OnStateEnter(CharacterState previousState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {

    }

    public void OnStateExit(CharacterState nextState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {

    }

    public void OnStatePhysicsUpdate(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {

    }

    public void OnStateVariableUpdate(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {

    }
}

public struct DeadState : IPlatformerCharacterState
{
    public void GetCameraParameters(in PlatformerCharacterComponent character, out Entity cameraTarget, out bool calculateUpFromGravity)
    {
        cameraTarget = Entity.Null;
        calculateUpFromGravity = false;
    }

    public void GetMoveVectorFromPlayerInput(in PlatformerPlayerInputs inputs, quaternion cameraRotation, out float3 moveVector)
    {
        moveVector = new float3();
    }

    public void OnStateEnter(CharacterState previousState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {

    }

    public void OnStateExit(CharacterState nextState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {

    }

    public void OnStatePhysicsUpdate(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {

    }

    public void OnStateVariableUpdate(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {

    }
}

public enum CharacterState
{
    Uninitialized,

    GroundMove,
    Crouched,
    AirMove,
    WallRun,
    Rolling,
    LedgeGrab,
    LedgeStandingUp,
    Dashing,
    Swimming,
    Climbing,
    FlyingNoCollisions,
    RopeSwing,
    Casting,
    Stunned,
    Dead,
}