using Unity.Burst;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class DreamManagerAuthoring : MonoBehaviour
{
    class Baker : Baker<DreamManagerAuthoring>
    {
        public override void Bake(DreamManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new DreamManager
            {

            });
        }
    }
}

public struct DreamManager : IComponentData
{
    public float bossJuice;
    public bool nextEncounterIsBoss;
    public int bossesSpawned;
    public int bossesKilled;
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct DreamManagerSystem : ISystem
{
    Unity.Mathematics.Random random;

    public void OnCreate(ref SystemState state)
    {
        random = new Unity.Mathematics.Random(uint.MaxValue);
    }

    public void OnUpdate(ref SystemState state)
    {
        var encounters = SystemAPI.GetSingletonBuffer<Encounter>(true);
        var manager = SystemAPI.GetSingleton<DreamManager>();
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        {
            foreach (var (grantsBossJuiceSetup, entity) in SystemAPI.Query<RefRO<GrantsBossJuiceOnDestructionSetup>>().WithNone<GrantsBossJuiceOnDestruction>().WithEntityAccess())
            {
                commandBuffer.AddComponent(entity, new GrantsBossJuiceOnDestruction { amount = grantsBossJuiceSetup.ValueRO.amount });
            }

            foreach (var (grantsBossJuice, entity) in SystemAPI.Query<RefRO<GrantsBossJuiceOnDestruction>>().WithNone<GrantsBossJuiceOnDestructionSetup>().WithEntityAccess())
            {
                manager.bossJuice += grantsBossJuice.ValueRO.amount;
                commandBuffer.RemoveComponent<GrantsBossJuiceOnDestruction>(entity);
            }
        } // Set up boss juice

        if (manager.bossJuice > 15) // TODO magic numbuh
        {
            manager.bossJuice = 0;
            manager.nextEncounterIsBoss = true;
        }

        {
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

                    if (manager.nextEncounterIsBoss == true)
                    {
                        Debug.Log("Prepare to die!");
                        encounterPrefab = Encounter.GetRandomEncounterByTag(encounters, EncounterFlags.Boss, ref random).prefab;
                    }

                    var encounterInstance = commandBuffer.Instantiate(encounterPrefab);
                    var encounterPosition = transform.ValueRO.Translate(new float3(0, -10, 0)).Position;
                    var encounterTransform = LocalTransform.FromPosition(encounterPosition);

                    commandBuffer.SetComponent(encounterInstance, encounterTransform);
                }
            }
        } // Spawn encounters

        {
            foreach (var (bossTagSetup, entity) in SystemAPI.Query<RefRO<BossTagSetup>>().WithEntityAccess().WithNone<BossTag>())
            {
                manager.bossesSpawned += 1;
                commandBuffer.AddComponent<BossTag>(entity);
            }

            foreach (var (bossTag, entity) in SystemAPI.Query<RefRO<BossTag>>().WithEntityAccess().WithNone<BossTagSetup>())
            {
                manager.bossesKilled += 1;
                commandBuffer.RemoveComponent<BossTag>(entity);
            }
        } // Boss setup

        var bossesHaveSpawned = manager.bossesKilled > 0;
        var allBossesHaveBeenKilled = manager.bossesKilled >= manager.bossesSpawned;

        if (bossesHaveSpawned && allBossesHaveBeenKilled)
        {
            manager.bossesSpawned = 0;
            manager.bossesKilled = 0;

            foreach (var (postBossSpawner, localTransform, entity) in SystemAPI.Query<RefRO<PostBossSpawner>, RefRO<LocalTransform>>().WithEntityAccess())
            {
                var instance = commandBuffer.Instantiate(postBossSpawner.ValueRO.prefab);

                commandBuffer.SetComponent(instance, localTransform.ValueRO);
                commandBuffer.DestroyEntity(entity);
            }
        }

        SystemAPI.SetSingleton(manager);
    }
}

public struct BossTagSetup : IComponentData { }

public struct BossTag : ICleanupComponentData { }

public struct GrantsBossJuiceOnDestructionSetup : IComponentData
{
    public float amount;
}

public struct GrantsBossJuiceOnDestruction : ICleanupComponentData
{
    public float amount;
}