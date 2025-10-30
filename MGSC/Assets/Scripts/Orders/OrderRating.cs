using Cooking;

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

    }
}