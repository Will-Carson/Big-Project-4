///
///
///

using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public struct TalentAuthoring
{
    public string name;
    public StatType stat;
    public int pointCost;
    public int levelRequirement;
    public int maxTalentLevel;

    // Stats required to allocate a talent
    public StatRequirement[] requires;

    // Stats granted by allocating a talent
    public StatData[] grants;
}

public static class TalentDefinitions
{
    public static TalentAuthoring[] Talents = new TalentAuthoring[]
    {
        // Level 1 talents
        new TalentAuthoring
        {
            name = "Physique",
            stat = StatType.TalentPhysique,
            pointCost = 1,
            levelRequirement = 1,
            maxTalentLevel = 10,
            requires = new StatRequirement[]
            {
                // No special requirements
            },
            grants = new StatData[]
            {
                new StatData { stat = StatType.Strength, value = 10 },
            },
        }, // Physique
        new TalentAuthoring
        {
            name = "Reason",
            stat = StatType.TalentReason,
            pointCost = 1,
            levelRequirement = 1,
            maxTalentLevel = 10,
            requires = new StatRequirement[]
            {
                // No special requirements
            },
            grants = new StatData[]
            {
                new StatData { stat = StatType.Strength, value = 10 },
            },
        }, // Reason
        new TalentAuthoring
        {
            name = "Dexterity",
            stat = StatType.TalentDexterity,
            pointCost = 1,
            levelRequirement = 1,
            maxTalentLevel = 10,
            requires = new StatRequirement[]
            {
                // No special requirements
            },
            grants = new StatData[]
            {
                new StatData { stat = StatType.Strength, value = 10 },
            },
        }, // Dexterity
        new TalentAuthoring
        {
            name = "Perception",
            stat = StatType.TalentPerception,
            pointCost = 1,
            levelRequirement = 1,
            maxTalentLevel = 10,
            requires = new StatRequirement[]
            {
                // No special requirements
            },
            grants = new StatData[]
            {
                new StatData { stat = StatType.Strength, value = 10 },
            },
        }, // Perception
        new TalentAuthoring
        {
            name = "Melee",
            stat = StatType.TalentMelee,
            pointCost = 1,
            levelRequirement = 1,
            maxTalentLevel = 10,
            requires = new StatRequirement[]
            {
                // No special requirements
            },
            grants = new StatData[]
            {
                new StatData { stat = StatType.Strength, value = 10 },
            },
        }, // Melee
        new TalentAuthoring
        {
            name = "Ranged",
            stat = StatType.TalentRanged,
            pointCost = 1,
            levelRequirement = 1,
            maxTalentLevel = 10,
            requires = new StatRequirement[]
            {
                // No special requirements
            },
            grants = new StatData[]
            {
                new StatData { stat = StatType.Strength, value = 10 },
            },
        }, // Ranged
        new TalentAuthoring
        {
            name = "Engineering",
            stat = StatType.TalentEngineering,
            pointCost = 1,
            levelRequirement = 1,
            maxTalentLevel = 10,
            requires = new StatRequirement[]
            {
                // No special requirements
            },
            grants = new StatData[]
            {
                new StatData { stat = StatType.Strength, value = 10 },
            },
        }, // Engineering
        new TalentAuthoring
        {
            name = "Mysticism",
            stat = StatType.TalentMysticism,
            pointCost = 1,
            levelRequirement = 1,
            maxTalentLevel = 10,
            requires = new StatRequirement[]
            {
                // No special requirements
            },
            grants = new StatData[]
            {
                new StatData { stat = StatType.Strength, value = 10 },
            },
        }, // Mysticism
        new TalentAuthoring
        {
            name = "Medicine",
            stat = StatType.TalentMedicine,
            pointCost = 1,
            levelRequirement = 1,
            maxTalentLevel = 10,
            requires = new StatRequirement[]
            {
                // No special requirements
            },
            grants = new StatData[]
            {
                new StatData { stat = StatType.Strength, value = 10 },
            },
        }, // Medicine
        new TalentAuthoring
        {
            name = "Defense",
            stat = StatType.TalentDefense,
            pointCost = 1,
            levelRequirement = 1,
            maxTalentLevel = 10,
            requires = new StatRequirement[]
            {
                // No special requirements
            },
            grants = new StatData[]
            {
                new StatData { stat = StatType.Strength, value = 10 },
            },
        }, // Defense

        // Level 4 talents
        new TalentAuthoring
        {
            name = "Technique",
            stat = StatType.TalentTechnique,
            pointCost = 1,
            levelRequirement = 4,
            maxTalentLevel = 10,
            requires = new StatRequirement[]
            {
                // No special requirements
            },
            grants = new StatData[]
            {
                new StatData { stat = StatType.Strength, value = 10 },
            },
        }, // Technique
    };

    public static void CreateTalentsAsEntities(EntityManager em)
    {
        var entities = new Entity[Talents.Length];
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
            var requirementsBuffer = em.AddBuffer<StatRequirementContainer>(talentEntity);

            /// Do not allow players to take the talent if they do not meet level requirements, or if
            /// they already have too many points allocated in this talent. Next add the talent points
            /// requirement. Then add the addition requirements that are specific to a talent.
            requirementsBuffer.Add(new StatRequirementContainer
            {
                requirement = new StatRequirement
                {
                    stat = StatType.Level,
                    min = talent.levelRequirement,
                    max = int.MaxValue
                }
            });
            requirementsBuffer.Add(new StatRequirementContainer
            {
                requirement = new StatRequirement
                {
                    stat = talent.stat,
                    min = 0,
                    max = talent.maxTalentLevel
                }
            });
            requirementsBuffer.Add(new StatRequirementContainer
            {
                requirement = new StatRequirement
                {
                    stat = StatType.TalentPoint,
                    min = talent.pointCost,
                    max = int.MaxValue
                }
            });
            foreach (var req in talent.requires)
            {
                requirementsBuffer.Add(new StatRequirementContainer { requirement = req });
            }

            // Add granted stats
            var statsBuffer = em.AddBuffer<StatContainer>(talentEntity);

            /// Add the talent to the granted stats buffer, the talent point cost, and then add 
            /// the regular granted stats.
            statsBuffer.Add(new StatContainer
            {
                stat = new StatData
                {
                    stat = talent.stat,
                    value = 1,
                }
            });
            statsBuffer.Add(new StatContainer
            {
                stat = new StatData
                {
                    stat = StatType.TalentPoint,
                    value = -talent.pointCost
                }
            });
            foreach (var grants in talent.grants)
            {
                statsBuffer.Add(grants);
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
        in ReceiveRpcCommandRequestComponent receive,
        in Entity entity) =>
        {
            commandBuffer.DestroyEntity(entity);
            var targetEntity = SystemAPI.GetComponent<CommandTargetComponent>(receive.SourceConnection).targetEntity;

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
    public StatType stat;
    public bool deallocate;
}

public struct TalentComponent : IComponentData
{
    public StatType stat;
}