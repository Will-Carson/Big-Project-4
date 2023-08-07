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
            var baseStatStickStats = new Stats(3, Allocator.Persistent);

            baseStatStickStats.AddStat(Stat.AdditionalLife, 100);
            baseStatStickStats.AddStat(Stat.TalentPoint, 10);
            baseStatStickStats.AddStat(Stat.Level, 10);
            AddComponent(baseStatStick, new StatsContainer(baseStatStickStats));

            var equipTo = AddBuffer<EquipStatStickRequest>(entity);
            equipTo.Add(new EquipStatStickRequest
            {
                entity = baseStatStick,
                unequip = false
            });

            AddComponent<StatContainerTag>(entity);
            AddBuffer<DerivedStat>(entity);
            AddBuffer<StatStickContainer>(entity);
            AddComponent<StatRecalculationTag>(entity);
        }
    }
}
