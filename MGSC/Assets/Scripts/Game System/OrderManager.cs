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
            order.ServingInfo = info;
            OrderRating rating = OrderEvaluator.Evaluate(order);
            order.Resolve(info, rating);
            OnOrderResolved?.Invoke(order);
            Debug.Log("[OrderManager] ResolveOrder : Order " + (order.OrderRating.IsCorrectRecipe ? "successfully": "poorly") + " completed in " + Mathf.Round((info.Timestamp - order.Timestamp)*100)/100 + "s for client " + order.Owner.name
                + ". Money gained : " + MoneyEvaluator.Evaluate(order) + ". Expected '" + order.Recipe.Name + "', received '" + info.Meal.Recipe.Name + "'.");
            // Tell the money manager that money's been made
            StashOrder(order);
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
                Debug.LogError("[OrderManager] ResolveOrder : Trying to stash an order that has either " +
                    "not been created by the OrderManager, or that is not active anymore");
            }
            _activeOrders.Remove(order);
            _pastOrders.Add(order);
        }
        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }
        public void Unregister()
        {
            ReferenceManager.Instance.RemoveManager(this);
        }
        public void Init() { }
        #endregion

    }
}