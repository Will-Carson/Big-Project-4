using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

[assembly: RegisterGenericComponentType(typeof(TalentAllocationRequestRpc))]

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct GoInGameClientSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PrefabContainer>();
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId>()
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
        foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithEntityAccess().WithNone<NetworkStreamInGame>())
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(entity);
            var req = commandBuffer.CreateEntity();
            commandBuffer.AddComponent<GoInGameRpc>(req);
            commandBuffer.AddComponent(req, new SendRpcCommandRequest { TargetConnection = entity });
        }
        commandBuffer.Playback(state.EntityManager);
    }
}

[BurstCompile]
// When server receives go in game request, go in game and delete request
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GoInGameServerSystem : ISystem
{
    private ComponentLookup<NetworkId> networkIdFromEntity;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PrefabContainer>();
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GoInGameRpc>()
            .WithAll<ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
        networkIdFromEntity = state.GetComponentLookup<NetworkId>(true);
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
        var weaponPrefab = PrefabContainer.GetEntityWithId(prefabs, "Shotgun");
        state.EntityManager.GetName(playerPrefab, out var prefabName);
        var worldName = new FixedString32Bytes(state.WorldUnmanaged.Name);

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        networkIdFromEntity.Update(ref state);

        foreach (var (reqSrc, reqEntity) in SystemAPI.Query<
            RefRO<ReceiveRpcCommandRequest>>()
            .WithAll<GoInGameRpc>()
            .WithEntityAccess())
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(reqSrc.ValueRO.SourceConnection);
            var networkId = networkIdFromEntity[reqSrc.ValueRO.SourceConnection].Value;

            UnityEngine.Debug.Log($"'{worldName}' setting connection '{networkId}' to in game, spawning a Ghost '{prefabName}' for them!");

            var player = commandBuffer.Instantiate(playerPrefab);
            var character = commandBuffer.Instantiate(characterPrefab);
            commandBuffer.SetComponent(player, new GhostOwner { NetworkId = networkId });
            commandBuffer.SetComponent(character, new GhostOwner { NetworkId = networkId });
            commandBuffer.SetComponent(reqSrc.ValueRO.SourceConnection, 
                new CommandTarget
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

            var spawnPoint = SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<InitialPlayerSpawnPoint>());
            commandBuffer.SetComponent(character, spawnPoint);

            {
                // Set up the players containers
                var rootContainer = commandBuffer.AddBuffer<ContainerChild>(player);

                var inventoryEntity = commandBuffer.Instantiate(itemPrefab);
                var equipmentEntity = commandBuffer.Instantiate(itemPrefab);
                var abilitiesEntity = commandBuffer.Instantiate(itemPrefab);
                var foreignEntity = commandBuffer.Instantiate(itemPrefab);

                commandBuffer.AddComponent(equipmentEntity, new EquipmentContainer(character));

                rootContainer.Add(new ContainerChild(inventoryEntity));
                rootContainer.Add(new ContainerChild(equipmentEntity));
                rootContainer.Add(new ContainerChild(abilitiesEntity));
                rootContainer.Add(new ContainerChild(foreignEntity));

                // Give the different containers their stats
                var inventory = commandBuffer.AddBuffer<ContainerChild>(inventoryEntity);
                for (int i = 0; i < 16; i++)
                {
                    inventory.Add(new ContainerChild());
                }

                var testItem = commandBuffer.Instantiate(itemPrefab);
                commandBuffer.AddComponent(testItem, new ItemRestrictions(Restrictions.Helm));
                commandBuffer.AddComponent(testItem, new ItemData 
                { 
                    name = "bababooey",
                    description = "This is a cool item",
                    artAddress2d = "headgear_01",
                });
                commandBuffer.SetComponent(testItem, new GhostOwner { NetworkId = networkId });
                inventory[5] = new ContainerChild(testItem);
                commandBuffer.SetComponent(testItem, new ContainerParent(inventoryEntity));
                var itemStats = commandBuffer.AddBuffer<StatElement>(testItem);
                itemStats.Add(new StatElement(Stat.IncreasedLife, 10));
                itemStats.Add(new StatElement(Stat.MoreLife, 10));

                var equipment = commandBuffer.AddBuffer<ContainerChild>(equipmentEntity);
                for (var i = 0; i < 10; i++)
                {
                    equipment.Add(new ContainerChild());
                }
                var equipmentRestrictionsBuffer = commandBuffer.AddBuffer<ContainerChildRestrictions>(equipmentEntity);
                equipmentRestrictionsBuffer.Add(new ContainerChildRestrictions(Restrictions.Helm));
                equipmentRestrictionsBuffer.Add(new ContainerChildRestrictions(Restrictions.Body));
                equipmentRestrictionsBuffer.Add(new ContainerChildRestrictions(Restrictions.Belt));
                equipmentRestrictionsBuffer.Add(new ContainerChildRestrictions(Restrictions.Boots));
                equipmentRestrictionsBuffer.Add(new ContainerChildRestrictions(Restrictions.Gloves));
                equipmentRestrictionsBuffer.Add(new ContainerChildRestrictions(Restrictions.HainHand));
                equipmentRestrictionsBuffer.Add(new ContainerChildRestrictions(Restrictions.OffHand));
                equipmentRestrictionsBuffer.Add(new ContainerChildRestrictions(Restrictions.Amulet));
                equipmentRestrictionsBuffer.Add(new ContainerChildRestrictions(Restrictions.LeftRing));
                equipmentRestrictionsBuffer.Add(new ContainerChildRestrictions(Restrictions.RightRing));

                // Set the owner for the container
                commandBuffer.SetComponent(inventoryEntity, new GhostOwner { NetworkId = networkId });
                commandBuffer.SetComponent(equipmentEntity, new GhostOwner { NetworkId = networkId });
                commandBuffer.SetComponent(abilitiesEntity, new GhostOwner { NetworkId = networkId });
                commandBuffer.SetComponent(foreignEntity, new GhostOwner { NetworkId = networkId });

                // Make sure these entities get destroyed when the player that holds them is destroyed.
                commandBuffer.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup { Value = inventoryEntity });
                commandBuffer.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup { Value = equipmentEntity });
                commandBuffer.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup { Value = abilitiesEntity });
                commandBuffer.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup { Value = foreignEntity });
            } // Container set up

            // Spawn & assign starting weapon
            var weaponEntity = commandBuffer.Instantiate(weaponPrefab);
            commandBuffer.SetComponent(weaponEntity, new GhostOwner { NetworkId = networkId });
            commandBuffer.SetComponent(character, new ActiveWeapon { entity = weaponEntity });
        }
        commandBuffer.Playback(state.EntityManager);
    }
}

public struct GoInGameRpc : IRpcCommand { }

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
            commandBuffer.AddComponent<SendRpcCommandRequest>(rpcEntity);
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
            commandBuffer.AddComponent<SendRpcCommandRequest>(rpcEntity);
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