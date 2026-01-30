using System;
using Controller;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace GameSystem
{
    public class ShiftManager : IManager
    {
        #region Properties
        public Action OnShiftFailed { get; set; }
        public Action OnShiftSucceeded { get; set; }
        
        
        public Action OnShiftBegin { get; set; }
        public Action OnShiftNoon { get; set; }
        public Action OnNextShift { get; set; }
        public Action OnShiftEnded { get; set; }
        public int ShiftCount => _shiftCount;
        public float ShiftDuration => _shiftDuration;
        public MoneyInfo CurrentRentPrice => new MoneyInfo(_currentRentPrice);
    
        #endregion

        #region Fields
    
        // Refacto : Ensure all these dependencies are necessary, and won't break the shiftManager if missing
        // Refacto : Ensure that features that depend on other Managers (or even ones that don't) are optional and can be toggled
        private DataManager _dataManager;
        private TimerManager _timerManager;
        private GameLoop _gameLoop;
        private ClientManager _clientManager;
        private UIManager _uiManager;
        private MoneyManager _moneyManager;
        private ItemManager _itemManager;

    
        private int _shiftCount = -1;
        private float _currentRentPrice = 0 ;
        private AnimationCurve _rentPriceCurve;
        private float _shiftDuration = 120;
        private float _noonDuration = 30;
        
        
    
        #endregion

        #region Methods
   
        public ShiftManager()
        {
            Register();
        }

        public ShiftManager(float shiftDuration,float shiftNoonDuration, AnimationCurve rentPriceCurve)
        {
            Register();
            SetShiftDuration(shiftDuration);
            SetShiftNoonDuration(shiftNoonDuration);
            SetRentPriceCurve(rentPriceCurve);
        }

        public void SetShiftCount(int count) => _shiftCount = count;
        public void SetShiftDuration(float duration) => _shiftDuration = duration;
        public void SetShiftNoonDuration(float duration) => _noonDuration = duration;
        public void SetRentPriceCurve(AnimationCurve rentPriceCurve) => _rentPriceCurve = rentPriceCurve;
        public void SetRentPrice(float price) => _currentRentPrice = price;
        public float GetCurrentRentPrice() => GetRentPrice(_shiftCount);
        public float GetRentPrice(int shiftCount) => _rentPriceCurve.Evaluate(shiftCount);
        public void UpdateRentPrice() { SetRentPrice(GetCurrentRentPrice()); }

        public void LaunchShift()=> LaunchShift(_shiftDuration - _noonDuration);
        void LaunchShift(float duration)
        {
            this.Log(LogType.Log, $"Launching new shift for {duration+_noonDuration}s");
            OnShiftBegin?.Invoke();
            _timerManager.StartTimer("shiftDuration", duration, () => LaunchShiftNoon(), false);
            _timerManager.StartTimer("shiftNoonDuration",_noonDuration,() => EndShift(), false);
            _timerManager.StopTimer("shiftNoonDuration");
        }

        void LaunchShiftNoon()
        {
            OnShiftNoon?.Invoke();
            this.Log(LogType.Log, $"{_noonDuration}s left before shift end");
            _timerManager.ResumeTimer("shiftNoonDuration");
        }

        void DelayEndShift(bool tutorialEnd = false)
        {
            _timerManager.StartTimer("shiftEndDelay", 2, () => TryEndShift(tutorialEnd), false);
        }

        void EndShift()
        {
            OnShiftEnded?.Invoke();
            TryEndShift();
        }

        public void TryEndShift(bool tutorialEnd = false)
        {
            int activeClients = _clientManager.CurrentClients.Count;
            
            this.Log(LogType.Log, $"End of the Shift triggered. Active clients: {activeClients}");

            if ((activeClients != 0 || _timerManager.IsTimerRunning("Disable client") || _timerManager.IsTimerRunning("Eating") || _timerManager.IsTimerRunning("Dying",false)) && !tutorialEnd)
            {
                DelayEndShift();
            }
            else
            {
                if (!tutorialEnd)
                {
                    if (_uiManager) _uiManager.PauseGame();
                    ResetAll();

                    // Pay rent
                    UpdateRentPrice();
                    if (_moneyManager.MoneyInfo < _currentRentPrice)
                    {
                        this.Log(LogType.Log,
                            $"Not enough money ({_moneyManager.MoneyInfo}) to pay the rent ({_currentRentPrice}€)");
                        _moneyManager.AddMoney(-_currentRentPrice);
                        OnShiftFailed?.Invoke();
                        return;
                    }
                    _moneyManager.AddMoney(-_currentRentPrice);

                    OnShiftSucceeded?.Invoke();
                }
                else
                {
                    this.Log("Tutorial has ended, skipping to the next shift...");
                    ResetAll();
                    UpdateRentPrice();
                    NextShift();
                
                }

            }
        }

        private void ResetAll()
        {
            this.Log("Reset everything for the next shift");
            _itemManager.StoreAllActive();

            foreach (var blender in Reference.GetAll<Cooking.Blender>())
            {
                blender.Reset();
            }

            foreach (var oven in Reference.GetAll<Cooking.Oven>())
            {
                oven.Reset();
            }
            var player = Reference.GetObject<PlayerController>();
            var playerStart = Reference.GetObject<PlayerStart>();
            if (player != null && playerStart != null)
            {
                player.transform.position = playerStart.transform.position;
                player.transform.rotation = playerStart.transform.rotation;
            }
        }

        public void NextShift()
        {
            _shiftCount++;

            SetupShift(_shiftCount);

            this.Log(LogType.Log, $"Shift {_shiftCount - 1} is over, launching shift {_shiftCount}");

            if (_uiManager) _uiManager.ResumeGame();

            OnNextShift?.Invoke();

            LaunchShift(_shiftDuration - _noonDuration);
        }
        /// <summary>
        /// Resets the specified shift without moving to the next one. 
        /// </summary>
        public void SetupShift(int shiftCount)
        {
            _currentRentPrice = _rentPriceCurve.Evaluate(shiftCount);

            if (shiftCount >= 1) // si ce n'est pas le tuto
            {
                // J'ai commenté la ligne ci-dessous parce que c'est bizarre que notre argent soit reset à chaque shift
                //ReferenceManager.Instance.GetManager<MoneyManager>().MoneyInfo = new MoneyInfo(0);
            }

            _dataManager.SaveGameData();

            _itemManager.StoreAllActive();
            _itemManager.SpawnInitialItems(shiftCount);
        }

        public void Register()
        {
            Reference.SetManager(this);
        }
        public void Unregister()
        {
            Reference.RemoveManager(this);
        }

        public void Init()
        {
            Reference.GetManager(out _dataManager);
            Reference.GetManager(out _timerManager);
            Reference.GetManager(out _gameLoop);
            Reference.GetManager(out _clientManager);
            Reference.GetManager(out _uiManager);
            Reference.GetManager(out _moneyManager);
            Reference.GetManager(out _itemManager);
        }

        public void PostInit()
        {
            //Debug.Log("[ShiftManager] : PostInit running (shift duration set to " + shiftDuration + "s)."); 
            this.Log(LogType.Log, $"PostInit running (shift duration set to {_shiftDuration}s)");
            
            _dataManager.LoadGlobalData();
            //LaunchShift(_shiftDuration); // Removed because it doesn't call everything the right way
            ResetAll();
            SetupShift(_shiftCount);
            
            if (_gameLoop)
            {
                OnShiftBegin += () => _gameLoop.ShiftBegin?.Invoke();
                OnShiftNoon += () => _gameLoop.ShiftNoon?.Invoke();
                OnShiftEnded += () => _gameLoop.ShiftEnd?.Invoke();
                OnShiftFailed += () => _gameLoop.RentGameOver?.Invoke();
            }
        }

        public void SkipTutorialShift()
        {
            if (_shiftCount == 0)
            {
                _shiftCount = 1;
                _dataManager.SaveGameData();
            }
        }
        #endregion
    }

}