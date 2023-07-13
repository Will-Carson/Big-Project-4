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

            AddComponent(entity, new DelayedSpawn
            {
                spawn = GetEntity(authoring.spawn, TransformUsageFlags.Dynamic),
                transform = LocalTransform.FromPositionRotation(authoring.transform.position, authoring.transform.rotation)
            });
        }
    }
}

public struct DelayedSpawn : IComponentData
{
    public Entity spawn;
    public LocalTransform transform;
}

[BurstCompile]
public partial struct DelayedSpawnHandler : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (delayedSpawn, entity) in SystemAPI.Query<RefRO<DelayedSpawn>>().WithEntityAccess())
        {
            var instance = commandBuffer.Instantiate(delayedSpawn.ValueRO.spawn);
            commandBuffer.SetComponent(instance, delayedSpawn.ValueRO.transform);
            commandBuffer.DestroyEntity(entity);
        }
    }
}