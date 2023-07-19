using Unity.Entities;
using UnityEngine;

public class BossTagAuthoring : MonoBehaviour
{
    class Baker : Baker<BossTagAuthoring>
    {
        public override void Bake(BossTagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<BossTagSetup>(entity);
        }
    }
}
