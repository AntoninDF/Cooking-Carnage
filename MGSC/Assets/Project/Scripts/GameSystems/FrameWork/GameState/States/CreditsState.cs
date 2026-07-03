
using System;
using UI;

public class CreditsState : MenuState
{
    protected override Type DefaultMenuType => typeof(CreditsMenu);
    public override GameState FindNext(StateTransitions transition = StateTransitions.None)
    {
        return new MainMenuState();
    }
}
