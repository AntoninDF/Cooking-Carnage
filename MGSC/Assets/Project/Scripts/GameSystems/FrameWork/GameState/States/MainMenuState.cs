using UnityEngine;

public class MainMenuState : MenuState
{
    // Si on appuie sur le bouton Play du menu principal, ça lance une transition

    // TRANSITIONS

    // → Play
    // → Credits
    // → Settings
    public override GameState FindNext(StateTransitions transition = StateTransitions.None)
    {
        switch(transition)
        {
            case StateTransitions.StartShift:
                {
                    if (!Globals.Statistics.TutorialHasBeenPlayed)
                        return new TutorialState();
                    if (Globals.Gameplay.ShiftNumber != 0)
                        return new UpgradeState();
                    return new PreShiftState();
                }

            case StateTransitions.OpenCredits:
                return new CreditsState();
            default:
            case StateTransitions.None:
                return base.FindNext(transition);
                
        }
    }
}
