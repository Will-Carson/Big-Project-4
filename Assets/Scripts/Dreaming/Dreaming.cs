using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

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
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct DreamOrbSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PrefabContainer>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var (orb, entity) in SystemAPI.Query<DreamOrb>().WithEntityAccess())
        {

        }
        foreach (var (player, entity) in SystemAPI.Query<PlatformerPlayerInputs>().WithEntityAccess())
        {
            var prefabs = SystemAPI.GetSingletonBuffer<PrefabContainer>(true);
            var landmassPrefab = PrefabContainer.GetEntityWithId(prefabs, "InitialEncounter");

            var landmassInstance = commandBuffer.Instantiate(landmassPrefab);
            commandBuffer.SetComponent(landmassInstance, LocalTransform.FromPosition(new float3(20, -5, 20)));

            Debug.Log("WE BE DREAMIN'");
            state.Enabled = false;
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