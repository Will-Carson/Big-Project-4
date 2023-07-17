using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class PlayerZapperAuthoring : MonoBehaviour
{
    class Baker : Baker<PlayerZapperAuthoring>
    {
        public override void Bake(PlayerZapperAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<PlayerZapper>(entity);
        }
    }
}

public struct PlayerZapper : IComponentData { }

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct PlayerZapperSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerZapper>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        var playerZapper = SystemAPI.GetSingletonEntity<PlayerZapper>();
        var playerZapperTransform = SystemAPI.GetComponent<LocalTransform>(playerZapper);

        foreach (var playerTransform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<Jumper>())
        {
            playerTransform.ValueRW = playerZapperTransform;
        }

        commandBuffer.DestroyEntity(playerZapper);
    }
}