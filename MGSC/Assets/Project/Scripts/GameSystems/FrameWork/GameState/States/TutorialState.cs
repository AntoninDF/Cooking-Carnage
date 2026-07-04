using UI;
public class TutorialState : GameplayState
{
    HeadsUpDisplay HUD;
    public override void OnEnter()
    {
        base.OnEnter();
        if (!Reference.TryGetManager(out UISystem ui)) return;
        HUD = ui.GetWidget<HeadsUpDisplay>();
        HUD?.Enable();

        // The tutorial automatically skips for now, but it has to be done
        GameLoop.RequestTransition();
    }

    public override void OnExit()
    {
        base.OnExit();
        if (HUD != null)
            HUD.Disable();
    }

    public override GameState FindNext(StateTransitions transition = StateTransitions.None)
    {
        return new PreShiftState();
    }
}
