
public class CreditsState : GameState
{
    public override GameState FindNext(StateTransitions transition = StateTransitions.None)
    {
        return new MainMenuState();
    }
}
