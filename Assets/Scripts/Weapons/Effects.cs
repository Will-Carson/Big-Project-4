using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class EffectSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var damageResourceEffectLookup = SystemAPI.GetComponentLookup<DamageResourceEffect>();

        Entities
        .ForEach((
        Entity entity,
        ref DynamicBuffer<EffectBuffer> effects,
        ref DynamicBuffer<ResourceContainer> resources,
        ref DynamicBuffer<EquipStatStickRequest> equipStatStickRequests) =>
        {
            for (var i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];

                // Process resource effects
                if (damageResourceEffectLookup.TryGetComponent(effect.effectEntity, out var damageResourceEffect))
                {
                    for (var r = 0; r < resources.Length; r++)
                    {
                        var resource = resources[r];
                        if (resource.maxStat.stat == damageResourceEffect.resource)
                        {
                            resource.ModifyCurrent(damageResourceEffect.damageValue);
                        }
                    }
                }

                // Process stat effects
                // TODO
            }
        })
        .Run();
    }
}

public struct EffectBuffer : IBufferElementData
{
    public Entity effectEntity;
}

public struct DamageResourceEffect : IComponentData
{
    public StatType resource;
    public int damageValue;
}