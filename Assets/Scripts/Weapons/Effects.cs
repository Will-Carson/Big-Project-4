using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(CustomStatHandlingSystemGroup))]
public partial class BuildWeaponEffectsSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
        .WithAll<StatRecalculationTag>()
        .ForEach((
        Entity entity, 
        ref DynamicBuffer<EffectBuffer> effects) =>
        {

        })
        .Run();
    }
}


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
public partial class EffectSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        var healthLookup = SystemAPI.GetComponentLookup<Health>();

        Entities
        .ForEach((
        Entity entity,
        ref DynamicBuffer<ApplyEffectToEntityBuffer> applyToEntityBuffer,
        in DamageHealthEffect damageEffect) =>
        {
            /// 

            for (var i = 0; i < applyToEntityBuffer.Length; i++)
            {
                var targetEntity = applyToEntityBuffer[i].entity;

                if (healthLookup.TryGetComponent(targetEntity, out var targetHealth))
                {
                    targetHealth.currentHealth -= damageEffect.damageValue;
                    healthLookup[targetEntity] = targetHealth;
                }
            }
        })
        .Run();

        Entities
        .ForEach((
        Entity entity,
        ref DynamicBuffer<ApplyEffectAtPositionBuffer> applyAtPositionBuffer,
        in CastEffectEffect castEffect) =>
        {
            /// 

            for (var i = 0; i < applyAtPositionBuffer.Length; i++)
            {
                var position = applyAtPositionBuffer[i].position;

                var instance = commandBuffer.Instantiate(castEffect.entity);
                commandBuffer.SetComponent(instance, LocalTransform.FromPosition(position));
            }
        })
        .Run();

        Entities
        .ForEach((
        ref DynamicBuffer<ApplyEffectToEntityBuffer> applyEffectToEntityBuffer) =>
        {
            applyEffectToEntityBuffer.Clear();
        })
        .Run();

        Entities
        .ForEach((
        ref DynamicBuffer<ApplyEffectAtPositionBuffer> applyEffectAtPositionBuffer) =>
        {
            applyEffectAtPositionBuffer.Clear();
        })
        .Run();
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
[UpdateAfter(typeof(PostPredictionPreTransformsECBSystem))]
public partial class ScaleFadeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        var elapsedTime = (float)SystemAPI.Time.ElapsedTime;

        Entities
        .ForEach((
        Entity entity,
        ref ScaleFade scaleFade,
        //ref PostTransformScale postTransformScale,
        ref LocalTransform localTransform) =>
        {
            if (!scaleFade.HasInitialized)
            {
                scaleFade.StartTime = elapsedTime;

                // Scale
                scaleFade.StartingScale = localTransform.Scale;

                scaleFade.HasInitialized = true;
            }

            if (scaleFade.LifeTime > 0f)
            {
                float timeRatio = (elapsedTime - scaleFade.StartTime) / scaleFade.LifeTime;
                float clampedTimeRatio = math.clamp(timeRatio, 0f, 1f);
                float invTimeRatio = 1f - clampedTimeRatio;

                localTransform.Scale = scaleFade.StartingScale * invTimeRatio;

                if (timeRatio >= 1f)
                {
                    commandBuffer.DestroyEntity(entity);
                }
            }
            else
            {
                commandBuffer.DestroyEntity(entity);
            }
        })
        .Run();
    }
}

public struct ScaleFade : IComponentData
{
    public float LifeTime;
    public float Width;

    public float StartTime;
    public float StartingScale;
    public bool HasInitialized;
}

public struct CastEffectEffect : IComponentData
{
    public Entity entity;
}

public struct EffectBuffer : IBufferElementData
{
    public Entity entity;
}

public struct DamageHealthEffect : IComponentData
{
    public int damageValue;
}

public struct ApplyEffectToEntityBuffer : IBufferElementData
{
    public Entity entity;
}

public struct ApplyEffectAtPositionBuffer : IBufferElementData
{
    public float3 position;
}

public struct Health : IComponentData
{
    public int maxHealth;
    public int currentHealth;
}