using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public struct TalentAuthoring
{
    public string name;
    public Stat stat;
    public int pointCost;
    public int levelRequirement;
    public int maxTalentLevel;

    // Stats required to allocate a talent
    public (Stat, float, float)[] requires;

    // Stats granted by allocating a talent
    public (Stat, float)[] grants;
}

public static class TalentDefinitions
{
    public static TalentAuthoring[] Talents = new TalentAuthoring[]
    {
        // Level 1 talents
        new TalentAuthoring
        {
            name = "Physique",
            stat = Stat.TalentPhysique,
            pointCost = 1,
            levelRequirement = 1,
            maxTalentLevel = 10,
            requires = new[]
            {
                ( Stat.TalentPoint, 1f, float.MaxValue ),
            },
            grants = new[]
            {
                ( Stat.AdditionalStrength, 10f ),
            },
        }, // Physique
        //new TalentAuthoring
        //{
        //    name = "Reason",
        //    stat = Stat.TalentReason,
        //    pointCost = 1,
        //    levelRequirement = 1,
        //    maxTalentLevel = 10,
        //    requires = new StatRequirement[]
        //    {
        //        // No special requirements
        //    },
        //    grants = new StatContainer[]
        //    {
        //        new StatContainer { stat = Stat.AdditionalStrength, value = 10 },
        //    },
        //}, // Reason
        //new TalentAuthoring
        //{
        //    name = "Dexterity",
        //    stat = Stat.TalentDexterity,
        //    pointCost = 1,
        //    levelRequirement = 1,
        //    maxTalentLevel = 10,
        //    requires = new StatRequirement[]
        //    {
        //        // No special requirements
        //    },
        //    grants = new StatContainer[]
        //    {
        //        new StatContainer { stat = Stat.AdditionalStrength, value = 10 },
        //    },
        //}, // Dexterity
        //new TalentAuthoring
        //{
        //    name = "Perception",
        //    stat = Stat.TalentPerception,
        //    pointCost = 1,
        //    levelRequirement = 1,
        //    maxTalentLevel = 10,
        //    requires = new StatRequirement[]
        //    {
        //        // No special requirements
        //    },
        //    grants = new StatContainer[]
        //    {
        //        new StatContainer { stat = Stat.AdditionalStrength, value = 10 },
        //    },
        //}, // Perception
        //new TalentAuthoring
        //{
        //    name = "Melee",
        //    stat = Stat.TalentMelee,
        //    pointCost = 1,
        //    levelRequirement = 1,
        //    maxTalentLevel = 10,
        //    requires = new StatRequirement[]
        //    {
        //        // No special requirements
        //    },
        //    grants = new StatContainer[]
        //    {
        //        new StatContainer { stat = Stat.AdditionalStrength, value = 10 },
        //    },
        //}, // Melee
        //new TalentAuthoring
        //{
        //    name = "Ranged",
        //    stat = Stat.TalentRanged,
        //    pointCost = 1,
        //    levelRequirement = 1,
        //    maxTalentLevel = 10,
        //    requires = new StatRequirement[]
        //    {
        //        // No special requirements
        //    },
        //    grants = new StatContainer[]
        //    {
        //        new StatContainer { stat = Stat.AdditionalStrength, value = 10 },
        //    },
        //}, // Ranged
        //new TalentAuthoring
        //{
        //    name = "Engineering",
        //    stat = Stat.TalentEngineering,
        //    pointCost = 1,
        //    levelRequirement = 1,
        //    maxTalentLevel = 10,
        //    requires = new StatRequirement[]
        //    {
        //        // No special requirements
        //    },
        //    grants = new StatContainer[]
        //    {
        //        new StatContainer { stat = Stat.AdditionalStrength, value = 10 },
        //    },
        //}, // Engineering
        //new TalentAuthoring
        //{
        //    name = "Mysticism",
        //    stat = Stat.TalentMysticism,
        //    pointCost = 1,
        //    levelRequirement = 1,
        //    maxTalentLevel = 10,
        //    requires = new StatRequirement[]
        //    {
        //        // No special requirements
        //    },
        //    grants = new StatContainer[]
        //    {
        //        new StatContainer { stat = Stat.AdditionalStrength, value = 10 },
        //    },
        //}, // Mysticism
        //new TalentAuthoring
        //{
        //    name = "Medicine",
        //    stat = Stat.TalentMedicine,
        //    pointCost = 1,
        //    levelRequirement = 1,
        //    maxTalentLevel = 10,
        //    requires = new StatRequirement[]
        //    {
        //        // No special requirements
        //    },
        //    grants = new StatContainer[]
        //    {
        //        new StatContainer { stat = Stat.AdditionalStrength, value = 10 },
        //    },
        //}, // Medicine
        //new TalentAuthoring
        //{
        //    name = "Defense",
        //    stat = Stat.TalentDefense,
        //    pointCost = 1,
        //    levelRequirement = 1,
        //    maxTalentLevel = 10,
        //    requires = new StatRequirement[]
        //    {
        //        // No special requirements
        //    },
        //    grants = new StatContainer[]
        //    {
        //        new StatContainer { stat = Stat.AdditionalStrength, value = 10 },
        //    },
        //}, // Defense

        //// Level 4 talents
        //new TalentAuthoring
        //{
        //    name = "Technique",
        //    stat = Stat.TalentTechnique,
        //    pointCost = 1,
        //    levelRequirement = 4,
        //    maxTalentLevel = 10,
        //    requires = new StatRequirement[]
        //    {
        //        // No special requirements
        //    },
        //    grants = new StatContainer[]
        //    {
        //        new StatContainer { stat = Stat.AdditionalStrength, value = 10 },
        //    },
        //}, // Technique
    };

    public static void CreateTalentsAsEntities(EntityManager em)
    {
        for (var i = 0; i < Talents.Length; i++)
        {
            var talent = Talents[i];

            var talentEntity = em.CreateEntity();
            em.SetName(talentEntity, "Talent-" + talent.name);

            // Add talent component
            em.AddComponentData(talentEntity, new TalentComponent
            {
                stat = talent.stat
            });

            // Add requirements
            var requirements = new StatRanges(talent.requires.Length + 3, Allocator.Persistent);

            /// Do not allow players to take the talent if they do not meet level requirements, or if
            /// they already have too many points allocated in this talent. Next add the talent points
            /// requirement. Then add the addition requirements that are specific to a talent.
            requirements.AddRange(Stat.Level, Range.FromMin(talent.levelRequirement));
            requirements.AddRange(talent.stat, Range.FromMax(talent.maxTalentLevel));
            requirements.AddRange(Stat.TalentPoint, Range.FromMin(talent.pointCost));

            foreach (var req in talent.requires)
            {
                requirements.AddRange(req);
            }

            em.AddComponentData(talentEntity, new StatRequirements(requirements));

            // Add granted stats
            var stats = new Stats(talent.grants.Length + 2, Allocator.Persistent);

            /// Add the talent to the granted stats buffer, the talent point cost, and then add 
            /// the regular granted stats.
            stats.AddStat(talent.stat, 1);
            stats.AddStat(Stat.TalentPoint, -talent.pointCost);
            foreach (var grants in talent.grants)
            {
                stats.AddStat(grants);
            }
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class TalentServerSystem : SystemBase
{
    /// <summary>
    /// Create talent entities
    /// Recieve requests from the client to allocate talents
    /// </summary>
    
    protected override void OnCreate()
    {
        TalentDefinitions.CreateTalentsAsEntities(EntityManager);
    }

    protected override void OnUpdate()
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        var talentQuery = GetEntityQuery(typeof(TalentComponent));
        var talentEntities = talentQuery.ToEntityArray(Allocator.Temp);
        var talentComponents = talentQuery.ToComponentDataArray<TalentComponent>(Allocator.Temp);

        Entities
        .ForEach((
        in TalentAllocationRequestRpc rpc,
        in ReceiveRpcCommandRequest receive,
        in Entity entity) =>
        {
            commandBuffer.DestroyEntity(entity);
            var targetEntity = SystemAPI.GetComponent<CommandTarget>(receive.SourceConnection).targetEntity;

            for (var i = 0; i < talentComponents.Length; i++)
            {
                var talentComponent = talentComponents[i];
                var talentEntity = talentEntities[i];

                if (talentComponent.stat == rpc.stat)
                {
                    // Allocate or deallocate here
                    commandBuffer.AppendToBuffer(targetEntity, new EquipStatStickRequest
                    {
                        unequip = rpc.deallocate,
                        entity = talentEntity
                    });
                }
            }
        })
        .Run();
    }
}

public struct TalentAllocationRequestRpc : IRpcCommand
{
    public Stat stat;
    public bool deallocate;
}

public struct TalentComponent : IComponentData
{
    public Stat stat;
}