using Unity.Entities;
using UnityEngine;

public class WeaponWielderAuthoring : MonoBehaviour
{
    class Baker : Baker<WeaponWielderAuthoring>
    {
        public override void Bake(WeaponWielderAuthoring authoring)
        {
            AddComponent<CurrentWeaponContainer>();
            AddBuffer<WeaponContainer>();
        }
    }
}
