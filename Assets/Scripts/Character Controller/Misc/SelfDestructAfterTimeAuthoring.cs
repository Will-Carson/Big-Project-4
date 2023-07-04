using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SelfDestructAfterTimeAuthoring : MonoBehaviour
{
    public float LifeTime = 1f;

    public class Baker : Baker<SelfDestructAfterTimeAuthoring>
    {
        public override void Bake(SelfDestructAfterTimeAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SelfDestructAfterTime
            {
                LifeTime = authoring.LifeTime,
            });
        }
    }
}