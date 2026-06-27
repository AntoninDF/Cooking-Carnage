using GameSystem;
using System.Timers;
using UnityEngine;

public class Todelete : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Reference.TryGetManager(out ShiftManager shiftMgr))
        {
            shiftMgr.OnShiftBegin += () => Debug.Log("Shift begin (" + Mathf.Round(Time.timeSinceLevelLoad * 10f) / 10f + ")");
            shiftMgr.OnShiftEnded += () => Debug.Log("Shift ended (" + Mathf.Round(Time.timeSinceLevelLoad * 10f) / 10f + ")");
            shiftMgr.OnShiftNoon += () => Debug.Log("Shift noon (" + Mathf.Round(Time.timeSinceLevelLoad * 10f) / 10f + ")");
        }
        TimerManager.StartTimer("Shift test", Globals.Gameplay.ShiftDuration, null, false, 
                elapsed =>
                {
                    if (Mathf.Round(elapsed-0.5f) + Time.deltaTime > elapsed)
                        Debug.Log(Mathf.Round(elapsed).ToString() + "s elapsed");
                });
        
    }
}
