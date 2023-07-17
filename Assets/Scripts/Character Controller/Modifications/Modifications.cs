/// All modifications to the Platformer Unity.CharacterController sample are held here, to better facilitate upgrades.
using Unity.CharacterController;
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
            case CharacterState.Stunned:
                StunnedState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.Dead:
                DeadState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
        }
    }

    public void GetMoveVectorFromPlayerInput(CharacterState state, in PlatformerPlayerInputs inputs, quaternion lookRotation, out float3 moveVector)
    {
        moveVector = default;

        switch (state)
        {
            case CharacterState.GroundMove:
                GroundMoveState.GetMoveVectorFromPlayerInput(in inputs, lookRotation, out moveVector);
                break;
            case CharacterState.Crouched:
                CrouchedState.GetMoveVectorFromPlayerInput(in inputs, lookRotation, out moveVector);
                break;
            case CharacterState.AirMove:
                AirMoveState.GetMoveVectorFromPlayerInput(in inputs, lookRotation, out moveVector);
                break;
            case CharacterState.WallRun:
                WallRunState.GetMoveVectorFromPlayerInput(in inputs, lookRotation, out moveVector);
                break;
            case CharacterState.Rolling:
                RollingState.GetMoveVectorFromPlayerInput(in inputs, lookRotation, out moveVector);
                break;
            case CharacterState.LedgeGrab:
                LedgeGrabState.GetMoveVectorFromPlayerInput(in inputs, lookRotation, out moveVector);
                break;
            case CharacterState.LedgeStandingUp:
                LedgeStandingUpState.GetMoveVectorFromPlayerInput(in inputs, lookRotation, out moveVector);
                break;
            case CharacterState.Dashing:
                DashingState.GetMoveVectorFromPlayerInput(in inputs, lookRotation, out moveVector);
                break;
            case CharacterState.Swimming:
                SwimmingState.GetMoveVectorFromPlayerInput(in inputs, lookRotation, out moveVector);
                break;
            case CharacterState.Climbing:
                ClimbingState.GetMoveVectorFromPlayerInput(in inputs, lookRotation, out moveVector);
                break;
            case CharacterState.FlyingNoCollisions:
                FlyingNoCollisionsState.GetMoveVectorFromPlayerInput(in inputs, lookRotation, out moveVector);
                break;
            case CharacterState.RopeSwing:
                RopeSwingState.GetMoveVectorFromPlayerInput(in inputs, lookRotation, out moveVector);
                break;
            case CharacterState.Stunned:
                StunnedState.GetMoveVectorFromPlayerInput(in inputs, lookRotation, out moveVector);
                break;
            case CharacterState.Dead:
                DeadState.GetMoveVectorFromPlayerInput(in inputs, lookRotation, out moveVector);
                break;
        }
    }
}

public struct StunnedState : IPlatformerCharacterState
{
    public void GetCameraParameters(in PlatformerCharacterComponent character, out Entity cameraTarget, out bool calculateUpFromGravity)
    {
        cameraTarget = Entity.Null;
        calculateUpFromGravity = false;
    }

    public void GetMoveVectorFromPlayerInput(in PlatformerPlayerInputs inputs, quaternion lookRotation, out float3 moveVector)
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
    float deathTime;

    public void GetCameraParameters(in PlatformerCharacterComponent character, out Entity cameraTarget, out bool calculateUpFromGravity)
    {
        cameraTarget = Entity.Null;
        calculateUpFromGravity = false;
    }

    public void GetMoveVectorFromPlayerInput(in PlatformerPlayerInputs inputs, quaternion lookRotation, out float3 moveVector)
    {
        moveVector = new float3();
    }

    public void OnStateEnter(CharacterState previousState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        deathTime = (float)baseContext.Time.ElapsedTime;
    }

    public void OnStateExit(CharacterState nextState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {

    }

    public void OnStatePhysicsUpdate(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {

    }

    public void OnStateVariableUpdate(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect)
    {
        var currentTime = (float)baseContext.Time.ElapsedTime;
        if (currentTime - deathTime > .2f)
        {
            context.commandBuffer.DestroyEntity(context.ChunkIndex, aspect.CharacterAspect.Entity);
        }
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
    Stunned,
    Dead,
}