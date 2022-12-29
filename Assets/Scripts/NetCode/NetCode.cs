using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public struct LocalPlayerTag : IComponentData { }

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct GoInGameClientSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PrefabContainer>();
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkIdComponent>()
            .WithNone<NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkIdComponent>>().WithEntityAccess().WithNone<NetworkStreamInGame>())
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(entity);
            var req = commandBuffer.CreateEntity();
            commandBuffer.AddComponent<GoInGameRpc>(req);
            commandBuffer.AddComponent(req, new SendRpcCommandRequestComponent { TargetConnection = entity });
        }
        commandBuffer.Playback(state.EntityManager);
    }
}

[BurstCompile]
// When server receives go in game request, go in game and delete request
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GoInGameServerSystem : ISystem
{
    private ComponentLookup<NetworkIdComponent> networkIdFromEntity;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PrefabContainer>();
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GoInGameRpc>()
            .WithAll<ReceiveRpcCommandRequestComponent>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
        networkIdFromEntity = state.GetComponentLookup<NetworkIdComponent>(true);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var prefabs = SystemAPI.GetSingletonBuffer<PrefabContainer>();
        var playerPrefab = PrefabContainer.GetEntityWithId(prefabs, "PlatformerPlayer");
        var characterPrefab = PrefabContainer.GetEntityWithId(prefabs, "PlatformerCharacter");
        var itemPrefab = PrefabContainer.GetEntityWithId(prefabs, "Item");
        state.EntityManager.GetName(playerPrefab, out var prefabName);
        var worldName = new FixedString32Bytes(state.WorldUnmanaged.Name);

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        networkIdFromEntity.Update(ref state);

        foreach (var (reqSrc, reqEntity) in SystemAPI.Query<
            RefRO<ReceiveRpcCommandRequestComponent>>()
            .WithAll<GoInGameRpc>()
            .WithEntityAccess())
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(reqSrc.ValueRO.SourceConnection);
            var networkId = networkIdFromEntity[reqSrc.ValueRO.SourceConnection].Value;

            UnityEngine.Debug.Log($"'{worldName}' setting connection '{networkId}' to in game, spawning a Ghost '{prefabName}' for them!");

            var player = commandBuffer.Instantiate(playerPrefab);
            var character = commandBuffer.Instantiate(characterPrefab);
            commandBuffer.SetComponent(player, new GhostOwnerComponent { NetworkId = networkId });
            commandBuffer.SetComponent(character, new GhostOwnerComponent { NetworkId = networkId });
            commandBuffer.SetComponent(reqSrc.ValueRO.SourceConnection, 
                new CommandTargetComponent
                {
                    targetEntity = player
                });
            commandBuffer.SetComponent(player, 
                new PlatformerPlayer 
                { 
                    ControlledCharacter = character, 
                    Name = "TEST" 
                });

            // Add the player to the linked entity group so it is destroyed automatically on disconnect
            commandBuffer.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup { Value = player });
            commandBuffer.DestroyEntity(reqEntity);
        }
        commandBuffer.Playback(state.EntityManager);
    }
}

public struct GoInGameRpc : IRpcCommand { }

public struct PrefabContainer : IBufferElementData
{
    public FixedString64Bytes id;
    public Entity prefab;

    [BurstCompile]
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

// Might be something to this but for now it doesn't work.
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class ClientRpcQueueSystem<T> : SystemBase
    where T : unmanaged, IRpcCommand
{
    public delegate void AddRpcToQueueEvent(T rpc);

    public static event AddRpcToQueueEvent AddToQueueEvent;

    NativeList<T> rpcs = new NativeList<T>(Allocator.Persistent);

    protected override void OnCreate()
    {
        AddToQueueEvent += AddToQueue;
    }

    protected override void OnDestroy()
    {
        AddToQueueEvent -= AddToQueue;
        rpcs.Dispose();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        for (int i = 0; i < rpcs.Length; i++)
        {
            var rpc = rpcs[i];
            var rpcEntity = commandBuffer.CreateEntity();
            commandBuffer.AddComponent<SendRpcCommandRequestComponent>(rpcEntity);
            commandBuffer.AddComponent(rpcEntity, rpc);
        }
        rpcs.Clear();
    }

    public void AddToQueue(T rpc)
    {
        rpcs.Add(rpc);
    }
}

// Something good here?
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class ClientRpcEventSystem<T> : SystemBase
    where T : unmanaged, IRpcCommand
{
    NativeList<T> rpcs = new NativeList<T>(Allocator.Persistent);

    protected override void OnCreate()
    {
        var singletonEntity = EntityManager.CreateEntity();
        var singleton = new Singleton
        {
            rpcs = new NativeList<T>(Allocator.Persistent)
        };
        EntityManager.AddComponentData(singletonEntity, singleton);
    }

    protected override void OnDestroy()
    {
        rpcs.Dispose();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        var rpcs = SystemAPI.GetSingleton<Singleton>().rpcs;

        for (int i = 0; i < rpcs.Length; i++)
        {
            var rpc = rpcs[i];
            var rpcEntity = commandBuffer.CreateEntity();
            commandBuffer.AddComponent<SendRpcCommandRequestComponent>(rpcEntity);
            commandBuffer.AddComponent(rpcEntity, rpc);
        }
        rpcs.Clear();
    }

    public struct Singleton : IComponentData
    {
        public NativeList<T> rpcs;

        public void OnCreate()
        {
            rpcs = new NativeList<T>(Allocator.Persistent);
        }

        public void OnDestroy()
        {
            rpcs.Dispose();
        }

        public void Trigger(T rpc)
        {
            rpcs.Add(rpc);
        }
    }
}