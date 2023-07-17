using System;
using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
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

[Flags]
public enum SpawnableFlags
{
    Uninitialized = 0,
    Boss = 1 << 0,
    Pack = 2 << 1,
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

            var spawnablePrefab = Spawnable.GetRandomSpawnableByFlags(spawnables, spawnFlags, ref random).prefab;
            var instance = commandBuffer.Instantiate(spawnablePrefab);
            commandBuffer.SetComponent(instance, LocalTransform.FromPositionRotation(transform.ValueRO.Position, transform.ValueRO.Rotation));

            commandBuffer.DestroyEntity(entity);
        }
    }
}