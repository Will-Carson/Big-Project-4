using Unity.Burst;
using Unity.Entities;

/// DREAMING
/// 
/// 
/// 1. Player shoots the podium
/// 2. If all players are on the podium, spawn a dream
/// 3. Initial encounter area is spawned. Two choice gates are spawned.
/// 4. A player runs through a choice gate. The other gate despawns. A new encounter area is spawned. <summary>


public struct SpawnDreamRequest : IComponentData
{

}

[BurstCompile]
public partial struct SpawnDreamRequestHandler : ISystem
{
    [BurstCompile]
    public void OnUpdate(SystemState state)
    {
        foreach (var (request, entity) in SystemAPI.Query<SpawnDreamRequest>().WithEntityAccess())
        {

        }
    }
}