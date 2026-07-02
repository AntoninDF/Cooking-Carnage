using GameSystem;
using System;
using System.Collections.Generic;

public static class GameLoop
{
    public static bool IsRunning { get; private set; } = true;
    public static Action OnPause { get; set; }
    public static Action OnResume { get; set; }

    private static List<object> _objectsRequiringPause = new();

    /// <summary>
    /// Checks if a pause has been requested and updates the gameloop's status based on that
    /// </summary>
    public static void Update()
    {
        if (_objectsRequiringPause.Count > 0)
            Pause();
        else
            Resume();
    }

    /// <summary>
    /// Tells the GameLoop that the requester no longer needs the game to be paused.
    /// If the requester was the last object still requiring the game to be paused, the game resumes
    /// </summary>
    /// <param name="requester"></param>
    public static void TryResume(object requester)
    {
        _objectsRequiringPause.Remove(requester);
        Update();
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
        Update();
    }

    /// <summary>
    /// Removes every request for the game to pause so that it can go back to playing.
    /// </summary>
    public static void ClearRequests()
    {
        _objectsRequiringPause.Clear();
        Update();
    }

    

    static void Pause()
    {
        if (!IsRunning) return;
        IsRunning = false;
        UnityEngine.Debug.Log("GameLoop paused");
        OnPause?.Invoke();
    }
    static void Resume()
    {
        if (IsRunning) return;
        IsRunning = true;
        UnityEngine.Debug.Log("GameLoop resumed");
        OnResume?.Invoke();
    }

    public static void BindTimer(Timer timer)
    {
        OnPause += timer.Pause;
        OnResume += timer.Resume;
    }
    public static void UnbindTimer(Timer timer)
    {
        OnPause -= timer.Pause;
        OnResume -= timer.Resume;
    }
}
