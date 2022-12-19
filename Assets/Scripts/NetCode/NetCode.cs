using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

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
    public int id;
    public Entity prefab;
}