/// STATS
/// 
/// Stats are an important part of any game. A generalized stat system will be
/// an important foundation for just about any modern game, and a complex RPG is
/// probably the hardest test for this type of system.
/// 
/// The goal of this stat system is maintainability, CPU performance, and network 
/// performance.
/// 
/// The versatility of stats is important. This means two things:
/// 1. Stats must be able to be derived from any place; Auras, talent trees,
/// equipment, or in the case of minions, from their 'owner.'
/// 2. Stats must be able to be used to effect any aspect of game state.
/// 
/// The concept of the StatStick solves our first problem. A StatStick is any source
/// of stats. Entities keep track of which StatSticks they have equipped, but
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

using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
using Unity.Burst;
using Unity.Mathematics;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateBefore(typeof(StatRecalculationSystemGroup))]
[BurstCompile]
public partial struct StatStickEquipper : ISystem
{
    private BufferLookup<EquippedTo> equippedToLookup;
    private BufferLookup<StatRequirementContainer> statStickRequirementLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state) 
    { 
        equippedToLookup = state.GetBufferLookup<EquippedTo>();
        statStickRequirementLookup = state.GetBufferLookup<StatRequirementContainer>(true);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        equippedToLookup.Update(ref state);
        statStickRequirementLookup.Update(ref state);

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (requests, statSticks, stats, entity) in SystemAPI.Query<
            DynamicBuffer<EquipStatStickRequest>,
            DynamicBuffer<StatStickContainer>,
            DynamicBuffer<StatContainer>>()
            .WithEntityAccess())
        {
            var wasChanged = false;
            for (var i = 0; i < requests.Length; i++)
            {
                var req = requests[i];
                
                if (!equippedToLookup.HasBuffer(req.statStick))
                {
                    // Throw error? This should not happen.
                    continue;
                }
                var statStickEquippedToBuffer = equippedToLookup[req.statStick];

                if (req.unequip)
                {
                    for (var j = 0; j < statSticks.Length; j++)
                    {
                        var statStick = statSticks[j].statStick;
                        if (statStick == req.statStick)
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
                    var requirementsMet = true;

                    // Check if this stat stick can be equipped. If the stat stick has no requirements, skip this step
                    if (statStickRequirementLookup.TryGetBuffer(req.statStick, out var requirementsBuffer))
                    {
                        for (var j = 0; j < requirementsBuffer.Length; j++)
                        {
                            var requirement = requirementsBuffer[j].requirement;
                            
                            for (var k = 0; k < stats.Length; k++)
                            {
                                var stat = stats[k].stat;
                                if (requirement.stat == stat.type && !requirement.IsInRange(stat))
                                {
                                    requirementsMet = false;
                                    break;
                                }
                            }

                            // If we have already figured out we don't meet requirements, exit early
                            if (!requirementsMet) break;
                        }
                    }

                    if (!requirementsMet) continue;

                    // Equip the statstick
                    statSticks.Add(new StatStickContainer { statStick = req.statStick });

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
    public Entity statStick;
    public bool unequip;
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(StatRecalculationSystemGroup))]
[BurstCompile]
public partial struct StatTotaller : ISystem
{
    private BufferLookup<StatContainer> statsLookup;
    private BufferLookup<StatStickContainer> statSticksLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        statsLookup = state.GetBufferLookup<StatContainer>(true);
        statSticksLookup = state.GetBufferLookup<StatStickContainer>(true);

        state.RequireAnyForUpdate(state.GetEntityQuery(typeof(StatRecalculationTag)));
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Clear stats for the next step
        foreach (var (tag, stats) in SystemAPI.Query<
            StatRecalculationTag,
            DynamicBuffer<StatContainer>>())
        {
            stats.Clear();
        }

        // Does it matter where these go if I have multiple foreachs?
        statsLookup.Update(ref state);
        statSticksLookup.Update(ref state);

        // Total stats from stat sticks and write that to the entities StatContainer buffer
        foreach (var (tag, stats, entity) in SystemAPI.Query<
            StatRecalculationTag,
            DynamicBuffer<StatContainer>>()
            .WithEntityAccess())
        {
            if (!statSticksLookup.HasBuffer(entity)) return;

            var statSticks = statSticksLookup[entity];
            var statTotals = new NativeHashMap<int, int>(10, Allocator.Temp);

            for (var i = 0; i < statSticks.Length; i++)
            {
                var statStick = statSticks[i];

                if (!statsLookup.HasBuffer(statStick.statStick)) return;

                var statStickStats = statsLookup[statStick.statStick];

                for (var j = 0; j < statStickStats.Length; j++)
                {
                    var statStickStat = statStickStats[j].stat;
                    var statKey = (int)statStickStat.type;
                    var statValue = statStickStat.value;

                    if (statTotals.ContainsKey(statKey))
                    {
                        statTotals[statKey] = statTotals[statKey] + statValue;
                    }
                    else
                    {
                        statTotals.Add(statKey, statValue);
                    }
                }
            }

            // Add all stats to the StatContainer buffer
            var keyArray = statTotals.GetKeyArray(Allocator.Temp);
            for (var i = 0; i < keyArray.Length; i++)
            {
                var key = keyArray[i];
                stats.Add(new StatData { type = (StatType)key, value = statTotals[key]});
            }

            // Dispose of Native* types
            keyArray.Dispose();
            statTotals.Dispose();
        }
    }
}

public struct StatStickContainer : IBufferElementData
{
    public Entity statStick;
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

        state.RequireAnyForUpdate(state.GetEntityQuery(typeof(StatRecalculationTag)));
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        /// Sort the derived stats before we actually derive any stats. This
        /// ensures a reliable order to the way derived stats are processed.
        /// It also imposes the limitation that the StatType enum order is
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
                    if (derivedStats[i].toStat.type > derivedStats[i + 1].toStat.type)
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

        foreach (var (tag, stats, entity) in SystemAPI.Query<
            StatRecalculationTag,
            DynamicBuffer<StatContainer>>()
            .WithEntityAccess())
        {
            if (!derivedStatsLookup.HasBuffer(entity)) return;

            var derivedStats = derivedStatsLookup[entity];

            for (var i = 0; i < derivedStats.Length; i++)
            {
                var derivedStat = derivedStats[i];
                for (var j = 0; j < stats.Length; j++)
                {
                    var stat = stats[j].stat;
                    if (derivedStat.fromStat.type != stat.type) continue;

                    var statToAdd = new StatData(derivedStat.toStat.type, (stat.value / derivedStat.fromStat.value) * derivedStat.toStat.value);
                    StatContainer.Add(stats, statToAdd); // Does this need to be "ref stats"?
                }
            }
        }
    }
}

public struct DerivedStat : IBufferElementData
{
    public StatData fromStat;
    public StatData toStat;
}

/// <summary>
/// Updates the min and max value of resources when the corresponding stats update.
/// Must be run inside of the StatRecalculationSystemGroup.
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(StatRecalculationSystemGroup))]
[UpdateAfter(typeof(DerivedStatHandlerSystem))]
[BurstCompile]
public partial struct StatToResourceSystem : ISystem
{
    private BufferLookup<StatContainer> statsLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        statsLookup = state.GetBufferLookup<StatContainer>(true);

        state.RequireAnyForUpdate(state.GetEntityQuery(typeof(StatRecalculationTag)));
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        statsLookup.Update(ref state);

        foreach (var (tag, resources, entity) in SystemAPI.Query<
            StatRecalculationTag, 
            DynamicBuffer<ResourceContainer>>()
            .WithEntityAccess())
        {
            if (!statsLookup.HasBuffer(entity)) return;

            var stats = statsLookup[entity];

            for (var i = 0; i < stats.Length; i++)
            {
                var stat = stats[i].stat;
                for (var j = 0; j < resources.Length; j++)
                {
                    var resource = resources[j];
                    if (stat.type == resource.minStat.type)
                    {
                        resource.minStat.value = stat.value;
                    }
                    if (stat.type == resource.maxStat.type)
                    {
                        resource.maxStat.value = stat.value;
                    }
                    resource.Clamp();
                    resources.RemoveAt(j);
                    resources.Add(resource);
                }
            }
        }
    }
}

/// <summary>
/// Cleans up StatRecalculationTag components. Should by definition be run at the end of a simulation tick.
/// Only needs to be ran on the server.
/// Updates AFTER the StatRecalculationSystemGroup.
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(StatRecalculationSystemGroup))]
[BurstCompile]
public partial struct StatRecalculationTagCleanUpSystem : ISystem
{
    private BufferLookup<EquippedTo> equippedToLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state) 
    {
        equippedToLookup = state.GetBufferLookup<EquippedTo>();

        state.RequireAnyForUpdate(state.GetEntityQuery(typeof(StatRecalculationTag)));
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
public class StatRecalculationSystemGroup : ComponentSystemGroup
{

}

/// <summary>
/// Marks entities for stat recalculation
/// </summary>
public struct StatRecalculationTag : IComponentData { }

[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)]
public struct StatContainer : IBufferElementData
{
    [GhostField]
    public StatData stat;

    public static void Add(DynamicBuffer<StatContainer> stats, StatData statToAdd)
    {
        var hasStat = false;
        for (var i = 0; i < stats.Length; i++)
        {
            var stat = stats[i].stat;
            if (stat.type == statToAdd.type)
            {
                hasStat = true;
                stats.Insert(i, statToAdd + stat);
            }
        }
        if (!hasStat)
        {
            stats.Add(statToAdd);
        }
    }

    public static implicit operator StatContainer(StatData d) => new StatContainer { stat = d };
}

/// <summary>
/// Defines what stat decides the min/max of a resource as well as its current value
/// </summary>
[GhostComponent]
public struct ResourceContainer : IBufferElementData
{
    [GhostField]
    public StatData minStat;
    [GhostField]
    public StatData maxStat;
    [GhostField]
    public int currentValue;

    public void Clamp()
    {
        currentValue = math.clamp(
            currentValue,
            maxStat.value,
            minStat.value);
    }
}

public struct StatRequirementContainer : IBufferElementData
{
    public StatRequirement requirement;
}

public struct StatRequirement
{
    public StatType stat;
    public int min;
    public int max;

    public bool IsInRange(StatData stat)
    {
        if (this.stat != stat.type) return false;
        return stat.value >= min && stat.value <= max;
    }
}

/// <summary>
/// Matches a stat and a value
/// </summary>
public struct StatData
{
    public StatType type;
    public int value;

    public StatData(StatType type, int value)
    {
        this.type = type;
        this.value = value;
    }

    public static StatData operator+ (StatData a, StatData b)
    {
        if (a.type == b.type) return new StatData();

        return new StatData { type = a.type, value = a.value + b.value };
    }
}

public enum StatType
{
    None,

    // Meta stats. Must be at the top since they can grant anything. 
    Level,
    TalentPoint,

    // Talents. Talents must be early since 
    TalentBrawny,
    TalentBrainy,
    TalentLithe,

    // Base stats
    Strength,
    Intelligence,
    Dexterity,

    // Resources
    Health,

    // Other stats
}