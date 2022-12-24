using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            // Add container components
            AddComponent<HandSlot>();
            AddBuffer<ContainerSlot>();

            // Stat stuff
            AddBuffer<StatContainer>();
            AddBuffer<ResourceContainer>();
            AddBuffer<DerivedStat>();
            AddBuffer<StatStickContainer>();
            AddBuffer<EquipStatStickRequest>();

            AddComponent<LocalPlayerTag>();
        }
    }
}
