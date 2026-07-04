using System;
using UI;
using UnityEngine;

/// <summary>
/// Menu states should override DefaultMenuType so that it returns the type of the associated menu
/// </summary>
public class MenuState : GameState
{
    protected virtual Type DefaultMenuType {  get;}
    public FullscreenWidget Menu {  get; private set; }
    
    public override void OnEnter()
    {
        base.OnEnter();

        if (DefaultMenuType == null) return;
        if (!Reference.TryGetManager(out UISystem ui))
            return;

        Menu = ui.GetWidget(DefaultMenuType);
        if (Menu == null) return;
        Menu.Enable();
        OnStateEnd += Menu.Disable;
    }

  
}
