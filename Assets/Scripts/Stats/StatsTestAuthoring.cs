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
            extraStatStickStats.Add(new StatContainer { stat = new StatData { stat = StatType.Intelligence, value = 100 } });

            var baseStatStickEntity = CreateAdditionalEntity(entityName: "Base stats");
            AddBuffer<EquippedTo>(baseStatStickEntity);
            var baseStatStickStats = AddBuffer<StatContainer>(baseStatStickEntity);
            baseStatStickStats.Add(new StatContainer { stat = new StatData { stat = StatType.Health, value = 100 } });
            //baseStatStickStats.Add(new StatContainer { stat = new StatData { type = StatType.Strength, value = 100 } });

            var equipStatSticks = AddBuffer<EquipStatStickRequest>();
            equipStatSticks.Add(new EquipStatStickRequest { unequip = false, statStick = extraStatStickEntity });
            //equipStatSticks.Add(new EquipStatStickRequest { equip = true, statStick = baseStatStickEntity });

            var derivedStats = AddBuffer<DerivedStat>();
            derivedStats.Add(new DerivedStat
            {
                fromStat = new StatData { stat = StatType.Intelligence, value = 10 },
                toStat = new StatData { stat = StatType.Health, value = 2 }
            });

            AddBuffer<StatContainer>();
            var resources = AddBuffer<ResourceContainer>();
            resources.Add(new ResourceContainer
            {
                currentValue = 120,
                maxStat = new StatData { stat = StatType.Health }
            });

            AddComponent<StatRecalculationTag>();
        }
    }
}