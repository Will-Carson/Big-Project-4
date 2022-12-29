using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct CustomGravity : IComponentData
{
    public float GravityMultiplier;

    public float3 Gravity;
    public bool TouchedByNonGlobalGravity;
    public Entity CurrentZoneEntity;
    public Entity LastZoneEntity;
}