using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Rival;
using Unity.Mathematics;

public interface IPlatformerCharacterState
{
    void OnStateEnter(CharacterState previousState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect);
    void OnStateExit(CharacterState nextState, ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect);
    void OnStatePhysicsUpdate(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect);
    void OnStateVariableUpdate(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in PlatformerCharacterAspect aspect);
    void GetCameraParameters(in PlatformerCharacterComponent character, out Entity cameraTarget, out bool calculateUpFromGravity);
    void GetMoveVectorFromPlayerInput(in PlatformerPlayerInputs inputs, quaternion lookRotation, out float3 moveVector);
}