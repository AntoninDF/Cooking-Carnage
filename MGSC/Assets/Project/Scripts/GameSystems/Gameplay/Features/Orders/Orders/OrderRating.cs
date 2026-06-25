using Cooking;
using System;
using System.Collections.Generic;

namespace Orders
{
    /// <summary>
    /// Gives information on the result of an order
    /// </summary>
    public struct OrderRating
    {
        public Order RatedOrder { get; private set; }
        public SO_Recipe TargetRecipe { get; private set; }
        public SO_Recipe ActualRecipe { get; private set; }
        public bool IsCorrectRecipe => TargetRecipe == ActualRecipe && ActualRecipe != null;
        
        public int Stars { get; private set; }  
        public float MoneyGained { get; private set; }

        public OrderRating
            (
            Order ratedOrder,
            SO_Recipe servedRecipe,
            int stars,
            float moneyGained=0f
            )
        {
            RatedOrder = ratedOrder;
            TargetRecipe = ratedOrder.Recipe;
            ActualRecipe = servedRecipe;
            Stars = stars;
            MoneyGained = moneyGained;
        }

        public static bool operator ==( OrderRating left, OrderRating right ) { return left.Equals( right ); }
        public static bool operator !=( OrderRating left, OrderRating right ) { return !left.Equals( right ); }

        public override bool Equals(object obj)
        {
            return obj is OrderRating rating &&
                   EqualityComparer<Order>.Default.Equals(RatedOrder, rating.RatedOrder) &&
                   EqualityComparer<SO_Recipe>.Default.Equals(TargetRecipe, rating.TargetRecipe) &&
                   EqualityComparer<SO_Recipe>.Default.Equals(ActualRecipe, rating.ActualRecipe) &&
                   IsCorrectRecipe == rating.IsCorrectRecipe &&
                   Stars == rating.Stars &&
                   MoneyGained == rating.MoneyGained;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RatedOrder, TargetRecipe, ActualRecipe, Stars , IsCorrectRecipe, MoneyGained);
        }
    }
}