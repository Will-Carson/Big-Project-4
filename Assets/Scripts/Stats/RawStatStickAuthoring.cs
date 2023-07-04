using Unity.Entities;
using UnityEngine;

public class RawStatStickAuthoring : MonoBehaviour
{
    class Baker : Baker<RawStatStickAuthoring>
    {
        public override void Bake(RawStatStickAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddBuffer<StatContainer>(entity);
            AddBuffer<EquippedTo>(entity);
        }
    }
}
