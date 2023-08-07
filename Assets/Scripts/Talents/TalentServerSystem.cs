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

        foreach (var (talents, stats) in SystemAPI.Query<RefRW<TalentsComponent>, RefRO<StatsContainer>>().WithChangeFilter<StatsContainer>())
        {
            UnityEngine.Debug.Log("Talents updated!");
            talents.ValueRW.Update(stats.ValueRO.stats);
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

            em.AddComponentData(talentEntity, new StatsContainer(stats));

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

[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)]
public struct TalentsComponent : IComponentData, IStatDerived
{
    [GhostField] public byte TalentPhysique;
    [GhostField] public byte TalentReason;
    [GhostField] public byte TalentDexterity;
    [GhostField] public byte TalentPerception;
    [GhostField] public byte TalentMelee;
    [GhostField] public byte TalentRanged;
    [GhostField] public byte TalentEngineering;
    [GhostField] public byte TalentMysticism;
    [GhostField] public byte TalentMedicine;
    [GhostField] public byte TalentDefense;

    [GhostField] public byte TalentTechnique;

    public void Update(Stats stats)
    {
        TalentPhysique = (byte)stats.GetStatValue(Stat.TalentPhysique);
        TalentReason = (byte)stats.GetStatValue(Stat.TalentReason);
        TalentDexterity = (byte)stats.GetStatValue(Stat.TalentDexterity);
        TalentPerception = (byte)stats.GetStatValue(Stat.TalentPerception);
        TalentMelee = (byte)stats.GetStatValue(Stat.TalentMelee);
        TalentRanged = (byte)stats.GetStatValue(Stat.TalentRanged);
        TalentEngineering = (byte)stats.GetStatValue(Stat.TalentEngineering);
        TalentMysticism = (byte)stats.GetStatValue(Stat.TalentMysticism);
        TalentMedicine = (byte)stats.GetStatValue(Stat.TalentMedicine);
        TalentDefense = (byte)stats.GetStatValue(Stat.TalentDefense);

        TalentTechnique = (byte)stats.GetStatValue(Stat.TalentTechnique);
    }
}

public interface IStatDerived
{
    public void Update(Stats stats);
}