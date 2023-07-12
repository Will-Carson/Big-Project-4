using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

[ExecuteAlways]
public class EncounterAuthoring : MonoBehaviour
{
    public bool save = false;
    public bool build = false;

    private void Update()
    {
        //if (save)
        //{
        //    Save();
        //    save = false;
        //}

        //if (build)
        //{
        //    Build();
        //    build = false;
        //}
    }

    private void Build()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void Save()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    class Baker : Baker<EncounterAuthoring>
    {
        public override void Bake(EncounterAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var encounterBuffer = AddBuffer<Encounter>(entity);
            foreach (Transform child in authoring.transform)
            {
                encounterBuffer.Add(new Encounter(GetEntity(child, TransformUsageFlags.Dynamic), LocalTransformExtensions.FromTransform(child)));
            }
        }
    }
}

public struct Encounter : IBufferElementData
{
    public Entity prefab;
    public LocalTransform transform;

    public Encounter(Entity prefab, LocalTransform transform) : this()
    {
        this.prefab = prefab;
        this.transform = transform;
    }
}

public static class LocalTransformExtensions
{
    public static LocalTransform FromTransform(Transform transform)
    {
        return LocalTransform.FromPositionRotation(transform.position, transform.rotation);
    }
} 