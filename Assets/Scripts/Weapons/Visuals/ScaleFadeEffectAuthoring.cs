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
            //AddComponent(new PostTransformScale { Value = float3x3.Scale(1f) });
            AddComponent(new ScaleFade
            {
                LifeTime = authoring.Lifetime,
                Width = authoring.Width,
            });
        }
    }
}
