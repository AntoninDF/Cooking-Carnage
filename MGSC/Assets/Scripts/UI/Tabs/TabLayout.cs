using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TabLayout : MonoBehaviour
{
    public UITabButton DefaultTab;
    public List<UITabButton> tabButtons;

    [HideInInspector] public UITabButton CurrentTab;


    private void Start()
    {
        BindEvents();
        EnableDefaultTab();
    }

    private void EnableDefaultTab()
    {
        foreach (var tab in tabButtons)
            tab.DisableTab();

        if (DefaultTab == null && tabButtons != null && tabButtons.Count > 0)
            DefaultTab = tabButtons[0];
        if (DefaultTab != null)
            EnableTab(DefaultTab);
    }

    private void BindEvents()
    {
        foreach (var button in tabButtons)
        {
            if (button == null) continue;
            if (button.Button == null) continue;
            Debug.Log("Binding button");
            button.Button.onClick.AddListener(()=>EnableTab(button));
        }
    }

    public void EnableTab(UITabButton tab)
    {
        Debug.Log("Enabling tab from TabLayout");
        if (tab == null) return;
        if (tab == CurrentTab) return;

        CurrentTab?.DisableTab();
        tab.EnableTab();
        CurrentTab = tab;
    }
}
