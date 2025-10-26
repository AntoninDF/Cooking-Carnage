using Cooking;
using GameSystem;
namespace Orders
{
    public static class OrderEvaluator
    {
        public static OrderRating Evaluate(Order order, ServingInfo info)
        {
            float rating=0f;
            float moneyGained=MoneyEvaluator.Evaluate(order);
            float reputationGained=0f;

            OrderRating orderRating = new OrderRating(order, info.Recipe, rating, moneyGained, reputationGained);

            return orderRating;
        }
    }
}