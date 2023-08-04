using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuUIManager : MonoBehaviour
{
    public UIStateMachine mainMenu;
}

public class UIStateMachine : MonoBehaviour
{
    public List<UIState> states;
    public StyleColor highlightColor;
    public UIState currentState;

    private void SetState(UIState newState)
    {
        foreach (var s in states)
        {
            if (s == newState)
            {
                currentState = newState;
            }
        }
    }

    public void SelectElement(UIState state)
    {
        foreach (var s in states)
        {
            if (s == state)
            {
                s.SetColor(highlightColor);
                SetState(state);
            }
            else
            {
                s.ResetColor();
            }
        }
    }

    public void ToggleElement(UIState state)
    {
        foreach (var s in states)
        {
            if (s == state)
            {
                s.visualElement.style.display = (s.visualElement.style.display == DisplayStyle.None) ? DisplayStyle.Flex : DisplayStyle.None;
            }
            else
            {
                s.visualElement.style.display = DisplayStyle.None;
            }
        }
    }

    public void SetElementState(UIState state, bool active)
    {
        foreach (var s in states)
        {
            if (s == state)
            {
                s.visualElement.style.display = (active) ? DisplayStyle.Flex : DisplayStyle.None;
            }
            else
            {
                s.visualElement.style.display = DisplayStyle.None;
            }
        }
    }
}

public class UIState : MonoBehaviour
{
    public VisualElement visualElement;
    private StyleColor defaultColor;

    private void Awake()
    {
        defaultColor = visualElement.style.backgroundColor;
    }

    public void ResetColor()
    {
        SetColor(defaultColor);
    }

    public void SetColor(StyleColor color)
    {
        visualElement.style.backgroundColor = color;
    }
}