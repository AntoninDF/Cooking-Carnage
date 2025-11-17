
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
        public MoneyInfo MoneyInfo { get; set; }
        public Action<float> OnMoneyChange { get; set; }
        #endregion

        #region Fields
        private DataManager _dataManager;
        private OrderManager _orderManager;
        private Dictionary<int, MoneyInfo> _moneyGainedPerShift = new();
        #endregion

        #region Methods

        public MoneyManager()
        {
            Register();
        }

        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }
        public void Unregister()
        {
            ReferenceManager.Instance.RemoveManager(this);
        }

        public void Init()
        {
            EnforceReferences();
            SetSubscriptionToAll(true);
        }

        private void EnforceReferences()
        {
            _dataManager = ReferenceManager.Instance.GetManager<DataManager>();
            _orderManager = ReferenceManager.Instance.GetManager<OrderManager>();
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

        public MoneyInfo GetMoneyGainedOnShift(int shiftCount) => _moneyGainedPerShift.ContainsKey(shiftCount) ? _moneyGainedPerShift[shiftCount] : new MoneyInfo();
        public MoneyInfo GetMoneyGainedThisShift() => GetMoneyGainedOnShift(_dataManager.ShiftCount);

        public void AddMoney(float amount)
        {
            MoneyInfo += amount;
            if (!_moneyGainedPerShift.ContainsKey(_dataManager.ShiftCount)) _moneyGainedPerShift[_dataManager.ShiftCount] = new MoneyInfo();
            _moneyGainedPerShift[_dataManager.ShiftCount] += amount;
            OnMoneyChange?.Invoke(amount);
        }
        #endregion
    }
}