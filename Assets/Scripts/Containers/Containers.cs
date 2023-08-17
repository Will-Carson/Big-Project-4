using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[BurstCompile]
public partial struct ContainerServerSystem : ISystem
{
    ComponentLookup<ContainerRestrictions> containerRestrictionsLookup;
    ComponentLookup<ItemRestrictions> itemRestrictionsLookup;
    ComponentLookup<ContainerParent> containerParentLookup;
    ComponentLookup<EquipmentContainer> equipmentContainerLookup;
    BufferLookup<ContainerChildRestrictions> containerChildRestrictionsLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        containerRestrictionsLookup = SystemAPI.GetComponentLookup<ContainerRestrictions>(true);
        itemRestrictionsLookup = SystemAPI.GetComponentLookup<ItemRestrictions>(true);
        containerParentLookup = SystemAPI.GetComponentLookup<ContainerParent>();
        equipmentContainerLookup = SystemAPI.GetComponentLookup<EquipmentContainer>(true);
        containerChildRestrictionsLookup = SystemAPI.GetBufferLookup<ContainerChildRestrictions>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        containerRestrictionsLookup.Update(ref state);
        itemRestrictionsLookup.Update(ref state);
        containerParentLookup.Update(ref state);
        equipmentContainerLookup.Update(ref state);
        containerChildRestrictionsLookup.Update(ref state);

        foreach (var (request, rpc, rpcEntity) in SystemAPI
            .Query<RefRO<ClickItemRpc>, RefRO<ReceiveRpcCommandRequest>>()
            .WithEntityAccess())
        {
            commandBuffer.DestroyEntity(rpcEntity);

            var playerEntity = SystemAPI.GetComponent<CommandTarget>(rpc.ValueRO.SourceConnection).targetEntity;

            var clickedContainerEntity = Entity.Null;
            var clickedItemEntity = Entity.Null;
            var clickedItemSlot = request.ValueRO.slot;
            foreach (var (containerSessionId, children, entity) in SystemAPI
                .Query<RefRO<GhostInstance>, DynamicBuffer<ContainerChild>>()
                .WithAll<ContainerChild>()
                .WithEntityAccess())
            {
                if (containerSessionId.ValueRO.ghostId == request.ValueRO.containerSessionId.ghostId &&
                    containerSessionId.ValueRO.ghostType == request.ValueRO.containerSessionId.ghostType &&
                    containerSessionId.ValueRO.spawnTick == request.ValueRO.containerSessionId.spawnTick)
                {
                    clickedContainerEntity = entity;
                    clickedItemEntity = children.ElementAt(request.ValueRO.slot).child;
                    break;
                }
            }
            if (clickedContainerEntity == Entity.Null)
            {
                continue;
            }

            var selectedItem = SystemAPI.GetComponent<SelectedItem>(playerEntity);
            if (selectedItem.entity == Entity.Null)
            {
                selectedItem.entity = clickedItemEntity;
                SystemAPI.SetComponent(playerEntity, selectedItem);
            }
            else
            {
                // Set selectedItem back to null
                SystemAPI.SetComponent(playerEntity, new SelectedItem(Entity.Null));

                var selectedContainerEntity = SystemAPI.GetComponent<ContainerParent>(selectedItem.entity).entity;

                // Check if the item meets the ContainerRestrictions
                itemRestrictionsLookup.TryGetComponent(selectedItem.entity, out var selectedItemRestrictions);
                itemRestrictionsLookup.TryGetComponent(clickedItemEntity, out var clickedItemRestrictions);

                containerRestrictionsLookup.TryGetComponent(selectedContainerEntity, out var selectedContainerRestrictions);
                containerRestrictionsLookup.TryGetComponent(clickedContainerEntity, out var clickedContainerRestrictions);

                if (!selectedItemRestrictions.ItemMeetsRestrictions(clickedContainerRestrictions.restrictions) ||
                    !clickedItemRestrictions.ItemMeetsRestrictions(selectedContainerRestrictions.restrictions))
                {
                    continue;
                }
 
                var selectedItemContainer = SystemAPI.GetBuffer<ContainerChild>(selectedContainerEntity);
                var clickedItemContainer = SystemAPI.GetBuffer<ContainerChild>(clickedContainerEntity);

                ContainerChild.TryGetIndexOfChild(selectedItemContainer, selectedItem.entity, out var selectedItemSlot);

                containerChildRestrictionsLookup.TryGetBuffer(selectedContainerEntity, out var selectedContainerChildRestrictions);
                containerChildRestrictionsLookup.TryGetBuffer(clickedContainerEntity, out var clickedContainerChildRestrictions);

                // Check if the item meets the slot specific restrictuions
                var selectedItemMeetsRestrictions = ContainerChildRestrictions.RestrictionsMet(
                    clickedContainerChildRestrictions,
                    clickedItemSlot,
                    selectedItemRestrictions);
                var clickedItemMeetsRestrictions = ContainerChildRestrictions.RestrictionsMet(
                    selectedContainerChildRestrictions, 
                    selectedItemSlot, 
                    clickedItemRestrictions);

                if (!selectedItemMeetsRestrictions || !clickedItemMeetsRestrictions)
                {
                    continue;
                }

                // Check if the player has permissions for the item/container
                // TODO

                var selectedItemStatEntity = default(StatEntity);
                var clickedItemStatEntity = default(StatEntity);

                if (selectedItem.entity != Entity.Null) selectedItemStatEntity = SystemAPI.GetAspect<StatEntity>(selectedItem.entity);
                if (clickedItemEntity != Entity.Null) clickedItemStatEntity = SystemAPI.GetAspect<StatEntity>(clickedItemEntity);

                // Check if the items can be put in their resepective slots and exit early if they cannot.
                if (equipmentContainerLookup.TryGetComponent(selectedContainerEntity, out var selectedEquipmentContainer))
                {
                    var equipmentTarget = selectedEquipmentContainer.target;
                    var targetStatEntity = SystemAPI.GetAspect<StatEntity>(equipmentTarget);

                    if (targetStatEntity.IsEquipped(clickedItemStatEntity))
                    {
                        continue;
                    }
                }

                if (equipmentContainerLookup.TryGetComponent(clickedContainerEntity, out var clickedEquipmentContainer))
                {
                    var equipmentTarget = clickedEquipmentContainer.target;
                    var targetStatEntity = SystemAPI.GetAspect<StatEntity>(equipmentTarget);

                    if (targetStatEntity.IsEquipped(selectedItemStatEntity))
                    {
                        continue;
                    }
                }

                if (equipmentContainerLookup.HasComponent(selectedContainerEntity))
                {
                    var equipmentTarget = selectedEquipmentContainer.target;
                    var targetStatEntity = SystemAPI.GetAspect<StatEntity>(equipmentTarget);

                    targetStatEntity.TryEquipUniqueStatStick(clickedItemStatEntity);
                    targetStatEntity.TryUnequipStatStick(selectedItemStatEntity);
                }

                if (equipmentContainerLookup.HasComponent(clickedContainerEntity))
                {
                    var equipmentTarget = clickedEquipmentContainer.target;
                    var targetStatEntity = SystemAPI.GetAspect<StatEntity>(equipmentTarget);

                    targetStatEntity.TryEquipUniqueStatStick(selectedItemStatEntity);
                    targetStatEntity.TryUnequipStatStick(clickedItemStatEntity);
                }

                // Swap the items
                ContainerChild.PlaceItemInSlot(selectedItemContainer, selectedItemSlot, clickedItemEntity);
                ContainerChild.PlaceItemInSlot(clickedItemContainer, clickedItemSlot, selectedItem.entity);

                // These could just be Entity.Null checks...
                if (containerParentLookup.HasComponent(selectedItem.entity))
                {
                    containerParentLookup[selectedItem.entity] = new ContainerParent(clickedContainerEntity);
                }
                if (containerParentLookup.HasComponent(clickedItemEntity))
                {
                    containerParentLookup[clickedItemEntity] = new ContainerParent(selectedContainerEntity);
                }
            }
        }
    }
}

public struct SelectedItem : IComponentData
{
    public Entity entity;

    public SelectedItem(Entity entity) : this()
    {
        this.entity = entity;
    }
}

public struct Item : IComponentData { }

public struct ItemRestrictions : IComponentData
{
    public Restrictions restrictions;

    public ItemRestrictions(Restrictions restrictions) : this()
    {
        this.restrictions = restrictions;
    }

    public bool ItemMeetsRestrictions(Restrictions otherRestrictions)
    {
        return (otherRestrictions == Restrictions.None) ? true : (otherRestrictions & restrictions) == restrictions;
    }
}

public struct ClickItemRpc : IRpcCommand
{
    public GhostInstance containerSessionId;
    public int slot;
}

public struct ContainerParent : IComponentData
{
    public Entity entity;

    public ContainerParent(Entity entity) : this()
    {
        this.entity = entity;
    }
}

[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)]
public struct ContainerChild : IBufferElementData
{
    [GhostField] public Entity child;

    public ContainerChild(Entity child) : this()
    {
        this.child = child;
    }

    public static bool TryGetIndexOfChild(DynamicBuffer<ContainerChild> container, Entity child, out int index)
    {
        index = 0;
        for (var i = 0; i < container.Length; i++)
        {
            var c = container[i];
            if (c.child == child)
            {
                index = i;
                return true;
            }
        }

        return false;
    }

    public static void PlaceItemInSlot(DynamicBuffer<ContainerChild> container, int slot, Entity newChild)
    {
        container[slot] = new ContainerChild { child = newChild };
    }
}

public struct ContainerChildRestrictions : IBufferElementData
{
    public Restrictions restrictions;

    public ContainerChildRestrictions(Restrictions restrictions) : this()
    {
        this.restrictions = restrictions;
    }

    public static bool RestrictionsMet(DynamicBuffer<ContainerChildRestrictions> containerChildRestrictions, int index, ItemRestrictions itemRestrictions)
    {
        if (!containerChildRestrictions.IsCreated || containerChildRestrictions.Length < index + 1) return true;
        var restrictionsAtIndex = containerChildRestrictions.ElementAt(index).restrictions;
        return itemRestrictions.ItemMeetsRestrictions(restrictionsAtIndex);
    }
}

public struct ContainerRestrictions : IComponentData
{
    public Restrictions restrictions;
}

[Flags]
public enum Restrictions
{
    None = 0,
    Helm = 1,
    Body = 1 << 1,
    Belt = 1 << 2,
    Boots = 1 << 3,
    Gloves = 1 << 4,
    HainHand = 1 << 5,
    OffHand = 1 << 6,
    Amulet = 1 << 7,
    LeftRing = 1 << 8,
    RightRing = 1 << 9,
}

[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)]
public struct ItemData : IComponentData
{
    [GhostField] public FixedString128Bytes name;
    [GhostField] public FixedString512Bytes description;
    [GhostField] public FixedString128Bytes artAddress2d;
    [GhostField] public FixedString128Bytes artAddress3d;
}

public struct EquipmentContainer : IComponentData 
{
    public Entity target;

    public EquipmentContainer(Entity target) : this()
    {
        this.target = target;
    }
}