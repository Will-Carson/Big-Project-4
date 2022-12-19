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
                new StatData { type = StatType.Strength, value = 10 },
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
                new StatData { type = StatType.Strength, value = 10 },
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
                new StatData { type = StatType.Strength, value = 10 },
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
                new StatData { type = StatType.Strength, value = 10 },
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
                new StatData { type = StatType.Strength, value = 10 },
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
                new StatData { type = StatType.Strength, value = 10 },
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
                new StatData { type = StatType.Strength, value = 10 },
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
                new StatData { type = StatType.Strength, value = 10 },
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
                new StatData { type = StatType.Strength, value = 10 },
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
                new StatData { type = StatType.Strength, value = 10 },
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
                new StatData { type = StatType.Strength, value = 10 },
            },
        }, // Technique
    };

    public static void CreateTalentsAsEntities(EntityManager entityManager)
    {
        var entities = new Entity[Talents.Length];
        for (var i = 0; i < Talents.Length; i++)
        {
            var talent = Talents[i];

            var talentEntity = entityManager.CreateEntity();

            // Add talent component
            entityManager.AddComponentData(talentEntity, new TalentComponent
            {
                stat = talent.stat
            });

            // Add requirements
            var requirementsBuffer = entityManager.AddBuffer<StatRequirementContainer>(talentEntity);

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
            var statsBuffer = entityManager.AddBuffer<StatContainer>(talentEntity);

            /// Add the talent to the granted stats buffer, the talent point cost, and then add 
            /// the regular granted stats.
            statsBuffer.Add(new StatContainer
            {
                stat = new StatData
                {
                    type = talent.stat,
                    value = 1,
                }
            });
            statsBuffer.Add(new StatContainer
            {
                stat = new StatData
                {
                    type = StatType.TalentPoint,
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

        var netIdToEntity = NetworkServerSystem.NetIdToEntity;

        Entities
        .ForEach((
        in TalentAllocationRequestRPC rpc,
        in ReceiveRpcCommandRequestComponent receive,
        in Entity entity) =>
        {
            var networkId = SystemAPI.GetComponent<NetworkIdComponent>(receive.SourceConnection).Value;
            netIdToEntity.TryGetValue(networkId, out var targetEntity);

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
                        statStick = talentEntity
                    });
                }
            }
            commandBuffer.DestroyEntity(entity);
        })
        .Run();
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class NetworkServerSystem : SystemBase
{
    public static NativeHashMap<int, Entity> NetIdToEntity = 
        new NativeHashMap<int, Entity>(10, Allocator.Persistent);

    public static NativeHashMap<Entity, int> EntityToNetId = 
        new NativeHashMap<Entity, int>(10, Allocator.Persistent);

    protected override void OnDestroy()
    {
        NetIdToEntity.Dispose();
        EntityToNetId.Dispose();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        // Add new owned entities to the hashmaps.
        Entities
        .WithNone<GhostIdSystemCleanupTag>()
        .ForEach((
        in GhostOwnerComponent owner,
        in Entity entity) =>
        {
            commandBuffer.AddComponent(entity, new GhostIdSystemCleanupTag { networkId = owner.NetworkId });
            NetIdToEntity.Add(owner.NetworkId, entity);
            EntityToNetId.Add(entity, owner.NetworkId);
        })
        .Run();

        // Remove expired entities from the hashmaps.
        Entities
        .WithNone<GhostOwnerComponent>()
        .ForEach((
        in GhostIdSystemCleanupTag cleanup,
        in Entity entity) =>
        {
            commandBuffer.DestroyEntity(entity);
            NetIdToEntity.Remove(cleanup.networkId);
            EntityToNetId.Remove(entity);
        })
        .Run();
    }

    public struct GhostIdSystemCleanupTag : ICleanupComponentData 
    { 
        public int networkId;
    }
}

public struct TalentAllocationRequestRPC : IRpcCommand
{
    public StatType stat;
    public bool deallocate;
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class TalentClientSystem : SystemBase
{
    /// <summary>
    /// Update the UI when the player successfully allocates
    /// Allow the player to un-allocate talents
    /// </summary>

    protected override void OnUpdate()
    {

    }
}

public struct TalentComponent : IComponentData
{
    public StatType stat;
}