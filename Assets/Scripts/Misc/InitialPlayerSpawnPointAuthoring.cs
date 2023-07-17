using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class InitialPlayerSpawnPointAuthoring : MonoBehaviour
{
    public class Baker : Baker<InitialPlayerSpawnPointAuthoring>
    {
        public override void Bake(InitialPlayerSpawnPointAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new InitialPlayerSpawnPoint());
        }
    }
}