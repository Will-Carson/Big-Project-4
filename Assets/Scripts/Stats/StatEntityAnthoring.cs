using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class StatEntityAnthoring : MonoBehaviour
{
    class Baker : Baker<StatEntityAnthoring>
    {
        public override void Bake(StatEntityAnthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            var baseStatStick = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
            AddBuffer<EquippedTo>(baseStatStick);
            var baseStatStickStats = AddBuffer<StatElement>(baseStatStick);

            StatElement.AddStat(baseStatStickStats, Stat.AdditionalLife, 100);
            StatElement.AddStat(baseStatStickStats, Stat.TalentPoint, 10);
            StatElement.AddStat(baseStatStickStats, Stat.Level, 10);

            var equipTo = AddBuffer<EquipStatStickRequest>(entity);
            equipTo.Add(new EquipStatStickRequest
            {
                entity = baseStatStick,
                unequip = false
            });

            AddBuffer<StatElement>(entity);
            AddBuffer<DerivedStat>(entity);
            AddBuffer<StatStickContainer>(entity);
            AddComponent<StatRecalculationTag>(entity);
        }
    }
}
