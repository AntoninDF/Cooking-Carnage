using System;
using UI;
using UnityEngine;

public class MenuState : GameState
{
    public static Type DefaultMenuType {  get; private set; }
    public UIWidget Menu {  get; private set; }
    
    public MenuState()
    {
        Debug.Log("Menu state tries to enable a menu");
        if (DefaultMenuType == null) return;
        Debug.Log("1");
        if (!Reference.TryGetManager(out UISystem ui))
            return;
        Debug.Log("2");

        Menu = ui.GetWidget(DefaultMenuType);
        Debug.Log("Menu is " + Menu);
        if (Menu == null) return;
        Debug.Log("3");
        Menu.Enable();
        OnStateEnd += Menu.Disable;
    }

    public static void SetDefaultMenu<T>() where T : UIWidget
    {
        DefaultMenuType = typeof(T);
    }
}
