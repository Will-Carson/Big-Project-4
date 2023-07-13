using UnityEngine;
using Unity.Entities;

public class DreamSpawnpointAuthoring : MonoBehaviour
{
    class Baker : Baker<DreamSpawnpointAuthoring>
    {
        public override void Bake(DreamSpawnpointAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<DreamSpawnpoint>(entity);
        }
    }
}

public struct DreamSpawnpoint : IComponentData { }