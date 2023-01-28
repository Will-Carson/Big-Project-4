using Unity.Entities;
using UnityEngine;

public class ItemAuthoring : MonoBehaviour
{
    class ItemAuthoringBaker : Baker<ItemAuthoring>
    {
        public override void Bake(ItemAuthoring authoring)
        {
            AddBuffer<ContainerSlot>();
            AddComponent<ItemSlotRestriction>();
            AddComponent<ItemSessionId>();
            AddComponent<ItemIcon>();
            AddComponent<ContainerDisplayId>();
        }
    }
}
