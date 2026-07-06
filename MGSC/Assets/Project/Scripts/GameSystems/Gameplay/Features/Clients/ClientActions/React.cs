using System;
using GameSystem;
using UnityEngine;

namespace Features
{
    public class React : BaseClientAction
    {
        public Action GoodReaction{get;set;}
        public Action BadReaction { get; set; }
        
        private Order _order;

        public React(ClientBehaviour owner, Order order) : base(owner)
        {
            _order = order;
        }
        
        public override void Perform()
        {
            base.Perform();

            if (_order.OrderRating.Stars >= 2)
            {
                GoodReaction?.Invoke();
            }
            else if(_order.OrderRating.Stars  <1)
            {
                BadReaction?.Invoke();
            }
        }
        
        
        
    }
}