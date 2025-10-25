using Cooking;
using System.Collections.Generic;
using UnityEngine;
using Orders;
using Clients;
using System;

namespace GameSystem
{
    public class OrderManager : IManager
    {
        #region Properties
        public List<Order> Orders => _activeOrders;
        public List<Order> PastOrders => _pastOrders;
        public Action<Order> OnNewOrder;
        public Action<Order> OnOrderResolved;

        #endregion

        #region Fields

        private List<Order> _activeOrders = new(); 
        private List<Order> _pastOrders = new(); 
        #endregion

        #region Methods

        public OrderManager()
        {
            Register();
        }

        public Order NewOrder(Client client, SO_Recipe recipe)
        {
            int id = _pastOrders.Count + _activeOrders.Count + 1;
            Order order = new Order(client, recipe, id);
            _activeOrders.Add(order);
            OnNewOrder?.Invoke(order);
            return order;
        }

        public void ResolveOrder(Order order, ServingInfo info)
        {
            StashOrder(order);
            OrderRating rating = OrderEvaluator.Evaluate(order, info);
            order.Resolve(info, rating);
            OnOrderResolved?.Invoke(order);
            Debug.Log("[OrderManager] ResolveOrder : The order with the recipe '" + order.Recipe.Name + "' has been completed in " + Mathf.Round((info.Timestamp - order.Timestamp)*100)/100 + "s for client " + order.Owner.name
                + ". Expected meal : '" + order.Recipe.Name + "'. Received meal : '" + info.Meal.Recipe.Name + "'. Money gained : " + rating.MoneyGained + ".");
            // Tell the money manager that money's been made
        }

        public void CancelOrder(Order order)
        {
            StashOrder(order);
            order.Cancel();
        }

        /// <summary>
        /// Sets the order as over, and stores it in memory
        /// </summary>
        /// <param name="order">Order to stash</param>
        private void StashOrder(Order order)
        {
            if (!_activeOrders.Contains(order))
            {
                Debug.LogError("[OrderManager] ResolveOrder : Trying to resolve an order that has either " +
                    "not been created by the OrderManager, or that is not active anymore");
            }
            _activeOrders.Remove(order);
            _pastOrders.Add(order);
        }
        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }

        #endregion

    }
}