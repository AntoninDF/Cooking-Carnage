using Cooking;
using System.Collections.Generic;
using UnityEngine;
using Orders;
using Clients;

namespace GameSystem
{
    public class OrderManager : Manager
    {
        #region Properties
        public List<Order> Orders => _orders;


        #endregion

        #region Fields

        private List<Order> _orders; 
        #endregion

        #region Methods

        public OrderManager() : base() { }

        public Order NewOrder(Client client, SO_Recipe recipe)
        {
            Order order = new Order(client, recipe);
            _orders.Add(order);

            return order;
        }

        #endregion

    }
}