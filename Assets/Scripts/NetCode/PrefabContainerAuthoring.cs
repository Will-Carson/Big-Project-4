using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class PrefabContainerAuthoring : MonoBehaviour
{
    public List<GameObject> prefabs = new List<GameObject>();

    public class Baker : Baker<PrefabContainerAuthoring>
    {
        public override void Bake(PrefabContainerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            var buffer = AddBuffer<PrefabContainer>(entity);
            foreach (var prefab in authoring.prefabs)
            {
                buffer.Add(new PrefabContainer
                {
                    id = prefab.name,
                    prefab = GetEntity(prefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}

public struct PrefabContainer : IBufferElementData
{
    public FixedString64Bytes id;
    public Entity prefab;

    public static Entity GetEntityWithId(DynamicBuffer<PrefabContainer> prefabs, FixedString64Bytes id)
    {
        for (var i = 0; i < prefabs.Length; i++)
        {
            var prefab = prefabs[i];
            if (prefab.id == id)
            {
                return prefab.prefab;
            }
        }
        return Entity.Null;
    }
}
