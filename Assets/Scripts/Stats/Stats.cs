/// STATS
/// 
/// Stats are an important part of any game. A generalized stat system will be
/// an important foundation for just about any modern game, and a complex RPG is
/// probably the hardest test for this type of system.
/// 
/// The goal of this stat system is maintainability, CPU performance, network 
/// performance, and versatility
/// 
/// The versatility of stats is important. This means two things:
/// 1. Stats must be able to be derived from any place; Auras, talent trees,
/// equipment, or from an 'owner' in the case of pets/minions.
/// 2. Stats must be able to be used to effect any aspect of game state.
/// 
/// The concept of the StatStick solves our first problem. A StatStick is any source
/// of stats. Entities keep track of which StatSticks are applied to them, but
/// StatSticks also keep track of which entities they are equipped to. This allows
/// entities to update their own stats, and allows StatSticks force updates on
/// entities they are equipped to when necessary (say, when an entity moves out of
/// range of a StatStick aura.)
/// 
/// The second problem is more complex and requires specific solutions depending on
/// the use case. Some stats should level up abilities; this requires specific
/// interaction with the ability systems. Some stats effect movement speed. Perhaps
/// the movement speed stat should write to a MovementSpeed component that actually
/// governs movement speed. It really depends on the case.
/// 
/// TODO Currently this implementation uses a "full rebuild" strategy for stat
/// calculation. That is, when an entities stats change the stat buffer is cleared
/// and completely rebuilt. It would be good to test this vs a partial rebuild
/// implementation that simply adds or removes stats from a statstick when it is
/// added or removed.
/// 
/// TODO Theory craft a use for components that have Native containers on them.

using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
using Unity.Burst;
using System;
using Unity.Collections.LowLevel.Unsafe;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateBefore(typeof(StatRecalculationSystemGroup))]
[BurstCompile]
public partial struct ApplyStatSticks : ISystem
{
    private BufferLookup<EquippedTo> equippedToLookup;
    private ComponentLookup<StatRequirements> statRequirementLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        equippedToLookup = state.GetBufferLookup<EquippedTo>();
        statRequirementLookup = state.GetComponentLookup<StatRequirements>(true);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        equippedToLookup.Update(ref state);
        statRequirementLookup.Update(ref state);

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (requests, statSticks, stats, entity) in SystemAPI.Query<
            DynamicBuffer<EquipStatStickRequest>,
            DynamicBuffer<StatStickContainer>,
            RefRO<StatContainer>>()
            .WithEntityAccess())
        {
            var wasChanged = false;
            for (var i = 0; i < requests.Length; i++)
            {
                var req = requests[i];

                if (!equippedToLookup.HasBuffer(req.entity))
                {
                    // Throw error? This should not happen.
                    continue;
                }
                var statStickEquippedToBuffer = equippedToLookup[req.entity];

                if (req.unequip)
                {
                    for (var j = 0; j < statSticks.Length; j++)
                    {
                        var statStick = statSticks[j].entity;
                        if (statStick == req.entity)
                        {
                            statSticks.RemoveAtSwapBack(j);

                            // Remove from the EquippedTo buffer on the stat stick
                            for (var k = 0; k < statStickEquippedToBuffer.Length; k++)
                            {
                                var statStickEquippedTo = statStickEquippedToBuffer[k];
                                if (statStickEquippedTo.entity == entity)
                                {
                                    statStickEquippedToBuffer.RemoveAtSwapBack(k);
                                    break;
                                }
                            }

                            wasChanged = true;
                            break;
                        }
                    }
                }
                else
                {
                    // Check if this stat stick can be equipped. If the stat stick has no requirements, skip this step
                    if (statRequirementLookup.TryGetComponent(req.entity, out var requirements))
                    {
                        if (!requirements.StatsMeetRequirements(stats.ValueRO.stats))
                        {
                            continue;
                        }
                    }

                    // Equip the statstick
                    statSticks.Add(new StatStickContainer { entity = req.entity });

                    // Add to the EquippedTo buffer on the stat stick
                    statStickEquippedToBuffer.Add(new EquippedTo { entity = entity });

                    wasChanged = true;
                }
            }
            if (wasChanged)
            {
                commandBuffer.AddComponent<StatRecalculationTag>(entity);
            }
            requests.Clear();
        }

        commandBuffer.Playback(state.EntityManager);
    }
}

public struct EquipStatStickRequest : IBufferElementData
{
    public Entity entity;
    public bool unequip;
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(StatRecalculationSystemGroup))]
[BurstCompile]
public partial struct StatTotaller : ISystem
{
    private ComponentLookup<StatContainer> statsLookup;
    private BufferLookup<StatStickContainer> statSticksLookup;

    //[BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        statsLookup = state.GetComponentLookup<StatContainer>(true);
        statSticksLookup = state.GetBufferLookup<StatStickContainer>(true);

        state.RequireForUpdate(state.GetEntityQuery(typeof(StatRecalculationTag)));
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Does it matter where these go if I have multiple foreachs?
        statsLookup.Update(ref state);
        statSticksLookup.Update(ref state);

        // Total stats from stat sticks and write that to the entities StatContainer buffer
        foreach (var (tag, stats, entity) in SystemAPI.Query<
            StatRecalculationTag,
            RefRW<StatContainer>>()
            .WithEntityAccess())
        {
            if (!statSticksLookup.HasBuffer(entity)) return;

            stats.ValueRW.stats.Clear();

            var statSticks = statSticksLookup[entity];

            for (var i = 0; i < statSticks.Length; i++)
            {
                var statStick = statSticks[i];

                if (!statsLookup.HasComponent(statStick.entity)) return;

                var statStickStats = statsLookup[statStick.entity].stats;

                stats.ValueRW.stats.AddStats(statStickStats);
            }
        }
    }
}

public struct StatStickContainer : IBufferElementData
{
    public Entity entity;
}

public struct EquippedTo : IBufferElementData
{
    public Entity entity;
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(StatRecalculationSystemGroup))]
[BurstCompile]
public partial struct DerivedStatHandlerSystem : ISystem
{
    BufferLookup<DerivedStat> derivedStatsLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        derivedStatsLookup = state.GetBufferLookup<DerivedStat>(true);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        /// Sort the derived stats before we actually derive any stats. This
        /// ensures a reliable order to the way derived stats are processed.
        /// It also imposes the limitation that the Stat enum order is
        /// significant for determining which stats can meaningfully derive from
        /// which others. This may not be desireable behaviour in the future.
        foreach (var (tag, derivedStats) in SystemAPI.Query<
            StatRecalculationTag,
            DynamicBuffer<DerivedStat>>())
        {
            for (var j = derivedStats.Length - 1; j > 0; j--)
            {
                for (var i = 0; i < j; i++)
                {
                    if (derivedStats[i].toStat > derivedStats[i + 1].toStat)
                    {
                        var derivedStat1 = derivedStats[i];
                        var derivedStat2 = derivedStats[i + 1];

                        derivedStats.Insert(i, derivedStat2);
                        derivedStats.Insert(i + 1, derivedStat1);
                    }
                }
            }
        }

        derivedStatsLookup.Update(ref state);

        foreach (var (stats, entity) in SystemAPI.Query<
            RefRW<StatContainer>>()
            .WithEntityAccess()
            .WithAll<StatRecalculationTag>())
        {
            if (!derivedStatsLookup.HasBuffer(entity)) return;

            var derivedStats = derivedStatsLookup[entity];

            for (var i = 0; i < derivedStats.Length; i++)
            {
                var derivedStat = derivedStats[i];

                var fromStatValue = stats.ValueRW.stats.GetStatValue(derivedStat.fromStat);
                var fromStatDivisor = derivedStat.fromValue;
                var toStatMultiplier = derivedStat.toValue;
                var bonusStatTotal = fromStatValue / fromStatDivisor * toStatMultiplier;

                stats.ValueRW.stats.AddStat(derivedStat.toStat, bonusStatTotal);
            }
        }
    }
}

public struct DerivedStat : IBufferElementData
{
    public Stat fromStat;
    public int fromValue;
    public Stat toStat;
    public int toValue;
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(StatRecalculationSystemGroup))]
[UpdateAfter(typeof(DerivedStatHandlerSystem))]
[BurstCompile]
public partial struct StructCombinedStatCalculationSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        var statDefinitions = SystemAPI.GetSingleton<StatDefinitions.Singleton>();

        // Build StatContainers on CombinedStatResultContainers.
        foreach (var (combinedStatResults, stats, entity) in SystemAPI.Query<
            DynamicBuffer<StatFlavors>, 
            RefRO<StatContainer>>()
            .WithEntityAccess()
            .WithAll<StatRecalculationTag>())
        {
            for (var i = 0; i < combinedStatResults.Length; i++)
            {
                var combinedStatResult = combinedStatResults[i];

                var combinedStats = new Stats(100, Allocator.Persistent); // TODO the number 100 was chosen arbitrarily and WILL cause strange behavior in the future.

                statDefinitions.TotalStatsWithFlavor(stats.ValueRO.stats, combinedStatResult.statFlavorFlags, ref combinedStats);

                if (combinedStatResult.stats.Initialized())
                {
                    combinedStatResult.stats.Dispose();
                }

                combinedStatResult.stats = combinedStats;
            }
        }
    }
}

public struct StatFlavors : IBufferElementData
{
    public StatFlavorFlag statFlavorFlags;
    public Stats stats;
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(StatRecalculationSystemGroup))]
[UpdateBefore(typeof(StatRecalculationTagCleanUpSystem))]
public partial class CustomStatHandlingSystemGroup : ComponentSystemGroup
{

}

/// <summary>
/// Cleans up StatRecalculationTag components. Should by definition be run at the end of a simulation tick.
/// Only needs to be ran on the server.
/// Updates AFTER the StatRecalculationSystemGroup.
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(StatRecalculationSystemGroup), OrderLast = true)]
[BurstCompile]
public partial struct StatRecalculationTagCleanUpSystem : ISystem
{
    private BufferLookup<EquippedTo> equippedToLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        equippedToLookup = state.GetBufferLookup<EquippedTo>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        equippedToLookup.Update(ref state);

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (tag, entity) in SystemAPI.Query<StatRecalculationTag>().WithEntityAccess())
        {
            // Trigger recalculation on anything this is equipped to.
            if (equippedToLookup.TryGetBuffer(entity, out var equippedToBuffer))
            {
                for (var i = 0; i < equippedToBuffer.Length; i++)
                {
                    var equippedTo = equippedToBuffer[i];
                    commandBuffer.AddComponent<StatRecalculationTag>(equippedTo.entity);
                }
            }

            commandBuffer.RemoveComponent<StatRecalculationTag>(entity);
        }
        commandBuffer.Playback(state.EntityManager);
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class StatRecalculationSystemGroup : ComponentSystemGroup
{

}

/// <summary>
/// Marks entities for stat recalculation
/// </summary>
public struct StatRecalculationTag : IComponentData { }

[ChunkSerializable]
public struct Stats
{
    private UnsafeHashMap<uint, float> stats;

    public Stats(int size, Allocator allocator)
    {
        stats = new UnsafeHashMap<uint, float>(size, allocator);
    }

    public bool GetStatValue(Stat stat, out float value)
    {
        return stats.TryGetValue((uint)stat, out value);
    }

    public void AddStat(Stat stat, float value)
    {
        if (value == 0) return;

        if (GetStatValue(stat, out var oldValue))
        {
            if (value + oldValue == 0)
            {
                stats.Remove((uint)stat);
                return;
            }
            stats[(uint)stat] = value + oldValue;
        }
        else
        {
            stats.Add((uint)stat, value);
        }
    }

    public void AddStat((Stat, float) stat)
    {
        AddStat(stat.Item1, stat.Item2);
    }

    public float GetStatValue(Stat stat)
    {
        if (stats.TryGetValue((uint)stat, out var value))
        {
            return value;
        }
        return 0;
    }

    public void AddStats(Stats statsToAdd)
    {
        var stats = statsToAdd.GetEnumerator();

        while (stats.MoveNext())
        {
            var stat = stats.Current;

            AddStat((Stat)stat.Key, stat.Value);
        }
    }

    public void RemoveStatsFrom(Stats statsToRemove)
    {
        var stats = statsToRemove.GetEnumerator();

        while (stats.MoveNext())
        {
            var stat = stats.Current;

            AddStat((Stat)stat.Key, -stat.Value);
        }
    }

    public UnsafeHashMap<uint, float>.Enumerator GetEnumerator()
    {
        return stats.GetEnumerator();
    }

    public void Clear()
    {
        stats.Clear();
    }

    public void Dispose()
    {
        stats.Dispose();
    }

    public bool Initialized()
    {
        return stats.IsCreated;
    }

    public override string ToString()
    {
        var returnValue = "";

        var stats = this.stats.GetEnumerator();

        while (stats.MoveNext())
        {
            var stat = stats.Current;
            returnValue += $"{(Stat)stat.Key} : {stat.Value}\n";
        }

        return returnValue;
    }
}

public struct StatContainer : IComponentData
{
    public Stats stats;

    public StatContainer(Stats baseStatStickStats)
    {
        stats = baseStatStickStats;
    }

    public StatContainer(int size, Allocator allocator)
    {
        stats = new Stats(size, allocator);
    }

    public void Dispose()
    {
        stats.Dispose();
    }
}

[ChunkSerializable]
public struct StatRanges
{
    private UnsafeHashMap<uint, Range> ranges;

    public StatRanges(int size, Allocator allocator)
    {
        ranges = new UnsafeHashMap<uint, Range>(size, allocator);
    }

    public void AddRange(Stat stat, Range range)
    {
        if (ranges.ContainsKey((uint)stat))
        {
            ranges[(uint)stat] = range;
        }
        else
        {
            ranges.Add((uint)stat, range);
        }
    }

    public void AddRange((Stat, float, float) range)
    {
        AddRange(range.Item1, new Range(range.Item2, range.Item3));
    }

    public bool StatInRange(Stat stat, float value)
    {
        if (ranges.TryGetValue((uint)stat, out var range))
        {
            return range.IsInRange(value);
        }
        return false;
    }

    public bool StatsInRange(Stats stats)
    {
        var ranges = GetEnumerator();
        while (ranges.MoveNext())
        {
            var kvp = ranges.Current;
            var stat = (Stat)kvp.Key;
            var value = stats.GetStatValue(stat);

            if (!StatInRange(stat, value))
            {
                return false;
            }
        }
        return true;
    }

    public UnsafeHashMap<uint, Range>.Enumerator GetEnumerator()
    {
        return ranges.GetEnumerator();
    }

    public void Dispose()
    {
        ranges.Dispose();
    }
}

public struct Range
{
    private float min;
    private float max;

    public Range(float min, float max)
    {
        this.min = min;
        this.max = max;

        if (min > max) throw new ArgumentException("Min is larger than max in range.");
    }

    public bool IsInRange(float value)
    {
        return value >= min && value <= max;
    }

    public static Range FromMin(float min)
    {
        return new Range(min, float.MaxValue);
    }

    public static Range FromMax(float max)
    {
        return new Range(float.MinValue, max);
    }
}

public struct StatRequirements : IComponentData
{
    public StatRanges requirements;

    public StatRequirements(StatRanges ranges)
    {
        requirements = ranges;
    }

    public StatRequirements(int size, Allocator allocator)
    {
        requirements = new StatRanges(size, allocator);
    }

    public bool StatsMeetRequirements(Stats stats)
    {
        return requirements.StatsInRange(stats);
    }

    public UnsafeHashMap<uint, Range>.Enumerator GetEnumerator()
    {
        return requirements.GetEnumerator();
    }

    public void Dispose() 
    { 
        requirements.Dispose(); 
    }
}

public readonly partial struct StatStickAspect : IAspect
{
    public readonly Entity entity;
    public readonly RefRW<StatContainer> stats;
    public readonly RefRW<StatRequirements> requirements;
    public readonly DynamicBuffer<EquippedTo> equippedTo;
}

public readonly partial struct AdvancedStatStick : IAspect
{
    public readonly Entity entity;
    public readonly RefRW<StatContainer> stats;
    public readonly RefRW<StatRequirements> requirements;
    public readonly DynamicBuffer<EquippedTo> equippedTo;
    public readonly DynamicBuffer<StatStickContainer> statSticks;
    public readonly DynamicBuffer<EquipStatStickRequest> equipRequests;
}

public readonly partial struct StatEntityAspect : IAspect
{
    public readonly Entity entity;
    public readonly RefRW<StatContainer> stats;
    public readonly DynamicBuffer<DerivedStat> derivedStats;
    public readonly DynamicBuffer<StatStickContainer> statSticks;
    public readonly DynamicBuffer<EquipStatStickRequest> equipRequests;
}