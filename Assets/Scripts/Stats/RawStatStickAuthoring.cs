using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class RawStatStickAuthoring : MonoBehaviour
{
    class Baker : Baker<RawStatStickAuthoring>
    {
        public override void Bake(RawStatStickAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new StatsContainer(100, Allocator.Persistent));
            AddBuffer<EquippedTo>(entity);
        }
    }
}
