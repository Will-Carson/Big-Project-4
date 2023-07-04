using System.Collections.Generic;
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
