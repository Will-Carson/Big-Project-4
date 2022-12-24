using Unity.Entities;
using UnityEngine;

public class RawStatStickAuthoring : MonoBehaviour
{
    class Baker : Baker<RawStatStickAuthoring>
    {
        public override void Bake(RawStatStickAuthoring authoring)
        {
            AddBuffer<StatContainer>();
            AddBuffer<EquippedTo>();
        }
    }
}
