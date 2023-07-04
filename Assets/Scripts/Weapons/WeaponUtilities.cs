using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.CharacterController;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

public static class WeaponUtilities 
{
    public static void AddBasicWeaponBakingComponents<T>(Baker<T> baker) where T : MonoBehaviour
    {
        var entity = baker.GetEntity(TransformUsageFlags.Dynamic);

        baker.AddComponent(entity, new WeaponControl());
        baker.AddComponent(entity, new WeaponOwner());
        baker.AddComponent(entity, new WeaponShotSimulationOriginOverride());
        baker.AddBuffer<WeaponShotIgnoredEntity>(entity);
    }

    public static bool GetClosestValidWeaponRaycastHit(
        in NativeList<RaycastHit> hits, 
        in ComponentLookup<StoredKinematicCharacterData> storedKinematicCharacterDataLookup,
        in DynamicBuffer<WeaponShotIgnoredEntity> ignoredEntities,
        out RaycastHit closestValidHit)
    {
        closestValidHit = default;
        closestValidHit.Fraction = float.MaxValue;
        for (int j = 0; j < hits.Length; j++)
        {
            RaycastHit tmpHit = hits[j];

            //Check closest so far
            if (tmpHit.Fraction < closestValidHit.Fraction)
            {
                // Check collidable
                //if (KinematicCharacterUtilities.IsHitCollidableOrCharacter(in storedKinematicCharacterDataLookup, tmpHit.Material, tmpHit.Entity))
                if (true)
                {
                    // Check entity ignore
                    bool entityValid = true;
                    for (int k = 0; k < ignoredEntities.Length; k++)
                    {
                        if (tmpHit.Entity == ignoredEntities[k].Entity)
                        {
                            entityValid = false;
                            break;
                        }
                    }

                    // Final hit
                    if (entityValid)
                    {
                        closestValidHit = tmpHit;
                    }
                }
            }
        }

        return closestValidHit.Entity != Entity.Null;
    }

    public static void ComputeMultishotDetails(
        ref StandardRaycastWeapon weapon,
        in WeaponShotSimulationOriginOverride shotSimulationOriginOverride,
        in DynamicBuffer<WeaponShotIgnoredEntity> ignoredEntities,
        ref NativeList<RaycastHit> Hits,
        ref NativeList<RaycastHit> validHits,
        in CollisionWorld CollisionWorld,
        in ComponentLookup<LocalToWorld> LocalToWorldLookup,
        in ComponentLookup<StoredKinematicCharacterData> StoredKinematicCharacterDataLookup,
        ref DynamicBuffer<StandardRaycastWeaponShotVFXRequest> shotVisualsRequests,
        bool IsServer,
        bool IsFirstTimeFullyPredictingTick)
    {
        // Allow firing multiple projectiles per shot
        for (int s = 0; s < weapon.ProjectilesCount; s++)
        {
            ComputeShotDetails(
                ref weapon,
                shotSimulationOriginOverride,
                ignoredEntities,
                ref Hits,
                ref validHits,
                CollisionWorld,
                LocalToWorldLookup,
                StoredKinematicCharacterDataLookup,
                ref shotVisualsRequests,
                IsServer,
                IsFirstTimeFullyPredictingTick);
        }
    }

    public static void ComputeShotDetails(
        ref StandardRaycastWeapon weapon,
        in WeaponShotSimulationOriginOverride shotSimulationOriginOverride,
        in DynamicBuffer<WeaponShotIgnoredEntity> ignoredEntities,
        ref NativeList<RaycastHit> Hits,
        ref NativeList<RaycastHit> validHits,
        in CollisionWorld CollisionWorld,
        in ComponentLookup<LocalToWorld> LocalToWorldLookup,
        in ComponentLookup<StoredKinematicCharacterData> StoredKinematicCharacterDataLookup,
        ref DynamicBuffer<StandardRaycastWeaponShotVFXRequest> shotVisualsRequests,
        bool IsServer,
        bool IsFirstTimeFullyPredictingTick)
    {
        // In a FPS game, it is often desirable for the weapon shot raycast to start from the camera (screen center) rather than from the actual barrel of the weapon mesh.
        // This is because it will precisely match the crosshair at the center of the screen.
        // The shot "Simulation" represents the camera point for the raycast, while the shot "Visual" represents the point where the shot mesh is spawned. 
        var shotSimulationOriginEntity = LocalToWorldLookup.HasComponent(shotSimulationOriginOverride.Entity) ? shotSimulationOriginOverride.Entity : weapon.ShotOrigin;
        var shotSimulationOriginLtW = LocalToWorldLookup[shotSimulationOriginEntity];

        // Calculate spread
        var shotSpreadRotation = quaternion.identity;
        if (weapon.SpreadRadians > 0f)
        {
            shotSpreadRotation = math.slerp(weapon.Random.NextQuaternionRotation(), quaternion.identity, (math.PI - math.clamp(weapon.SpreadRadians, 0f, math.PI)) / math.PI);
        }
        var finalShotSimulationDirection = math.rotate(shotSpreadRotation, shotSimulationOriginLtW.Forward);

        // Hit detection
        Hits.Clear();
        var rayInput = new RaycastInput
        {
            Start = shotSimulationOriginLtW.Position,
            End = shotSimulationOriginLtW.Position + (finalShotSimulationDirection * weapon.Range),
            Filter = weapon.HitCollisionFilter,
        };
        CollisionWorld.CastRay(rayInput, ref Hits);
        WeaponUtilities.GetClosestValidWeaponRaycastHit(in Hits, in StoredKinematicCharacterDataLookup, in ignoredEntities, out var closestValidHit);

        // Hit processing
        var hitFound = closestValidHit.Entity != Entity.Null;
        var hitDistance = weapon.Range;
        if (closestValidHit.Entity != Entity.Null)
        {
            hitDistance = closestValidHit.Fraction * weapon.Range;
            validHits.Add(closestValidHit);
        }

        Debug.Log(hitFound);

        // No need to do visuals on resimulated ticks
        if (IsFirstTimeFullyPredictingTick)
            return;

        // Shot visuals
        if (!IsServer)
        {
            var shotVisualOriginLtW = LocalToWorldLookup[weapon.ShotOrigin];

            var visualOriginToSimulationHit = shotVisualOriginLtW.Position + (finalShotSimulationDirection * hitDistance);
            if (hitFound)
            {
                visualOriginToSimulationHit = closestValidHit.Position - shotVisualOriginLtW.Position;
            }

            shotVisualsRequests.Add(new StandardRaycastWeaponShotVFXRequest(
                new StandardRaycastWeaponShotVisualsData
                {
                    VisualOrigin = shotVisualOriginLtW.Position,
                    SimulationOrigin = shotSimulationOriginLtW.Position,
                    SimulationDirection = finalShotSimulationDirection,
                    SimulationUp = shotSimulationOriginLtW.Up,
                    SimulationHitDistance = hitDistance,
                    Hit = closestValidHit,
                    VisualOriginToHit = visualOriginToSimulationHit,
                })
            );
        }
    }
}
