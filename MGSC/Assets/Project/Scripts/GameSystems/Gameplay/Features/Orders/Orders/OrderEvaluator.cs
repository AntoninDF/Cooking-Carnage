using Cooking;
using GameSystem;
using Unity.VisualScripting;
namespace Orders
{
    public static class OrderEvaluator
    {
        public static OrderRating Evaluate(Order order)
        {
            if (order.ServingInfo == default(ServingInfo))
            {
                throw new System.Exception("The member 'ServingInfo' of Order was not set to a specific value.");
            }
            float rating=0f;
            float moneyGained=MoneyEvaluator.Evaluate(order);
            float reputationGained=0f;

            OrderRating orderRating = new OrderRating(order, order.ServingInfo.Recipe, rating, moneyGained, reputationGained);

            return orderRating;
        }
    }
}