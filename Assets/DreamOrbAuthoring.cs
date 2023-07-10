using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class DreamOrbAuthoring : MonoBehaviour
{
    class Baker : Baker<DreamOrbAuthoring>
    {
        public override void Bake(DreamOrbAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<DreamOrb>(entity);
        }
    }
}
