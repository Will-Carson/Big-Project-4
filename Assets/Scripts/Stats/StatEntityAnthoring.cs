using Unity.Entities;
using UnityEngine;

public class StatEntityAnthoring : MonoBehaviour
{
    public StatContainer[] baseStats = new StatContainer[]
    {
        new StatContainer { stat = new StatData { stat = StatType.Health, value = 100 } },
    };

    class Baker : Baker<StatEntityAnthoring>
    {
        public override void Bake(StatEntityAnthoring authoring)
        {
            var baseStatStick = CreateAdditionalEntity();
            AddBuffer<EquippedTo>(baseStatStick);
            var baseStatStickStats = AddBuffer<StatContainer>(baseStatStick);

            foreach (var stat in authoring.baseStats)
            {
                baseStatStickStats.Add(stat);
            }

            var equipTo = AddBuffer<EquipStatStickRequest>();
            equipTo.Add(new EquipStatStickRequest
            {
                statStick = baseStatStick,
                unequip = false
            });

            AddBuffer<StatContainer>();
            AddBuffer<ResourceContainer>();
            AddBuffer<DerivedStat>();
            AddBuffer<StatStickContainer>();
            AddComponent<StatRecalculationTag>();
        }
    }
}
