using Unity.Entities;
using Unity.Mathematics;

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


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class EffectSystem : SystemBase
{
    protected override void OnUpdate()
    {
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

            applyToEntityBuffer.Clear();
        })
        .Run();

        Entities
        .ForEach((
        Entity entity,
        ref DynamicBuffer<ApplyEffectAtPositionBuffer> applyAtPositionBuffer,
        in CastEffectEffect castEffect) =>
        {
            /// 

            applyAtPositionBuffer.Clear();
        })
        .Run();
    }
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