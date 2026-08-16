using GameSystem;
using UI;
using UnityEngine;

public class DemoEndState : MenuState
{
    protected override System.Type DefaultMenuType => typeof(DemoEndMenu);

    public override void OnEnter()
    {
        base.OnEnter();
        SaveManager.ResetSave();
        SaveManager.LoadGlobalData();
    }

    public override GameState FindNext(StateTransitions transition = StateTransitions.None)
    {
        return new MainMenuState();
        // return base.FindNext(transition);
    }
}
