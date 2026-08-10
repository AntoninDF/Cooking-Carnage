using GameSystem;
using UI;

public class ShiftState : GameplayState
{
    HeadsUpDisplay HUD;
    public override void OnEnter()
    {
        base.OnEnter();
        if (!Reference.TryGetManager(out UISystem ui)) return;
        HUD = ui.GetWidget<HeadsUpDisplay>();
        HUD?.Enable();

        if (!Reference.TryGetManager(out ShiftManager mgr))
        {
            UnityEngine.Debug.LogError("[GameLoop] ShiftState couldn't launch the shift, since there was no ShiftManager available");
            return;
        }

        mgr.RequestShiftTimer();
        
        AudioSystem.SetEventParameter(AudioSystem.Database.beforeShiftMusic, "Ingame Music Sequence", 1); // 1 Triggers the shift music (opposed to the preshift music that should be currently playing)
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
            case StateTransitions.ShowShiftRecap:
                return new RecapState();
            case StateTransitions.OpenMainMenu:
                return new MainMenuState();


            default:
            case StateTransitions.None:
                return base.FindNext(transition);
        }
    }
}
