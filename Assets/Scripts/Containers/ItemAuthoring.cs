using Unity.Entities;
using UnityEngine;

public class ItemAuthoring : MonoBehaviour
{
    class ItemAuthoringBaker : Baker<ItemAuthoring>
    {
        public override void Bake(ItemAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddBuffer<ContainerSlot>(entity);
            AddComponent<ItemSlotRestriction>(entity);
            AddComponent<ItemSessionId>(entity);
            AddComponent<ItemIcon>(entity);
            AddComponent<ContainerDisplayId>(entity);
        }
    }
}
