using GameSystem;
using System;

public static class GameLoop
{
    public static bool IsRunning { get; private set; }
    public static Action OnPause { get; set; }
    public static Action OnResume { get; set; }

    // This method can be extended later to check if the game can be paused before actually doing it
    public static bool RequestPause()
    {
        Pause();
        return true;
    }
    // This method can be extended later to check if the game can be paused before actually doing it
    public static bool RequestResume()
    {
        Resume();
        return true;
    }

    static void Pause()
    {
        IsRunning = false;
        OnPause?.Invoke();
    }
    static void Resume()
    {
        IsRunning = true;
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
