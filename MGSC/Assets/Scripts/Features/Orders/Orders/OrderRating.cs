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
        public float Rating { get; private set; }
        public float MoneyGained { get; private set; }
        public float ReputationGained { get; private set; }

        public OrderRating
            (
            Order ratedOrder,
            SO_Recipe servedRecipe,
            float rating=0f,
            float moneyGained=0f,
            float reputationGained=0f
            )
        {
            RatedOrder = ratedOrder;
            TargetRecipe = ratedOrder.Recipe;
            ActualRecipe = servedRecipe;
            Rating = rating;
            MoneyGained = moneyGained;
            ReputationGained = reputationGained;
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
                   Rating == rating.Rating &&
                   MoneyGained == rating.MoneyGained &&
                   ReputationGained == rating.ReputationGained;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RatedOrder, TargetRecipe, ActualRecipe, IsCorrectRecipe, Rating, MoneyGained, ReputationGained);
        }
    }
}