
using Orders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace GameSystem
{

    public class MoneyManager : IManager
    {
        #region Properties
        public float MoneyInfo { get; set; }
        public Action<float> OnMoneyChange { get; set; }
        #endregion

        #region Fields
        private SaveManager _dataManager;
        private OrderManager _orderManager;
        private Dictionary<int, float> _moneyGainedPerShift = new();
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
            Reference.GetManager(out _dataManager);
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
        public float GetMoneyGainedThisShift() => GetMoneyGainedOnShift(_dataManager.ShiftCount);

        public void AddMoney(float amount)
        {
            MoneyInfo += amount;
            if (!_moneyGainedPerShift.ContainsKey(_dataManager.ShiftCount))
                _moneyGainedPerShift[_dataManager.ShiftCount] = 0f;
            
            if(amount>=0) _moneyGainedPerShift[_dataManager.ShiftCount] += amount;
            
            OnMoneyChange?.Invoke(amount);
            
            this.Log(LogType.Log, $"{amount}€ update");
        }
        #endregion
    }
}