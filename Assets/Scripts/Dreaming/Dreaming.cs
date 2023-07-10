using Unity.Burst;
using Unity.Entities;

/// DREAMING
/// 
/// 
/// 1. Player shoots the podium
/// 2. If all players are on the podium, spawn a dream
/// 3. Initial encounter area is spawned. Two choice gates are spawned.
/// 4. A player runs through a choice gate. The other gate despawns. A new encounter area is spawned. <summary>


public struct DreamOrb : IComponentData
{

}

/// <summary>
/// Detects when a dream orb is hit. Spawns an encounter.
/// </summary>
[BurstCompile]
public partial struct DreamOrbSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (orb, entity) in SystemAPI.Query<DreamOrb>().WithEntityAccess())
        {

        }
    }
}

public struct SpawnDreamRequest : IComponentData
{

}

[BurstCompile]
public partial struct SpawnDreamRequestHandler : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (request, entity) in SystemAPI.Query<SpawnDreamRequest>().WithEntityAccess())
        {

        }
    }
}