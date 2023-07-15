using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

[ExecuteAlways]
public class EncounterAuthoring : MonoBehaviour
{
    public EncounterBuffer[] encounter;

    class Baker : Baker<EncounterAuthoring>
    {
        public override void Bake(EncounterAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var buffer = AddBuffer<Encounter>(entity);

            foreach (var encounter in authoring.encounter)
            {
                buffer.Add(new Encounter
                {
                    prefab = GetEntity(encounter.prefab, TransformUsageFlags.Dynamic),
                    position = encounter.position,
                    rotation = encounter.rotation,
                });
            }
        }
    }
}

[Serializable]
public struct EncounterBuffer
{
    public GameObject prefab;
    public float3 position;
    public quaternion rotation;
}

public struct Encounter : IBufferElementData
{
    public Entity prefab;
    public float3 position;
    public quaternion rotation;
}

[BurstCompile]
public partial struct DelayedSpawnHandler : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (encounters, transform, entity) in SystemAPI.Query<DynamicBuffer<Encounter>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            foreach (var encounter in encounters)
            {
                var instance = commandBuffer.Instantiate(encounter.prefab);

                var instanceTransform = LocalTransform.FromPositionRotation(
                    encounter.position + transform.ValueRO.Position, 
                    math.mul(encounter.rotation, transform.ValueRO.Rotation)
                );
                commandBuffer.SetComponent(instance, instanceTransform);
            }
            commandBuffer.DestroyEntity(entity);
        }
    }
}