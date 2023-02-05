using Rival;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderFirst = true)]
[UpdateBefore(typeof(PredictedFixedStepSimulationSystemGroup))]
[BurstCompile]
public partial struct ActiveWeaponSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    { }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var setupJob = new ActiveWeaponSetupJob
        {
            ECB = SystemAPI.GetSingletonRW<PostPredictionPreTransformsECBSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged),
            WeaponControlLookup = SystemAPI.GetComponentLookup<WeaponControl>(true),
            platformerCharacterComponentLookup = SystemAPI.GetComponentLookup<PlatformerCharacterComponent>(true),
            WeaponSimulationShotOriginOverrideLookup = SystemAPI.GetComponentLookup<WeaponShotSimulationOriginOverride>(false),
            LinkedEntityGroupLookup = SystemAPI.GetBufferLookup<LinkedEntityGroup>(false),
            WeaponShotIgnoredEntityLookup = SystemAPI.GetBufferLookup<WeaponShotIgnoredEntity>(false),
        };
        setupJob.Schedule();
    }

    [BurstCompile]
    public partial struct ActiveWeaponSetupJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        [ReadOnly] public ComponentLookup<WeaponControl> WeaponControlLookup;
        [ReadOnly] public ComponentLookup<PlatformerCharacterComponent> platformerCharacterComponentLookup;
        public ComponentLookup<WeaponShotSimulationOriginOverride> WeaponSimulationShotOriginOverrideLookup;
        public BufferLookup<LinkedEntityGroup> LinkedEntityGroupLookup;
        public BufferLookup<WeaponShotIgnoredEntity> WeaponShotIgnoredEntityLookup;

        void Execute(Entity entity, ref ActiveWeapon activeWeapon)
        {
            // Detect changes in active weapon
            if (activeWeapon.entity != activeWeapon.previousEntity)
            {
                // Setup new weapon
                if (WeaponControlLookup.HasComponent(activeWeapon.entity))
                {
                    // Setup for characters
                    if (platformerCharacterComponentLookup.TryGetComponent(entity, out var character))
                    {
                        // Remember weapon owner
                        ECB.SetComponent(activeWeapon.entity, new WeaponOwner { Entity = entity });

                        // Make weapon linked to the character
                        var linkedEntityBuffer = LinkedEntityGroupLookup[entity];
                        linkedEntityBuffer.Add(new LinkedEntityGroup { Value = activeWeapon.entity });
                        
                        // Add character as ignored shot entities
                        if (WeaponShotIgnoredEntityLookup.TryGetBuffer(activeWeapon.entity, out var ignoredEntities))
                        {
                            ignoredEntities.Add(new WeaponShotIgnoredEntity { Entity = entity });
                        }
                    }
                }
                
                // TODO: Un-setup previous weapon
                // if (WeaponControlLookup.HasComponent(activeWeapon.PreviousEntity))
                // {
                        // Disable weapon update, reset owner, reset data, unparent, etc...
                // }
            }
            
            activeWeapon.previousEntity = activeWeapon.entity;
        }
    }

}

[BurstCompile]
public partial class WeaponMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(false);

        Entities
        .ForEach((
        Entity entity,
        in WeaponOwner owner) =>
        {
            var transform = localTransformLookup[entity];
            if (owner.Entity == Entity.Null)
            {
                return;
            }

            var state = SystemAPI.GetComponent<PlatformerCharacterStateMachine>(owner.Entity).CurrentState;
            if (state == CharacterState.Climbing || state == CharacterState.Stunned || state == CharacterState.Dead)
            {
                return;
            }

            var characterControl = SystemAPI.GetComponent<PlatformerCharacterControl>(owner.Entity);
            var weaponSocketEntity = SystemAPI.GetComponent<PlatformerCharacterComponent>(owner.Entity).WeaponAnimationSocketEntity;
            var characterTransform = SystemAPI.GetComponent<LocalTransform>(owner.Entity);

            CharacterControlUtilities.SlerpRotationTowardsDirection(ref transform.Rotation, deltaTime, math.normalizesafe(characterControl.LookVector), float.MaxValue);

            var targetPosition = characterTransform.Position + math.mul(characterTransform.Rotation, new float3(.5f, 1, 1));
            transform.Position = math.lerp(transform.Position, targetPosition, 1);

            localTransformLookup[entity] = transform;
        })
        .Run();
    }
}
