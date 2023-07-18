using Unity.Entities;
using UnityEngine;

public class GrantsBossJuiceOnDestructionAuthoring : MonoBehaviour
{
    public float amount;

    class Baker : Baker<GrantsBossJuiceOnDestructionAuthoring>
    {
        public override void Bake(GrantsBossJuiceOnDestructionAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new GrantsBossJuiceOnDestructionSetup { amount = authoring.amount });
        }
    }
}
