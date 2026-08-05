using UI;

public class RecapState : MenuState
{
    // To Do : Change the menu according to wether the player has succeeded or failed
    protected override System.Type DefaultMenuType => typeof(ShiftRecapMenu);

    public override void OnEnter()
    {
        base.OnEnter();
        
        AudioSystem.PlayAudio(AudioSystem.Database.upgradeMusic,true);
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override GameState FindNext(StateTransitions transition = StateTransitions.None)
    {
        switch (transition)
        {
            case StateTransitions.UpgradeShop:
                return new UpgradeState();
            case StateTransitions.OpenMainMenu:
                return new MainMenuState();


            default:
            case StateTransitions.None:
                return base.FindNext(transition);
        }
    }
}
