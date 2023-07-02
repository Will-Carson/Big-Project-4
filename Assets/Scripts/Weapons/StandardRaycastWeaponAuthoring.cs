using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class StandardRaycastWeaponAuthoring : MonoBehaviour
{
    public StandardWeaponFiringMecanism.Authoring FiringMecanism = StandardWeaponFiringMecanism.Authoring.GetDefault();
    public WeaponVisualFeedback.Authoring VisualFeedback = WeaponVisualFeedback.Authoring.GetDefault();
    public GameObject ShotOrigin;
    public GameObject ProjectileVisualPrefab;
    public GameObject CastOnHitEffect;
    public float Range = 1000f;
    public int Damage = 1000;
    public float SpreadDegrees = 0f;
    public int ProjectilesCount = 1;
    //public PhysicsCategoryTags HitCollisionFilter;
    
    public class Baker : Baker<StandardRaycastWeaponAuthoring>
    {
        public override void Bake(StandardRaycastWeaponAuthoring authoring)
        {
            WeaponUtilities.AddBasicWeaponBakingComponents(this);

            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new WeaponVisualFeedback(authoring.VisualFeedback));
            AddComponent(entity, new StandardWeaponFiringMecanism(authoring.FiringMecanism));
            AddComponent(entity, new StandardRaycastWeapon
            {
                ShotOrigin = GetEntity(authoring.ShotOrigin, TransformUsageFlags.Dynamic),
                ProjectileVisualPrefab = GetEntity(authoring.ProjectileVisualPrefab, TransformUsageFlags.Dynamic),
                Range = authoring.Range,
                Damage = authoring.Damage,
                SpreadRadians = math.radians(authoring.SpreadDegrees),
                ProjectilesCount = authoring.ProjectilesCount,
                //HitCollisionFilter = new CollisionFilter { BelongsTo = CollisionFilter.Default.BelongsTo, CollidesWith = authoring.HitCollisionFilter.Value },
                Random = Random.CreateFromIndex(0),
            });
            AddComponent<InterpolationDelay>(entity);
            AddBuffer<StandardRaycastWeaponShotVFXRequest>(entity);

            var damageEffectEntity = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
            AddComponent(damageEffectEntity, new DamageHealthEffect { damageValue = authoring.Damage });
            if (authoring.CastOnHitEffect != null)
            {
                AddComponent(damageEffectEntity, new CastEffectEffect { entity = GetEntity(authoring.CastOnHitEffect, TransformUsageFlags.None) });
            }

            AddBuffer<ApplyEffectToEntityBuffer>(damageEffectEntity);
            AddBuffer<ApplyEffectAtPositionBuffer>(damageEffectEntity);

            var effects = AddBuffer<EffectBuffer>(entity);
            effects.Add(new EffectBuffer
            {
                entity = damageEffectEntity
            });
        }
    }
}
