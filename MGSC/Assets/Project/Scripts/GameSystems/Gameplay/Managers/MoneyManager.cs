using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameSystem
{

    public class MoneyManager : IManager
    {
        #region Properties
        public float MoneyInfo { get; private set; }
        public float LastMoneyDelta { get; set; }
        public Action<float> OnMoneyChange { get; set; }
        [GlobalEvent("Money/Money Gained")]
        public static Action OnMoneyGain;
        [GlobalEvent("Money/Money Lost")]
        public static Action OnMoneyLoss;
        #endregion

        #region Fields
        private OrderManager _orderManager;
        private Dictionary<int, List<float>> _moneyTransactionsPerShift = new();
        private int shiftNumber => Globals.Gameplay.ShiftNumber;
        #endregion

        #region Methods

        public MoneyManager()
        {
            Register();
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
            EnforceReferences();
            SetSubscriptionToAll(true);
        }

        private void EnforceReferences()
        {
            Reference.GetManager(out _orderManager);
        }

        private void SetSubscriptionToAll(bool subscribe)
        {
            SetSubscriptionToServings(subscribe);
        }

        private void SetSubscriptionToServings(bool subscribe)
        {
            if (subscribe) _orderManager.OnOrderResolved += OnOrderResolved;
            else _orderManager.OnOrderResolved -= OnOrderResolved;
        }

        private void OnOrderResolved(Order order)
        {
            EnforceReferences();
            AddMoney(order.OrderRating.MoneyGained);
        }


        public float GetMoneyGainedSinceBeginning()
        {
            List<float> gains = new();
            foreach (var list in _moneyTransactionsPerShift.Values)
                foreach (float value in list)
                    if (value > 0)
                        gains.Add(value);
            return gains.Sum();
        }

        public float GetMoneyGainedOnShift(int shiftNumber)
            => _moneyTransactionsPerShift.ContainsKey(shiftNumber) ?
            _moneyTransactionsPerShift[shiftNumber].Where(value => value > 0).Sum() :
            0f;
        /// <summary> Will return a negative value </summary>
        public float GetMoneyLostOnShift(int shiftNumber)
            => _moneyTransactionsPerShift.ContainsKey(shiftNumber) ?
            _moneyTransactionsPerShift[shiftNumber].Where(value => value < 0).Sum() :
            0f;
        public List<float> GetMoneyTransactionsOnShift(int shiftNumber)
            => _moneyTransactionsPerShift.ContainsKey(shiftNumber) ?
            _moneyTransactionsPerShift[shiftNumber] :
            new();
        public float GetMoneyGainedThisShift() => GetMoneyGainedOnShift(shiftNumber);
        /// <summary> Will return a negative value </summary>
        public float GetMoneyLostThisShift() => GetMoneyLostOnShift(shiftNumber);
        public List<float> GetMoneyTransactionsThisShift() => GetMoneyTransactionsOnShift(shiftNumber);
        public float GetMoneyAtTheBeginningOfThisShift()
            => MoneyInfo - GetMoneyTransactionsThisShift().Sum();
        public float GetMoneyOnPreviousShift()
        {
            if (shiftNumber <= 0) 
                return 0f;
            else
                return GetMoneyGainedOnShift(shiftNumber - 1);
        }

        public void AddMoney(float amount)
        {
            Debug.Log("Money gained : " + amount);
            MoneyInfo += amount;
            if (!_moneyTransactionsPerShift.ContainsKey(shiftNumber))
                _moneyTransactionsPerShift[shiftNumber] = new();
            
            _moneyTransactionsPerShift[shiftNumber].Add(amount);
            
            OnMoneyChange?.Invoke(amount);

            if (amount > 0)
            {
                OnMoneyGain?.Invoke();
                Debug.Log("OnMoneyGain called");
            }
            if (amount < 0) OnMoneyLoss?.Invoke();

            LastMoneyDelta = amount;
            
            this.Log(LogType.Log, $"{amount}€ update");
        }

        internal void SetCurrentMoney(float money)
        {
            MoneyInfo = money;
        }
        #endregion
    }
}