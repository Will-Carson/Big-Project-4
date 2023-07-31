//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.6.1
//     from Assets/Scripts/Character Controller/PlatformerInputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @PlatformerInputActions: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlatformerInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlatformerInputActions"",
    ""maps"": [
        {
            ""name"": ""GameplayMap"",
            ""id"": ""34ad15fe-9aca-4100-b22a-63eced3d7198"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""5926d1fd-8bc7-48e8-a671-ff59c7d8fae6"",
                    ""expectedControlType"": """",
                    ""processors"": ""Clamp(max=1)"",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""LookDelta"",
                    ""type"": ""Value"",
                    ""id"": ""8313772a-4db2-4934-89d6-3410caf6d19c"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""LookConst"",
                    ""type"": ""Value"",
                    ""id"": ""c7859773-cf29-4a35-8002-7294da914c20"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""e72bb4f0-769e-419a-b126-e5cceb2c9b10"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Roll"",
                    ""type"": ""Button"",
                    ""id"": ""12d27c10-f0e3-42fc-a7b2-ba873ab82682"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Sprint"",
                    ""type"": ""Button"",
                    ""id"": ""fe3235ec-7c88-4156-8908-e23daba138a4"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""CameraZoom"",
                    ""type"": ""Value"",
                    ""id"": ""b9342fd8-9c52-4100-bb70-6c6d361609b7"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Dash"",
                    ""type"": ""Button"",
                    ""id"": ""12230fe0-5d15-4f77-90f7-e62d57d5940e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Crouch"",
                    ""type"": ""Button"",
                    ""id"": ""c25ed409-b6b4-4e95-a936-3284cb98bd8f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Rope"",
                    ""type"": ""Button"",
                    ""id"": ""56cac786-2ffc-454b-9435-dc8361542e44"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Climb"",
                    ""type"": ""Button"",
                    ""id"": ""1361423f-7b42-42bd-b1b3-ba59d5ef7212"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""FlyNoCollisions"",
                    ""type"": ""Button"",
                    ""id"": ""70df9dca-62d7-4902-9c9e-8629a25d62b2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Fire1"",
                    ""type"": ""Button"",
                    ""id"": ""28191030-f870-4e77-be70-5b5723ffa01d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Fire2"",
                    ""type"": ""Button"",
                    ""id"": ""fdcf7a4a-407c-4f7a-befa-5b7c24c681d6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""CameraRotation"",
                    ""type"": ""Button"",
                    ""id"": ""6fab347d-cc7e-4bae-a48e-2041fbe9cb7a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""e4976612-b5ff-40fd-936f-e69fc0bd3f82"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": ""ScaleVector2(x=0.05,y=0.05)"",
                    ""groups"": """",
                    ""action"": ""LookDelta"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8f2809af-0cc9-4cf9-a30d-41e59cc01524"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a82e4e39-40fd-48fb-8fa6-6b8ceea0bc27"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""343b41fc-355f-42c2-b00a-c8ae30ad612a"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Roll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dab2fec4-1ac5-4df6-b615-e90912460c46"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Roll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""2e2dc72a-9e80-4b08-96de-571c9cd72773"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""b9bfcc8c-1a2c-41f9-9230-d96b226090d9"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""d44b095e-aed9-481d-aae4-eab8119e9854"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""37210dad-672f-4306-a3d3-355b7547b03f"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""51a570f0-8ea8-43fc-ae72-b41792b8d971"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""6ec1be08-235d-437e-9d3c-ca301bc06aeb"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": ""StickDeadzone"",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b143f747-5e51-4c9c-9267-39ff22088fdf"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e023a517-781e-496f-96e1-62a85c46f78e"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e601188a-0696-45cd-80b7-ba3eac4314ab"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=0.1),Invert"",
                    ""groups"": """",
                    ""action"": ""CameraZoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""404415c5-c8d3-4ecd-acaa-7f7037641a0e"",
                    ""path"": ""<Gamepad>/dpad/y"",
                    ""interactions"": """",
                    ""processors"": ""Invert"",
                    ""groups"": """",
                    ""action"": ""CameraZoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""922282e4-fb57-400d-aaa9-cb143fa897ce"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Dash"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b9844999-9546-46f6-8859-bea770cbbb69"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Dash"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b4fc3d14-c1f9-485d-81b7-5869df204075"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3d5a467e-67c3-4320-ad53-5c7fb09bec0b"",
                    ""path"": ""<Gamepad>/rightStickPress"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6c4217fc-df5f-40b3-bde8-524d2d9c1a57"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rope"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a892b883-92b3-45bf-b2c8-4e0a81b4fdda"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rope"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8c686a2f-07d8-40d3-ab87-dfb197892165"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Climb"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""af69a741-4607-4b4c-9e46-6f53158ba1e5"",
                    ""path"": ""<Gamepad>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Climb"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""22707d2a-a98c-4a5f-aecd-4777414fff8d"",
                    ""path"": ""<Keyboard>/z"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FlyNoCollisions"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""644381cf-4f4c-480d-a1cf-2b25df88971f"",
                    ""path"": ""<Gamepad>/select"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""FlyNoCollisions"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c896ef92-dd57-40e7-bb64-9e096d169c1b"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": ""StickDeadzone,ScaleVector2(x=70,y=70)"",
                    ""groups"": """",
                    ""action"": ""LookConst"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3a29aeca-fc43-48a3-be6d-2909349b8efc"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fire2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""56346cb4-5a7d-4372-9722-b0e5d3b38bee"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fire1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f0d58b39-321f-4e81-8460-5480e96315ff"",
                    ""path"": ""<Keyboard>/Q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraRotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // GameplayMap
        m_GameplayMap = asset.FindActionMap("GameplayMap", throwIfNotFound: true);
        m_GameplayMap_Move = m_GameplayMap.FindAction("Move", throwIfNotFound: true);
        m_GameplayMap_LookDelta = m_GameplayMap.FindAction("LookDelta", throwIfNotFound: true);
        m_GameplayMap_LookConst = m_GameplayMap.FindAction("LookConst", throwIfNotFound: true);
        m_GameplayMap_Jump = m_GameplayMap.FindAction("Jump", throwIfNotFound: true);
        m_GameplayMap_Roll = m_GameplayMap.FindAction("Roll", throwIfNotFound: true);
        m_GameplayMap_Sprint = m_GameplayMap.FindAction("Sprint", throwIfNotFound: true);
        m_GameplayMap_CameraZoom = m_GameplayMap.FindAction("CameraZoom", throwIfNotFound: true);
        m_GameplayMap_Dash = m_GameplayMap.FindAction("Dash", throwIfNotFound: true);
        m_GameplayMap_Crouch = m_GameplayMap.FindAction("Crouch", throwIfNotFound: true);
        m_GameplayMap_Rope = m_GameplayMap.FindAction("Rope", throwIfNotFound: true);
        m_GameplayMap_Climb = m_GameplayMap.FindAction("Climb", throwIfNotFound: true);
        m_GameplayMap_FlyNoCollisions = m_GameplayMap.FindAction("FlyNoCollisions", throwIfNotFound: true);
        m_GameplayMap_Fire1 = m_GameplayMap.FindAction("Fire1", throwIfNotFound: true);
        m_GameplayMap_Fire2 = m_GameplayMap.FindAction("Fire2", throwIfNotFound: true);
        m_GameplayMap_CameraRotation = m_GameplayMap.FindAction("CameraRotation", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // GameplayMap
    private readonly InputActionMap m_GameplayMap;
    private List<IGameplayMapActions> m_GameplayMapActionsCallbackInterfaces = new List<IGameplayMapActions>();
    private readonly InputAction m_GameplayMap_Move;
    private readonly InputAction m_GameplayMap_LookDelta;
    private readonly InputAction m_GameplayMap_LookConst;
    private readonly InputAction m_GameplayMap_Jump;
    private readonly InputAction m_GameplayMap_Roll;
    private readonly InputAction m_GameplayMap_Sprint;
    private readonly InputAction m_GameplayMap_CameraZoom;
    private readonly InputAction m_GameplayMap_Dash;
    private readonly InputAction m_GameplayMap_Crouch;
    private readonly InputAction m_GameplayMap_Rope;
    private readonly InputAction m_GameplayMap_Climb;
    private readonly InputAction m_GameplayMap_FlyNoCollisions;
    private readonly InputAction m_GameplayMap_Fire1;
    private readonly InputAction m_GameplayMap_Fire2;
    private readonly InputAction m_GameplayMap_CameraRotation;
    public struct GameplayMapActions
    {
        private @PlatformerInputActions m_Wrapper;
        public GameplayMapActions(@PlatformerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_GameplayMap_Move;
        public InputAction @LookDelta => m_Wrapper.m_GameplayMap_LookDelta;
        public InputAction @LookConst => m_Wrapper.m_GameplayMap_LookConst;
        public InputAction @Jump => m_Wrapper.m_GameplayMap_Jump;
        public InputAction @Roll => m_Wrapper.m_GameplayMap_Roll;
        public InputAction @Sprint => m_Wrapper.m_GameplayMap_Sprint;
        public InputAction @CameraZoom => m_Wrapper.m_GameplayMap_CameraZoom;
        public InputAction @Dash => m_Wrapper.m_GameplayMap_Dash;
        public InputAction @Crouch => m_Wrapper.m_GameplayMap_Crouch;
        public InputAction @Rope => m_Wrapper.m_GameplayMap_Rope;
        public InputAction @Climb => m_Wrapper.m_GameplayMap_Climb;
        public InputAction @FlyNoCollisions => m_Wrapper.m_GameplayMap_FlyNoCollisions;
        public InputAction @Fire1 => m_Wrapper.m_GameplayMap_Fire1;
        public InputAction @Fire2 => m_Wrapper.m_GameplayMap_Fire2;
        public InputAction @CameraRotation => m_Wrapper.m_GameplayMap_CameraRotation;
        public InputActionMap Get() { return m_Wrapper.m_GameplayMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GameplayMapActions set) { return set.Get(); }
        public void AddCallbacks(IGameplayMapActions instance)
        {
            if (instance == null || m_Wrapper.m_GameplayMapActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_GameplayMapActionsCallbackInterfaces.Add(instance);
            @Move.started += instance.OnMove;
            @Move.performed += instance.OnMove;
            @Move.canceled += instance.OnMove;
            @LookDelta.started += instance.OnLookDelta;
            @LookDelta.performed += instance.OnLookDelta;
            @LookDelta.canceled += instance.OnLookDelta;
            @LookConst.started += instance.OnLookConst;
            @LookConst.performed += instance.OnLookConst;
            @LookConst.canceled += instance.OnLookConst;
            @Jump.started += instance.OnJump;
            @Jump.performed += instance.OnJump;
            @Jump.canceled += instance.OnJump;
            @Roll.started += instance.OnRoll;
            @Roll.performed += instance.OnRoll;
            @Roll.canceled += instance.OnRoll;
            @Sprint.started += instance.OnSprint;
            @Sprint.performed += instance.OnSprint;
            @Sprint.canceled += instance.OnSprint;
            @CameraZoom.started += instance.OnCameraZoom;
            @CameraZoom.performed += instance.OnCameraZoom;
            @CameraZoom.canceled += instance.OnCameraZoom;
            @Dash.started += instance.OnDash;
            @Dash.performed += instance.OnDash;
            @Dash.canceled += instance.OnDash;
            @Crouch.started += instance.OnCrouch;
            @Crouch.performed += instance.OnCrouch;
            @Crouch.canceled += instance.OnCrouch;
            @Rope.started += instance.OnRope;
            @Rope.performed += instance.OnRope;
            @Rope.canceled += instance.OnRope;
            @Climb.started += instance.OnClimb;
            @Climb.performed += instance.OnClimb;
            @Climb.canceled += instance.OnClimb;
            @FlyNoCollisions.started += instance.OnFlyNoCollisions;
            @FlyNoCollisions.performed += instance.OnFlyNoCollisions;
            @FlyNoCollisions.canceled += instance.OnFlyNoCollisions;
            @Fire1.started += instance.OnFire1;
            @Fire1.performed += instance.OnFire1;
            @Fire1.canceled += instance.OnFire1;
            @Fire2.started += instance.OnFire2;
            @Fire2.performed += instance.OnFire2;
            @Fire2.canceled += instance.OnFire2;
            @CameraRotation.started += instance.OnCameraRotation;
            @CameraRotation.performed += instance.OnCameraRotation;
            @CameraRotation.canceled += instance.OnCameraRotation;
        }

        private void UnregisterCallbacks(IGameplayMapActions instance)
        {
            @Move.started -= instance.OnMove;
            @Move.performed -= instance.OnMove;
            @Move.canceled -= instance.OnMove;
            @LookDelta.started -= instance.OnLookDelta;
            @LookDelta.performed -= instance.OnLookDelta;
            @LookDelta.canceled -= instance.OnLookDelta;
            @LookConst.started -= instance.OnLookConst;
            @LookConst.performed -= instance.OnLookConst;
            @LookConst.canceled -= instance.OnLookConst;
            @Jump.started -= instance.OnJump;
            @Jump.performed -= instance.OnJump;
            @Jump.canceled -= instance.OnJump;
            @Roll.started -= instance.OnRoll;
            @Roll.performed -= instance.OnRoll;
            @Roll.canceled -= instance.OnRoll;
            @Sprint.started -= instance.OnSprint;
            @Sprint.performed -= instance.OnSprint;
            @Sprint.canceled -= instance.OnSprint;
            @CameraZoom.started -= instance.OnCameraZoom;
            @CameraZoom.performed -= instance.OnCameraZoom;
            @CameraZoom.canceled -= instance.OnCameraZoom;
            @Dash.started -= instance.OnDash;
            @Dash.performed -= instance.OnDash;
            @Dash.canceled -= instance.OnDash;
            @Crouch.started -= instance.OnCrouch;
            @Crouch.performed -= instance.OnCrouch;
            @Crouch.canceled -= instance.OnCrouch;
            @Rope.started -= instance.OnRope;
            @Rope.performed -= instance.OnRope;
            @Rope.canceled -= instance.OnRope;
            @Climb.started -= instance.OnClimb;
            @Climb.performed -= instance.OnClimb;
            @Climb.canceled -= instance.OnClimb;
            @FlyNoCollisions.started -= instance.OnFlyNoCollisions;
            @FlyNoCollisions.performed -= instance.OnFlyNoCollisions;
            @FlyNoCollisions.canceled -= instance.OnFlyNoCollisions;
            @Fire1.started -= instance.OnFire1;
            @Fire1.performed -= instance.OnFire1;
            @Fire1.canceled -= instance.OnFire1;
            @Fire2.started -= instance.OnFire2;
            @Fire2.performed -= instance.OnFire2;
            @Fire2.canceled -= instance.OnFire2;
            @CameraRotation.started -= instance.OnCameraRotation;
            @CameraRotation.performed -= instance.OnCameraRotation;
            @CameraRotation.canceled -= instance.OnCameraRotation;
        }

        public void RemoveCallbacks(IGameplayMapActions instance)
        {
            if (m_Wrapper.m_GameplayMapActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IGameplayMapActions instance)
        {
            foreach (var item in m_Wrapper.m_GameplayMapActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_GameplayMapActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public GameplayMapActions @GameplayMap => new GameplayMapActions(this);
    public interface IGameplayMapActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnLookDelta(InputAction.CallbackContext context);
        void OnLookConst(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnRoll(InputAction.CallbackContext context);
        void OnSprint(InputAction.CallbackContext context);
        void OnCameraZoom(InputAction.CallbackContext context);
        void OnDash(InputAction.CallbackContext context);
        void OnCrouch(InputAction.CallbackContext context);
        void OnRope(InputAction.CallbackContext context);
        void OnClimb(InputAction.CallbackContext context);
        void OnFlyNoCollisions(InputAction.CallbackContext context);
        void OnFire1(InputAction.CallbackContext context);
        void OnFire2(InputAction.CallbackContext context);
        void OnCameraRotation(InputAction.CallbackContext context);
    }
}
