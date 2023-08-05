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
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class HealthBarManagerSystem : SystemBase
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

        var nameplateHolder = gameUI.rootVisualElement.Q<VisualElement>("nameplate-holder");

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
            MoveElementToWorldPosition(healthUIElement.namePlate.parent, namePlateTransform.Position, new float2(1, 1));
        }
    }

    public void MoveElementToWorldPosition(VisualElement element, float3 worldPosition, float2 worldSize)
    {
        Rect rect = RuntimePanelUtils.CameraTransformWorldToPanelRect(element.panel, worldPosition, worldSize, Camera.main);
        //Vector2 layoutSize = element.layout.size;

        // Don't set scale to 0 or a negative number.
        //Vector2 scale = layoutSize.x > 0 && layoutSize.y > 0 ? rect.size / layoutSize : Vector2.one * 1e-5f;

        element.transform.position = rect.position;
        //element.transform.scale = new Vector3(scale.x, scale.y, 1);
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

        //focusBar = new RadialProgress()
        //{
        //    style = {
        //        color = Color.yellow,
        //        position = Position.Absolute,
        //        left = 20, top = 20, width = 40, height = 40
        //    }
        //};

        var focusBarInstance = gm.focusBarTemplate.Instantiate();
        focusBar = focusBarInstance.Q<RadialProgress>();

        //var label = focusBar.Q<Label>();
        //label.style.opacity = 0;

        gameUI.rootVisualElement.Q<VisualElement>("nameplate-holder").Add(focusBarInstance);
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