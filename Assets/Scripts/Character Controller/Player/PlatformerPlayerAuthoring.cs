using UnityEngine;
using Unity.Entities;

[DisallowMultipleComponent]
public class PlatformerPlayerAuthoring : MonoBehaviour
{
    public class Baker : Baker<PlatformerPlayerAuthoring>
    {
        public override void Bake(PlatformerPlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PlatformerPlayer());
            AddComponent(entity, new PlatformerPlayerInputs());
        }
    }
}