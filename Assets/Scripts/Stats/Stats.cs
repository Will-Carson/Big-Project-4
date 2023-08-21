/// 
/// The versatility of stats is important. This means two things:
/// 1. Stats must be able to be derived from any place; Auras, talent trees,
/// equipment, or in the case of minions, from their 'owner.'
/// 2. Stats must be able to be used to effect any aspect of game state.
/// 
/// The concept of the StatStick solves our first problem. A StatStick is any source
/// of stats. Entities keep track of which StatSticks they have equipped, but
/// StatSticks also keep track of which entities they are equipped to. This allows
/// entities to update their own stats, and allows StatSticks force updates on
/// entities they are equipped to when necessary (say, when an entity moves out of
/// range of a StatStick aura.)
/// 
/// The second problem is more complex and requires specific solutions depending on
/// the use case. Some stats should level up abilities; this requires specific
/// interaction with the ability systems. Some stats effect movement speed. Perhaps
/// the movement speed stat should write to a MovementSpeed component that actually
/// governs movement speed. It really depends on the case.

using Unity.Entities;
using Unity.NetCode;
using System;

[Serializable]
[GhostComponent]
public struct StatElement : IBufferElementData
{
    [GhostField] public Stat stat;
    [GhostField] public float value;

    public StatElement(Stat stat, float value) : this()
    {
        this.stat = stat;
        this.value = value;
    }
}

public struct EquippedElement : IBufferElementData
{
    public Entity entity;

    public EquippedElement(Entity entity) : this()
    {
        this.entity = entity;
    }
}

public struct EquippedToElement : IBufferElementData
{
    public Entity entity;

    public EquippedToElement(Entity entity) : this()
    {
        this.entity = entity;
    }
}


[Serializable]
public struct StatRangeElement : IBufferElementData
{
    public Stat stat;
    public Range range;

    public StatRangeElement(Stat stat, Range range) : this()
    {
        this.stat = stat;
        this.range = range;
    }

    public int Stat
    {
        get => (int)stat;
        set
        {
            stat = (Stat)value;
        }
    }

    public bool IsMet(float value)
    {
        return range.IsInRange(value);
    }
}

public struct Range
{
    public float min;
    public float max;

    public static Range FromMin(float min)
    {
        return new Range(min, float.MaxValue);
    }

    public static Range FromMax(float max)
    {
        return new Range(float.MinValue, max);
    }

    public Range(float min, float max)
    {
        if (min > max) UnityEngine.Debug.LogError("Min is greater than max!");

        this.min = min;
        this.max = max;
    }

    public bool IsInRange(float value)
    {
        return value >= min && value <= max;
    }
}

public readonly partial struct StatEntity : IAspect
{
    readonly Entity self;
    readonly DynamicBuffer<StatElement> stats;
    [Optional] readonly DynamicBuffer<EquippedElement> equipped;
    [Optional] readonly DynamicBuffer<EquippedToElement> equippedTo;
    [Optional] readonly DynamicBuffer<StatRangeElement> requirements;

    public bool TryEquipStatStick(StatEntity statStickEntity)
    {
        if (!equipped.IsCreated) return false;
        if (!statStickEntity.equippedTo.IsCreated) return false;
        if (!RequirementsMet(statStickEntity)) return false;

        equipped.Add(new EquippedElement(statStickEntity.self));
        statStickEntity.equippedTo.Add(new EquippedToElement(self));
        AddStats(statStickEntity);
        return true;
    }

    public bool TryEquipUniqueStatStick(StatEntity statStickEntity)
    {
        if (IsEquipped(statStickEntity)) return false;
        
        return TryEquipStatStick(statStickEntity);
    }

    public bool TryUnequipStatStick(StatEntity statStickEntity)
    {
        if (!equipped.IsCreated) return false;

        for (var i = 0; i < equipped.Length; i++)
        {
            var item = equipped[i].entity;
            if (item.Equals(statStickEntity.self))
            {
                equipped.RemoveAt(i);
                RemoveStats(statStickEntity);
                statStickEntity.UnequipFrom(self);
                return true;
            }
        }
        return false;
    }

    public bool IsEquipped(StatEntity statStickEntity)
    {
        if (!equipped.IsCreated) return false;

        for (var i = 0; i < equipped.Length; i++)
        {
            var item = equipped[i].entity;
            if (item.Equals(statStickEntity.self))
            {
                return true;
            }
        }
        return false;
    }

    private void AddStat(StatElement stat)
    {
        for (var i = 0; i < stats.Length; i++)
        {
            var currentStat = stats[i];
            if (currentStat.stat == stat.stat)
            {
                currentStat.value += stat.value;
                stats.RemoveAt(i);
                if (currentStat.value != 0)
                {
                    stats.Insert(i, currentStat);
                }
                return;
            }
        }
        stats.Add(stat);
    }

    private void AddStats(StatEntity statEntity)
    {
        for (var i = 0; i < statEntity.stats.Length; i++)
        {
            var stat = statEntity.stats[i];
            AddStat(stat);
        }
    }

    private void RemoveStats(StatEntity statEntity)
    {
        for (var i = 0; i < statEntity.stats.Length; i++)
        {
            var stat = statEntity.stats[i];
            stat.value *= -1;
            AddStat(stat);
        }
    }

    public bool RequirementsMet(StatEntity statStickEntity)
    {
        // If requirements does not exist, automatically pass
        if (!statStickEntity.requirements.IsCreated) return true;

        for (var i = 0; i < statStickEntity.requirements.Length; i++)
        {
            var requirement = statStickEntity.requirements[i];
            var value = GetStat(requirement.stat);
            if (!requirement.IsMet(value))
            {
                return false;
            }
        }
        return true;
    }

    public bool UnequipFrom(Entity target)
    {
        if (!equippedTo.IsCreated) return false;

        for (var i = 0; i < equippedTo.Length; i++)
        {
            var equippedToEntity = equippedTo[i].entity;
            if (target.Equals(equippedToEntity))
            {
                equippedTo.RemoveAtSwapBack(i);
                return true;
            }
        }
        return false;
    }

    public float GetStat(Stat stat)
    {
        for (var i = 0; i < stats.Length; i++)
        {
            var s = stats[i];
            if (s.stat == stat)
            {
                return s.value;
            }
        }
        return 0;
    }
}

public interface IStatDerived
{
    public void Update(StatEntity statEntity);
}

public struct Health : IComponentData, IStatDerived
{
    public float current;
    public float max;

    public Health(float max)
    {
        this.current = max;
        this.max = max;
    }

    public void Update(StatEntity statEntity)
    {
        var additional = statEntity.GetStat(Stat.AdditionalLife);
        var increased = statEntity.GetStat(Stat.IncreasedLife) + 100;
        var more = statEntity.GetStat(Stat.MoreLife) + 100;

        max = additional + (increased / 100) + (more / 100);
    }
}