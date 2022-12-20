using System;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[assembly: RegisterGenericComponentType(typeof(PressContainerSlotRpc))]
[assembly: RegisterGenericComponentType(typeof(TalentAllocationRequestRpc))]

public struct LocalPlayerTag : IComponentData { }

public partial class GoInGameClientSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        // Once there is a connection, send the rpc to spawn the player
        Entities
        .WithNone<NetworkStreamInGame>()
        .ForEach((
        in NetworkIdComponent networkId,
        in Entity entity) =>
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(entity);
            var req = commandBuffer.CreateEntity();
            commandBuffer.AddComponent<GoInGameRpc>(req);
            commandBuffer.AddComponent(req, new SendRpcCommandRequestComponent { TargetConnection = entity });
        })
        .Run();
    }
}

public partial class GoInGameServerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        Entities
        .ForEach((
        in GoInGameRpc rpc,
        in ReceiveRpcCommandRequestComponent request,
        in Entity entity) =>
        {
            // Create player
            var player = commandBuffer.CreateEntity();

            // Configure player
            commandBuffer.AddComponent(player, 
                new GhostOwnerComponent 
                { 
                    NetworkId = SystemAPI.GetComponent<NetworkIdComponent>(request.SourceConnection).Value 
                });
            commandBuffer.AddComponent(request.SourceConnection, 
                new CommandTargetComponent 
                { 
                    targetEntity = player 
                });

            commandBuffer.DestroyEntity(entity);
        })
        .Run();
    }
}

public struct GoInGameRpc : IRpcCommand { }

public struct PrefabContainer : IBufferElementData
{
    public FixedString64Bytes id;
    public Entity prefab;
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