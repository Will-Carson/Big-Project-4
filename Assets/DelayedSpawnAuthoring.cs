using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class DelayedSpawnAuthoring : MonoBehaviour
{
    public GameObject spawn;

    class Baker : Baker<DelayedSpawnAuthoring>
    {
        public override void Bake(DelayedSpawnAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new DelayedSpawn { spawn = GetEntity(authoring.spawn, TransformUsageFlags.Dynamic) });
        }
    }
}

public struct DelayedSpawn : IComponentData
{
    public Entity spawn;
}

[BurstCompile]
public partial struct DelayedSpawnHandler : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (delayedSpawn, localToWorld, entity) in SystemAPI.Query<RefRO<DelayedSpawn>, RefRO<LocalToWorld>>().WithEntityAccess())
        {
            var instance = commandBuffer.Instantiate(delayedSpawn.ValueRO.spawn);
            commandBuffer.SetComponent(instance, LocalTransform.FromPositionRotation(localToWorld.ValueRO.Position, localToWorld.ValueRO.Rotation));
            commandBuffer.DestroyEntity(entity);
        }
    }
}