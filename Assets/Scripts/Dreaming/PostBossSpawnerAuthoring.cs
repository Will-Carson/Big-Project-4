using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class PostBossSpawnerAuthoring : MonoBehaviour
{
    public GameObject prefab;

    class Baker : Baker<PostBossSpawnerAuthoring>
    {
        public override void Bake(PostBossSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PostBossSpawner
            {
                prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct PostBossSpawner : IComponentData
{
    public Entity prefab;
}