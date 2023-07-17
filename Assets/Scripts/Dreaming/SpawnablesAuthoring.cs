using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class SpawnablesAuthoring : MonoBehaviour
{
    public List<SpawnableAuthoring> spawnables;

    class Baker : Baker<SpawnablesAuthoring>
    {
        public override void Bake(SpawnablesAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            var buffer = AddBuffer<Spawnable>(entity);
            foreach (var spawnable in authoring.spawnables)
            {
                buffer.Add(new Spawnable
                {
                    flags = spawnable.flags,
                    weight = spawnable.weight,
                    prefab = GetEntity(spawnable.prefab, TransformUsageFlags.Dynamic),
                    multiplier = spawnable.multiplier,
                });
            }
        }
    }
}

[Flags]
public enum SpawnableFlags
{
    Uninitialized = 0,
    Boss = 1 << 0,
    Pack = 2 << 1,
}

public struct Spawnable : IBufferElementData
{
    public SpawnableFlags flags;
    public float weight;
    public Entity prefab;
    public float multiplier;

    [BurstCompile]
    public static Spawnable GetRandomSpawnableByFlags(DynamicBuffer<Spawnable> spawnables, SpawnableFlags flags, ref Unity.Mathematics.Random random)
    {
        var result = default(Spawnable);
        var possibleSpawnables = GetSpawnablesByTag(spawnables, flags);

        float totalWeight = 0f;
        foreach (var spawnable in possibleSpawnables)
        {
            totalWeight += spawnable.weight;
        }

        float randomWeight = random.NextFloat(0, totalWeight);
        float cumulativeWeight = 0f;

        foreach (var spawnable in possibleSpawnables)
        {
            cumulativeWeight += spawnable.weight;
            if (cumulativeWeight >= randomWeight)
            {
                result = spawnable;
                break;
            }
        }

        possibleSpawnables.Dispose();
        return result;
    }

    [BurstCompile]
    public static NativeList<Spawnable> GetSpawnablesByTag(DynamicBuffer<Spawnable> spawnables, SpawnableFlags flags)
    {
        var result = new NativeList<Spawnable>(Allocator.Temp);

        foreach (var spawnable in spawnables)
        {
            if ((spawnable.flags & flags) == flags)
            {
                result.Add(spawnable);
            }
        }
        return result;
    }
}

[Serializable]
public struct SpawnableAuthoring
{
    public SpawnableFlags flags;
    public float weight;
    public GameObject prefab;
    public float multiplier;
}