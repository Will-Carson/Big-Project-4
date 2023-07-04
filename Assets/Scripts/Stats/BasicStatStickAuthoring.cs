using Unity.Entities;
using UnityEngine;

public class BasicStatStickAuthoring : MonoBehaviour
{
    class Baker : Baker<StatStickAuthoring>
    {
        public override void Bake(StatStickAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddBuffer<StatContainer>(entity);
            AddBuffer<StatRequirementContainer>(entity);
            AddBuffer<EquippedTo>(entity);
        }
    }
}
