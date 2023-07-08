using Unity.Entities;
using UnityEngine;

public class BasicStatStickAuthoring : MonoBehaviour
{
    class Baker : Baker<StatStickAuthoring>
    {
        public override void Bake(StatStickAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<StatContainer>(entity);
            AddComponent<StatRequirements>(entity);
            AddBuffer<EquippedTo>(entity);
        }
    }
}
