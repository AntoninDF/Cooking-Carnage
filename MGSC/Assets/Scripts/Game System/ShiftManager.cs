using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameSystem
{
    public class ShiftManager : IManager
    {
        #region Properties
        public Action OnShiftFailed { get; set; }
        public Action OnNextShift { get; set; }
        public Action OnShiftEnded { get; set; }
        public int ShiftCount => _shiftCount;
        public float ShiftDuration => _shiftDuration;
        public float CurrentRentPrice => _currentRentPrice;
    
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
        public float GetRentPrice(int shiftCount) => _rentPriceCurve.Evaluate(shiftCount);
        public void UpdateRentPrice() { SetRentPrice(GetCurrentRentPrice()); }

        void LaunchShift(float duration)
        {
            //Debug.Log($"[ShiftManager] Launching new shift for {duration}s...");
            ReferenceManager.Instance.GetManager<TimerManager>().StartTimer("shiftDuration", duration, () => EndShift(), false);
        }

        void DelayEndShift()
        {
            //Debug.Log("[ShiftManager] Clients still active — delaying end of shift by 2s.");
            ReferenceManager.Instance
                .GetManager<TimerManager>()
                .StartTimer("shiftEndDelay", 2, () => EndShift(), false);
        }

        void EndShift()
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
                OnShiftEnded?.Invoke();
                //Debug.Log("[ShiftManager] No clients remaining — ending shift and reloading scene.");
                ReferenceManager.Instance.GetManager<GameLoop>().StopRunning();
                
                UpdateRentPrice();
                if (ReferenceManager.Instance.GetManager<MoneyManager>().MoneyInfo < _currentRentPrice)
                {
                    Debug.Log("Defeat");
                    OnShiftFailed?.Invoke();
                    return;
                }
                
                Debug.Log("WIP : Devrait ouvrir le menu recap puis amélioration");
                
                //NextShift();
            }
        }

        // REFACTO
        public void NextShift()
        {
            _shiftCount++;
            _currentRentPrice = _rentPriceCurve.Evaluate(_shiftCount);

            ReferenceManager.Instance.GetManager<DataManager>().SaveGameData();
            ReferenceManager.Instance.ResetInstance();
            
            OnNextShift?.Invoke();
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