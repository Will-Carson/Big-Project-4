using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ScaleFadeEffectAuthoring : MonoBehaviour
{
    public float Lifetime = 1f;
    public float Width = 1f;

    public class Baker : Baker<ScaleFadeEffectAuthoring>
    {
        public override void Bake(ScaleFadeEffectAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new ScaleFade
            {
                LifeTime = authoring.Lifetime,
                Width = authoring.Width,
            });
        }
    }
}
