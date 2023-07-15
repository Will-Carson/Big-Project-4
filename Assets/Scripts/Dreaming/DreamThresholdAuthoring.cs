using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public class DreamThresholdAuthoring : MonoBehaviour
{
    class Baker : Baker<DreamThresholdAuthoring>
    {
        public override void Bake(DreamThresholdAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<DreamThreshold>(entity);
        }
    }
}

public struct DreamThreshold : IComponentData { }

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[BurstCompile]
public partial struct DreamThresholdCollisionDetectionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) 
    {
        Debug.Log("Bababooey");
        state.Dependency = new CollisionDetection
        {
            commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged),
            dreamThresholdLookup = state.GetComponentLookup<DreamThreshold>(true),
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }

    [BurstCompile]
    struct CollisionDetection : ICollisionEventsJob
    {
        public EntityCommandBuffer commandBuffer;
        [ReadOnly] public ComponentLookup<DreamThreshold> dreamThresholdLookup;

        public void Execute(CollisionEvent collisionEvent)
        {
            if (dreamThresholdLookup.HasComponent(collisionEvent.EntityA))
            {
                commandBuffer.DestroyEntity(collisionEvent.EntityA);
            }
            if (dreamThresholdLookup.HasComponent(collisionEvent.EntityB))
            {
                commandBuffer.DestroyEntity(collisionEvent.EntityB);
            }
        }
    }
}