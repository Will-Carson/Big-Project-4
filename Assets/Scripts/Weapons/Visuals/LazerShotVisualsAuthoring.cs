using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class LazerShotVisualsAuthoring : MonoBehaviour
{
    public float Lifetime = 1f;
    public float Width = 1f;
    public GameObject HitVisualPrefab;
    
    class Baker : Baker<LazerShotVisualsAuthoring>
    {
        public override void Bake(LazerShotVisualsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new LazerShotVisuals
            {
                LifeTime = authoring.Lifetime,
                Width = authoring.Width,
                HitVisualsPrefab = GetEntity(authoring.HitVisualPrefab, TransformUsageFlags.NonUniformScale),
            });
            AddComponent(entity, new PostTransformMatrix { Value = float4x4.Scale(1f) });
        }
    }
}
