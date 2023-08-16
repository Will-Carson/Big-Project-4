using Unity.Burst;
using Unity.CharacterController;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(KinematicCharacterPhysicsUpdateGroup))]
[BurstCompile]
public partial struct EffectSystem : ISystem
{
    private ComponentLookup<Health> healthLookup;
    private EntityQuery query;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        query = new EntityQueryBuilder(Allocator.Temp).WithAll<Health>().Build(ref state);
        state.RequireForUpdate(query);
        healthLookup = SystemAPI.GetComponentLookup<Health>(); // TODO this is wrong
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        healthLookup.Update(ref state);
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        /// I can't figure out how to make the Health component "blocking" for this system.
        /// Basically, if I don't have this here then other systems will complain if they access
        /// health inside of a job. I SHOULD be able to just add Health to the RequireForUpdate 
        /// for this system but that doesn't work. This does.
        foreach (var health in SystemAPI.Query<RefRW<Health>>())
        {
            break;
        }

        foreach (var (applyToEntityBuffer, damageEffect) in SystemAPI.Query<
            DynamicBuffer<ApplyEffectToEntityBuffer>,
            RefRO<DamageHealthEffect>>())
        {
            for (var i = 0; i < applyToEntityBuffer.Length; i++)
            {
                var targetEntity = applyToEntityBuffer[i].entity;

                if (healthLookup.TryGetComponent(targetEntity, out var targetHealth))
                {
                    targetHealth.current -= damageEffect.ValueRO.damageValue;
                    healthLookup[targetEntity] = targetHealth;
                }
            }
        }

        foreach (var (applyAtPositionBuffer, castEffect) in SystemAPI.Query<
            DynamicBuffer<ApplyEffectAtPositionBuffer>,
            RefRO<CastEffectEffect>>())
        {
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
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        var elapsedTime = (float)SystemAPI.Time.ElapsedTime;

        foreach (var (scaleFade, localTransform, entity) in SystemAPI.Query<
            RefRW<ScaleFade>,
            RefRW<LocalTransform>>()
            .WithEntityAccess())
        {
            if (!scaleFade.ValueRO.HasInitialized)
            {
                scaleFade.ValueRW.StartTime = elapsedTime;

                // Scale
                scaleFade.ValueRW.StartingScale = localTransform.ValueRO.Scale;

                scaleFade.ValueRW.HasInitialized = true;
            }

            if (scaleFade.ValueRO.LifeTime > 0f)
            {
                float timeRatio = (elapsedTime - scaleFade.ValueRO.StartTime) / scaleFade.ValueRO.LifeTime;
                float clampedTimeRatio = math.clamp(timeRatio, 0f, 1f);
                float invTimeRatio = 1f - clampedTimeRatio;

                localTransform.ValueRW.Scale = scaleFade.ValueRO.StartingScale * invTimeRatio;

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