using System;

namespace Features
{
    public class OrderingCounter : Feature
    {
        public Action<Order> OnNewOrder { get; set; }


        public OrderingCounter(IFeatureCollection owner) : base(owner)
        {
        }

        public Order GetNewOrder()
        {
            var result = OrderBuilder
                .New()
                .Build();
            OnNewOrder?.Invoke(result);
            return result;
        }
    }
}