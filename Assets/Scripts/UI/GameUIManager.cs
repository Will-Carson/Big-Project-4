using MyUILibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

public class GameUIManager : MonoBehaviour
{
    /// X Health bar
    /// X Focus bar
    /// Experience bar
    /// Inventory screen
    /// X Talent screen
    /// Menu screen

    public UIDocument gameUI;
    public VisualTreeAsset namePlateTemplate;
    public VisualTreeAsset focusBarTemplate;
    public VisualTreeAsset talentColumnTemplate;
    public VisualTreeAsset talentPlateTemplate;

    List<VisualElement> exclusiveGameScreens = new List<VisualElement>();
    VisualElement defaultGameScreen;
    VisualElement talentScreen;

    VisualElement rightPanel;
    List<VisualElement> exclusiveRightPanels = new List<VisualElement>();
    VisualElement inventoryPanel;

    VisualElement leftPanel;
    List<VisualElement> exclusiveLeftPanels = new List<VisualElement>();

    PlatformerInputActions.GameplayMapActions _defaultActionsMap;

    private void Start()
    {
        PlatformerInputActions inputActions = new PlatformerInputActions();
        inputActions.Enable();
        inputActions.GameplayMap.Enable();
        _defaultActionsMap = inputActions.GameplayMap;

        defaultGameScreen = gameUI.rootVisualElement.Q<VisualElement>("game-screen");
        talentScreen = gameUI.rootVisualElement.Q<VisualElement>("talent-screen");
        exclusiveGameScreens.Add(defaultGameScreen);
        exclusiveGameScreens.Add(talentScreen);

        rightPanel = gameUI.rootVisualElement.Q<VisualElement>("right-panel");
        inventoryPanel = gameUI.rootVisualElement.Q<VisualElement>("inventory-panel");
        exclusiveRightPanels.Add(inventoryPanel);

        leftPanel = gameUI.rootVisualElement.Q<VisualElement>("left-panel");
    }

    private void Update()
    {
        if (_defaultActionsMap.TalentMenu.WasPressedThisFrame())
        {
            ToggleTalentScreen();
        }
        if (_defaultActionsMap.Inventory.WasPressedThisFrame())
        {
            ToggleInventoryScreen();
        }
    }

    public void ToggleDefaultScreen(bool activate)
    {
        foreach (var s in exclusiveGameScreens)
        {
            if (s == defaultGameScreen)
            {
                s.style.display = (activate) ? DisplayStyle.Flex : DisplayStyle.None;
                //s.style.display = DisplayStyle.Flex;
            }
            else
            {
                s.style.display = DisplayStyle.None;
            }
        }
    }

    public void ToggleTalentScreen()
    {
        foreach (var s in exclusiveGameScreens)
        {
            if (s == talentScreen)
            {
                s.style.display = (s.style.display == DisplayStyle.Flex) ? DisplayStyle.None : DisplayStyle.Flex;
            }
            else
            {
                s.style.display = DisplayStyle.None;
            }
        }

        if (talentScreen.style.display == DisplayStyle.None)
        {
            defaultGameScreen.style.display = DisplayStyle.Flex;
        }
    }

    public void ToggleInventoryScreen()
    {
        foreach (var p in exclusiveRightPanels)
        {
            if (p == inventoryPanel)
            {
                //ToggleDefaultScreen(p.style.display == DisplayStyle.Flex);
                p.style.display = (p.style.display == DisplayStyle.Flex) ? DisplayStyle.None : DisplayStyle.Flex;
                rightPanel.style.display = p.style.display;
            }
            else
            {
                p.style.display = DisplayStyle.None;
            }
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class NameplateManagerSystem : SystemBase
{
    UIDocument gameUI;
    VisualTreeAsset namePlateTemplate;

    protected override void OnStartRunning()
    {
        var gm = UnityEngine.Object.FindObjectOfType<GameUIManager>();

        gameUI = gm.gameUI;
        namePlateTemplate = gm.namePlateTemplate;
    }

    protected override void OnUpdate()
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        var localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);

        var nameplateHolder = gameUI.rootVisualElement.Q<VisualElement>("world-space");

        foreach (var localToWorld in SystemAPI.Query<LocalToWorld>())
        {
            break;
        }

        foreach (var (health, entity) in SystemAPI.Query<RefRO<Health>>().WithEntityAccess().WithNone<NamePlateComponent>())
        {
            VisualElement healthBarInstance = namePlateTemplate.Instantiate();
            nameplateHolder.Add(healthBarInstance);

            commandBuffer.AddComponent(entity, new NamePlateComponent
            {
                namePlate = healthBarInstance.Q<NamePlate>("name-plate")
            });
        }

        foreach (var (health, healthUIElement, namePlate) in SystemAPI.Query<RefRO<Health>, NamePlateComponent, RefRO<NamePlateTargetEntity>>())
        {
            healthUIElement.SetHealth(health.ValueRO);

            localToWorldLookup.TryGetComponent(namePlate.ValueRO.entity, out var namePlateTransform);
            MoveElementToWorldPosition(healthUIElement.namePlate.parent, namePlateTransform.Position);
        }
    }

    public void MoveElementToWorldPosition(VisualElement element, float3 worldPosition)
    {
        var screen = Camera.main.WorldToScreenPoint(worldPosition);
        element.style.left = screen.x - (element.layout.width / 2);
        element.style.top = (Screen.height - screen.y) - 100;
    }
}

public class NamePlateComponent : IComponentData, IDisposable 
{ 
    public NamePlate namePlate;

    public void Dispose()
    {
        namePlate.RemoveFromHierarchy();
    }

    public void SetHealth(Health health)
    {
        namePlate.MaximumHealth = health.maxHealth;
        namePlate.CurrentHealth = health.currentHealth;
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class FocusBarManagerSystem : SystemBase
{
    UIDocument gameUI;
    RadialProgress focusBar;

    protected override void OnStartRunning()
    {
        var gm = UnityEngine.Object.FindObjectOfType<GameUIManager>();

        gameUI = gm.gameUI;

        var focusBarInstance = gm.focusBarTemplate.Instantiate();
        focusBar = focusBarInstance.Q<RadialProgress>();

        gameUI.rootVisualElement.Q<VisualElement>("world-space").Add(focusBarInstance);
    }

    float currentFocus = 0;
    protected override void OnUpdate()
    {
        var focus = default(Focus);
        foreach (var focusRef in SystemAPI.Query<RefRO<Focus>>().WithAll<GhostOwnerIsLocal>())
        {
            focus = focusRef.ValueRO;
            break;
        }

        var mousePosition = RuntimePanelUtils.ScreenToPanel(gameUI.rootVisualElement.panel, Input.mousePosition);
        var mousePositionCorrected = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
        mousePositionCorrected = RuntimePanelUtils.ScreenToPanel(gameUI.rootVisualElement.panel, mousePositionCorrected);
        focusBar.transform.position = mousePositionCorrected;

        focusBar.progress = focus.Percentage() * 100;
        focusBar.progress = currentFocus;
        currentFocus += .1f;
        if (currentFocus >= 100) currentFocus = 0;
    }
}

public struct Focus : IComponentData
{
    public float currentFocus;
    public float maxFocus;

    public float Percentage()
    {
        return (maxFocus == 0) ? 1 : currentFocus / maxFocus;
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class TalentUIManager : SystemBase
{
    UIDocument gameUI;
    TalentScreen talentScreen;
    List<TalentAllocationRequestRpc> requests = new List<TalentAllocationRequestRpc>();

    protected override void OnStartRunning()
    {
        var gameUIManager = UnityEngine.Object.FindObjectOfType<GameUIManager>();

        gameUI = gameUIManager.gameUI;

        talentScreen = gameUI.rootVisualElement.Q<TalentScreen>();
        var talents = UnityEngine.Resources.LoadAll<TalentDefinition>("Talent definitions");
        talentScreen.BuildTalentScreen(talents, gameUIManager.talentColumnTemplate, gameUIManager.talentPlateTemplate);
        talentScreen.talentClicked += TalentScreen_talentClicked;
    }

    private void TalentScreen_talentClicked(TalentAllocationRequestRpc request)
    {
        requests.Add(request);
    }

    protected override void OnUpdate()
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        foreach (var request in requests)
        {
            var entity = commandBuffer.CreateEntity();
            commandBuffer.AddComponent<SendRpcCommandRequest>(entity);
            commandBuffer.AddComponent(entity, request);
        }
        requests.Clear();

        // TODO runs every frame...
        foreach (var stats in SystemAPI.Query<DynamicBuffer<StatElement>>()
            .WithAll<GhostOwnerIsLocal, PlatformerCharacterComponent>())
        {
            talentScreen.OnStatsChange(stats);
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class InventoryUIManager : SystemBase
{
    ClientContainer inventory;
    ClientContainer equipment;

    private void Container_clicked(int index, GhostInstance containerSessionId)
    {
        Debug.Log($"{index} // {containerSessionId}");
    }

    protected override void OnUpdate()
    {
        if (inventory == null) // For now this will work fine
        {
            foreach (var container in SystemAPI.Query<DynamicBuffer<ContainerChild>>().WithAll<GhostOwnerIsLocal, PlatformerPlayer>())
            {
                var gameUIManager = UnityEngine.Object.FindObjectOfType<GameUIManager>();
                var gameUI = gameUIManager.gameUI;
                var inventoryPanel = gameUI.rootVisualElement.Q<VisualElement>("inventory-panel");

                var inventoryEntity = container[0].child;
                var inventorySessionId = SystemAPI.GetComponent<GhostInstance>(inventoryEntity);
                var inventorySlotIds = new string[16];
                for (var i = 0; i < inventorySlotIds.Length; i++)
                {
                    inventorySlotIds[i] = $"item-slot-{i}";
                }

                var equipmentEntity = container[1].child;
                var equipmentSessionId = SystemAPI.GetComponent<GhostInstance>(equipmentEntity);
                string[] equipmentSlotIds = new string[]
                {
                    "helm-slot",
                    "body-slot",
                    "belt-slot",
                    "boots-slot",
                    "gloves-slot",
                    "main-hand-slot",
                    "off-hand-slot",
                    "amulet-slot",
                    "left-ring-slot",
                    "right-ring-slot",
                };

                inventory = new ClientContainer(inventorySessionId, inventoryPanel, inventorySlotIds);
                equipment = new ClientContainer(equipmentSessionId, inventoryPanel, equipmentSlotIds);

                inventory.clicked += Container_clicked;
                equipment.clicked += Container_clicked;
            }

            Debug.Log("Containers set up.");
        }

        foreach (var container in SystemAPI.Query<DynamicBuffer<ContainerChild>>().WithAll<GhostOwnerIsLocal, PlatformerPlayer>())
        {
            var inventoryEntity = container[0].child;
            var equipmentEntity = container[1].child;
            var inventoryContainer = SystemAPI.GetBuffer<ContainerChild>(inventoryEntity);
            var equipmentContainer = SystemAPI.GetBuffer<ContainerChild>(equipmentEntity);

            UpdateContainer(inventory, inventoryContainer);
            UpdateContainer(equipment, equipmentContainer);
        }
    }

    public void UpdateContainer(ClientContainer clientContainer, DynamicBuffer<ContainerChild> container)
    {
        for (var i = 0; i < container.Length; i++)
        {
            var child = container[i].child;
            var plate = clientContainer.containerSlots[i];
            UpdateItemPlate(plate, child);
        }
    }

    public void UpdateItemPlate(ClientItemPlate itemPlate, Entity entity)
    {
        var item = itemPlate.item;
        //item.name = SystemAPI.GetComponent<ItemName>(entity).name.ToString();
    }
}

public class ClientContainer
{
    public Action<int, GhostInstance> clicked;
    public GhostInstance containerSessionId;
    public Dictionary<int, ClientItemPlate> containerSlots = new Dictionary<int, ClientItemPlate>();

    public ClientContainer(GhostInstance containerSessionId, VisualElement slotParent, string[] slotIds)
    {
        this.containerSessionId = containerSessionId;
        for (var i = 0; i < slotIds.Length; i++)
        {
            var slotId = slotIds[i];
            var button = slotParent.Q<VisualElement>(slotId).Q<Button>();
            var containerSlotPlate = new ClientItemPlate(button, i);
            containerSlotPlate.clicked += ContainerSlotPlate_clicked;
            containerSlots.Add(i, containerSlotPlate);
        }
    }

    private void ContainerSlotPlate_clicked(int slot)
    {
        clicked.Invoke(slot, containerSessionId);
    }
}

public class ClientItemPlate
{
    public Action<int> clicked;
    public int index;
    public ItemData item;
    Button button;

    public ClientItemPlate(Button button, int index)
    {
        this.button = button;
        this.index = index;
        button.clicked += Button_clicked;
    }

    private void Button_clicked()
    {
        clicked.Invoke(index);
    }
}

public class ItemData
{
    public string name;
}