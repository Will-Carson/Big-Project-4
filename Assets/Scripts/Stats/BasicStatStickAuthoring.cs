using Unity.Entities;
using UnityEngine;

public class BasicStatStickAuthoring : MonoBehaviour
{
    class Baker : Baker<StatStickAuthoring>
    {
        public override void Bake(StatStickAuthoring authoring)
        {
            AddBuffer<StatContainer>();
            AddBuffer<StatRequirementContainer>();
            AddBuffer<EquippedTo>();
        }
    }
}
