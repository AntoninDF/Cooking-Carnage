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
        public Action<Order> OnOrderCancelled;

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
            
            this.Log(LogType.Log, "Order n°"+id+ " generated for "+client.ToString()+", Recipe : "+recipe);
            
            _activeOrders.Add(order);
            OnNewOrder?.Invoke(order);
            
            this.Log(LogType.Log, "*New Order* Feedbacks Invoked");
            
            return order;
        }

        public void ResolveOrder(Order order, ServingInfo info)
        {
            order.Resolve(info);

            this.Log(LogType.Log, "Order n°" + order.ID + " Resolved "
                                  + (order.OrderRating.IsCorrectRecipe ? "successfully" : "poorly")
                                  + " completed in " + Mathf.Round((info.Timestamp - order.Timestamp) * 100) / 100
                                  + "s for client " + order.Owner.name + ". Money gained : "
                                  + order.OrderRating.MoneyGained + ". Expected '" + order.Recipe.Name
                                  + "', received '" + info.Recipe?.Name + "'.");

            OnOrderResolved?.Invoke(order);
            Reference.GetManager<MoneyManager>().AddMoney(order.OrderRating.MoneyGained);
            StashOrder(order);
        }

        public void CancelOrder(Order order)
        {
            StashOrder(order);
            order.Cancel();
            OnOrderCancelled?.Invoke(order);
            this.Log(LogType.Log, "*Order Cancelled* Feedbacks Invoked");
        }

        /// <summary>
        /// Sets the order as over, and stores it in memory
        /// </summary>
        /// <param name="order">Order to stash</param>
        private void StashOrder(Order order)
        {
            if (!_activeOrders.Contains(order))
            {
                this.Log(LogType.Error, "Trying to stash an order that has either not been created by the OrderManager, or that is not active anymore");
                return;
            }
            _activeOrders.Remove(order);
            _pastOrders.Add(order);
            
            this.Log(LogType.Log, "Order n°"+order.ID+ " has been stashed");
        }

        public Order GetOrderByID(int id)
        {
            foreach (Order order in _pastOrders)
                if (order.ID == id)
                    return order;
            foreach (Order order in _activeOrders)
                if (order.ID == id)
                    return order;
            return null;
        }
        public void Register()
        {
            Reference.SetManager(this);
        }
        public void Unregister()
        {
            Reference.RemoveManager(this);
        }
        public void Init() { }
        #endregion

    }
}