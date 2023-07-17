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
                    tags = encounter.tags,
                    weight = encounter.weight,
                    prefab = GetEntity(encounter.prefab, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
}

[Flags]
public enum EncounterTags
{
    Uninitialized = 0,

    InitialEncounter = 1 << 0,
    Shop = 1 << 1,
    Combat = 1 << 2,
}

[BurstCompile]
public struct Encounter : IBufferElementData
{
    public EncounterTags tags;
    public float weight;
    public Entity prefab;

    [BurstCompile]
    public static Encounter GetRandomEncounterByTag(DynamicBuffer<Encounter> encounters, EncounterTags tags)
    {
        var result = default(Encounter);
        var possibleEncounters = GetEncountersByTag(encounters, tags);

        foreach (var encounter in possibleEncounters)
        {
            result = encounter;
            break;
        } // TODO actually sort them by weight

        possibleEncounters.Dispose();
        return result;
    }

    [BurstCompile]
    public static NativeList<Encounter> GetEncountersByTag(DynamicBuffer<Encounter> encounters, EncounterTags tags)
    {
        var result = new NativeList<Encounter>(Allocator.Temp);

        foreach (var encounter in encounters)
        {
            if ((encounter.tags & tags) == tags)
            {
                result.Add(encounter);
            }
        }
        return result;
    }
}
