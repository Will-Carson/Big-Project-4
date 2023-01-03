using UnityEngine;
using Unity.Entities;

public class StatsTestAuthoring : MonoBehaviour
{
    class StatsTestAuthoringBaker : Baker<StatsTestAuthoring>
    {
        public override void Bake(StatsTestAuthoring authoring)
        {
            var statSticks = AddBuffer<StatStickContainer>();

            var extraStatStickEntity = CreateAdditionalEntity(entityName: "Extra stats");
            AddBuffer<EquippedTo>(extraStatStickEntity);
            var extraStatStickStats = AddBuffer<StatContainer>(extraStatStickEntity);
            extraStatStickStats.Add(new StatContainer { stat = StatType.AdditionalStrength, value = 100 });

            var baseStatStickEntity = CreateAdditionalEntity(entityName: "Base stats");
            AddBuffer<EquippedTo>(baseStatStickEntity);
            var baseStatStickStats = AddBuffer<StatContainer>(baseStatStickEntity);
            baseStatStickStats.Add(new StatContainer { stat = StatType.AdditionalLife, value = 100 });

            var equipStatSticks = AddBuffer<EquipStatStickRequest>();
            equipStatSticks.Add(new EquipStatStickRequest { unequip = false, entity = extraStatStickEntity });

            var derivedStats = AddBuffer<DerivedStat>();
            derivedStats.Add(new DerivedStat
            {
                fromStat = StatType.AdditionalStrength,
                fromValue = 10,
                toStat = StatType.AdditionalLife,
                toValue = 2,
            });

            AddBuffer<StatContainer>();

            AddComponent<StatRecalculationTag>();
        }
    }
}