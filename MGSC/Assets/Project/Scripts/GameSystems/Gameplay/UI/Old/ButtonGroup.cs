using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Centralizes the UnityEvents of the listed buttons
/// </summary>
public class ButtonGroup : MonoBehaviour
{
    public UnityEvent OnAnyButton;
    public List<ButtonRelay> Buttons;

    private void Start()
    {
        BindButtons();
    }

    private void BindButtons()
    {
        foreach(var button in Buttons)
        {
            if (button.Button != null)
            {
                button.Button.onClick.AddListener(button.Event.Invoke);
                button.Button.onClick.AddListener(OnAnyButton.Invoke);
            }
        }
    }
}

[System.Serializable]
public struct ButtonRelay
{
    public UnityEngine.UI.Button Button;
    public UnityEngine.Events.UnityEvent Event;

}
