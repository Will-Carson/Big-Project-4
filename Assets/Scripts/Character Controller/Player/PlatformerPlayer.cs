using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[Serializable]
[GhostComponent]
public struct PlatformerPlayer : IComponentData
{
    [GhostField]
    public FixedString128Bytes Name;
    [GhostField]
    public Entity ControlledCharacter;
}

public struct ControlledCameraComponent : IComponentData
{
    public Entity ControlledCamera;
}

[Serializable]
public struct PlatformerPlayerInputs : IInputComponentData
{
    public float2 Move;
    public quaternion LookDirection;
    
    public bool SprintHeld;
    public bool RollHeld;
    public bool JumpHeld;
    
    public FixedInputEvent JumpPressed;
    public FixedInputEvent DashPressed;
    public FixedInputEvent CrouchPressed;
    public FixedInputEvent RopePressed;
    public FixedInputEvent ClimbPressed;
    public FixedInputEvent FlyNoCollisionsPressed;
}
