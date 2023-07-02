using UnityEngine;
using Unity.Entities;

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
            var extraStatStickStats = AddBuffer<StatContainer>(extraStatStickEntity);
            extraStatStickStats.Add(new StatContainer { stat = StatType.AdditionalStrength, value = 100 });

            var baseStatStickEntity = CreateAdditionalEntity(TransformUsageFlags.None, entityName: "Base stats");
            AddBuffer<EquippedTo>(baseStatStickEntity);
            var baseStatStickStats = AddBuffer<StatContainer>(baseStatStickEntity);
            baseStatStickStats.Add(new StatContainer { stat = StatType.AdditionalLife, value = 100 });

            var equipStatSticks = AddBuffer<EquipStatStickRequest>(entity);
            equipStatSticks.Add(new EquipStatStickRequest { unequip = false, entity = extraStatStickEntity });

            var derivedStats = AddBuffer<DerivedStat>(entity);
            derivedStats.Add(new DerivedStat
            {
                fromStat = StatType.AdditionalStrength,
                fromValue = 10,
                toStat = StatType.AdditionalLife,
                toValue = 2,
            });

            AddBuffer<StatContainer>(entity);

            AddComponent<StatRecalculationTag>(entity);
        }
    }
}