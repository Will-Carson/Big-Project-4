using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class BasicStatStickAuthoring : MonoBehaviour
{
    class Baker : Baker<StatStickAuthoring>
    {
        public override void Bake(StatStickAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<StatContainerTag>(entity);
            AddComponent<StatRequirementsTag>(entity);
            AddBuffer<EquippedTo>(entity);
        }
    }
}
