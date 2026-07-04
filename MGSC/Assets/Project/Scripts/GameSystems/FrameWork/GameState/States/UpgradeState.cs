using UI;

public class UpgradeState : MenuState
{

    protected override System.Type DefaultMenuType => typeof(UpgradeMenu);

    public override GameState FindNext(StateTransitions transition = StateTransitions.None)
    {
        switch (transition)
        {
            case StateTransitions.StartShift :
                return new PreShiftState();
            case StateTransitions.OpenMainMenu:
                return new MainMenuState();
            

            default:
            case StateTransitions.None:
                return base.FindNext(transition);
        }
    }
}
