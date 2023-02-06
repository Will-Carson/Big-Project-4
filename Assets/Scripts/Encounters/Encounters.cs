using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

[Serializable]
[GhostComponent]
public struct PlatformerMonster : IComponentData
{
    [GhostField]
    public FixedString128Bytes Name;
    [GhostField]
    public Entity ControlledCharacter;
}

[Serializable]
public struct PlatformerMonsterInputs : IComponentData
{
    public float2 Move;
    public float3 Look;

    public bool SprintHeld;
    public bool RollHeld;
    public bool JumpHeld;

    public InputEvent JumpPressed;
    public InputEvent DashPressed;
    public InputEvent CrouchPressed;
    public InputEvent RopePressed;
    public InputEvent ClimbPressed;
    public InputEvent FlyNoCollisionsPressed;

    public InputEvent Fire1Pressed;
    public InputEvent Fire1Released;
    public InputEvent Fire2Pressed;
    public InputEvent Fire2Released;
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class MonsterSpawningSystem : SystemBase
{
    int maxMonsters = 1;
    Random random;

    protected override void OnCreate()
    {
        random = new Random(int.MaxValue);
    }

    protected override void OnUpdate()
    {
        var monsterQuery = GetEntityQuery(typeof(PlatformerMonster));

        if (monsterQuery.CalculateEntityCount() >= maxMonsters)
        {
            return;
        }

        var prefabs = SystemAPI.GetSingletonBuffer<PrefabContainer>();

        var monsterPrefab = PrefabContainer.GetEntityWithId(prefabs, "PlatformerMonster");
        var characterPrefab = PrefabContainer.GetEntityWithId(prefabs, "PlatformerUnownedCharacter");

        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        var monsterInstance = commandBuffer.Instantiate(monsterPrefab);
        var characterInstance = commandBuffer.Instantiate(characterPrefab);

        var spawnPosition = new float3(1, 0, 0);
        spawnPosition = math.mul(random.NextQuaternionRotation(), spawnPosition);
        spawnPosition *= random.NextFloat(0, 40);
        spawnPosition.y = 0;

        commandBuffer.SetComponent(characterInstance, LocalTransform.FromPosition(spawnPosition));
        commandBuffer.SetComponent(monsterInstance, new PlatformerMonster { ControlledCharacter = characterInstance });
    }
}