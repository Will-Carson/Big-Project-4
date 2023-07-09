using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
[BurstCompile]
public partial struct EffectSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(SystemState state)
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        var healthLookup = SystemAPI.GetComponentLookup<Health>();

        foreach (var (applyToEntityBuffer, damageEffect) in SystemAPI.Query<
            DynamicBuffer<ApplyEffectToEntityBuffer>,
            RefRO<DamageHealthEffect>>())
        {
            /// 

            for (var i = 0; i < applyToEntityBuffer.Length; i++)
            {
                var targetEntity = applyToEntityBuffer[i].entity;
                Debug.Log(targetEntity);

                if (healthLookup.TryGetComponent(targetEntity, out var targetHealth))
                {
                    targetHealth.currentHealth -= damageEffect.ValueRO.damageValue;
                    healthLookup[targetEntity] = targetHealth;
                }
            }
        }

        foreach (var (applyAtPositionBuffer, castEffect) in SystemAPI.Query<
            DynamicBuffer<ApplyEffectAtPositionBuffer>,
            RefRO<CastEffectEffect>>())
        {
            /// 

            for (var i = 0; i < applyAtPositionBuffer.Length; i++)
            {
                var position = applyAtPositionBuffer[i].position;

                var instance = commandBuffer.Instantiate(castEffect.ValueRO.entity);
                commandBuffer.SetComponent(instance, LocalTransform.FromPosition(position));
            }
        }

        foreach (var applyEffectToEntityBuffer in SystemAPI.Query<
            DynamicBuffer<ApplyEffectToEntityBuffer>>())
        {
            applyEffectToEntityBuffer.Clear();
        }

        foreach (var applyAtPositionBuffer in SystemAPI.Query<
            DynamicBuffer<ApplyEffectAtPositionBuffer>>())
        {
            applyAtPositionBuffer.Clear();
        }
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
[UpdateAfter(typeof(PostPredictionPreTransformsECBSystem))]
public partial struct ScaleFadeSystem : ISystem
{
    public void OnUpdate(SystemState state)
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        var elapsedTime = (float)SystemAPI.Time.ElapsedTime;

        foreach (var (scaleFadeRef, localTransformRef, entity) in SystemAPI.Query<
            RefRW<ScaleFade>,
            RefRW<LocalTransform>>()
            .WithEntityAccess())
        {
            var scaleFade = scaleFadeRef.ValueRW;
            var localTransform = localTransformRef.ValueRW;

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
        }
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