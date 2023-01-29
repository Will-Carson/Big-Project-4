using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Rival;
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
        baker.AddComponent(new WeaponControl());
        baker.AddComponent(new WeaponOwner());
        baker.AddComponent(new WeaponShotSimulationOriginOverride());
        baker.AddBuffer<WeaponShotIgnoredEntity>();
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

            // Check closest so far
            if (tmpHit.Fraction < closestValidHit.Fraction)
            {
                // Check collidable
                if (KinematicCharacterUtilities.IsHitCollidableOrCharacter(in storedKinematicCharacterDataLookup, tmpHit.Material, tmpHit.Entity))
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

    public static void ComputeShotDetails(
        ref StandardRaycastWeapon weapon,
        in WeaponShotSimulationOriginOverride shotSimulationOriginOverride,
        in DynamicBuffer<WeaponShotIgnoredEntity> ignoredEntities,
        ref NativeList<RaycastHit> Hits,
        in CollisionWorld CollisionWorld,
        in ComponentLookup<LocalToWorld> LocalToWorldLookup,
        in ComponentLookup<StoredKinematicCharacterData> StoredKinematicCharacterDataLookup,
        out bool hitFound,
        out RaycastHit closestValidHit,
        ref DynamicBuffer<StandardRaycastWeaponShotVFXRequest> shotVisualsRequests,
        bool IsServer,
        bool IsFirstTimeFullyPredictingTick)
    {
        hitFound = default;
        closestValidHit = default;
        
        // In a FPS game, it is often desirable for the weapon shot raycast to start from the camera (screen center) rather than from the actual barrel of the weapon mesh.
        // This is because it will precisely match the crosshair at the center of the screen.
        // The shot "Simulation" represents the camera point for the raycast, while the shot "Visual" represents the point where the shot mesh is spawned. 
        var shotSimulationOriginEntity = LocalToWorldLookup.HasComponent(shotSimulationOriginOverride.Entity) ? shotSimulationOriginOverride.Entity : weapon.ShotOrigin;
        var shotSimulationOriginLtW = LocalToWorldLookup[shotSimulationOriginEntity];
    
        // Allow firing multiple projectiles per shot
        for (int s = 0; s < weapon.ProjectilesCount; s++)
        {
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
            hitFound = WeaponUtilities.GetClosestValidWeaponRaycastHit(in Hits, in StoredKinematicCharacterDataLookup, in ignoredEntities, out closestValidHit);
    
            // Hit processing
            var hitDistance = weapon.Range;
            if (hitFound)
            {
                hitDistance = closestValidHit.Fraction * weapon.Range;
                hitFound = true;
            }

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
}
