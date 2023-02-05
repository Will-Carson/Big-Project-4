using Unity.Collections;
using Unity.Entities;
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

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class StatDefinitions : SystemBase
{
    public StatAuthoring[] StatAuthorings = new StatAuthoring[]
    {
        // Projectile bounces
        new StatAuthoring
        {
            stat = StatType.AdditionalProjectileBounceWithBallisticWeapons,
            flags = StatFlavorFlag.WeaponsBallistic,
            combinedStat = StatType.AdditionalProjectileBounce,
        },

        // Projectile penetration
        new StatAuthoring
        {
            stat = StatType.AdditionalProjectilePenetration,
            flags = StatFlavorFlag.General,
            combinedStat = StatType.IncreasedProjectileSpread,
        },

        // Projectile spread
        new StatAuthoring
        {
            stat = StatType.IncreasedProjectileSpread,
            flags = StatFlavorFlag.General,
            combinedStat = StatType.IncreasedProjectileSpread,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedProjectileSpreadWith1HWeapons,
            flags = StatFlavorFlag.Weapons1H,
            combinedStat = StatType.IncreasedProjectileSpread,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedProjectileSpreadWith2HWeapons,
            flags = StatFlavorFlag.Weapons2H,
            combinedStat = StatType.IncreasedProjectileSpread,
        },

        // Attack speed
        new StatAuthoring
        {
            stat = StatType.IncreasedAttackSpeed,
            flags = StatFlavorFlag.General,
            combinedStat = StatType.IncreasedAttackSpeed,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedAttackSpeedWith1HWeapons,
            flags = StatFlavorFlag.Weapons1H,
            combinedStat = StatType.IncreasedAttackSpeed,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedAttackSpeedWith2HWeapons,
            flags = StatFlavorFlag.Weapons2H,
            combinedStat = StatType.IncreasedAttackSpeed,
        },

        // Damage
        new StatAuthoring
        {
            stat = StatType.IncreasedDamage,
            flags = StatFlavorFlag.General,
            combinedStat = StatType.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreDamage,
            flags = StatFlavorFlag.General,
            combinedStat = StatType.MoreDamage,
        },

        // Added elemental damage
        new StatAuthoring
        {
            stat = StatType.AdditionalPhysicalDamage,
            flags = StatFlavorFlag.Physical,
            combinedStat = StatType.AdditionalDamage,
        },
        new StatAuthoring
        {
            stat = StatType.AdditionalThermalDamage,
            flags = StatFlavorFlag.Thermal,
            combinedStat = StatType.AdditionalDamage,
        },
        new StatAuthoring
        {
            stat = StatType.AdditionalCryoDamage,
            flags = StatFlavorFlag.Cryo,
            combinedStat = StatType.AdditionalDamage,
        },
        new StatAuthoring
        {
            stat = StatType.AdditionalElectricityDamage,
            flags = StatFlavorFlag.Electricity,
            combinedStat = StatType.AdditionalDamage,
        },
        new StatAuthoring
        {
            stat = StatType.AdditionalChaosDamage,
            flags = StatFlavorFlag.Chaos,
            combinedStat = StatType.AdditionalDamage,
        },

        // Increased elemental damage
        new StatAuthoring
        {
            stat = StatType.IncreasedPhysicalDamage,
            flags = StatFlavorFlag.Physical,
            combinedStat = StatType.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedThermalDamage,
            flags = StatFlavorFlag.Thermal,
            combinedStat = StatType.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedCryoDamage,
            flags = StatFlavorFlag.Cryo,
            combinedStat = StatType.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedElectricityDamage,
            flags = StatFlavorFlag.Electricity,
            combinedStat = StatType.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedChaosDamage,
            flags = StatFlavorFlag.Chaos,
            combinedStat = StatType.IncreasedDamage,
        },

        // More elemental damage
        new StatAuthoring
        {
            stat = StatType.MorePhysicalDamage,
            flags = StatFlavorFlag.Physical,
            combinedStat = StatType.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreThermalDamage,
            flags = StatFlavorFlag.Thermal,
            combinedStat = StatType.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreCryoDamage,
            flags = StatFlavorFlag.Cryo,
            combinedStat = StatType.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreElectricityDamage,
            flags = StatFlavorFlag.Electricity,
            combinedStat = StatType.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreChaosDamage,
            flags = StatFlavorFlag.Chaos,
            combinedStat = StatType.MoreDamage,
        },

        // Increased damage over time
        new StatAuthoring
        {
            stat = StatType.IncreasedPhysicalDamageOverTime,
            flags = StatFlavorFlag.Physical | StatFlavorFlag.OverTime,
            combinedStat = StatType.IncreasedDamageOverTime,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedThermalDamageOverTime,
            flags = StatFlavorFlag.Thermal | StatFlavorFlag.OverTime,
            combinedStat = StatType.IncreasedDamageOverTime,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedCryoDamageOverTime,
            flags = StatFlavorFlag.Cryo | StatFlavorFlag.OverTime,
            combinedStat = StatType.IncreasedDamageOverTime,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedElectricityDamageOverTime,
            flags = StatFlavorFlag.Electricity | StatFlavorFlag.OverTime,
            combinedStat = StatType.IncreasedDamageOverTime,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedChaosDamageOverTime,
            flags = StatFlavorFlag.Chaos | StatFlavorFlag.OverTime,
            combinedStat = StatType.IncreasedDamageOverTime,
        },

        // More damage over time
        new StatAuthoring
        {
            stat = StatType.MorePhysicalDamageOverTime,
            flags = StatFlavorFlag.Physical | StatFlavorFlag.OverTime,
            combinedStat = StatType.MoreDamageOverTime,
        },
        new StatAuthoring
        {
            stat = StatType.MoreThermalDamageOverTime,
            flags = StatFlavorFlag.Thermal | StatFlavorFlag.OverTime,
            combinedStat = StatType.MoreDamageOverTime,
        },
        new StatAuthoring
        {
            stat = StatType.MoreCryoDamageOverTime,
            flags = StatFlavorFlag.Cryo | StatFlavorFlag.OverTime,
            combinedStat = StatType.MoreDamageOverTime,
        },
        new StatAuthoring
        {
            stat = StatType.MoreElectricityDamageOverTime,
            flags = StatFlavorFlag.Electricity | StatFlavorFlag.OverTime,
            combinedStat = StatType.MoreDamageOverTime,
        },
        new StatAuthoring
        {
            stat = StatType.MoreChaosDamageOverTime,
            flags = StatFlavorFlag.Chaos | StatFlavorFlag.OverTime,
            combinedStat = StatType.MoreDamageOverTime,
        },

        // Damage by 1h/2h
        new StatAuthoring
        {
            stat = StatType.Increased1HWeaponDamage,
            flags = StatFlavorFlag.Weapons1H,
            combinedStat = StatType.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.Increased2HWeaponDamage,
            flags = StatFlavorFlag.Weapons2H,
            combinedStat = StatType.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.More1HWeaponDamage,
            flags = StatFlavorFlag.Weapons1H,
            combinedStat = StatType.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.More2HWeaponDamage,
            flags = StatFlavorFlag.Weapons2H,
            combinedStat = StatType.MoreDamage,
        },

        // Damage by weapon type
        new StatAuthoring
        {
            stat = StatType.IncreasedEnergyWeaponDamage,
            flags = StatFlavorFlag.WeaponsEnergy,
            combinedStat = StatType.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedBallisticWeaponDamage,
            flags = StatFlavorFlag.WeaponsBallistic,
            combinedStat = StatType.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedAutomaticDamage,
            flags = StatFlavorFlag.WeaponsAutomatic,
            combinedStat = StatType.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedShotgunDamage,
            flags = StatFlavorFlag.WeaponsShotgun,
            combinedStat = StatType.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedRifleDamage,
            flags = StatFlavorFlag.WeaponsRifle,
            combinedStat = StatType.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedBeamDamage,
            flags = StatFlavorFlag.WeaponsBeam,
            combinedStat = StatType.IncreasedDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreEnergyWeaponDamage,
            flags = StatFlavorFlag.WeaponsEnergy,
            combinedStat = StatType.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreBallisticWeaponDamage,
            flags = StatFlavorFlag.WeaponsBallistic,
            combinedStat = StatType.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreAutomaticDamage,
            flags = StatFlavorFlag.WeaponsAutomatic,
            combinedStat = StatType.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreShotgunDamage,
            flags = StatFlavorFlag.WeaponsShotgun,
            combinedStat = StatType.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreRifleDamage,
            flags = StatFlavorFlag.WeaponsRifle,
            combinedStat = StatType.MoreDamage,
        },
        new StatAuthoring
        {
            stat = StatType.MoreBeamDamage,
            flags = StatFlavorFlag.WeaponsBeam,
            combinedStat = StatType.MoreDamage,
        },

        // Critical strikes
        new StatAuthoring
        {
            stat = StatType.BaseCriticalStrikeChance1TenthPercent,
            flags = StatFlavorFlag.General,
            combinedStat = StatType.BaseCriticalStrikeChance1TenthPercent,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedCriticalStrikeChance,
            flags = StatFlavorFlag.General,
            combinedStat = StatType.IncreasedCriticalStrikeChance,
        },
        new StatAuthoring
        {
            stat = StatType.MoreCriticalStrikeChance,
            flags = StatFlavorFlag.General,
            combinedStat = StatType.MoreCriticalStrikeChance,
        },
        new StatAuthoring
        {
            stat = StatType.IncreasedCriticalStrikeDamageMultiplier,
            flags = StatFlavorFlag.General,
            combinedStat = StatType.IncreasedCriticalStrikeDamageMultiplier,
        },
        new StatAuthoring
        {
            stat = StatType.MoreCriticalStrikeDamageMultiplier,
            flags = StatFlavorFlag.General,
            combinedStat = StatType.MoreCriticalStrikeDamageMultiplier,
        },

        new StatAuthoring
        {
            stat = StatType.IncreasedCriticalStrikeChanceWith2HWeapons,
            flags = StatFlavorFlag.Weapons2H,
            combinedStat = StatType.IncreasedCriticalStrikeChance,
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

    public struct Singleton : IComponentData
    {
        public NativeHashMap<int, StatFlavorFlag> StatToStatFlavorFlags;
        public NativeHashMap<int, StatType> StatToStatType;

        public Singleton(StatAuthoring[] StatAuthorings)
        {
            StatToStatFlavorFlags = new NativeHashMap<int, StatFlavorFlag>(StatAuthorings.Length, Allocator.Persistent);
            StatToStatType = new NativeHashMap<int, StatType>(StatAuthorings.Length, Allocator.Persistent);

            for (var i = 0; i < StatAuthorings.Length; i++)
            {
                var statAuthoring = StatAuthorings[i];

                StatToStatFlavorFlags.Add((int)statAuthoring.stat, statAuthoring.flags);
                StatToStatType.Add((int)statAuthoring.stat, statAuthoring.combinedStat);
            }
        }

        public void OnDestroy()
        {
            StatToStatFlavorFlags.Dispose();
            StatToStatType.Dispose();
        }

        public void TotalStatsWithFlavor(in DynamicBuffer<StatContainer> stats, StatFlavorFlag inputFlavorFlags, ref NativeHashMap<int, int> results)
        {
            /// Iterate over stat keys, 
            /// iterate over matches.
            /// Get the matching tags from the StatMatchesTags dictionary.
            /// If every tag from the StatMatchesTags dictionary is in the matches array, add the value to the results dictionary
            /// based on the StatGrantsTags dictionary.
            for (var i = 0; i < stats.Length; i++)
            {
                var stat = (int)stats[i].stat;

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

                if (!StatToStatType.TryGetValue(stat, out var grants))
                {
                    continue;
                }

                // Combine the stat into the hashmap.
                if (results.TryGetValue((int)grants, out var value))
                {
                    results[(int)grants] = value + stats[i].value;
                }
                else
                {
                    results.Add((int)grants, stats[i].value);
                }
            }
        }
    }
}

public struct StatAuthoring
{
    public StatType stat;
    public StatFlavorFlag flags;
    public StatType combinedStat;
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
