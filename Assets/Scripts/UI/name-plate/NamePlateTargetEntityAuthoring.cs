using Unity.Entities;
using UnityEngine;

public class NamePlateTargetEntityAuthoring : MonoBehaviour
{
    public GameObject target;

    class Baker : Baker<NamePlateTargetEntityAuthoring>
    {
        public override void Bake(NamePlateTargetEntityAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            var targetEntity = GetEntity(authoring.target, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NamePlateTargetEntity
            {
                entity = targetEntity
            });
        }
    }
}

public struct NamePlateTargetEntity : IComponentData
{
    public Entity entity;
}