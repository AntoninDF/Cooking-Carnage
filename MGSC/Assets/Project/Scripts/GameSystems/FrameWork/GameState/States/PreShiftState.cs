using UI;
public class PreShiftState : GameplayState
{
    HeadsUpDisplay HUD;
    public override void OnEnter()
    {
        base.OnEnter();
        if (!Reference.TryGetManager(out UISystem ui)) return;
        HUD = ui.GetWidget<HeadsUpDisplay>();
        HUD?.Enable();
    }

    public override void OnExit()
    {
        base.OnExit();
        if (HUD != null)
            HUD.Disable();
    }

    public override GameState FindNext(StateTransitions transition = StateTransitions.None)
    {
        switch (transition)
        {
            case StateTransitions.StartShiftCountdown:
                return new ShiftState();
            case StateTransitions.OpenMainMenu:
                return new MainMenuState();


            default:
            case StateTransitions.None:
                return base.FindNext(transition);
        }
    }
}
