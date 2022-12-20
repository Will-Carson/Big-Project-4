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

    NativeList<TalentAllocationRequestRPC> talentRpcs = new NativeList<TalentAllocationRequestRPC>(Allocator.Persistent);
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
    }

    bool setup = false;
    public void Setup()
    {
        if (setup) return;

        MainMenuSetup();
        InventoryAndEquipmentUIElementSetup();
        TalentUIElementSetup();

        setup = true;
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
                talentRpcs.Add(new TalentAllocationRequestRPC
                {
                    deallocate = talentRefundToggle.value,
                    stat = talent.stat
                });
            });

            column.Add(talentButton);
        }
    }

    private void InventoryAndEquipmentUIElementSetup()
    {
        // TODO add sending rpcs to callbacks
        var inventoryAndEquipmentRoot = Object.FindObjectOfType<UIDocument>().rootVisualElement.Q("inventory-and-equipment-root");

        // Configure inventory slots
        for (var i = 0; i < 16; i++)
        {
            var button = inventoryAndEquipmentRoot.Q<Button>("inventory-slot-" + i);

            if (button == null) continue;

            var slotNum = i;
            button.RegisterCallback<MouseUpEvent>(e =>
            {
                Debug.Log((e.target as VisualElement).name);
                containerRpcs.Add(new PressContainerSlotRpc
                {
                    containerType = ContainerType.Inventory,
                    slot = slotNum
                });
            });
        }

        //Configure equipment slots
        inventoryAndEquipmentRoot.Q<Button>("right-hand-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        {
            Debug.Log((e.target as VisualElement).name);
            containerRpcs.Add(new PressContainerSlotRpc
            {
                containerType = ContainerType.Equipment,
                slot = (int)EquipmentSlot.RightHand
            });
        });
        inventoryAndEquipmentRoot.Q<Button>("left-hand-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        {
            Debug.Log((e.target as VisualElement).name);
            containerRpcs.Add(new PressContainerSlotRpc
            {
                containerType = ContainerType.Equipment,
                slot = (int)EquipmentSlot.LeftHand
            });
        });
        inventoryAndEquipmentRoot.Q<Button>("head-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        {
            Debug.Log((e.target as VisualElement).name);
            containerRpcs.Add(new PressContainerSlotRpc
            {
                containerType = ContainerType.Equipment,
                slot = (int)EquipmentSlot.Head
            });
        });
        inventoryAndEquipmentRoot.Q<Button>("chest-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        {
            Debug.Log((e.target as VisualElement).name);
            containerRpcs.Add(new PressContainerSlotRpc
            {
                containerType = ContainerType.Equipment,
                slot = (int)EquipmentSlot.Chest
            });
        });
        inventoryAndEquipmentRoot.Q<Button>("hands-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        {
            Debug.Log((e.target as VisualElement).name);
            containerRpcs.Add(new PressContainerSlotRpc
            {
                containerType = ContainerType.Equipment,
                slot = (int)EquipmentSlot.Hands
            });
        });
        inventoryAndEquipmentRoot.Q<Button>("feet-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        {
            Debug.Log((e.target as VisualElement).name);
            containerRpcs.Add(new PressContainerSlotRpc
            {
                containerType = ContainerType.Equipment,
                slot = (int)EquipmentSlot.Feet
            });
        });
        inventoryAndEquipmentRoot.Q<Button>("neck-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        {
            Debug.Log((e.target as VisualElement).name);
            containerRpcs.Add(new PressContainerSlotRpc
            {
                containerType = ContainerType.Equipment,
                slot = (int)EquipmentSlot.Neck
            });
        });
        inventoryAndEquipmentRoot.Q<Button>("waist-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        {
            Debug.Log((e.target as VisualElement).name);
            containerRpcs.Add(new PressContainerSlotRpc
            {
                containerType = ContainerType.Equipment,
                slot = (int)EquipmentSlot.Waist
            });
        });
        inventoryAndEquipmentRoot.Q<Button>("right-ring-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        {
            Debug.Log((e.target as VisualElement).name);
            containerRpcs.Add(new PressContainerSlotRpc
            {
                containerType = ContainerType.Equipment,
                slot = (int)EquipmentSlot.RightRing
            });
        });
        inventoryAndEquipmentRoot.Q<Button>("left-ring-equipment-button").RegisterCallback<MouseUpEvent>(e =>
        {
            Debug.Log((e.target as VisualElement).name);
            containerRpcs.Add(new PressContainerSlotRpc
            {
                containerType = ContainerType.Equipment,
                slot = (int)EquipmentSlot.LeftRing
            });
        });
    }
}

public struct PressContainerSlotRpc : IRpcCommand
{
    public ContainerType containerType;
    public int slot;
}