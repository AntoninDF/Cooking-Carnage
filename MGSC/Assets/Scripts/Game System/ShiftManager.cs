using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameSystem
{
    public class ShiftManager : IManager
    {
        #region Properties

        public int ShiftCount => _shiftCount;
    
        #endregion

        #region Fields
    
        private DataManager _dataManager;
    
        private int _shiftCount;
        private float _currentRentPrice = 0 ;
        private AnimationCurve _rentPriceCurve;
        private float _shiftDuration = 120;
    
        #endregion

        #region Methods
   
        public ShiftManager()
        {
            Register();
        }
        public ShiftManager(float shiftDuration)
        {
            Register();
            SetShiftDuration(shiftDuration);
        }
        public ShiftManager(float shiftDuration, AnimationCurve rentPriceCurve)
        {
            Register();
            SetShiftDuration(shiftDuration);
            SetRentPriceCurve(rentPriceCurve);
        }

        public void SetShiftCount(int count) => _shiftCount = count;
        public void SetShiftDuration(float duration) => _shiftDuration = duration;
        public void SetRentPriceCurve(AnimationCurve rentPriceCurve) => _rentPriceCurve = rentPriceCurve;
        public void SetRentPrice(float price) => _currentRentPrice = price;
        public float GetCurrentRentPrice() => GetRentPrice(_shiftCount);
        public float GetRentPrice(int shiftCount)
        {
            return _rentPriceCurve.Evaluate(shiftCount);
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

                    if (ReferenceManager.Instance.GetManager<MoneyManager>().MoneyInfo < GetCurrentRentPrice())
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
            _shiftCount++;
            _currentRentPrice = _rentPriceCurve.Evaluate(_shiftCount);

            ReferenceManager.Instance.GetManager<DataManager>().SaveGameData();
            ReferenceManager.Instance.ResetInstance();
        
            // REFACTO : Arrêter de reload la scène, et ne reload que ce qui doit être reload
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
            //Debug.Log("[ShiftManager] Registered in ReferenceManager.");
        }
        public void Unregister()
        {
            ReferenceManager.Instance.RemoveManager(this);
        }

        public void Init()
        {
            _dataManager = ReferenceManager.Instance.GetManager<DataManager>();
        }

        public void PostInit()
        {
            //Debug.Log("[ShiftManager] : PostInit running (shift duration set to " + shiftDuration + "s)."); 
            _dataManager.LoadGlobalData();
            LaunchShift(_shiftDuration);
        }
        #endregion
    }

}