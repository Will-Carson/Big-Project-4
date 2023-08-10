using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[BurstCompile]
public partial struct ContainerServerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (request, rpc, rpcEntity) in SystemAPI
            .Query<RefRO<ClickItemRpc>, RefRO<ReceiveRpcCommandRequest>>()
            .WithEntityAccess())
        {
            commandBuffer.DestroyEntity(rpcEntity);

            var playerEntity = SystemAPI.GetComponent<CommandTarget>(rpc.ValueRO.SourceConnection).targetEntity;

            var clickedContainerEntity = Entity.Null;
            var clickedItemEntity = Entity.Null;
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
            if (clickedContainerEntity == Entity.Null ||
                clickedItemEntity == Entity.Null)
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
                var selectedContainerEntity = SystemAPI.GetComponent<ContainerParent>(selectedItem.entity).entity;

                // Check if the item meets the ContainerRestrictions
                var selectedItemRestrictions = SystemAPI.GetComponent<ItemRestrictions>(selectedItem.entity);
                var clickedItemRestrictions = SystemAPI.GetComponent<ItemRestrictions>(clickedItemEntity);

                var selectedContainerRestrictions = SystemAPI.GetComponent<ContainerRestrictions>(selectedContainerEntity).restrictions;
                var clickedContainerRestrictions = SystemAPI.GetComponent<ContainerRestrictions>(clickedItemEntity).restrictions;

                if (!selectedItemRestrictions.ItemMeetsRestrictions(clickedContainerRestrictions) ||
                    !clickedItemRestrictions.ItemMeetsRestrictions(selectedContainerRestrictions))
                {
                    continue;
                }
 
                var selectedItemContainer = SystemAPI.GetBuffer<ContainerChild>(selectedContainerEntity);
                var clickedItemContainer = SystemAPI.GetBuffer<ContainerChild>(clickedItemEntity);

                ContainerChild.TryGetIndexOfChild(selectedItemContainer, selectedItem.entity, out var selectedItemSlot);
                ContainerChild.TryGetIndexOfChild(clickedItemContainer, clickedItemEntity, out var clickedItemSlot);

                // Check if the item meets the slot specific restrictuions
                var selectedItemMeetsRestrictions = ContainerChildRestrictions.RestrictionsMet(
                    SystemAPI.GetBuffer<ContainerChildRestrictions>(clickedItemEntity),
                    clickedItemSlot,
                    selectedItemRestrictions);
                var clickedItemMeetsRestrictions = ContainerChildRestrictions.RestrictionsMet(
                    SystemAPI.GetBuffer<ContainerChildRestrictions>(selectedContainerEntity), 
                    selectedItemSlot, 
                    clickedItemRestrictions);

                if (!selectedItemMeetsRestrictions || !clickedItemMeetsRestrictions)
                {
                    continue;
                }

                // Check if the player has permissions for the item/container
                // TODO

                // Swap the items
                ContainerChild.PlaceItemInSlot(selectedItemContainer, selectedItemSlot, clickedItemEntity);
                ContainerChild.PlaceItemInSlot(clickedItemContainer, clickedItemSlot, selectedItem.entity);
                SystemAPI.SetComponent(selectedItem.entity, new ContainerParent { entity = clickedItemEntity });
                SystemAPI.SetComponent(clickedItemEntity, new ContainerParent { entity = selectedContainerEntity });

                // Set selectedItem back to null
                SystemAPI.SetComponent(playerEntity, new SelectedItem { entity = Entity.Null });
            }
        }
    }
}

public struct SelectedItem : IComponentData
{
    public Entity entity;
}

public struct Item : IComponentData { }

public struct ItemRestrictions : IComponentData
{
    public Restrictions restrictions;

    public bool ItemMeetsRestrictions(Restrictions otherRestrictions)
    {
        return (otherRestrictions & restrictions) == restrictions;
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
}

[GhostComponent]
public struct ContainerChild : IBufferElementData
{
    [GhostField] public Entity child;

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
        container.Insert(slot, new ContainerChild { child = newChild });
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

public struct ItemName : IComponentData
{
    public FixedString64Bytes name;
}