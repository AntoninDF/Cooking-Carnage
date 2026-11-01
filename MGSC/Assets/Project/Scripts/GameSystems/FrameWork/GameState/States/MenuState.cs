using GameSystem;
using System;
using UI;

/// <summary>
/// Menu states should override DefaultMenuType so that it returns the type of the associated menu
/// </summary>
public class MenuState : GameState
{
    protected virtual Type DefaultMenuType {  get;}
    protected virtual float WaitBeforeDisablingMenu => 0f;
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
        string timerName = "Waiting before disabling menu " + DefaultMenuType.Name;
        TimerManager.GetTimer(timerName)?.Pause();
        OnStateEnd += () => TimerManager.StartTimer(timerName, WaitBeforeDisablingMenu, Menu.Disable);
    }

  
}
