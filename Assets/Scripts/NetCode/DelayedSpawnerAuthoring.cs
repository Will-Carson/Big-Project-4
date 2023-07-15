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
public class DelayedSpawnerAuthoring : MonoBehaviour
{
    public DelayedSpawnBuffer[] spawnables;

    class Baker : Baker<DelayedSpawnerAuthoring>
    {
        public override void Bake(DelayedSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var buffer = AddBuffer<DelayedSpawn>(entity);

            foreach (var spawnable in authoring.spawnables)
            {
                buffer.Add(new DelayedSpawn
                {
                    prefab = GetEntity(spawnable.prefab, TransformUsageFlags.Dynamic),
                    position = spawnable.position,
                    rotation = spawnable.rotation,
                });
            }
        }
    }
}

[Serializable]
public struct DelayedSpawnBuffer
{
    public GameObject prefab;
    public float3 position;
    public quaternion rotation;
}

public struct DelayedSpawn : IBufferElementData
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

        foreach (var (spawnables, transform, entity) in SystemAPI.Query<DynamicBuffer<DelayedSpawn>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            foreach (var spawnable in spawnables)
            {
                var instance = commandBuffer.Instantiate(spawnable.prefab);

                var instanceTransform = LocalTransform.FromPositionRotation(
                    spawnable.position + transform.ValueRO.Position, 
                    math.mul(spawnable.rotation, transform.ValueRO.Rotation)
                );
                commandBuffer.SetComponent(instance, instanceTransform);
            }
            commandBuffer.DestroyEntity(entity);
        }
    }
}