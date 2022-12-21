/// Containers are where items exist in the world. Containers have several important
/// features. The first is nesting; Containers hold items, but items can themselves
/// be containers. The second is stat propagation. Some containers should be able to
/// effect their owning entities stats.

using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Maintains a hashmaps of container ids to containers
/// Processes PressContainerSlotRpcs into item swap requests 
/// Checks if items can be swaped before swaping
/// Triggers stat recalculation on parents when an item is "equipped"
/// </summary>
public partial class ServerContainerSystem : SystemBase
{
    private NativeHashMap<uint, Entity> idToContainerEntity = new NativeHashMap<uint, Entity>(100, Allocator.Persistent);
    private uint nextContainerId;

    protected override void OnDestroy()
    {
        idToContainerEntity.Dispose();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        var containerLookup = SystemAPI.GetBufferLookup<ContainerSlot>(false);
        var idToContainerEntity = this.idToContainerEntity;

        // TODO this needs another pass
        // Set up containers
        //Entities
        //.WithAll<DynamicBuffer<ContainerSlot>>()
        //.WithNone<ContainerSetupTag>()
        //.ForEach((
        //in Entity entity) =>
        //{
        //    commandBuffer.SetComponent(entity, new ContainerId
        //    {
        //        id = nextContainerId++
        //    });
        //    idToContainerEntity.Add(nextContainerId, entity);
        //})
        //.Run();

        //Entities
        //.WithNone<DynamicBuffer<ContainerSlot>>()
        //.WithAll<ContainerSetupTag>()
        //.ForEach((
        //in ContainerId containerId,
        //in Entity entity) =>
        //{
        //    commandBuffer.DestroyEntity(entity);
        //    idToContainerEntity.Remove(containerId.id);
        //})
        //.Run();

        // Process rpcs
        Entities
        .ForEach((
        in PressContainerSlotRpc rpc,
        in ReceiveRpcCommandRequestComponent receive,
        in Entity entity) =>
        {
            commandBuffer.DestroyEntity(entity);
            var targetEntity = SystemAPI.GetComponent<CommandTargetComponent>(receive.SourceConnection).targetEntity;

            // Get the item in the hand slot
            var heldItem = SystemAPI.GetComponent<HandSlot>(targetEntity).item;

            // Get the target container
            if (!idToContainerEntity.TryGetValue(rpc.containerId, out var selectedContainerEntity))
            {
                return;
            }

            if (!containerLookup.TryGetBuffer(selectedContainerEntity, out var selectedContainer))
            {
                return;
            }

            // Get the index of the selected slot
            var selectedSlotIndex = -1;
            var selectedSlotItem = Entity.Null;
            for (var i = 0; i < selectedContainer.Length; i++)
            {
                var slot = selectedContainer[i];
                if (slot.id == rpc.slotId)
                {
                    selectedSlotIndex = i;
                    selectedSlotItem = slot.item;
                }
            }

            if (selectedSlotIndex == -1)
            {
                return;
            }

            // Check if the handSlotItem can be put in the target container / slot
            var heldItemRestriction = SystemAPI.GetComponent<ItemSlotRestriction>(heldItem).restriction;

            if (selectedContainer[selectedSlotIndex].metaData.restriction != heldItemRestriction)
            {
                return;
            }

            // If all checks pass, swap the items between the handSlot and the selected slot
            commandBuffer.SetComponent(targetEntity, new HandSlot
            {
                item = selectedSlotItem
            });

            selectedContainer[selectedSlotIndex] = new ContainerSlot
            {
                id = rpc.slotId,
                item = heldItem
            };

            // If the selected container is equipment, equip the item to it.
            if (SystemAPI.HasBuffer<EquippedTo>(selectedContainerEntity))
            {
                // Equip the new item
                commandBuffer.AppendToBuffer(selectedContainerEntity, new EquipStatStickRequest
                {
                    unequip = false,
                    statStick = heldItem
                });

                // Unequip the old item
                commandBuffer.AppendToBuffer(selectedContainerEntity, new EquipStatStickRequest
                {
                    unequip = true,
                    statStick = selectedSlotItem
                });
            }
        })
        .Run();
    }
}

public struct ItemSlotRestriction : IComponentData
{
    public SlotRestriction restriction;
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
    public uint containerId;
    public uint slotId;
}

[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)]
public struct ContainerSlot : IBufferElementData
{
    [GhostField]
    public uint id;
    [GhostField]
    public Entity item;
    [GhostField]
    public SlotMetaData metaData;
}

[GhostComponent]
public struct ContainerId : IComponentData
{
    [GhostField]
    public uint id;
}

public struct ContainerSetupTag : ICleanupComponentData { }

public struct SwapItemRequest : IBufferElementData
{
    public Entity toContainer;
    public uint toSlot;
}

[GhostComponent]
public struct HandSlot : IComponentData
{
    [GhostField]
    public Entity item;
}

public struct SlotMetaData
{
    public SlotRestriction restriction;
}

public enum SlotRestriction
{
    None,
    MainHand,
    OffHand,
    MainHandOrOffHand,
    Head,
    Chest,
    Hands,
    Feet,
    Neck,
    Waist,
    Ring,
    Ability,
    AbilityAugmnet
}

[GhostComponent]
public struct ContainerIcon : IComponentData
{
    [GhostField]
    public FixedString64Bytes name;
}

[GhostComponent]
public struct ContainerLabel : IComponentData
{
    [GhostField]
    public FixedString64Bytes label;
}