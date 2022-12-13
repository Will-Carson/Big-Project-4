using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Burst;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class StatRecalculationSystemGroup : ComponentSystemGroup
{

}

/// <summary>
/// Cleans up StatRecalculationTag components. Should by definition be run at the end of a simulation tick.
/// Only needs to be ran on the server
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(StatRecalculationSystemGroup))]
[BurstCompile]
public partial struct StatRecalculationTagCleanUpSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) 
    { 
        state.RequireAnyForUpdate(state.GetEntityQuery(typeof(StatRecalculationTag)));
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        // Though 'tag' is unused, it appears there is no way to query for an entity *WITH* a tag without accessing that tag.
        foreach (var (tag, entity) in SystemAPI.Query<RefRO<StatRecalculationTag>>().WithEntityAccess())
        {
            commandBuffer.RemoveComponent<StatRecalculationTag>(entity);
        }
        commandBuffer.Playback(state.EntityManager);
    }
}

/// <summary>
/// Marks entities for stat recalculation
/// </summary>
public struct StatRecalculationTag : IComponentData { }

[GhostComponent]
public struct StatContainer : IBufferElementData
{
    [GhostField]
    public StatData stat;
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
}

/// <summary>
/// Matches a stat and a value
/// </summary>
public struct StatData
{
    public StatType type;
    public int value;
}

public enum StatType
{

}