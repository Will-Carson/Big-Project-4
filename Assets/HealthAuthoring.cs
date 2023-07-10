using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class HealthAuthoring : MonoBehaviour
{
    public float maxHealth = 100;

    class Baker : Baker<HealthAuthoring>
    {
        public override void Bake(HealthAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Health(authoring.maxHealth));
        }
    }
}
