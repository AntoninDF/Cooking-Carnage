using UI;
public class TutorialState : GameplayState
{
    HeadsUpDisplay HUD;
    SubtitleOverlay Subs;
    public override void OnEnter()
    {
        base.OnEnter();
        if (Reference.TryGetManager(out UISystem ui))
        {
            HUD = ui.GetWidget<HeadsUpDisplay>();
            HUD?.Enable();
        }
        GameLoop.RequestTransition(StateTransitions.StartShift);
        
        AudioSystem.PlayAudio(AudioSystem.Database.beforeShiftMusic,true);
        AudioSystem.PlayAudio(AudioSystem.Database.fliesAmbience);
        AudioSystem.PlayAudio(AudioSystem.Database.kitchenAmbience);
            //TutorialTimeline.StartTutorial();
    }

    public override void OnExit()
    {
        base.OnExit();
        if (HUD != null)
            HUD.Disable();
    }

    public override GameState FindNext(StateTransitions transition = StateTransitions.None)
    {
        if (Globals.Gameplay.ShiftNumber != 0)
            return new UpgradeState();
        return new PreShiftState();
    }
}
