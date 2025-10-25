

using Clients;
using Orders;
using System;

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
        #endregion

        #region Methods

        public MoneyManager()
        {
            Register();
            EnforceReferences();
            SetSubscriptionToAll(true);
        }

        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
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
            OrderManager orderManager = ReferenceManager.Instance.GetManager<OrderManager>();
            if (subscribe) orderManager.OnOrderResolved += OnOrderResolved;
            else orderManager.OnOrderResolved -= OnOrderResolved;
        }

        private void OnOrderResolved(Order order)
        {
            EnforceReferences();
            AddMoney(MoneyEvaluator.Evaluate(order));
        }

        private void AddMoney(float amount)
        {
            MoneyInfo += amount;
            OnMoneyChange?.Invoke(amount);
        }
        #endregion
    }
}