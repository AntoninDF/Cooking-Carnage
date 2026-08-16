using GameSystem;
using System;

/// <summary>
/// A GameState represents a portion of gameplay. GameState references should NOT be held durably by anything other than the gameloop
/// </summary>
public class GameState
{
    public Action OnStateBegin;
    public Action OnStateEnd;
    public virtual GameState FindNext(StateTransitions transition = default)
    {
        switch (transition)
        {
            case StateTransitions.OpenMainMenu:
                return new MainMenuState();
            default:
                return this;
        }
    }

    /// <summary>
    /// Notifies a state that it has been entered
    /// </summary>
    public virtual void OnEnter()
    {
        OnStateBegin?.Invoke();
    }
    /// <summary>
    /// Notifies a state that it has been exitted
    /// </summary>
    public virtual void OnExit()
    {
        OnStateEnd?.Invoke();
    }

}
