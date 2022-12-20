/// Containers are where items exist in the world. Containers have several important
/// features. The first is nesting; Containers hold items, but items can themselves
/// be containers. The second is stat propagation. Some containers should be able to
/// effect their owning entities stats.
/// 
/// There are 4 primary types of containers. A player may only display (at most) 1 of
/// each type of container at once. Here are the 4 types:
/// 1. Inventory - The most basic type, this is simply a store of items that the
/// player carries with them.
/// 2. Equipment - Equipments stats should effect the containers owner. Also, each
/// equipment slot is specialized. Only items of a particular type should go there.
/// 3. Abilities - A player has several ability slots, and each ability may have a
/// container of its own. Each ability is effected by the stats of the items in its 
/// own container. The player must also be able to "access" its abilities by using
/// them.
/// 4. Foreign - This covers all "other" inventories. Whether picking up loot or
/// trading, the container is foreign.

using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public partial class ServerContainerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Moving items between slots
        // Checking if a move is valid before doing it
        // Triggering stat updates on the effected entities

        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        var containerLookup = SystemAPI.GetBufferLookup<ContainerSlot>(false);

        Entities
        .ForEach((
        in PressContainerSlotRpc rpc,
        in ReceiveRpcCommandRequestComponent receive,
        in Entity entity) =>
        {
            var targetEntity = SystemAPI.GetComponent<CommandTargetComponent>(receive.SourceConnection).targetEntity;
            commandBuffer.DestroyEntity(entity);

            if (!containerLookup.TryGetBuffer(targetEntity, out var rootContainer))
            {
                return;
            }

            var inventoryContainer = new ContainerSlot();
            var equipmentContainer = new ContainerSlot();
            var abilitiesContainer = new ContainerSlot();
            var foreignContainer = new ContainerSlot();

            for (var i = 0; i < rootContainer.Length; i++)
            {
                var subContainer = rootContainer[i];
                if ((ContainerType)subContainer.id == ContainerType.Inventory)
                {
                    inventoryContainer = subContainer;
                }
                if ((ContainerType)subContainer.id == ContainerType.Equipment)
                {
                    equipmentContainer = subContainer;
                }
                if ((ContainerType)subContainer.id == ContainerType.Abilities)
                {
                    abilitiesContainer = subContainer;
                }
                if ((ContainerType)subContainer.id == ContainerType.Foreign)
                {
                    foreignContainer = subContainer;
                }
            }

            // The item on the players mouse is always in their inventory, and thus the from container is always inventory.
            var fromContainerEntity = inventoryContainer.item;
            var toContainerEntity = Entity.Null;

            {
                if (rpc.containerType == ContainerType.Inventory)
                {
                    toContainerEntity = inventoryContainer.item;
                }
                if (rpc.containerType == ContainerType.Equipment)
                {
                    toContainerEntity = equipmentContainer.item;
                }
                if (rpc.containerType == ContainerType.Abilities)
                {
                    toContainerEntity = abilitiesContainer.item;
                }
                if (rpc.containerType == ContainerType.Foreign)
                {
                    toContainerEntity = foreignContainer.item;
                }
            }

            // Validate so far
            if (fromContainerEntity == Entity.Null) return;
            if (toContainerEntity == Entity.Null) return;

            if (!containerLookup.TryGetBuffer(fromContainerEntity, out var fromContainer))
            {
                return;
            }
            if (!containerLookup.TryGetBuffer(toContainerEntity, out var toContainer))
            {
                return;
            }

            var item1 = Entity.Null;
            var item2 = Entity.Null;
            for (var i = 0; i < fromContainer.Length; i++)
            {
                var slot = fromContainer[i];

                if (slot.id == 0)
                {
                    item1 = slot.item;
                }
            }
            for (var i = 0; i < toContainer.Length; i++)
            {
                var slot = toContainer[i];

                if (slot.id == rpc.slotId)
                {
                    item2 = slot.item;
                }
            }

            // Here we have the containers and the items, now we need to check if the swap is valid.
            // Check if the toContainerEntity is an equipment or ability container

            var toContainerType = SystemAPI.GetComponent<ContainerTypeComponent>(toContainerEntity).type;

            /// WARNING FUTURE SELF: This line of thinking will not work for abilities, because it targets
            /// the second "level" of containers and not *any* level of containers. A better container
            /// system would be more dynamic.
        })
        .Run();
    }
}

public struct ContainerTypeComponent : IComponentData
{
    public ContainerType type;
}

public enum ContainerType
{
    None,
    Inventory,
    Equipment,
    Abilities,
    Foreign
}

public enum EquipmentSlot
{
    None,
    RightHand,
    LeftHand,
    Head,
    Chest,
    Hands,
    Feet,
    Neck,
    Waist,
    RightRing,
    LeftRing
}

public struct PressContainerSlotRpc : IRpcCommand
{
    public ContainerType containerType;
    public int slotId;
}

[GhostComponent]
public struct ContainerSlot : IBufferElementData
{
    [GhostField]
    public int id;
    [GhostField]
    public Entity item;
}