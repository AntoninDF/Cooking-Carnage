using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UITab : MonoBehaviour
{
    public UnityEngine.UI.Button Button;
    public CanvasGroup Content;
    public UnityEvent OnEnable;
    public UnityEvent OnDisable;



    public void Enable()
    {
        if (Content == null) return;
        Content.alpha = 1f;
        OnEnable?.Invoke();
    }
    public void Disable()
    {
        if (Content == null) return;
        Content.alpha = 0f;
        OnDisable?.Invoke();
    }



}
