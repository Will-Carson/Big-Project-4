using MyUILibrary;
using System;
using System.Collections.Generic;
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
    /// X Inventory screen
    /// X Talent screen
    /// Anvil screen
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
        namePlate.MaximumHealth = health.max;
        namePlate.CurrentHealth = health.current;
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
    TalentScreen talentScreen;
    List<TalentAllocationRequestRpc> requests = new List<TalentAllocationRequestRpc>();

    protected override void OnStartRunning()
    {
        var gameUIManager = UnityEngine.Object.FindObjectOfType<GameUIManager>();

        var gameUI = gameUIManager.gameUI;

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
        foreach (var statEntity in SystemAPI.Query<StatEntity>()
            .WithAll<GhostOwnerIsLocal, PlatformerCharacterComponent>())
        {
            talentScreen.OnStatsChange(statEntity);
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class InventoryUIManager : SystemBase
{
    ClientContainer inventory;
    ClientContainer equipment;
    ClientContainer anvil;

    ComponentLookup<ItemData> itemDataLookup;

    protected override void OnCreate()
    {
        itemDataLookup = GetComponentLookup<ItemData>();
    }

    private void Container_clicked(int index, GhostInstance containerSessionId)
    {
        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        var entity = commandBuffer.CreateEntity();
        commandBuffer.AddComponent<SendRpcCommandRequest>(entity);
        commandBuffer.AddComponent(entity, new ClickItemRpc { slot = index, containerSessionId = containerSessionId });
    }

    protected override void OnUpdate()
    {
        itemDataLookup.Update(this);

        if (inventory == null) // For now this will work fine
        {
            foreach (var container in SystemAPI.Query<DynamicBuffer<ContainerChild>>().WithAll<GhostOwnerIsLocal, PlatformerPlayer>())
            {
                var gameUIManager = UnityEngine.Object.FindObjectOfType<GameUIManager>();
                var gameUI = gameUIManager.gameUI;
                var inventoryPanel = gameUI.rootVisualElement.Q<VisualElement>("inventory-panel");

                var inventoryEntity = container[0].child;
                if (inventoryEntity == Entity.Null) return;
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

                var anvilEntity = container[2].child;
                var anvilSessionId = SystemAPI.GetComponent<GhostInstance>(anvilEntity);
                string[] anvilSlotIds = new string[] { "anvil-slot" };
                var anvilPanel = gameUI.rootVisualElement.Q<VisualElement>("anvil-panel");

                inventory = new ClientContainer(inventorySessionId, inventoryPanel, inventorySlotIds);
                equipment = new ClientContainer(equipmentSessionId, inventoryPanel, equipmentSlotIds);
                anvil = new ClientContainer(anvilSessionId, anvilPanel, anvilSlotIds);

                inventory.clicked += Container_clicked;
                equipment.clicked += Container_clicked;
                anvil.clicked += Container_clicked;
            }

            Debug.Log("Containers set up.");
        }

        foreach (var container in SystemAPI.Query<DynamicBuffer<ContainerChild>>().WithAll<GhostOwnerIsLocal, PlatformerPlayer>())
        {
            var inventoryEntity = container[0].child;
            var equipmentEntity = container[1].child;
            var anvilEntity = container[2].child;
            var inventoryContainer = SystemAPI.GetBuffer<ContainerChild>(inventoryEntity);
            var equipmentContainer = SystemAPI.GetBuffer<ContainerChild>(equipmentEntity);
            var anvilContainer = SystemAPI.GetBuffer<ContainerChild>(anvilEntity);

            UpdateContainer(inventory, inventoryContainer);
            UpdateContainer(equipment, equipmentContainer);
            UpdateContainer(anvil, anvilContainer);
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
        itemDataLookup.TryGetComponent(entity, out var itemData);

        itemPlate.ItemData = itemData;
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
            var slot = slotParent.Q<VisualElement>(slotId);
            var containerSlotPlate = new ClientItemPlate(slot, i);
            containerSlotPlate.clicked += ContainerSlotPlate_clicked;
            containerSlots.Add(i, containerSlotPlate);
        }
    }

    private void ContainerSlotPlate_clicked(int slot)
    {
        clicked.Invoke(slot, containerSessionId);
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public class ClientItemPlate
{
    public static event EventHandler<ItemData> MouseOver;
    public static event EventHandler MouseOut;
    public Action<int> clicked;
    public int index;

    VisualElement visualElement;

    public ClientItemPlate(VisualElement visualElement, int index)
    {
        this.visualElement = visualElement;
        this.index = index;

        var button = visualElement.Q<Button>();
        button.clicked += Button_clicked;
        button.RegisterCallback<MouseOverEvent>((_) => { MouseOver?.Invoke(this, itemData); });
        button.RegisterCallback<MouseOutEvent>((_) => { MouseOut?.Invoke(this, null); });
    }

    ItemData itemData;
    public ItemData ItemData
    {
        get => itemData;
        set
        {
            if (itemData.Equals(value))
                return;

            itemData = value;

            SpriteAddress = value.artAddress2d.ToString();
        }
    }

    string spriteAddress;
    string SpriteAddress
    {
        get => spriteAddress;
        set
        {
            if (spriteAddress == value)
                return;

            spriteAddress = value;

            if (value.Length > 0)
            {
                var itemArt = Resources.Load<Sprite>($"Sprites/{value}");
                visualElement.Q<VisualElement>("portrait").style.backgroundImage = new StyleBackground(itemArt);
            }
            else
            {
                visualElement.Q<VisualElement>("portrait").style.backgroundImage = new StyleBackground(StyleKeyword.None);
            }
        }
    }

    private void Button_clicked()
    {
        clicked.Invoke(index);
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class TooltipUIManager : SystemBase
{
    TooltipPlate leftTooltipPlate;
    TooltipPlate rightTooltipPlate;

    protected override void OnStartRunning()
    {
        var gameUIManager = UnityEngine.Object.FindObjectOfType<GameUIManager>();
        var gameUI = gameUIManager.gameUI;

        ClientItemPlate.MouseOver += ClientItemPlate_MouseOver;
        ClientItemPlate.MouseOut += ClientItemPlate_MouseOut;
        leftTooltipPlate = new TooltipPlate(gameUI.rootVisualElement.Q<VisualElement>("tooltip-left"));
        rightTooltipPlate = new TooltipPlate(gameUI.rootVisualElement.Q<VisualElement>("tooltip-right"));
    }

    private void ClientItemPlate_MouseOver(object sender, ItemData itemData)
    {
        leftTooltipPlate.ItemData = default(ItemData);
        rightTooltipPlate.ItemData = itemData;
    }

    private void ClientItemPlate_MouseOut(object sender, EventArgs _)
    {
        leftTooltipPlate.ItemData = default(ItemData);
        rightTooltipPlate.ItemData = default(ItemData);
    }

    protected override void OnUpdate()
    {

    }
}

public class TooltipPlate
{
    VisualElement tooltipElement;
    Label name;
    Label baseItem;
    Label baseStats;
    Label requirements;
    Label stats;
    Label description;

    public TooltipPlate(VisualElement tooltipElement)
    {
        this.tooltipElement = tooltipElement;
        name = tooltipElement.Q<Label>("name");
        baseItem = tooltipElement.Q<Label>("base-item");
        baseStats = tooltipElement.Q<Label>("base-stats");
        requirements = tooltipElement.Q<Label>("requirements");
        stats = tooltipElement.Q<Label>("stats");
        description = tooltipElement.Q<Label>("description");

        tooltipElement.style.display = DisplayStyle.None;
    }

    ItemData itemData;
    public ItemData ItemData
    {
        get => itemData;
        set
        {
            if (itemData.Equals(value))
                return;

            itemData = value;
            Display(value.name != "");

            name.text = value.name.ToString();
            description.text = value.description.ToString();
        }
    }

    private void Display(bool display)
    {
        if (display)
        {
            tooltipElement.style.display = DisplayStyle.Flex;
        }
        else
        {
            tooltipElement.style.display = DisplayStyle.None;
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class AnvilUIManager : SystemBase
{
    private VisualElement effectPlateParent;
    private Button applyEffectButton;
    private VisualElement augmentParent;
    private ClientItemPlate anvilSlot;

    protected override void OnStartRunning()
    {
        //var gameUIManager = UnityEngine.Object.FindObjectOfType<GameUIManager>();
        //var gameUI = gameUIManager.gameUI;
        //var anvilPanel = gameUI.rootVisualElement.Q<VisualElement>("anvil-panel");

        //effectPlateParent = anvilPanel.Q<VisualElement>("effect-plate-parent");
        //applyEffectButton = anvilPanel.Q<Button>("apply-effect-button");
        //augmentParent = anvilPanel.Q<VisualElement>("augment-parent");
        //var anvilSlotElement = anvilPanel.Q<VisualElement>("anvil-slot");
        //anvilSlot = new ClientItemPlate(anvilSlotElement, 0);
    }

    protected override void OnUpdate()
    {
        
    }
}