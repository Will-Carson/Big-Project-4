using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Rival;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(WeaponPredictionUpdateGroup))]
[BurstCompile]
public partial struct StandardRaycastWeaponPredictionSystem : ISystem
{
    private NativeList<RaycastHit> _hits;
    private NativeList<RaycastHit> _validHits;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<StandardRaycastWeapon, StandardWeaponFiringMecanism>().Build());

        _hits = new NativeList<RaycastHit>(Allocator.Persistent);
        _validHits = new NativeList<RaycastHit>(Allocator.Persistent);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        _hits.Dispose();
        _validHits.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        StandardRaycastWeaponPredictionJob predictionJob = new StandardRaycastWeaponPredictionJob
        {
            IsServer = state.WorldUnmanaged.IsServer(), 
            
            NetworkTime = SystemAPI.GetSingleton<NetworkTime>(),
            PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
            PhysicsWorldHistory = SystemAPI.GetSingleton<PhysicsWorldHistorySingleton>(),
            LocalToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true),
            StoredKinematicCharacterDataLookup = SystemAPI.GetComponentLookup<StoredKinematicCharacterData>(true),
            Hits = _hits,
            validHits = _validHits,

            applyEffectToEntityBufferLookup = SystemAPI.GetBufferLookup<ApplyEffectToEntityBuffer>(false),
            applyEffectAtPositionBufferLookup = SystemAPI.GetBufferLookup<ApplyEffectAtPositionBuffer>(false),
        };
        predictionJob.Schedule();
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct StandardRaycastWeaponPredictionJob : IJobEntity
    {
        public bool IsServer;
        public NetworkTime NetworkTime;
        [ReadOnly] public PhysicsWorld PhysicsWorld;
        public PhysicsWorldHistorySingleton PhysicsWorldHistory;
        [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;
        [ReadOnly] public ComponentLookup<StoredKinematicCharacterData> StoredKinematicCharacterDataLookup;
        public NativeList<RaycastHit> Hits;
        public NativeList<RaycastHit> validHits;

        public BufferLookup<ApplyEffectToEntityBuffer> applyEffectToEntityBufferLookup;
        public BufferLookup<ApplyEffectAtPositionBuffer> applyEffectAtPositionBufferLookup;

        void Execute(
            Entity entity, 
            ref StandardRaycastWeapon weapon, 
            ref WeaponVisualFeedback weaponFeedback,
            ref DynamicBuffer<StandardRaycastWeaponShotVFXRequest> shotVFXRequestsBuffer,
            in InterpolationDelay interpolationDelay,
            in StandardWeaponFiringMecanism mecanism, 
            in WeaponShotSimulationOriginOverride shotSimulationOriginOverride, 
            in DynamicBuffer<WeaponShotIgnoredEntity> ignoredEntities,
            in DynamicBuffer<EffectBuffer> effectBuffer)
        {
            PhysicsWorldHistory.GetCollisionWorldFromTick(NetworkTime.ServerTick, interpolationDelay.Value, ref PhysicsWorld, out var collisionWorld);
            
            /// Here we loop based on the ShotsToFire, which causes our damage effect to only apply once because we're not looping over the shots.
            /// We need to separate out the functionality here so that we can calculate individual shots in ComputeShotDetails.
            for (int i = 0; i < mecanism.ShotsToFire; i++)
            {
                for (var shot = 0; shot < weapon.ProjectilesCount; shot++)
                {
                    WeaponUtilities.ComputeShotDetails(
                        ref weapon,
                        in shotSimulationOriginOverride,
                        in ignoredEntities,
                        ref Hits,
                        ref validHits,
                        in collisionWorld,
                        in LocalToWorldLookup,
                        in StoredKinematicCharacterDataLookup,
                        ref shotVFXRequestsBuffer,
                        IsServer,
                        NetworkTime.IsFirstTimeFullyPredictingTick);

                    /// The weapon will have an associated buffer of effects
                    /// Effects are entities with a buffer of entities / positions that the effect might be applied to
                    /// This avoids making new entities any time we want to apply an effect

                    for (var k = 0; k < Hits.Length; k++)
                    {
                        var hitEntity = Hits[k].Entity;

                        if (hitEntity == Entity.Null) continue;

                        for (var j = 0; j < effectBuffer.Length; j++)
                        {
                            var effect = effectBuffer[j];

                            if (applyEffectToEntityBufferLookup.TryGetBuffer(effect.entity, out var applyToEntityBuffer))
                            {
                                applyToEntityBuffer.Add(new ApplyEffectToEntityBuffer { entity = hitEntity });
                            }

                            if (applyEffectAtPositionBufferLookup.TryGetBuffer(effect.entity, out var applyAtPositionBuffer))
                            {
                                applyAtPositionBuffer.Add(new ApplyEffectAtPositionBuffer { position = Hits[k].Position });
                            }
                        }
                    }

                    // Recoil & FOV kick
                    if (IsServer)
                    {
                        weapon.RemoteShotsCount++;
                    }
                    else if (NetworkTime.IsFirstTimeFullyPredictingTick)
                    {
                        weaponFeedback.ShotFeedbackRequests++;
                    }
                }
            }
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct StandardRaycastWeaponVisualsSystem : ISystem
{
    private NativeList<RaycastHit> _hits;
    private NativeList<RaycastHit> _validHits;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<StandardRaycastWeapon, StandardWeaponFiringMecanism>().Build());

        _hits = new NativeList<RaycastHit>(Allocator.Persistent);
        _validHits = new NativeList<RaycastHit>(Allocator.Persistent);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        _hits.Dispose();
        _validHits.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var localNetId = -1;
        if (SystemAPI.HasSingleton<NetworkIdComponent>())
        {
            localNetId = SystemAPI.GetSingleton<NetworkIdComponent>().Value;
        }
        
        var remoteShotsJob = new StandardRaycastWeaponRemoteShotsJob
        {
            LocalNetId = localNetId,
            NetworkTime = SystemAPI.GetSingleton<NetworkTime>(),
            CollisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,
            LocalToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true),
            StoredKinematicCharacterDataLookup = SystemAPI.GetComponentLookup<StoredKinematicCharacterData>(true),
            Hits = _hits,
            validHits = _validHits,
        };
        remoteShotsJob.Schedule();

        var visualsJob = new StandardRaycastWeaponShotVisualsJob
        {
            ECB = SystemAPI.GetSingletonRW<PostPredictionPreTransformsECBSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged),
            CharacterWeaponVisualFeedbackLookup = SystemAPI.GetComponentLookup<CharacterWeaponVisualFeedback>(false),
        };
        visualsJob.Schedule();
    }

    [BurstCompile]
    public partial struct StandardRaycastWeaponRemoteShotsJob : IJobEntity
    {
        public int LocalNetId;
        public NetworkTime NetworkTime;
        public CollisionWorld CollisionWorld;
        [ReadOnly]
        public ComponentLookup<LocalToWorld> LocalToWorldLookup;
        [ReadOnly]
        public ComponentLookup<StoredKinematicCharacterData> StoredKinematicCharacterDataLookup;
        public NativeList<RaycastHit> Hits;
        public NativeList<RaycastHit> validHits;

        void Execute(
            Entity entity, 
            ref StandardRaycastWeapon weapon, 
            ref WeaponVisualFeedback weaponFeedback,
            ref DynamicBuffer<StandardRaycastWeaponShotVFXRequest> shotVFXRequestsBuffer,
            in GhostOwnerComponent ghostOwnerComponent,
            in WeaponShotSimulationOriginOverride shotSimulationOriginOverride, 
            in DynamicBuffer<WeaponShotIgnoredEntity> ignoredEntities)
        {
            if (ghostOwnerComponent.NetworkId != LocalNetId)
            {
                // TODO: should handle the case where a weapon goes out of client's area-of-interest and then comes back later with a high shots count diff
                uint shotsToProcess = weapon.RemoteShotsCount - weapon.LastRemoteShotsCount;
                weapon.LastRemoteShotsCount = weapon.RemoteShotsCount;

                for (int i = 0; i < shotsToProcess; i++)
                {
                    WeaponUtilities.ComputeMultishotDetails(
                        ref weapon,
                        in shotSimulationOriginOverride,
                        in ignoredEntities,
                        ref Hits,
                        ref validHits,
                        in CollisionWorld,
                        in LocalToWorldLookup,
                        in StoredKinematicCharacterDataLookup,
                        ref shotVFXRequestsBuffer,
                        false,
                        NetworkTime.IsFirstTimeFullyPredictingTick);

                    weaponFeedback.ShotFeedbackRequests++;
                }
            }
        }
    }

    [BurstCompile]
    public partial struct StandardRaycastWeaponShotVisualsJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public ComponentLookup<CharacterWeaponVisualFeedback> CharacterWeaponVisualFeedbackLookup;

        void Execute(
            Entity entity, 
            ref StandardRaycastWeapon weapon, 
            ref WeaponVisualFeedback weaponFeedback,
            ref DynamicBuffer<StandardRaycastWeaponShotVFXRequest> shotVFXRequestsBuffer,
            in WeaponOwner owner)
        {
            // Shot VFX
            for (int i = 0; i < shotVFXRequestsBuffer.Length; i++)
            {
                StandardRaycastWeaponShotVisualsData shotVisualsData = shotVFXRequestsBuffer[i].ShotVisualsData;
                
                Entity shotVisualsEntity = ECB.Instantiate(weapon.ProjectileVisualPrefab);
                ECB.SetComponent(shotVisualsEntity, LocalTransform.FromPositionRotation(shotVisualsData.VisualOrigin, quaternion.LookRotationSafe(shotVisualsData.SimulationDirection, shotVisualsData.SimulationUp)));
                ECB.AddComponent(shotVisualsEntity, shotVisualsData);
            }
            shotVFXRequestsBuffer.Clear();

            // Shot feedback
            for (int i = 0; i < weaponFeedback.ShotFeedbackRequests; i++)
            {
                if (CharacterWeaponVisualFeedbackLookup.TryGetComponent(owner.Entity, out CharacterWeaponVisualFeedback characterFeedback))
                {
                    characterFeedback.CurrentRecoil += weaponFeedback.RecoilStrength;
                    characterFeedback.TargetRecoilFOVKick += weaponFeedback.RecoilFOVKick;

                    CharacterWeaponVisualFeedbackLookup[owner.Entity] = characterFeedback;
                }
            }
            weaponFeedback.ShotFeedbackRequests = 0;
        }
    }
}