using UnityEngine;
using Unity.Entities;

[DisallowMultipleComponent]
public class PlatformerPlayerAuthoring : MonoBehaviour
{
    public GameObject ControlledCharacter;
    public GameObject ControlledCamera;

    public class Baker : Baker<PlatformerPlayerAuthoring>
    {
        public override void Bake(PlatformerPlayerAuthoring authoring)
        {
            AddComponent(new PlatformerPlayer
            {
                ControlledCharacter = GetEntity(authoring.ControlledCharacter)
            });
            AddComponent(new PlatformerPlayerInputs());
            AddComponent(new ControlledCameraComponent
            {
                ControlledCamera = GetEntity(authoring.ControlledCamera)
            });
        }
    }
}