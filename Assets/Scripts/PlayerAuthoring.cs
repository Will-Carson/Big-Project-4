using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // Add container components
            AddComponent<HandSlot>(entity);
            AddBuffer<ContainerSlot>(entity);

            // Stat stuff
            AddComponent<StatContainer>(entity);
            AddBuffer<DerivedStat>(entity);
            AddBuffer<StatStickContainer>(entity);
            AddBuffer<EquipStatStickRequest>(entity);

            // Character controller stuff

            AddComponent<LocalPlayerTag>(entity);
        }
    }
}
