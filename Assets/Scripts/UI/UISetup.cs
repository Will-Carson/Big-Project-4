using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UIElements;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class UISetup : SystemBase
{
    Dictionary<Button, string> tooltips = new Dictionary<Button, string>();

    NativeList<TalentAllocationRequestRpc> talentRpcs = new NativeList<TalentAllocationRequestRpc>(Allocator.Persistent);
    NativeList<PressContainerSlotRpc> containerRpcs = new NativeList<PressContainerSlotRpc>(Allocator.Persistent);

    protected override void OnDestroy()
    {
        talentRpcs.Dispose();
        containerRpcs.Dispose();
    }

    protected override void OnUpdate()
    {
        Setup();

        var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        for (int i = 0; i < talentRpcs.Length; i++)
        {
            var rpc = talentRpcs[i];
            var rpcEntity = commandBuffer.CreateEntity();
            commandBuffer.AddComponent<SendRpcCommandRequestComponent>(rpcEntity);
            commandBuffer.AddComponent(rpcEntity, rpc);
        }
        talentRpcs.Clear();

        for (int i = 0; i < containerRpcs.Length; i++)
        {
            var rpc = containerRpcs[i];
            var rpcEntity = commandBuffer.CreateEntity();
            commandBuffer.AddComponent<SendRpcCommandRequestComponent>(rpcEntity);
            commandBuffer.AddComponent(rpcEntity, rpc);
        }
        containerRpcs.Clear();

        var uiDocument = Object.FindObjectOfType<UIDocument>();
        Entities
        .WithAll<LocalPlayerTag>()
        .ForEach((
        in Entity entity,
        in DynamicBuffer<ContainerSlot> rootContainer) =>
        {
            // Destroy current container stuff
            // Build out new container stuff
            // Link rpcs to buttons

            /// FUCK
            /// iterate over containers
            /// match each container id to an object
            /// that object manages updating and displaying its container
            /// 

            for (var i = 0; i < rootContainer.Length; i++)
            {
                var subContainer = rootContainer[i];

                if (subContainer.id == 0)
                {
                    new ItemDisplay(uiDocument.rootVisualElement.Q("inventory"));
                }
                if (subContainer.id == 1)
                {
                    new ItemDisplay(uiDocument.rootVisualElement.Q("equipment"));
                }
                if (subContainer.id == 2)
                {
                    new ItemDisplay(uiDocument.rootVisualElement.Q("abilities"));
                }
                if (subContainer.id == 3)
                {
                    new ItemDisplay(uiDocument.rootVisualElement.Q("foreign"));
                }
            }
        })
        .WithoutBurst()
        .Run();
    }

    bool isSetup = false;
    public void Setup()
    {
        if (isSetup) return;

        MainMenuSetup();
        ContainerUIElementSetup();
        TalentUIElementSetup();

        isSetup = true;
    }

    private void MainMenuSetup()
    {
        var mainMenuRoot = Object.FindObjectOfType<UIDocument>().rootVisualElement.Q("main-menu-root");

        var initalScreen = mainMenuRoot.Q("initial-screen");
        var createAccountScreen = mainMenuRoot.Q("create-account-screen");
        var loginScreen = mainMenuRoot.Q("login-screen");

        // Setup initial screen
        {
            var createAccountButton = initalScreen.Q<Button>("create-account-button");
            var loginButton = initalScreen.Q<Button>("login-button");
            var exitButton = initalScreen.Q<Button>("exit-button");

            createAccountButton.RegisterCallback<MouseUpEvent>(e =>
            {
                initalScreen.style.display = DisplayStyle.None;
                createAccountScreen.style.display = DisplayStyle.Flex;
            }, TrickleDown.NoTrickleDown);

            loginButton.RegisterCallback<MouseUpEvent>(e =>
            {
                initalScreen.style.display = DisplayStyle.None;
                loginScreen.style.display = DisplayStyle.Flex;
            }, TrickleDown.NoTrickleDown);

            exitButton.RegisterCallback<MouseUpEvent>(e =>
            {
                Application.Quit();
            }, TrickleDown.NoTrickleDown);
        }

        // Setup create account screen
        {
            var usernameTextField = createAccountScreen.Q<TextField>("username-textfield");
            var emailTextField = createAccountScreen.Q<TextField>("email-textfield");
            var passwordTextField = createAccountScreen.Q<TextField>("password-textfield");
            var verifyPasswordTextField = createAccountScreen.Q<TextField>("verify-password-textfield");
            var createAccountButton = createAccountScreen.Q<Button>("create-account-button");
            var backButton = createAccountScreen.Q<Button>("back-button");
            var exitButton = createAccountScreen.Q<Button>("exit-button");

            createAccountButton.RegisterCallback<MouseUpEvent>(e =>
            {
                // Create the user account here

                createAccountScreen.style.display = DisplayStyle.None;
                loginScreen.style.display = DisplayStyle.Flex;
            }, TrickleDown.NoTrickleDown);

            backButton.RegisterCallback<MouseUpEvent>(e =>
            {
                createAccountScreen.style.display = DisplayStyle.None;
                initalScreen.style.display = DisplayStyle.Flex;
            }, TrickleDown.NoTrickleDown);

            exitButton.RegisterCallback<MouseUpEvent>(e =>
            {
                Application.Quit();
            }, TrickleDown.NoTrickleDown);
        }

        // Setup login screen
        {
            var usernameTextField = loginScreen.Q<TextField>("username-textfield");
            var passwordTextField = loginScreen.Q<TextField>("password-textfield");
            var loginButton = loginScreen.Q<Button>("login-button");
            var backButton = loginScreen.Q<Button>("back-button");
            var exitButton = loginScreen.Q<Button>("exit-button");

            loginButton.RegisterCallback<MouseUpEvent>(e =>
            {
                // Log the player in here

                mainMenuRoot.style.display = DisplayStyle.None;
            }, TrickleDown.NoTrickleDown);

            backButton.RegisterCallback<MouseUpEvent>(e =>
            {
                loginScreen.style.display = DisplayStyle.None;
                initalScreen.style.display = DisplayStyle.Flex;
            }, TrickleDown.NoTrickleDown);

            exitButton.RegisterCallback<MouseUpEvent>(e =>
            {
                Application.Quit();
            }, TrickleDown.NoTrickleDown);
        }
    }

    private void TalentUIElementSetup()
    {
        var talentsRoot = Object.FindObjectOfType<UIDocument>().rootVisualElement.Q("talents-root");

        var talentColumnParent = talentsRoot.Q("talent-column-parent");
        var talentColumnTemplate = Resources.Load<VisualTreeAsset>("talent-column");
        var talentButtonTemplate = Resources.Load<VisualTreeAsset>("talent-button");

        var tooltip = talentsRoot.Q<Label>("tooltip-text");
        var talentRefundToggle = talentsRoot.Q<Toggle>("talent-refund-toggle");

        var talentColumnDictionary = new Dictionary<int, TemplateContainer>();

        foreach (var talent in TalentDefinitions.Talents)
        {
            // If necessary build the new column for this talent, otherwise get an existing column
            if (!talentColumnDictionary.TryGetValue(talent.levelRequirement, out var column))
            {
                column = talentColumnTemplate.Instantiate();
                talentColumnDictionary.Add(talent.levelRequirement, column);

                var levelLabel = column.Q<Label>("level-label");
                levelLabel.text = talent.levelRequirement.ToString();

                column.style.flexShrink = 0; // I have to set this here like a damn fool
                talentColumnParent.Add(column);
            }

            // Build the talent button info and add it to its column
            var talentButton = talentButtonTemplate.Instantiate();
            var button = talentButton.Q<Button>("button");
            button.text = talent.name;
            talentButton.Q<Label>("points").text = "0 / " + talent.maxTalentLevel.ToString();

            // build the tooltip string
            var tooltipString = $"{talent.name}\n\nRequirements:\n\n";

            tooltipString += $"{StatType.TalentPoint} : {talent.pointCost}\n";

            foreach (var requirement in talent.requires)
            {
                tooltipString += requirement.ToString() + "\n";
            }

            tooltipString += "\nGrants:\n\n";

            foreach (var granted in talent.grants)
            {
                tooltipString += granted.ToString() + "\n";
            }

            tooltips.Add(button, tooltipString);

            // Register the callback
            button.RegisterCallback<MouseEnterEvent>(e => tooltip.text = tooltips[button]);
            button.RegisterCallback<MouseLeaveEvent>(e => tooltip.text = "");
            button.RegisterCallback<MouseUpEvent>(e =>
            {
                Debug.Log($"stat: {talent.stat}, refund: {talentRefundToggle.value}");
                talentRpcs.Add(new TalentAllocationRequestRpc
                {
                    deallocate = talentRefundToggle.value,
                    stat = talent.stat
                });
            });

            column.Add(talentButton);
        }
    }

    private void ContainerUIElementSetup()
    {


        //// TODO add sending rpcs to callbacks
        //var inventoryAndEquipmentRoot = Object.FindObjectOfType<UIDocument>().rootVisualElement.Q("inventory-and-equipment-root");

        //// Configure inventory slots
        //for (uint i = 1; i <= 16; i++)
        //{
        //    var button = inventoryAndEquipmentRoot.Q<Button>("inventory-slot-" + i);

        //    if (button == null) continue;

        //    var slotNum = i;
        //    button.RegisterCallback<MouseUpEvent>(e =>
        //    {
        //        Debug.Log((e.target as VisualElement).name);
        //        containerRpcs.Add(new PressContainerSlotRpc
        //        {
        //            containerType = ContainerType.Inventory,
        //            slotId = slotNum
        //        });
        //    });
        //}

        ////Configure equipment slots
        //inventoryAndEquipmentRoot.Q<Button>("right-hand-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        //{
        //    Debug.Log((e.target as VisualElement).name);
        //    containerRpcs.Add(new PressContainerSlotRpc
        //    {
        //        containerType = ContainerType.Equipment,
        //        slotId = (int)EquipmentSlot.RightHand
        //    });
        //});
        //inventoryAndEquipmentRoot.Q<Button>("left-hand-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        //{
        //    Debug.Log((e.target as VisualElement).name);
        //    containerRpcs.Add(new PressContainerSlotRpc
        //    {
        //        containerType = ContainerType.Equipment,
        //        slotId = (int)EquipmentSlot.LeftHand
        //    });
        //});
        //inventoryAndEquipmentRoot.Q<Button>("head-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        //{
        //    Debug.Log((e.target as VisualElement).name);
        //    containerRpcs.Add(new PressContainerSlotRpc
        //    {
        //        containerType = ContainerType.Equipment,
        //        slotId = (int)EquipmentSlot.Head
        //    });
        //});
        //inventoryAndEquipmentRoot.Q<Button>("chest-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        //{
        //    Debug.Log((e.target as VisualElement).name);
        //    containerRpcs.Add(new PressContainerSlotRpc
        //    {
        //        containerType = ContainerType.Equipment,
        //        slotId = (int)EquipmentSlot.Chest
        //    });
        //});
        //inventoryAndEquipmentRoot.Q<Button>("hands-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        //{
        //    Debug.Log((e.target as VisualElement).name);
        //    containerRpcs.Add(new PressContainerSlotRpc
        //    {
        //        containerType = ContainerType.Equipment,
        //        slotId = (int)EquipmentSlot.Hands
        //    });
        //});
        //inventoryAndEquipmentRoot.Q<Button>("feet-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        //{
        //    Debug.Log((e.target as VisualElement).name);
        //    containerRpcs.Add(new PressContainerSlotRpc
        //    {
        //        containerType = ContainerType.Equipment,
        //        slotId = (int)EquipmentSlot.Feet
        //    });
        //});
        //inventoryAndEquipmentRoot.Q<Button>("neck-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        //{
        //    Debug.Log((e.target as VisualElement).name);
        //    containerRpcs.Add(new PressContainerSlotRpc
        //    {
        //        containerType = ContainerType.Equipment,
        //        slotId = (int)EquipmentSlot.Neck
        //    });
        //});
        //inventoryAndEquipmentRoot.Q<Button>("waist-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        //{
        //    Debug.Log((e.target as VisualElement).name);
        //    containerRpcs.Add(new PressContainerSlotRpc
        //    {
        //        containerType = ContainerType.Equipment,
        //        slotId = (int)EquipmentSlot.Waist
        //    });
        //});
        //inventoryAndEquipmentRoot.Q<Button>("right-ring-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        //{
        //    Debug.Log((e.target as VisualElement).name);
        //    containerRpcs.Add(new PressContainerSlotRpc
        //    {
        //        containerType = ContainerType.Equipment,
        //        slotId = (int)EquipmentSlot.RightRing
        //    });
        //});
        //inventoryAndEquipmentRoot.Q<Button>("left-ring-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        //{
        //    Debug.Log((e.target as VisualElement).name);
        //    containerRpcs.Add(new PressContainerSlotRpc
        //    {
        //        containerType = ContainerType.Equipment,
        //        slotId = (int)EquipmentSlot.LeftRing
        //    });
        //});
    }
}

public class ItemDisplay
{
    public VisualElement visualElement;
    public VisualElement containerSlotParentElement;
    public Button itemButtonElement;

    Dictionary<uint, ItemDisplay> slots = new Dictionary<uint, ItemDisplay>();

    VisualTreeAsset childTemplateElement;

    public ItemDisplay(VisualElement visualElement)
    {
        this.visualElement = visualElement;
        containerSlotParentElement = visualElement.Q("container-slot-parent");
        itemButtonElement = visualElement.Q<Button>("item-button");
        childTemplateElement = Resources.Load<VisualTreeAsset>(visualElement.Q<Label>("child-path").text);
    }

    public void SetItem(Entity itemEntity, EntityManager em)
    {
        // Handle item data
        var icon = Resources.Load<Sprite>(em.GetComponentData<ContainerIcon>(itemEntity).name.ToString());
        itemButtonElement.style.backgroundImage = new StyleBackground(icon);

        var label = em.GetComponentData<ContainerLabel>(itemEntity).label.ToString();
        itemButtonElement.text = label;

        // Build tooltip
        var tooltip = "TEST TOOLTIP";
        itemButtonElement.tooltip = tooltip;

        // Handle container data
        if (!em.HasBuffer<ContainerSlot>(itemEntity))
        {
            return;
        }

        var container = em.GetBuffer<ContainerSlot>(itemEntity);

        if (containerSlotParentElement == null)
        {
            return;
        }

        if (childTemplateElement == null)
        {
            return;
        }

        if (!container.IsCreated)
        {
            return;
        }

        if (container.IsEmpty)
        {
            return;
        }

        for (var i = 0; i < container.Length; i++)
        {
            var slot = container[i];

            if (slots.ContainsKey(slot.id))
            {
                continue;
            }

            var childElement = childTemplateElement.Instantiate();
            visualElement.Add(childElement);
            var itemDisplay = new ItemDisplay(childElement);
            itemDisplay.SetItem(slot.item, em);
            slots.Add(slot.id, itemDisplay);
        }
    }
}