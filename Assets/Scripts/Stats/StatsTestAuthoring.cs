using UnityEngine;
using Unity.Entities;
using System.Linq.Expressions;
using Unity.Collections;

public class StatsTestAuthoring : MonoBehaviour
{
    class StatsTestAuthoringBaker : Baker<StatsTestAuthoring>
    {
        public override void Bake(StatsTestAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var statSticks = AddBuffer<StatStickContainer>(entity);

            var extraStatStickEntity = CreateAdditionalEntity(TransformUsageFlags.None, entityName: "Extra stats");
            AddBuffer<EquippedTo>(extraStatStickEntity);
            var extraStatStickStats = new Stats(1, Allocator.Persistent);
            extraStatStickStats.AddStat(Stat.AdditionalStrength, 100);
            AddComponent(extraStatStickEntity, new StatContainer(extraStatStickStats));

            var baseStatStickEntity = CreateAdditionalEntity(TransformUsageFlags.None, entityName: "Base stats");
            AddBuffer<EquippedTo>(baseStatStickEntity);
            var baseStatStickStats = new Stats(1, Allocator.Persistent);
            baseStatStickStats.AddStat(Stat.AdditionalLife, 100);
            AddComponent(baseStatStickEntity, new StatContainer(baseStatStickStats));

            var equipStatSticks = AddBuffer<EquipStatStickRequest>(entity);
            equipStatSticks.Add(new EquipStatStickRequest { unequip = false, entity = extraStatStickEntity });

            var derivedStats = AddBuffer<DerivedStat>(entity);
            derivedStats.Add(new DerivedStat
            {
                fromStat = Stat.AdditionalStrength,
                fromValue = 10,
                toStat = Stat.AdditionalLife,
                toValue = 2,
            });

            AddComponent<StatContainer>(entity);
            AddComponent<StatRecalculationTag>(entity);
        }
    }
}