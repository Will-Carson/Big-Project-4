/// Generate items
/// Handle affixes
/// Create tooltips
/// affixes -> stats

using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class ItemServerSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<PrefabContainer>();
    }

    protected override void OnUpdate()
    {
        var prefabs = SystemAPI.GetSingletonBuffer<PrefabContainer>();

        // Turn affix definitions into usable data here.
        AffixDefinitions.CreateAffixesAsEntities(EntityManager, prefabs);

        prefabs = SystemAPI.GetSingletonBuffer<PrefabContainer>();

        // Turn base item definitions into usable data here.
        BaseItemDefinitions.CreateBaseItemsAsEntities(EntityManager, prefabs);

        Enabled = false;
    }
}

public static class BaseItemDefinitions
{
    public static BaseItemData[] BaseItems = new BaseItemData[]
    {
        new BaseItemData
        {
            name = "Rusty Iron Helm",
            image = "",
            levelRequirement = 1,
            minItemLevel = 1,
            tags = new ItemTag[] { ItemTag.Armor, ItemTag.Body },
            grants = new StatRange[]
            {
                new StatRange { stat = Stat.AdditionalStrength, min = 3, max = 5 },
            },
            requirements = new StatRange[]
            {
                new StatRange { stat = Stat.AdditionalStrength, min = 10, max = int.MaxValue },
            },
        },
    };

    public static void CreateBaseItemsAsEntities(EntityManager em, DynamicBuffer<PrefabContainer> prefabs)
    {
        var baseItemPrefab = PrefabContainer.GetEntityWithId(prefabs, "StatItem");
        var rawStatStickPrefab = PrefabContainer.GetEntityWithId(prefabs, "RawStatStick");
        for (var i = 0; i < BaseItems.Length; i++)
        {
            var baseItem = BaseItems[i];

            var baseItemEntity = em.Instantiate(baseItemPrefab);
            em.SetName(baseItemEntity, "BaseItem-" + baseItem.name);

            var baseStatsStatStickEntity = em.Instantiate(rawStatStickPrefab);

            // Configure the entity
            var baseStats = em.AddComponent<StatsContainer>(baseStatsStatStickEntity);

            //foreach (var stat in baseItem.grants)
            //{
            //    baseStats.Add(new StatContainer
            //    {
            //        stat = stat.stat,
            //        value = stat.max
            //    });
            //}

            var equipStatStickRequestsBuffer = em.AddBuffer<EquipStatStickRequest>(baseItemEntity);
            equipStatStickRequestsBuffer.Add(new EquipStatStickRequest
            {
                unequip = false,
                entity = baseStatsStatStickEntity
            });

            // Add it back to the dictionary
            baseItem.entity = baseItemEntity;
            BaseItems[i] = baseItem;
        }
    }
}

public static class AffixDefinitions
{
    public static AffixData[] Affixes = new AffixData[]
    {
        new AffixData
        {
            name = "Hard",
            playerLevelRequirement = 1,
            itemLevelRequirement = 1,
            weight = 100,
            tags = new ItemTag[] { ItemTag.Armor, ItemTag.Jewellery },
            grants = new StatRange[]
            {
                new StatRange { stat = Stat.AdditionalStrength, min = 5, max = 8 },
            },
        },
    };

    public static Dictionary<ItemTag, List<Entity>> TagToAffix;

    public static void CreateAffixesAsEntities(EntityManager em, DynamicBuffer<PrefabContainer> prefabs)
    {
        //var statStickPrefab = PrefabContainer.GetEntityWithId(prefabs, "RawStatStick");
        //for (var i = 0; i < Affixes.Length; i++)
        //{
        //    var affix = Affixes[i];

        //    var affixEntity = em.Instantiate(statStickPrefab);
        //    em.SetName(affixEntity, "Affix-" + affix.name);

        //    // Configure the entity
        //    var statBuffer = em.AddBuffer<StatContainer>(affixEntity);
        //    for (var j = 0; j < affix.grants.Length; j++)
        //    {
        //        var grantedStat = affix.grants[j];
        //        statBuffer.Add(new StatContainer
        //        {
        //            stat = grantedStat.stat,
        //            value = grantedStat.max
        //        });
        //    }

        //    // Add it back to the dictionary
        //    affix.entity = affixEntity;
        //    Affixes[i] = affix;
        //}
    }

    public static List<Entity> GetAffixesWithTags(uint ilevel, List<ItemTag> tags)
    {
        var results = new List<Entity>();

        var tempAffixData = new List<AffixData>();
        for (var i = 0; i < tags.Count; i++)
        {
            var tag = tags[i];

            tempAffixData.AddRange(Affixes.Where(affix => affix.tags.Contains(tag)).ToList());
        }

        tempAffixData = tempAffixData.Where(affix => affix.itemLevelRequirement == ilevel).ToList();

        for (var i = 0; i < tempAffixData.Count; i++)
        {
            var affix = tempAffixData[i];
            results.Add(affix.entity);
        }

        return results;
    }
}

public struct AffixData
{
    public string name;
    public int playerLevelRequirement;
    public int itemLevelRequirement;
    public int weight;
    public ItemTag[] tags;
    public StatRange[] grants;
    public Entity entity;
}

public struct BaseItemData
{
    public string name;
    public string image;
    public int levelRequirement;
    public int minItemLevel;
    public ItemTag[] tags;
    public StatRange[] grants;
    public StatRange[] requirements;
    public Entity entity;
}

public struct StatRange
{
    public Stat stat;
    public float min;
    public float max;
}

public enum ItemTag
{
    None,

    // Weapons
    Weapon,
    MeleeWeapon,
    RangedWeapon,
    OneHander,
    TwoHander,
    Sword,
    Dagger,
    Axe,
    Hammer,
    Scepter,
    Bow,
    Wand,

    // Armor
    Armor,
    Shield,
    Helm,
    Body,
    Gloves,
    Boots,

    // Jewellery
    Jewellery,
    Neck,
    Waist,
    Ring,
}