using UnityEngine;
using Unity.Entities;
using System;
using Unity.Collections;
using Unity.Burst;
using System.Collections.Generic;

public class EncountersAuthoring : MonoBehaviour
{
    public List<EncounterAuthoring> encounters;

    class Baker : Baker<EncountersAuthoring>
    {
        public override void Bake(EncountersAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            var buffer = AddBuffer<Encounter>(entity);
            foreach (var encounter in authoring.encounters)
            {
                buffer.Add(new Encounter
                {
                    flags = encounter.tags,
                    weight = encounter.weight,
                    prefab = GetEntity(encounter.prefab, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
}

[Flags]
public enum EncounterFlags
{
    Uninitialized = 0,

    InitialEncounter = 1 << 0,
    Shop = 1 << 1,
    Combat = 1 << 2,
}

[BurstCompile]
public struct Encounter : IBufferElementData
{
    public EncounterFlags flags;
    public float weight;
    public Entity prefab;

    [BurstCompile]
    public static Encounter GetRandomEncounterByTag(DynamicBuffer<Encounter> encounters, EncounterFlags flags, ref Unity.Mathematics.Random random)
    {
        var result = default(Encounter);
        var possibleEncounters = GetEncountersByTag(encounters, flags);

        float totalWeight = 0f;
        foreach (var encounter in possibleEncounters)
        {
            totalWeight += encounter.weight;
        }

        float randomWeight = random.NextFloat(0, totalWeight);
        float cumulativeWeight = 0f;

        foreach (var encounter in possibleEncounters)
        {
            cumulativeWeight += encounter.weight;
            if (cumulativeWeight >= randomWeight)
            {
                result = encounter;
                break;
            }
        }

        possibleEncounters.Dispose();
        return result;
    }

    [BurstCompile]
    public static NativeList<Encounter> GetEncountersByTag(DynamicBuffer<Encounter> encounters, EncounterFlags flags)
    {
        var result = new NativeList<Encounter>(Allocator.Temp);

        foreach (var encounter in encounters)
        {
            if ((encounter.flags & flags) == flags)
            {
                result.Add(encounter);
            }
        }
        return result;
    }
}
