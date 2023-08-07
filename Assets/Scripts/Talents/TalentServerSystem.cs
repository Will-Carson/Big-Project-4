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
            var requirementsLength = (talent.requires == null) ? 3 : talent.requires.Length + 3;
            var requirements = new StatRanges(requirementsLength, Allocator.Persistent);

            /// Do not allow players to take the talent if they do not meet level requirements, or if
            /// they already have too many points allocated in this talent. Next add the talent points
            /// requirement. Then add the addition requirements that are specific to a talent.
            requirements.AddRange(Stat.Level, Range.FromMin(talent.levelRequirement));
            requirements.AddRange(talent.stat, Range.FromMax(talent.maxTalentLevel));
            requirements.AddRange(Stat.TalentPoint, Range.FromMin(talent.pointCost));

            if (talent.requires != null)
            {
                foreach (var req in talent.requires)
                {
                    requirements.AddRange(req);
                }
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
                stats.AddStat(grants.stat, grants.value);
            }
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

[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)]
public struct TalentsComponent : IComponentData
{
    [GhostField] byte talentDefense;


    public void SetValues(Stats stats)
    {
        talentDefense = (byte)stats.GetStatValue(Stat.TalentDefense);
    }
}