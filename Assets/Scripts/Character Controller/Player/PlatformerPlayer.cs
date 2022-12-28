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
    public float3 Look;
    
    public bool SprintHeld;
    public bool RollHeld;
    public bool JumpHeld;
    
    public InputEvent JumpPressed;
    public InputEvent DashPressed;
    public InputEvent CrouchPressed;
    public InputEvent RopePressed;
    public InputEvent ClimbPressed;
    public InputEvent FlyNoCollisionsPressed;

    public InputEvent Fire1Pressed;
    public InputEvent Fire1Released;
    public InputEvent Fire2Pressed;
    public InputEvent Fire2Released;
}
