using System;
using Clients;
using GameSystem;
using UnityEngine;

namespace Features
{
    public class React : BaseClientAction
    {
        public Action GoodReaction{get;set;}
        public Action BadReaction { get; set; }
        public Action PourboireReaction{get;set;}
        
        private Order _order;
        private Client _client;

        public React(ClientBehaviour owner, Order order) : base(owner)
        {
            _order = order;
            _client = owner.Owner as Client;
        
        }
        
        public override void Perform()
        {
            base.Perform();
            
            if (_client.IsAngry || !_order.OrderRating.IsCorrectRecipe)
            {
                BadReaction?.Invoke();
            }
            else 
            {
                GoodReaction?.Invoke();

                if (_order.OrderRating.Stars > 1)
                {
                    PourboireReaction?.Invoke();
                }
            }
           
        }
        
        
        
    }
}