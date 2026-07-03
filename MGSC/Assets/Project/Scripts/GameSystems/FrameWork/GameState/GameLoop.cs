using GameSystem;
using System;
using System.Collections.Generic;

public static class GameLoop
{
    public static bool StateNotNull => _currentState != null;
    public static GameState CurrentState
    {
        get => _currentState;
        set
        {
            if (_currentState == value) return;
            _currentState = value;
            OnStateChange?.Invoke(value);
        }
    }
    public static Action<GameState> OnStateChange;
    public static bool IsRunning { get; private set; } = true;
    [GlobalEvent("Gameloop/OnPause")]
    public static Action OnPause { get; set; }
    [GlobalEvent("Gameloop/OnResume")]
    public static Action OnResume { get; set; }

    private static List<object> _objectsRequiringPause = new();
    private static GameState _currentState;

    #region Pause
    /// <summary>
    /// Checks if a pause has been requested and updates the gameloop's status based on that
    /// </summary>
    public static void UpdatePause()
    {
        if (_objectsRequiringPause.Count > 0)
        {
            if (!IsRunning) return;
            IsRunning = false;
            OnPause?.Invoke();
        }
        else
        { 
            if (IsRunning) return;
                IsRunning = true;
                OnResume?.Invoke();
        }
    }

    /// <summary>
    /// Tells the GameLoop that the requester no longer needs the game to be paused.
    /// If the requester was the last object still requiring the game to be paused, the game resumes
    /// </summary>
    /// <param name="requester"></param>
    public static void TryResume(object requester)
    {
        _objectsRequiringPause.Remove(requester);
        UpdatePause();
    }
    /// <summary>
    /// Tells the cursor that the requester needs the cursor to be active and visible to function.
    /// Remember to call <see cref="TryFreeCursor(object)"/> when the requester no longer needs the cursor to work.
    /// </summary>
    /// <param name="requester"></param>
    public static void RequestPause(object requester)
    {
        if (!_objectsRequiringPause.Contains(requester))
            _objectsRequiringPause.Add(requester);
        UpdatePause();
    }


    public static void BindTimerToGamePause(Timer timer)
    {
        OnPause += timer.Pause;
        OnResume += timer.Resume;
    }
    public static void UnbindTimerFromGamePause(Timer timer)
    {
        OnPause -= timer.Pause;
        OnResume -= timer.Resume;
    }
    #endregion

    public static void SetState<T>() where T : GameState, new() => SetState(new T());
    private static void SetState(GameState newState)
    {
        if (newState == null) return;
        if (_currentState != null)
            _currentState.OnExit();
        _currentState = newState;
        _currentState.OnEnter();
        UnityEngine.Debug.Log("Current state is " + CurrentState.GetType().Name);
    }
    public static bool RequestTransition(StateTransitions transition = default)
    {
        if (!StateNotNull) return false;
        GameState next = CurrentState.FindNext(transition);
        if (next == CurrentState) return false;
        SetState(next);
        return true;
    }
}
