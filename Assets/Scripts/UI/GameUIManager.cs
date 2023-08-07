using MyUILibrary;
using System;
using System.Collections;
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
    /// Focus bar
    /// Experience bar
    /// Inventory screen
    /// Talent screen
    /// Menu screen

    public UIDocument gameUI;
    public VisualTreeAsset namePlateTemplate;
    public VisualTreeAsset focusBarTemplate;
    public VisualTreeAsset talentColumnTemplate;
    public VisualTreeAsset talentPlateTemplate;

    List<VisualElement> exclusiveGameScreens = new List<VisualElement>();
    VisualElement defaultGameScreen;
    VisualElement talentScreen;

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
    }

    private void Update()
    {
        if (_defaultActionsMap.TalentMenu.WasPressedThisFrame())
        {
            ToggleTalentScreen();
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

        var nameplateHolder = gameUI.rootVisualElement.Q<VisualElement>("game-screen");

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

        gameUI.rootVisualElement.Q<VisualElement>("game-screen").Add(focusBarInstance);
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
    VisualTreeAsset talentColumnTemplate;
    VisualTreeAsset talentPlateTemplate;

    PlatformerInputActions.GameplayMapActions _defaultActionsMap;

    Dictionary<int, VisualElement> columnsByLevel = new Dictionary<int, VisualElement>();
    Dictionary<Stat, TalentPlate> talents = new Dictionary<Stat, TalentPlate>();

    protected override void OnCreate()
    {
        PlatformerInputActions inputActions = new PlatformerInputActions();
        inputActions.Enable();
        inputActions.GameplayMap.Enable();
        _defaultActionsMap = inputActions.GameplayMap;
    }

    protected override void OnStartRunning()
    {
        var gameUIManager = UnityEngine.Object.FindObjectOfType<GameUIManager>();

        gameUI = gameUIManager.gameUI;
        talentColumnTemplate = gameUIManager.talentColumnTemplate;
        talentPlateTemplate = gameUIManager.talentPlateTemplate;

        var talentScreen = gameUI.rootVisualElement.Q<VisualElement>("talent-screen");
        var talentColumnParent = talentScreen.Q<VisualElement>("unity-content-container");

        var Talents = UnityEngine.Resources.LoadAll<TalentDefinition>("Talent definitions");

        foreach (var talent in Talents)
        {
            var talentPlateInstance = talentPlateTemplate.Instantiate();
            var talentPlate = talentPlateInstance.Q<TalentPlate>();
            talents.Add(talent.stat, talentPlate);
            talentPlate.Talent = talent;

            if (columnsByLevel.TryGetValue(talent.levelRequirement, out var talentColumn))
            {
                talentColumn.Q<VisualElement>("talent-parent").Add(talentPlateInstance);
            }
            else
            {
                var newTalentColumn = talentColumnTemplate.Instantiate();
                newTalentColumn.Q<Label>("level-label").text = talent.levelRequirement.ToString();
                talentColumnParent.Add(newTalentColumn);
                columnsByLevel.Add(talent.levelRequirement, newTalentColumn);
                newTalentColumn.Q<VisualElement>("talent-parent").Add(talentPlateInstance);
            }
        }
    }

    protected override void OnUpdate()
    {

    }
}