using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TabLayout : MonoBehaviour
{
    public UITab DefaultTab;
    public List<UITab> tabs;

    [HideInInspector] public UITab CurrentTab;


    private void Start()
    {
        EnableDefaultTab();
        BindEvents();
    }

    private void EnableDefaultTab()
    {
        foreach (var tab in tabs)
            tab.Disable();
        CurrentTab = DefaultTab;
        if (CurrentTab == null && tabs != null && tabs.Count > 0)
            CurrentTab = tabs[0];
        if (CurrentTab != null)
            EnableTab(CurrentTab);
    }

    private void BindEvents()
    {
        foreach (var tab in tabs)
        {
            if (tab ==  null) continue;
            if (tab.Button == null) continue;
            tab.Button.onClick.AddListener(()=>EnableTab(tab));
        }
    }

    public void EnableTab(UITab tab)
    {
        if (tab == null) return;
        if (tab == CurrentTab) return;

        CurrentTab?.Disable();
        tab.Enable();
        CurrentTab = tab;
    }
}
