using Unity.Entities;
using UnityEngine;

public class ItemAuthoring : MonoBehaviour
{
    class Baker : Baker<ItemAuthoring>
    {
        public override void Bake(ItemAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<Item>(entity);
            //AddComponent<ItemRestrictions>(entity);
            AddComponent<ContainerParent>(entity);

            AddComponent<ItemData>(entity);

            AddBuffer<StatElement>(entity);
            AddBuffer<EquippedElement>(entity);
            AddBuffer<EquippedToElement>(entity);
            AddBuffer<StatRangeElement>(entity);
        }
    }
}
