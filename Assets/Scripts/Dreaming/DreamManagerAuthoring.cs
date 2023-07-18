using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class DreamManagerAuthoring : MonoBehaviour
{
    class Baker : Baker<DreamManagerAuthoring>
    {
        public override void Bake(DreamManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new DreamManager
            {

            });
        }
    }
}

public struct DreamManager : IComponentData
{
    public float bossJuice;
    public bool nextEncounterIsBoss;
}

public partial struct DreamManagerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var manager = SystemAPI.GetSingleton<DreamManager>();
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (grantsBossJuiceSetup, entity) in SystemAPI.Query<RefRO<GrantsBossJuiceOnDestructionSetup>>().WithNone<GrantsBossJuiceOnDestruction>().WithEntityAccess())
        {
            commandBuffer.AddComponent(entity, new GrantsBossJuiceOnDestruction { amount = grantsBossJuiceSetup.ValueRO.amount });
        }

        foreach (var (grantsBossJuice, entity) in SystemAPI.Query<RefRO<GrantsBossJuiceOnDestruction>>().WithNone<GrantsBossJuiceOnDestructionSetup>().WithEntityAccess())
        {
            manager.bossJuice += grantsBossJuice.ValueRO.amount;
            commandBuffer.RemoveComponent<GrantsBossJuiceOnDestruction>(entity);
        }

        if (manager.bossJuice > 15) // TODO magic numbuh
        {
            manager.bossJuice = 0;
            manager.nextEncounterIsBoss = true;
        }

        SystemAPI.SetSingleton(manager);
    }
}

public struct GrantsBossJuiceOnDestructionSetup : IComponentData
{
    public float amount;
}

public struct GrantsBossJuiceOnDestruction : ICleanupComponentData
{
    public float amount;
}