using UI;
public class PreShiftState : GameplayState
{
    HeadsUpDisplay HUD;
    public override void OnEnter()
    {
        //GameLoop.ResetAll();
        base.OnEnter();
        if (Reference.TryGetManager(out UISystem ui))
        {
            HUD = ui.GetWidget<HeadsUpDisplay>();
            HUD?.Enable();
        }
        IngredientSpawnPosition.SpawnIngredients(6);
        
        AudioSystem.PlayAudio(AudioSystem.Database.beforeShiftMusic,true);
        AudioSystem.PlayAudio(AudioSystem.Database.fliesAmbience);
        AudioSystem.PlayAudio(AudioSystem.Database.kitchenAmbience);
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
