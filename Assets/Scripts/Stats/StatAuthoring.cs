using Unity.Collections;
using Unity.Entities;
using System;
using Unity.Burst;

/// <summary>
/// Some stats fit into categories; For instance, IncreasedDamage, IncreasedFireDamage, and
/// IncreasedRifleDamage might all apply to an attack with incindiary rounds from a rifle. 
/// However, how do we calculate the damage for that rifle shot? It's simple: The rifle has
/// a number of tags; in this case "Rifle" and "Fire." We need to be able to feed a function
/// a set of stats and tags and have it spit out a set of generic stats. For instance, if we
/// feed it "Rifle" and "Fire," and we have a set of stats that looks like the following:
/// FireDamage: 80, Damage: 10, IceDamage: 20, RifleDamage: 30
/// ...then we would end up with Damage: 120, ignoring the IceDamage. We do this by building
/// dictionaries that match a each stat to a match tag and a grants tag. Match tags are things
/// like "Fire" or "Rifle" or "Ice." Grants tags are things like "Damage" or "Penetration" or
/// "Projectile." For the above damage calculation, the attack must match all the "matches" of
/// the stat. An attack that is "Rifle/Fire" can accept "Rifle" damage, "Fire" damage, and 
/// "Rifle/Fire" damage. It cannot accept "Rifle/Ice" damage, or "Pistol/Fire" damage or any
/// other variant. 
/// </summary>

[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
public partial class StatDefinitions : SystemBase
{
    public StatAuthoring[] StatAuthorings = new StatAuthoring[]
    {
        // Projectile bounces
        new StatAuthoring
        {
            stat = Stat.AdditionalProjectileBounceWithBallisticWeapons,
            flags = StatFlavorFlag.WeaponsBallistic,
            combinedStat = Stat.AdditionalProjectileBounce,
        },

        // Projectile penetration
        new StatAuthoring
        {
            stat = Stat.AdditionalProjectilePenetration,
            flags = StatFlavorFlag.General,
            combinedStat = Stat.IncreasedProjectileSpread,
        },

        // Projectile spread
        new StatAuthoring
        {
            stat = Stat.IncreasedProjectileSpread,
            flags = StatFlavorFlag.General,
            combinedStat = Stat.IncreasedProjectileSpread,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedProjectileSpreadWith1HWeapons,
            flags = StatFlavorFlag.Weapons1H,
            combinedStat = Stat.IncreasedProjectileSpread,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedProjectileSpreadWith2HWeapons,
            flags = StatFlavorFlag.Weapons2H,
            combinedStat = Stat.IncreasedProjectileSpread,
        },

        // Attack speed
        new StatAuthoring
        {
            stat = Stat.IncreasedAttackSpeed,
            flags = StatFlavorFlag.General,
            combinedStat = Stat.IncreasedAttackSpeed,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedAttackSpeedWith1HWeapons,
            flags = StatFlavorFlag.Weapons1H,
            combinedStat = Stat.IncreasedAttackSpeed,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedAttackSpeedWith2HWeapons,
            flags = StatFlavorFlag.Weapons2H,
            combinedStat = Stat.IncreasedAttackSpeed,
        },

        // Damage
        new StatAuthoring
        {
            stat = Stat.IncreasedDamage,
            flags = StatFlavorFlag.General,
            combinedStat = Stat.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = Stat.MoreDamage,
            flags = StatFlavorFlag.General,
            combinedStat = Stat.MoreDamage,
        },

        // Added elemental damage
        new StatAuthoring
        {
            stat = Stat.AdditionalPhysicalDamage,
            flags = StatFlavorFlag.Physical,
            combinedStat = Stat.AdditionalDamage,
        },
        new StatAuthoring
        {
            stat = Stat.AdditionalThermalDamage,
            flags = StatFlavorFlag.Thermal,
            combinedStat = Stat.AdditionalDamage,
        },
        new StatAuthoring
        {
            stat = Stat.AdditionalCryoDamage,
            flags = StatFlavorFlag.Cryo,
            combinedStat = Stat.AdditionalDamage,
        },
        new StatAuthoring
        {
            stat = Stat.AdditionalElectricityDamage,
            flags = StatFlavorFlag.Electricity,
            combinedStat = Stat.AdditionalDamage,
        },
        new StatAuthoring
        {
            stat = Stat.AdditionalChaosDamage,
            flags = StatFlavorFlag.Chaos,
            combinedStat = Stat.AdditionalDamage,
        },

        // Increased elemental damage
        new StatAuthoring
        {
            stat = Stat.IncreasedPhysicalDamage,
            flags = StatFlavorFlag.Physical,
            combinedStat = Stat.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedThermalDamage,
            flags = StatFlavorFlag.Thermal,
            combinedStat = Stat.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedCryoDamage,
            flags = StatFlavorFlag.Cryo,
            combinedStat = Stat.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedElectricityDamage,
            flags = StatFlavorFlag.Electricity,
            combinedStat = Stat.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedChaosDamage,
            flags = StatFlavorFlag.Chaos,
            combinedStat = Stat.IncreasedDamage,
        },

        // More elemental damage
        new StatAuthoring
        {
            stat = Stat.MorePhysicalDamage,
            flags = StatFlavorFlag.Physical,
            combinedStat = Stat.MoreDamage,
        },
        new StatAuthoring
        {
            stat = Stat.MoreThermalDamage,
            flags = StatFlavorFlag.Thermal,
            combinedStat = Stat.MoreDamage,
        },
        new StatAuthoring
        {
            stat = Stat.MoreCryoDamage,
            flags = StatFlavorFlag.Cryo,
            combinedStat = Stat.MoreDamage,
        },
        new StatAuthoring
        {
            stat = Stat.MoreElectricityDamage,
            flags = StatFlavorFlag.Electricity,
            combinedStat = Stat.MoreDamage,
        },
        new StatAuthoring
        {
            stat = Stat.MoreChaosDamage,
            flags = StatFlavorFlag.Chaos,
            combinedStat = Stat.MoreDamage,
        },

        // Increased damage over time
        new StatAuthoring
        {
            stat = Stat.IncreasedPhysicalDamageOverTime,
            flags = StatFlavorFlag.Physical | StatFlavorFlag.OverTime,
            combinedStat = Stat.IncreasedDamageOverTime,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedThermalDamageOverTime,
            flags = StatFlavorFlag.Thermal | StatFlavorFlag.OverTime,
            combinedStat = Stat.IncreasedDamageOverTime,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedCryoDamageOverTime,
            flags = StatFlavorFlag.Cryo | StatFlavorFlag.OverTime,
            combinedStat = Stat.IncreasedDamageOverTime,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedElectricityDamageOverTime,
            flags = StatFlavorFlag.Electricity | StatFlavorFlag.OverTime,
            combinedStat = Stat.IncreasedDamageOverTime,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedChaosDamageOverTime,
            flags = StatFlavorFlag.Chaos | StatFlavorFlag.OverTime,
            combinedStat = Stat.IncreasedDamageOverTime,
        },

        // More damage over time
        new StatAuthoring
        {
            stat = Stat.MorePhysicalDamageOverTime,
            flags = StatFlavorFlag.Physical | StatFlavorFlag.OverTime,
            combinedStat = Stat.MoreDamageOverTime,
        },
        new StatAuthoring
        {
            stat = Stat.MoreThermalDamageOverTime,
            flags = StatFlavorFlag.Thermal | StatFlavorFlag.OverTime,
            combinedStat = Stat.MoreDamageOverTime,
        },
        new StatAuthoring
        {
            stat = Stat.MoreCryoDamageOverTime,
            flags = StatFlavorFlag.Cryo | StatFlavorFlag.OverTime,
            combinedStat = Stat.MoreDamageOverTime,
        },
        new StatAuthoring
        {
            stat = Stat.MoreElectricityDamageOverTime,
            flags = StatFlavorFlag.Electricity | StatFlavorFlag.OverTime,
            combinedStat = Stat.MoreDamageOverTime,
        },
        new StatAuthoring
        {
            stat = Stat.MoreChaosDamageOverTime,
            flags = StatFlavorFlag.Chaos | StatFlavorFlag.OverTime,
            combinedStat = Stat.MoreDamageOverTime,
        },

        // Damage by 1h/2h
        new StatAuthoring
        {
            stat = Stat.Increased1HWeaponDamage,
            flags = StatFlavorFlag.Weapons1H,
            combinedStat = Stat.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = Stat.Increased2HWeaponDamage,
            flags = StatFlavorFlag.Weapons2H,
            combinedStat = Stat.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = Stat.More1HWeaponDamage,
            flags = StatFlavorFlag.Weapons1H,
            combinedStat = Stat.MoreDamage,
        },
        new StatAuthoring
        {
            stat = Stat.More2HWeaponDamage,
            flags = StatFlavorFlag.Weapons2H,
            combinedStat = Stat.MoreDamage,
        },

        // Damage by weapon type
        new StatAuthoring
        {
            stat = Stat.IncreasedEnergyWeaponDamage,
            flags = StatFlavorFlag.WeaponsEnergy,
            combinedStat = Stat.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedBallisticWeaponDamage,
            flags = StatFlavorFlag.WeaponsBallistic,
            combinedStat = Stat.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedAutomaticDamage,
            flags = StatFlavorFlag.WeaponsAutomatic,
            combinedStat = Stat.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedShotgunDamage,
            flags = StatFlavorFlag.WeaponsShotgun,
            combinedStat = Stat.MoreDamage,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedRifleDamage,
            flags = StatFlavorFlag.WeaponsRifle,
            combinedStat = Stat.MoreDamage,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedBeamDamage,
            flags = StatFlavorFlag.WeaponsBeam,
            combinedStat = Stat.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = Stat.MoreEnergyWeaponDamage,
            flags = StatFlavorFlag.WeaponsEnergy,
            combinedStat = Stat.MoreDamage,
        },
        new StatAuthoring
        {
            stat = Stat.MoreBallisticWeaponDamage,
            flags = StatFlavorFlag.WeaponsBallistic,
            combinedStat = Stat.MoreDamage,
        },
        new StatAuthoring
        {
            stat = Stat.MoreAutomaticDamage,
            flags = StatFlavorFlag.WeaponsAutomatic,
            combinedStat = Stat.MoreDamage,
        },
        new StatAuthoring
        {
            stat = Stat.MoreShotgunDamage,
            flags = StatFlavorFlag.WeaponsShotgun,
            combinedStat = Stat.MoreDamage,
        },
        new StatAuthoring
        {
            stat = Stat.MoreRifleDamage,
            flags = StatFlavorFlag.WeaponsRifle,
            combinedStat = Stat.MoreDamage,
        },
        new StatAuthoring
        {
            stat = Stat.MoreBeamDamage,
            flags = StatFlavorFlag.WeaponsBeam,
            combinedStat = Stat.MoreDamage,
        },

        // Critical strikes
        new StatAuthoring
        {
            stat = Stat.BaseCriticalStrikeChance1TenthPercent,
            flags = StatFlavorFlag.General,
            combinedStat = Stat.BaseCriticalStrikeChance1TenthPercent,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedCriticalStrikeChance,
            flags = StatFlavorFlag.General,
            combinedStat = Stat.IncreasedCriticalStrikeChance,
        },
        new StatAuthoring
        {
            stat = Stat.MoreCriticalStrikeChance,
            flags = StatFlavorFlag.General,
            combinedStat = Stat.MoreCriticalStrikeChance,
        },
        new StatAuthoring
        {
            stat = Stat.IncreasedCriticalStrikeDamageMultiplier,
            flags = StatFlavorFlag.General,
            combinedStat = Stat.IncreasedCriticalStrikeDamageMultiplier,
        },
        new StatAuthoring
        {
            stat = Stat.MoreCriticalStrikeDamageMultiplier,
            flags = StatFlavorFlag.General,
            combinedStat = Stat.MoreCriticalStrikeDamageMultiplier,
        },

        new StatAuthoring
        {
            stat = Stat.IncreasedCriticalStrikeChanceWith2HWeapons,
            flags = StatFlavorFlag.Weapons2H,
            combinedStat = Stat.IncreasedCriticalStrikeChance,
        },
    };

    protected override void OnCreate()
    {
        var singletonEntity = World.EntityManager.CreateEntity();
        World.EntityManager.AddComponentData(singletonEntity, new Singleton(StatAuthorings));
    }

    protected override void OnDestroy()
    {
        SystemAPI.GetSingleton<Singleton>().OnDestroy();
    }

    protected override void OnUpdate() { }

    [BurstCompile]
    public struct Singleton : IComponentData
    {
        public NativeHashMap<uint, StatFlavorFlag> StatToStatFlavorFlags;
        public NativeHashMap<uint, Stat> StatToStatType;

        public Singleton(StatAuthoring[] StatAuthorings)
        {
            StatToStatFlavorFlags = new NativeHashMap<uint, StatFlavorFlag>(StatAuthorings.Length, Allocator.Persistent);
            StatToStatType = new NativeHashMap<uint, Stat>(StatAuthorings.Length, Allocator.Persistent);

            for (var i = 0; i < StatAuthorings.Length; i++)
            {
                var statAuthoring = StatAuthorings[i];

                StatToStatFlavorFlags.Add((uint)statAuthoring.stat, statAuthoring.flags);
                StatToStatType.Add((uint)statAuthoring.stat, statAuthoring.combinedStat);
            }
        }

        [BurstCompile]
        public void OnDestroy()
        {
            StatToStatFlavorFlags.Dispose();
            StatToStatType.Dispose();
        }

        /// <summary>
        /// Takes a set of stats and stat flavors, and returns a set of generic stats that match the value.
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="inputFlavorFlags"></param>
        /// <param name="results"></param>
        [BurstCompile]
        public void TotalStatsWithFlavor(in DynamicBuffer<StatElement> stats, StatFlavorFlag inputFlavorFlags, ref NativeHashMap<uint, float> results)
        {
            var statsEnumerator = stats.GetEnumerator();

            while (statsEnumerator.MoveNext())
            {
                var stat = (uint)statsEnumerator.Current.stat;
                var value = statsEnumerator.Current.value;

                if (!StatToStatFlavorFlags.TryGetValue(stat, out var statFlavorFlags))
                {
                    continue;
                }

                // If every '1' flag in the stats flavor flags is a 1 in the input flavor flags, then proceed. 
                if ((statFlavorFlags & inputFlavorFlags) != statFlavorFlags)
                {
                    continue;
                }

                if (!StatToStatType.TryGetValue(stat, out var grants))
                {
                    continue;
                }

                // Combine the stat into the hashmap.
                results.Add((uint)grants, value);
            }
        }
    }
}

public struct StatAuthoring
{
    public Stat stat;
    public StatFlavorFlag flags;
    public Stat combinedStat;
}

[Flags]
public enum StatFlavorFlag
{
    Uninitialized = 0,

    General = 1 << 0,

    WeaponsBallistic = 1 << 1,
    WeaponsEnergy = 1 << 2,
    WeaponsAutomatic = 1 << 3,
    WeaponsRifle = 1 << 4,
    WeaponsShotgun = 1 << 5,
    WeaponsBeam = 1 << 6,

    Weapons1H = 1 << 7,
    Weapons2H = 1 << 8,

    Physical = 1 << 9,
    Thermal = 1 << 10,
    Cryo = 1 << 11,
    Electricity = 1 << 12,
    Chaos = 1 << 13,

    OverTime = 1 << 14,
}

public enum Stat
{
    None,

    // Meta stats. Must be at the top since they can grant anything. 
    Level,
    TalentPoint,

    // Talents. Talents must be early since 
    TalentPhysique,
    TalentReason,
    TalentDexterity,
    TalentPerception,
    TalentMelee,
    TalentRanged,
    TalentEngineering,
    TalentMysticism,
    TalentMedicine,
    TalentDefense,

    TalentTechnique,

    // Attributes
    AdditionalStrength,
    AdditionalDexterity,
    AdditionalIntelligence,
    IncreasedStrength,
    IncreasedDexterity,
    IncreasedIntelligence,
    MoreStrength,
    MoreDexterity,
    MoreIntelligence,

    IncreasedAttributes,
    MoreAttributes,

    // Resources
    AdditionalLife,
    AdditionalShield,
    AdditionalEnergy,
    IncreasedLife,
    IncreasedShield,
    IncreasedEnergy,
    MoreLife,
    MoreShieldMax,
    MoreEnergyMax,
    BulwarkChargeMax,
    RampageChargeMax,
    PowerChargeMax,

    // Resource modification
    FlatLifeRegenerationPerSecond,
    FlatShieldRegenerationPerSecond,
    FlatEnergyRegenerationPerSecond,
    PercentLifeRegenerationPerSecond,
    PercentShieldRegenerationPerSecond,
    PercentEnergyRegenerationPerSecond,

    // Defenses
    PhysicalResistance,
    ThermalResistance,
    CryoResistance,
    ElectricityResistance,
    ChaosResistance,

    AdditionalDamageTaken,
    IncreasedDamageTaken,

    IncreasedChanceToBeStunned,
    CannotBeStunned,

    // Movement
    IncreasedMovementSpeed,
    MoreMovementSpeed,
    AdditionalDashCharge,
    SecondsInvulnerabilityAfterDash100ths,
    IncreasedDashDistance,

    // Modification
    AdditionalAmmoCapacity,
    AdditionalProjectile,
    IncreasedProjectileSpreadWhileMoving,
    ProjectileSpreadMultiplierWhileMoving,
    IncreasedProjectileSpeed,
    IncreasedProjectileRange,
    IncreasedWeaponChargeRate,
    IncreasedWeaponHeatReleaseRate,
    IncreasedWeaponDrawSpeed,
    IncreasedAreaOfEffect,
    MoreAreaOfEffect,
    IncreasedCharacterSize,

    // Bounce
    AdditionalProjectileBounce,
    AdditionalProjectileBounceWithBallisticWeapons,

    // Pierce
    AdditionalProjectilePenetration,

    // Projectile spread
    IncreasedProjectileSpread,
    IncreasedProjectileSpreadWith1HWeapons,
    IncreasedProjectileSpreadWith2HWeapons,

    NoMovementProjectileSpreadPenalty,

    IncreasedAttackSpeed,
    IncreasedAttackSpeedWith1HWeapons,
    IncreasedAttackSpeedWith2HWeapons,

    // Damage
    AdditionalDamage,
    IncreasedDamage,
    MoreDamage,

    // Elemental types
    AdditionalPhysicalDamage,
    AdditionalThermalDamage,
    AdditionalCryoDamage,
    AdditionalElectricityDamage,
    AdditionalChaosDamage,
    IncreasedPhysicalDamage,
    IncreasedThermalDamage,
    IncreasedCryoDamage,
    IncreasedElectricityDamage,
    IncreasedChaosDamage,
    MorePhysicalDamage,
    MoreThermalDamage,
    MoreCryoDamage,
    MoreElectricityDamage,
    MoreChaosDamage,

    // Over time
    IncreasedDamageOverTime,
    MoreDamageOverTime,

    IncreasedPhysicalDamageOverTime,
    IncreasedThermalDamageOverTime,
    IncreasedCryoDamageOverTime,
    IncreasedElectricityDamageOverTime,
    IncreasedChaosDamageOverTime,
    MorePhysicalDamageOverTime,
    MoreThermalDamageOverTime,
    MoreCryoDamageOverTime,
    MoreElectricityDamageOverTime,
    MoreChaosDamageOverTime,

    IncreasedAreaDamage,
    MoreAreaDamage,

    // Weapon types
    Increased1HWeaponDamage,
    Increased2HWeaponDamage,
    More1HWeaponDamage,
    More2HWeaponDamage,

    IncreasedEnergyWeaponDamage,
    IncreasedBallisticWeaponDamage,
    IncreasedShotgunDamage,
    IncreasedRifleDamage,
    IncreasedAutomaticDamage,
    IncreasedBeamDamage,
    MoreEnergyWeaponDamage,
    MoreBallisticWeaponDamage,
    MoreShotgunDamage,
    MoreRifleDamage,
    MoreAutomaticDamage,
    MoreBeamDamage,

    // Conditional
    MoreDamageWithFirstRound,
    MoreDamageWithLastRound,

    // Critical
    BaseCriticalStrikeChance1TenthPercent,
    IncreasedCriticalStrikeChance,
    MoreCriticalStrikeChance,
    IncreasedCriticalStrikeDamageMultiplier,
    MoreCriticalStrikeDamageMultiplier,

    IncreasedCriticalStrikeChanceWith2HWeapons,

    // Abilities
    GrantsAbilityBasicShot,
    GrantsAbilityMeltdown,
}
