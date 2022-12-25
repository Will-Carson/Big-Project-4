using UnityEngine;
using Unity.Entities;

[DisallowMultipleComponent]
public class PlatformerPlayerAuthoring : MonoBehaviour
{
    public class Baker : Baker<PlatformerPlayerAuthoring>
    {
        public override void Bake(PlatformerPlayerAuthoring authoring)
        {
            AddComponent(new PlatformerPlayer());
            AddComponent(new PlatformerPlayerInputs());
        }
    }
}