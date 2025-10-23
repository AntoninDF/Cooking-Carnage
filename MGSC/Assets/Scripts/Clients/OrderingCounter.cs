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

        public void TryAcceptOrder()
        {
            if (IsFree) return;
            AcceptOrder();
            return;
        }

        private void AcceptOrder()
        {
            if (!EnforceReferences()) return;
            Order newOrder = _orderManager.NewOrder(_currentClient, GetOrder());
            _currentClient.AcceptOrder(newOrder);
            Free();
        }

        /// <summary>
        /// Defines what order should be asked by the client. Can be modified to take the context into account.
        /// </summary>
        /// <returns></returns>
        private SO_Recipe GetOrder()
        {
            return _currentClient.RequestedRecipe;
        }

        private bool EnforceReferences()
        {
            if (_orderManager == null)
                _orderManager = ReferenceManager.Instance.GetManager<OrderManager>();
            if (_orderManager == null)
            {
                Debug.LogError("[OrderingCounter] : No order manager found.");
                return false;
            }
            return true;
        }

        #endregion

    }

}