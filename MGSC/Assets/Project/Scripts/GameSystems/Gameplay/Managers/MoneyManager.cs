using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{

    public class MoneyManager : IManager
    {
        #region Properties
        public float MoneyInfo { get; set; }
        public Action<float> OnMoneyChange { get; set; }
        [GlobalEvent("Money/Money Gained")]
        public static Action OnMoneyGain;
        [GlobalEvent("Money/Money Lost")]
        public static Action OnMoneyLoss;
        #endregion

        #region Fields
        private OrderManager _orderManager;
        private Dictionary<int, float> _moneyGainedPerShift = new();
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

        public float GetMoneyGainedOnShift(int shiftCount) => _moneyGainedPerShift.ContainsKey(shiftCount) ? _moneyGainedPerShift[shiftCount] : 0f;
        public float GetMoneyGainedThisShift() => GetMoneyGainedOnShift(shiftNumber);
        public float GetMoneyOnPreviousShift()
        {
            if (shiftNumber <= 0) 
                return 0f;
            else
                return GetMoneyGainedOnShift(shiftNumber - 1);
        }

        public void AddMoney(float amount)
        {
            MoneyInfo += amount;
            if (!_moneyGainedPerShift.ContainsKey(shiftNumber))
                _moneyGainedPerShift[shiftNumber] = 0f;
            
            if(amount>=0) _moneyGainedPerShift[shiftNumber] += amount;
            
            OnMoneyChange?.Invoke(amount);

            if (amount > 0) OnMoneyGain?.Invoke();
            if (amount < 0) OnMoneyLoss?.Invoke();
            
            this.Log(LogType.Log, $"{amount}€ update");
        }
        #endregion
    }
}