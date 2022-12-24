using Unity.Entities;
using UnityEngine;

public class StatStickAuthoring : MonoBehaviour
{
    class Baker : Baker<StatStickAuthoring>
    {
        public override void Bake(StatStickAuthoring authoring)
        {
            AddBuffer<StatContainer>();
            AddBuffer<StatRequirementContainer>();
            AddBuffer<EquippedTo>();
            AddBuffer<StatStickContainer>();
            AddBuffer<EquipStatStickRequest>();
        }
    }
}
