using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class TalentServerSystem : SystemBase
{
    /// <summary>
    /// Create talent entities
    /// Recieve requests from the client to allocate talents
    /// </summary>
    
    protected override void OnCreate()
    {
        CreateTalentsAsEntities(EntityManager);
    }

    protected override void OnUpdate()
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        var talentQuery = GetEntityQuery(typeof(TalentComponent));
        var talentEntities = talentQuery.ToEntityArray(Allocator.Temp);
        var talentComponents = talentQuery.ToComponentDataArray<TalentComponent>(Allocator.Temp);

        foreach (var (rpc, receive, entity) in SystemAPI.Query<RefRO<TalentAllocationRequestRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
        {
            commandBuffer.DestroyEntity(entity);
            var targetEntity = SystemAPI.GetComponent<CommandTarget>(receive.ValueRO.SourceConnection).targetEntity;
            var characterEntity = SystemAPI.GetComponent<PlatformerPlayer>(targetEntity).ControlledCharacter;

            for (var i = 0; i < talentComponents.Length; i++)
            {
                var talentComponent = talentComponents[i];
                var talentEntity = talentEntities[i];

                if (talentComponent.stat == rpc.ValueRO.stat)
                {
                    // Allocate or deallocate here
                    commandBuffer.AppendToBuffer(characterEntity, new EquipStatStickRequest
                    {
                        unequip = rpc.ValueRO.refund,
                        entity = talentEntity
                    });
                    break;
                }
            }
        }
    }

    public void CreateTalentsAsEntities(EntityManager em)
    {
        var Talents = UnityEngine.Resources.LoadAll<TalentDefinition>("Talent definitions");

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
            var requirements = em.AddBuffer<StatRequirementElement>(talentEntity);

            /// Do not allow players to take the talent if they do not meet level requirements, or if
            /// they already have too many points allocated in this talent. Next add the talent points
            /// requirement. Then add the addition requirements that are specific to a talent.
            requirements.Add(new StatRequirementElement(Stat.Level, Range.FromMin(talent.levelRequirement)));
            requirements.Add(new StatRequirementElement(talent.stat, Range.FromMax(talent.maxTalentLevel)));
            requirements.Add(new StatRequirementElement(Stat.TalentPoint, Range.FromMin(talent.pointCost)));

            if (talent.requires != null)
            {
                foreach (var req in talent.requires)
                {
                    requirements.Add(new StatRequirementElement(req));
                }
            }

            // Add granted stats
            var stats = em.AddBuffer<StatElement>(talentEntity);

            /// Add the talent to the granted stats buffer, the talent point cost, and then add 
            /// the regular granted stats.
            StatElement.AddStat(stats, talent.stat, 1);
            StatElement.AddStat(stats, Stat.TalentPoint, -talent.pointCost);
            foreach (var grants in talent.grants)
            {
                StatElement.AddStat(stats, grants.stat, grants.value);
            }

            em.AddBuffer<EquippedTo>(talentEntity);
        }
    }
}

public struct TalentAllocationRequestRpc : IRpcCommand
{
    public Stat stat;
    public bool refund;
}

public struct TalentComponent : IComponentData
{
    public Stat stat;
}