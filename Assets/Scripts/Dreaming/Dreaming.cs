using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.Rendering;
using Unity.Physics;
using Unity.CharacterController;
using System;
using System.Collections.Generic;

/// DREAMING
/// 
/// 
/// . Player shoots the podium
/// . If all players are on the podium, spawn a dream. Teleport the players to the dream.
/// . Initial encounter area is spawned. Two choice gates are spawned.
/// . A player runs through a choice gate. The other gate despawns. A new encounter area is spawned. <summary>

/// I have to be able to choose from a list of encounters
/// Encounters must have a list of tags and weights
/// When you defeat a boss and gain a key, you can jump off of an encounter


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
    Unity.Mathematics.Random random;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        random = new Unity.Mathematics.Random(uint.MaxValue);
        state.RequireForUpdate<PrefabContainer>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var encounters = SystemAPI.GetSingletonBuffer<Encounter>(true);
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        
        foreach (var (health, entity) in SystemAPI.Query<RefRO<Health>>().WithAll<DreamOrb>().WithEntityAccess())
        {
            if (health.ValueRO.currentHealth <= 0)
            {
                //health.ValueRW.currentHealth = health.ValueRW.maxHealth;
                var spawnPoint = SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<DreamSpawnpoint>());

                var encounterPrefab = Encounter.GetRandomEncounterByTag(encounters, EncounterFlags.InitialEncounter, ref random).prefab;

                var encounterInstance = commandBuffer.Instantiate(encounterPrefab);
                commandBuffer.SetComponent(encounterInstance, spawnPoint);

                commandBuffer.DestroyEntity(entity);
            }
        }

        var elapsedTime = SystemAPI.Time.ElapsedTime;
        foreach (var (character, transform, jumper) in SystemAPI.Query<RefRO<KinematicCharacterBody>, RefRO<LocalTransform>, RefRW<Jumper>>())
        {
            if (character.ValueRO.RelativeVelocity.y < -40 && jumper.ValueRW.CanJump(elapsedTime))
            {
                jumper.ValueRW.lastActivated = (float)elapsedTime;

                var encounterPrefab = Encounter.GetRandomEncounterByTag(encounters, EncounterFlags.Combat, ref random).prefab;

                var encounterInstance = commandBuffer.Instantiate(encounterPrefab);
                var encounterPosition = transform.ValueRO.Translate(new float3(0, -10, 0)).Position;
                var encounterTransform = LocalTransform.FromPosition(encounterPosition);

                commandBuffer.SetComponent(encounterInstance, encounterTransform);
            }
        }
    }
}

public struct Jumper : IComponentData
{
    public float delay;
    public float lastActivated;

    internal bool CanJump(double elapsedTime)
    {
        return lastActivated + delay < elapsedTime;
    }
}