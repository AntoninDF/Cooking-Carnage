using Cooking;
using System.Collections.Generic;
using UnityEngine;
using Orders;
using Clients;

namespace GameSystem
{
    public class OrderManager : IManager
    {
        #region Properties
        public List<Order> Orders => _activeOrders;
        public List<Order> PastOrders => _pastOrders;


        #endregion

        #region Fields

        private List<Order> _activeOrders; 
        private List<Order> _pastOrders; 
        #endregion

        #region Methods

        public OrderManager()
        {
            Register();
        }

        public Order NewOrder(Client client, SO_Recipe recipe)
        {
            Order order = new Order(client, recipe);
            _activeOrders.Add(order);

            return order;
        }

        public void ResolveOrder(Order order, ServingInfo info)
        {
            if (!_activeOrders.Contains(order))
            {
                Debug.LogError("[OrderManager] ResolveOrder : Trying to resolve an order that has either " +
                    "not been created by the OrderManager, or that is not active anymore");
            }
            OrderRating rating = OrderEvaluator.Evaluate(order, info);
            _activeOrders.Remove(order);
            _pastOrders.Add(order);

            Debug.Log("[OrderManager] ResolveOrder : The order with the '" + order.Recipe.Name + "' recipe has been completed in " + (info.Timestamp - order.Timestamp) + "s for client " + order.Owner.name);
            // Tell the money manager that money's been made
        }

        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }

        #endregion

    }
}