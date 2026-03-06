using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UITabContent : MonoBehaviour
{
    public bool Enabled { get; set; }
    public CanvasGroup Content;
    public UnityEvent OnEnable;
    public UnityEvent OnDisable;

    private void Awake()
    {
        Content.alpha = 0f;

    }
    public void Enable()
    {
        if (Enabled) return;
        if (Content == null) return;
        Enabled = true;
        Content.alpha = 1f;
        OnEnable?.Invoke();
    }
    public void Disable()
    {
        if (!Enabled) return;
        if (Content == null) return;
        Enabled = false;
        Content.alpha = 0f;
        OnDisable?.Invoke();
    }



}
