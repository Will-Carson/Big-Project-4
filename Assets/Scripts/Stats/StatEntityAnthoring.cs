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
            AddBuffer<EquippedToElement>(baseStatStick);
            var baseStatStickStats = AddBuffer<StatElement>(baseStatStick);

            baseStatStickStats.Add(new StatElement(Stat.AdditionalLife, 100));
            baseStatStickStats.Add(new StatElement(Stat.TalentPoint, 10));
            baseStatStickStats.Add(new StatElement(Stat.Level, 10));

            var equipped = AddBuffer<EquippedElement>(entity);
            equipped.Add(new EquippedElement(baseStatStick));

            var finalStats = AddBuffer<StatElement>(entity);

            finalStats.Add(new StatElement(Stat.AdditionalLife, 100));
            finalStats.Add(new StatElement(Stat.TalentPoint, 10));
            finalStats.Add(new StatElement(Stat.Level, 10));

            AddBuffer<EquippedToElement>(entity);
            AddBuffer<StatRequirementElement>(entity);
        }
    }
}