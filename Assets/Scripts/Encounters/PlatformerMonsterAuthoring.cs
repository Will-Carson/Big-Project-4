using UnityEngine;
using Unity.Entities;

[DisallowMultipleComponent]
public class PlatformerMonsterAuthoring : MonoBehaviour
{
    public class Baker : Baker<PlatformerMonsterAuthoring>
    {
        public override void Bake(PlatformerMonsterAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PlatformerMonster());
            AddComponent(entity, new PlatformerMonsterInputs());
        }
    }
}