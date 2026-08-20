using System;
using System.Collections.Generic;
using Clients;
using Cooking;
using NUnit.Framework;
using Orders;

namespace Features
{
    public class OrderingCounter : Feature
    {
        public Action<Order> OnNewOrder { get; set; }
        public Action<SO_Recipe> DisplayRecipe { get; set; }


        public OrderingCounter(IFeatureCollection owner) : base(owner)
        {
        }

        public Order GetNewOrder(Client client)
        {
            SO_Recipe recipe = Orders.OrderPicker.Pick();

            var order = Reference.GetManager<GameSystem.OrderManager>().NewOrder(client, recipe);
            order.Drink = OrderPicker.PickDrink();

            OnNewOrder?.Invoke(order);
            DisplayRecipe?.Invoke(order.Recipe);
            return order;
        }
    }
}