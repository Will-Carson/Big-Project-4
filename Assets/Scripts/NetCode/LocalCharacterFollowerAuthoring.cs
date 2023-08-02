using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

public class LocalCharacterFollowerAuthoring : MonoBehaviour
{
    public float3 offest;

    class Baker : Baker<LocalCharacterFollowerAuthoring>
    {
        public override void Bake(LocalCharacterFollowerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new LocalCharacterFollower
            {
                offset = authoring.offest,
            });
        }
    }
}

public struct LocalCharacterFollower : IComponentData
{
    public float3 offset;
}

public partial struct LocalCharacterFollowerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var playerTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<GhostOwnerIsLocal, PlatformerCharacterComponent>())
        {
            foreach (var (localCharacterFollower, followerTrasnform) in SystemAPI.Query<RefRO<LocalCharacterFollower>, RefRW<LocalTransform>>())
            {
                followerTrasnform.ValueRW = playerTransform.ValueRO;
                followerTrasnform.ValueRW.Position += localCharacterFollower.ValueRO.offset;
            }
        }
    }
}