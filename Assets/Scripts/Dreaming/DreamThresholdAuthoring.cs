using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
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
        var prefabs = SystemAPI.GetSingletonBuffer<PrefabContainer>(true);

        state.Dependency = new CollisionDetection
        {
            commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged),
            dreamThresholdLookup = state.GetComponentLookup<DreamThreshold>(true),
            localTransformLookup = state.GetComponentLookup<LocalTransform>(true),
            prefabs = prefabs,
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }

    [BurstCompile]
    struct CollisionDetection : ITriggerEventsJob
    {
        public EntityCommandBuffer commandBuffer;
        [ReadOnly] public ComponentLookup<DreamThreshold> dreamThresholdLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> localTransformLookup;
        [ReadOnly] public DynamicBuffer<PrefabContainer> prefabs;

        public void Execute(TriggerEvent triggerEvent)
        {
            var dreamThresholdEntity = Entity.Null;
            if (dreamThresholdLookup.HasComponent(triggerEvent.EntityA))
            {
                commandBuffer.DestroyEntity(triggerEvent.EntityA);
                dreamThresholdEntity = triggerEvent.EntityA;
            }
            if (dreamThresholdLookup.HasComponent(triggerEvent.EntityB))
            {
                commandBuffer.DestroyEntity(triggerEvent.EntityB);
                dreamThresholdEntity = triggerEvent.EntityB;
            }

            var prefab = PrefabContainer.GetEntityWithId(prefabs, "InitialEncounter");
            var instance = commandBuffer.Instantiate(prefab);

            if (localTransformLookup.TryGetComponent(dreamThresholdEntity, out var transform))
            {
                commandBuffer.SetComponent(instance, transform.Translate(new float3(0, -30, 0)));
            }
        }
    }
}