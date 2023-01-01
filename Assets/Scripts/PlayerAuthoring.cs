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
            AddBuffer<DerivedStat>();
            AddBuffer<StatStickContainer>();
            AddBuffer<EquipStatStickRequest>();

            // Character controller stuff

            AddComponent<LocalPlayerTag>();
        }
    }
}
