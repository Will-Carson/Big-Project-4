using Unity.Collections;
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
            AddComponent<StatContainerTag>(entity);
            AddBuffer<DerivedStat>(entity);
            AddBuffer<StatStickContainer>(entity);
            AddBuffer<EquipStatStickRequest>(entity);

            // Character controller stuff
            AddComponent<LocalPlayerTag>(entity); // TODO there's no way adding this to every player is appropriate
        }
    }
}
