using GameSystem;
using System;

/// <summary>
/// A GameState represents a portion of gameplay. GameState references should NOT be held durably by anything other than the gameloop
/// </summary>
public class GameState
{
    public static Action OnStateBegin;
    public static Action OnStateEnd;
    public virtual bool CanTransitionTo<T>() => false;
    public virtual GameState FindBestTransition()
    {
        return null;
    }

    /// <summary>
    /// Notifies a state that it has been entered
    /// </summary>
    public void OnEnter()
    {
        OnStateBegin?.Invoke();
    }
    /// <summary>
    /// Notifies a state that it has been exitted
    /// </summary>
    public void OnExit()
    {
        OnStateEnd?.Invoke();
    }

}
