using Unity.Collections;
using Unity.Entities;
using System.Linq;
using System;

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
public static class StatDefinitions
{
    public static StatAuthoring[] StatAuthorings = new StatAuthoring[]
    {
        // Projectile bounces
        new StatAuthoring
        {
            stat = StatType.AdditionalBounceWithBallisticProjectile,
            matches = StatFlavorFlag.WeaponsBallistic,
            grants = CombinedStatCategory.AdditionalProjectileBounce,
        },

        // Projectile penetration
        new StatAuthoring
        {
            stat = StatType.AdditionalProjectilePenetration,
            matches = StatFlavorFlag.General,
            grants = CombinedStatCategory.IncreasedProjectileSpread,
        },

        // Projectile spread
        new StatAuthoring
        {
            stat = StatType.IncreasedProjectileSpread,
            matches = StatFlavorFlag.General,
            grants = CombinedStatCategory.IncreasedProjectileSpread,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedProjectileSpreadWith1HWeapons,
            matches = StatFlavorFlag.Weapons1H,
            grants = CombinedStatCategory.IncreasedProjectileSpread,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedProjectileSpreadWith2HWeapons,
            matches = StatFlavorFlag.Weapons2H,
            grants = CombinedStatCategory.IncreasedProjectileSpread,
        },

        // Attack speed
        new StatAuthoring
        {
            stat = StatType.IncreasedAttackSpeed,
            matches = StatFlavorFlag.General,
            grants = CombinedStatCategory.IncreasedAttackSpeed,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedAttackSpeedWith1HWeapons,
            matches = StatFlavorFlag.Weapons1H,
            grants = CombinedStatCategory.IncreasedAttackSpeed,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedAttackSpeedWith2HWeapons,
            matches = StatFlavorFlag.Weapons2H,
            grants = CombinedStatCategory.IncreasedAttackSpeed,
        },

        // Damage
        new StatAuthoring
        {
            stat = StatType.IncreasedDamage,
            matches = StatFlavorFlag.General,
            grants = CombinedStatCategory.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreDamage,
            matches = StatFlavorFlag.General,
            grants = CombinedStatCategory.MoreDamage,
        },

        // Added elemental damage
        new StatAuthoring
        {
            stat = StatType.AdditionalPhysicalDamage,
            matches = StatFlavorFlag.Physical,
            grants = CombinedStatCategory.AdditionalDamage,
        },
        new StatAuthoring
        {
            stat = StatType.AdditionalThermalDamage,
            matches = StatFlavorFlag.Thermal,
            grants = CombinedStatCategory.AdditionalDamage,
        },
        new StatAuthoring
        {
            stat = StatType.AdditionalCryoDamage,
            matches = StatFlavorFlag.Cryo,
            grants = CombinedStatCategory.AdditionalDamage,
        },
        new StatAuthoring
        {
            stat = StatType.AdditionalElectricityDamage,
            matches = StatFlavorFlag.Electricity,
            grants = CombinedStatCategory.AdditionalDamage,
        },
        new StatAuthoring
        {
            stat = StatType.AdditionalChaosDamage,
            matches = StatFlavorFlag.Chaos,
            grants = CombinedStatCategory.AdditionalDamage,
        },

        // Increased elemental damage
        new StatAuthoring
        {
            stat = StatType.IncreasedPhysicalDamage,
            matches = StatFlavorFlag.Physical,
            grants = CombinedStatCategory.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedThermalDamage,
            matches = StatFlavorFlag.Thermal,
            grants = CombinedStatCategory.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedCryoDamage,
            matches = StatFlavorFlag.Cryo,
            grants = CombinedStatCategory.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedElectricityDamage,
            matches = StatFlavorFlag.Electricity,
            grants = CombinedStatCategory.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedChaosDamage,
            matches = StatFlavorFlag.Chaos,
            grants = CombinedStatCategory.IncreasedDamage,
        },

        // More elemental damage
        new StatAuthoring
        {
            stat = StatType.MorePhysicalDamage,
            matches = StatFlavorFlag.Physical,
            grants = CombinedStatCategory.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreThermalDamage,
            matches = StatFlavorFlag.Thermal,
            grants = CombinedStatCategory.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreCryoDamage,
            matches = StatFlavorFlag.Cryo,
            grants = CombinedStatCategory.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreElectricityDamage,
            matches = StatFlavorFlag.Electricity,
            grants = CombinedStatCategory.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreChaosDamage,
            matches = StatFlavorFlag.Chaos,
            grants = CombinedStatCategory.MoreDamage,
        },

        // Increased damage over time
        new StatAuthoring
        {
            stat = StatType.IncreasedPhysicalDamageOverTime,
            matches = StatFlavorFlag.Physical | StatFlavorFlag.OverTime,
            grants = CombinedStatCategory.IncreasedDamageOverTime,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedThermalDamageOverTime,
            matches = StatFlavorFlag.Thermal | StatFlavorFlag.OverTime,
            grants = CombinedStatCategory.IncreasedDamageOverTime,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedCryoDamageOverTime,
            matches = StatFlavorFlag.Cryo | StatFlavorFlag.OverTime,
            grants = CombinedStatCategory.IncreasedDamageOverTime,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedElectricityDamageOverTime,
            matches = StatFlavorFlag.Electricity | StatFlavorFlag.OverTime,
            grants = CombinedStatCategory.IncreasedDamageOverTime,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedChaosDamageOverTime,
            matches = StatFlavorFlag.Chaos | StatFlavorFlag.OverTime,
            grants = CombinedStatCategory.IncreasedDamageOverTime,
        },

        // More damage over time
        new StatAuthoring
        {
            stat = StatType.MorePhysicalDamageOverTime,
            matches = StatFlavorFlag.Physical | StatFlavorFlag.OverTime,
            grants = CombinedStatCategory.MoreDamageOverTime,
        },
        new StatAuthoring
        {
            stat = StatType.MoreThermalDamageOverTime,
            matches = StatFlavorFlag.Thermal | StatFlavorFlag.OverTime,
            grants = CombinedStatCategory.MoreDamageOverTime,
        },
        new StatAuthoring
        {
            stat = StatType.MoreCryoDamageOverTime,
            matches = StatFlavorFlag.Cryo | StatFlavorFlag.OverTime,
            grants = CombinedStatCategory.MoreDamageOverTime,
        },
        new StatAuthoring
        {
            stat = StatType.MoreElectricityDamageOverTime,
            matches = StatFlavorFlag.Electricity | StatFlavorFlag.OverTime,
            grants = CombinedStatCategory.MoreDamageOverTime,
        },
        new StatAuthoring
        {
            stat = StatType.MoreChaosDamageOverTime,
            matches = StatFlavorFlag.Chaos | StatFlavorFlag.OverTime,
            grants = CombinedStatCategory.MoreDamageOverTime,
        },

        // Damage by 1h/2h
        new StatAuthoring
        {
            stat = StatType.Increased1HWeaponDamage,
            matches = StatFlavorFlag.Weapons1H,
            grants = CombinedStatCategory.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.Increased2HWeaponDamage,
            matches = StatFlavorFlag.Weapons2H,
            grants = CombinedStatCategory.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.More1HWeaponDamage,
            matches = StatFlavorFlag.Weapons1H,
            grants = CombinedStatCategory.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.More2HWeaponDamage,
            matches = StatFlavorFlag.Weapons2H,
            grants = CombinedStatCategory.MoreDamage,
        },

        // Damage by weapon type
        new StatAuthoring
        {
            stat = StatType.IncreasedEnergyWeaponDamage,
            matches = StatFlavorFlag.WeaponsEnergy,
            grants = CombinedStatCategory.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedBallisticWeaponDamage,
            matches = StatFlavorFlag.WeaponsBallistic,
            grants = CombinedStatCategory.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedAutomaticDamage,
            matches = StatFlavorFlag.WeaponsAutomatic,
            grants = CombinedStatCategory.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedShotgunDamage,
            matches = StatFlavorFlag.WeaponsShotgun,
            grants = CombinedStatCategory.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedRifleDamage,
            matches = StatFlavorFlag.WeaponsRifle,
            grants = CombinedStatCategory.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedBeamDamage,
            matches = StatFlavorFlag.WeaponsBeam,
            grants = CombinedStatCategory.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreEnergyWeaponDamage,
            matches = StatFlavorFlag.WeaponsEnergy,
            grants = CombinedStatCategory.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreBallisticWeaponDamage,
            matches = StatFlavorFlag.WeaponsBallistic,
            grants = CombinedStatCategory.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreAutomaticDamage,
            matches = StatFlavorFlag.WeaponsAutomatic,
            grants = CombinedStatCategory.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreShotgunDamage,
            matches = StatFlavorFlag.WeaponsShotgun,
            grants = CombinedStatCategory.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreRifleDamage,
            matches = StatFlavorFlag.WeaponsRifle,
            grants = CombinedStatCategory.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreBeamDamage,
            matches = StatFlavorFlag.WeaponsBeam,
            grants = CombinedStatCategory.MoreDamage,
        },

        // Critical strikes
        new StatAuthoring
        {
            stat = StatType.BaseCriticalStrikeChance1TenthPercent,
            matches = StatFlavorFlag.General,
            grants = CombinedStatCategory.BaseCriticalStrikeChance1TenthPercent,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedCriticalStrikeChance,
            matches = StatFlavorFlag.General,
            grants = CombinedStatCategory.IncreasedCriticalStrikeChance,
        },
        new StatAuthoring
        {
            stat = StatType.MoreCriticalStrikeChance,
            matches = StatFlavorFlag.General,
            grants = CombinedStatCategory.MoreCriticalStrikeChance,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedCriticalStrikeDamageMultiplier,
            matches = StatFlavorFlag.General,
            grants = CombinedStatCategory.IncreasedCriticalStrikeDamageMultiplier,
        },
        new StatAuthoring
        {
            stat = StatType.MoreCriticalStrikeDamageMultiplier,
            matches = StatFlavorFlag.General,
            grants = CombinedStatCategory.MoreCriticalStrikeDamageMultiplier,
        },

        new StatAuthoring
        {
            stat = StatType.IncreasedCriticalStrikeChanceWith2HWeapons,
            matches = StatFlavorFlag.Weapons2H,
            grants = CombinedStatCategory.IncreasedCriticalStrikeChance,
        },
    };

    public static NativeHashMap<int, StatFlavorFlag> StatToStatFlavorFlags;
    public static NativeHashMap<int, CombinedStatCategory> StatToCombinedStatCategory;

    public static void Initialize()
    {
        StatToStatFlavorFlags = new NativeHashMap<int, StatFlavorFlag>(100, Allocator.Persistent);
        StatToCombinedStatCategory = new NativeHashMap<int, CombinedStatCategory>(100, Allocator.Persistent);

        for (var i = 0; i < StatAuthorings.Length; i++)
        {
            var statAuthoring = StatAuthorings[i];

            StatToStatFlavorFlags.Add((int)statAuthoring.stat, statAuthoring.matches);
            StatToCombinedStatCategory.Add((int)statAuthoring.stat, statAuthoring.grants);
        }
    }

    public static void OnDestroy()
    {
        StatToStatFlavorFlags.Dispose();
        StatToCombinedStatCategory.Dispose();
    }

    public static void TotalStatsWithFlavor(in DynamicBuffer<StatContainer> stats, StatFlavorFlag inputFlavorFlags, ref NativeHashMap<int, int> results)
    {
        /// Iterate over stat keys, 
        /// iterate over matches.
        /// Get the matching tags from the StatMatchesTags dictionary.
        /// If every tag from the StatMatchesTags dictionary is in the matches array, add the value to the results dictionary
        /// based on the StatGrantsTags dictionary.
        for (var i = 0; i < stats.Length; i++)
        {
            var stat = (int)stats[i].stat.stat;

            /// Iterate over the stats StatMatchesTags
            /// if the input StatFlavorFlag does not contain one of that stats StatFlavorFlag
            /// i.e., if the stat applies to Fire attacks and the input StatFlavorFlag are for Ice attacks,
            /// we will skip this stat. Otherwise we will add it to the results.
            if (!StatToStatFlavorFlags.TryGetValue(stat, out var statFlavorFlags))
            {
                continue;
            }

            /// If every '1' flag in the stats flavor flags is present in the input flavor flags, then proceed. 
            if ((statFlavorFlags & inputFlavorFlags) != statFlavorFlags)
            {
                continue;
            }

            if (!StatToCombinedStatCategory.TryGetValue(stat, out var grants))
            {
                continue;
            }

            results.Add((int)grants, stats[i].stat.value);
        }
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class StatSetupSystem : SystemBase
{
    protected override void OnCreate()
    {
        StatDefinitions.Initialize();
    }

    protected override void OnDestroy()
    {
        StatDefinitions.OnDestroy();
    }

    protected override void OnUpdate() { }
}

public struct StatAuthoring
{
    public StatType stat;
    public StatFlavorFlag matches;
    public CombinedStatCategory grants;
}

[Flags] 
public enum StatFlavorFlag
{
    Uninitialized       = 0,

    General             = 1 << 0,

    WeaponsBallistic    = 1 << 1,
    WeaponsEnergy       = 1 << 2,
    WeaponsAutomatic    = 1 << 3,
    WeaponsRifle        = 1 << 4,
    WeaponsShotgun      = 1 << 5,
    WeaponsBeam         = 1 << 6,

    Weapons1H           = 1 << 7,
    Weapons2H           = 1 << 8,

    Physical            = 1 << 9,
    Thermal             = 1 << 10,
    Cryo                = 1 << 11,
    Electricity         = 1 << 12,
    Chaos               = 1 << 13,

    OverTime            = 1 << 14,
}

public enum CombinedStatCategory
{
    Uninitialized,

    AdditionalDamage,
    IncreasedDamage,
    MoreDamage,

    IncreasedDamageOverTime,
    MoreDamageOverTime,

    AdditionalProjectile,

    AdditionalProjectileBounce,

    IncreasedProjectileSpread,
    IncreasedAttackSpeed,

    BaseCriticalStrikeChance1TenthPercent,
    IncreasedCriticalStrikeChance,
    MoreCriticalStrikeChance,
    IncreasedCriticalStrikeDamageMultiplier,
    MoreCriticalStrikeDamageMultiplier,
}

public enum StatType
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
    MoreLifeMax,
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
    AdditionalBounceWithBallisticProjectile,

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
