using Cooking;
using GameSystem;
using Orders;
using System;
using UnityEngine;

namespace Clients
{

    public class OrderingCounter : Counter
    {
        #region Properties

        #endregion

        #region Fields

        private OrderManager _orderManager;

        #endregion

        #region Methods
        private void Start()
        {
            _orderManager = ReferenceManager.Instance.GetManager<OrderManager>();
        }

        public void TryAcceptOrder()
        {
            if (IsFree) return;
            AcceptOrder();
            return;
        }

        private void AcceptOrder()
        {
            _currentClient.AcceptOrder();
            _orderManager.NewOrder(_currentClient, GetOrder());
            Free();
        }

        /// <summary>
        /// Defines what order should be asked by the client. Can be modified to take the context into account.
        /// </summary>
        /// <returns></returns>
        private SO_Recipe GetOrder()
        {
            return _currentClient.Order;
        }

        #endregion

    }

}