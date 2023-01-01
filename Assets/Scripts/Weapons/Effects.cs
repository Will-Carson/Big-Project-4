using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class EffectSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var damageResourceEffectLookup = SystemAPI.GetComponentLookup<DamageHealthEffect>();

        Entities
        .ForEach((
        Entity entity,
        ref DynamicBuffer<EffectBuffer> effects,
        ref DynamicBuffer<EquipStatStickRequest> equipStatStickRequests) =>
        {
            for (var i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];

                // Process resource effects
                if (damageResourceEffectLookup.TryGetComponent(effect.effectEntity, out var damageResourceEffect))
                {
                    
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

public struct DamageHealthEffect : IComponentData
{
    public int damageValue;
}