using System;
using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SpawnPointAuthoring : MonoBehaviour
{
    public SpawnableFlags flags;

    class Baker : Baker<SpawnPointAuthoring>
    {
        public override void Bake(SpawnPointAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SpawnPoint(authoring.flags));
        }
    }
}

public struct SpawnPoint : IComponentData
{
    public SpawnableFlags flags;

    public SpawnPoint(SpawnableFlags flags)
    {
        this.flags = flags;
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct SpawnPointHandlerSystem : ISystem
{
    private Unity.Mathematics.Random random;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        random = new Unity.Mathematics.Random(uint.MaxValue);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        var spawnables = SystemAPI.GetSingletonBuffer<Spawnable>(true);

        foreach (var (spawn, transform, entity) in SystemAPI.Query<RefRO<SpawnPoint>, RefRO<LocalToWorld>>().WithEntityAccess())
        {
            var spawnFlags = spawn.ValueRO.flags;

            var spawnable = Spawnable.GetRandomSpawnableByFlags(spawnables, spawnFlags, ref random);
            var spawnablePrefab = spawnable.prefab;
            var spawnNumber = 1 * spawnable.multiplier; // TODO replace 1 with some "mob density" value

            for (var i = 0; i < spawnNumber; i++)
            {
                var point = GetPointAlongSpiral(2, .2f, i * 1);
                var instance = commandBuffer.Instantiate(spawnablePrefab);
                commandBuffer.SetComponent(instance, LocalTransform.FromPositionRotation(transform.ValueRO.Position, transform.ValueRO.Rotation).Translate(point));
            }

            commandBuffer.DestroyEntity(entity);
        }
    }

    public float3 GetPointAlongSpiral(float a, float b, float theta)
    {
        var result = default(float3);

        result.x = (a + b * theta) * math.cos(theta);
        result.z = (a + b * theta) * math.sin(theta);

        return result;
    }
}