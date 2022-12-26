/// Get abilities from containers
/// Trigger ability spawning / casting
/// Use predictive spawning when casting
/// Define an archetype for an ability caster
/// Define an archetype for an ability 

using Unity.Entities;
using Unity.NetCode;


[GhostComponent]
public struct CurrentWeaponContainer : IComponentData
{
    public Entity entity;
}

[GhostComponent]
public struct WeaponContainer : IBufferElementData
{
    public ushort id;
    public Entity entity;
}

[GhostComponent]
public struct CastTimeComponent : IComponentData
{
    public float castTime;
}