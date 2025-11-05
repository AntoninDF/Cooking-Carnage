using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameSystem
{
   public class ShiftManager : IManager
{
    #region Properties

    public int ShiftCount => shiftCount;
    
    #endregion

    #region Fields
    
    private DataManager _dataManager;
    
    private int shiftCount;
    private float rentPrice = 0 ;
    private float shiftDuration = 30;
    
    #endregion

    #region Methods
   
    public ShiftManager()
    {
        Register();
        //Debug.Log($"[ShiftManager] Initialized. Shift duration = {shiftDuration}s");

        _dataManager = ReferenceManager.Instance.GetManager<DataManager>();
        _dataManager.LoadGlobalData();
        
        LaunchShift(shiftDuration);
    }

    public void SetShiftCount(int count)
    {
        shiftCount = count;
        //Debug.Log($"[ShiftManager] Shift count set to {shiftCount}");
    }

    void LaunchShift(float duration)
    {
        //Debug.Log($"[ShiftManager] Launching new shift for {duration}s...");
        ReferenceManager.Instance.GetManager<TimerManager>().StartTimer("shiftDuration", duration, () => EndShift()(), false);
    }

    void DelayEndShift()
    {
        //Debug.Log("[ShiftManager] Clients still active — delaying end of shift by 2s.");
        ReferenceManager.Instance
            .GetManager<TimerManager>()
            .StartTimer("shiftEndDelay", 2, () => EndShift()(), false);
    }

    Action EndShift()
    {
        return () =>
        {
            var clientManager = ReferenceManager.Instance.GetManager<ClientManager>();
            int activeClients = clientManager.CurrentClients.Count;

            //Debug.Log($"[ShiftManager] EndShift triggered. Active clients: {activeClients}");

            if (activeClients != 0)
            {
                DelayEndShift();
            }
            else
            {
                //Debug.Log("[ShiftManager] No clients remaining — ending shift and reloading scene.");
                ReferenceManager.Instance.GetManager<GameLoop>().StopRunning();

                if (ReferenceManager.Instance.GetManager<MoneyManager>().MoneyInfo < rentPrice)
                {
                    Debug.Log("Defeat");
                    return;
                }
                
                Debug.Log("WIP : Devrait ouvrir le menu recap puis amélioration");
                
                NextShift();
            }
        };
    }

    public void NextShift()
    {
        shiftCount++;
        
        ReferenceManager.Instance.GetManager<DataManager>().SaveGameData();
        ReferenceManager.Instance.ResetInstance();
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Register()
    {
        ReferenceManager.Instance.SetManager(this);
        //Debug.Log("[ShiftManager] Registered in ReferenceManager.");
    }
    
    #endregion
}

}