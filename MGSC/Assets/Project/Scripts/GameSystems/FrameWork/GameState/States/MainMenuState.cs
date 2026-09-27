
using UI;

public class MainMenuState : MenuState
{
    protected override System.Type DefaultMenuType => typeof(MainMenu);
    protected override float WaitBeforeDisablingMenu => 2;

    public override void OnEnter()
    {
        base.OnEnter();
        
        AudioSystem.PlayAudio(AudioSystem.Database.mainMenuMusic,true, true);
        TutorialTimeline.StopTutorial();
    }

    public override GameState FindNext(StateTransitions transition = StateTransitions.None)
    {
        switch (transition)
        {
            case StateTransitions.StartShift:
                {
                    if (!Globals.Statistics.TutorialHasBeenPlayed && Globals.Gameplay.ShiftNumber == 0)
                    {
                        GameLoop.ResetAll();
                        return new TutorialState();
                    }
                    if (Globals.Gameplay.ShiftNumber != 0)
                        return new UpgradeState();

                    GameLoop.ResetAll();
                    return new PreShiftState();
                }

            case StateTransitions.OpenCredits:
                return new CreditsState();
            case StateTransitions.UpgradeShop:
                return new UpgradeState();
            default:
            case StateTransitions.None:
                return base.FindNext(transition);

        }
    }
}
