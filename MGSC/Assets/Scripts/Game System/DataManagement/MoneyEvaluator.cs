

using Cooking;
using Orders;
using UnityEngine;

namespace GameSystem
{

    public static class MoneyEvaluator
    {
        #region Properties

        public static float WrongMealPrice { get; set; }
        public static float OvercookedMealPrice { get; set; }
        public static float RawMealPrice { get; set; }

        #endregion



        #region Methods
        public static float Evaluate(Order order)
        {
            float output;
            if (order.ServingInfo.Meal == null || order.ServingInfo.Meal.Recipe == null)
            {
                output = RawMealPrice;
                return output;
            }
            if (order.Recipe != order.ServingInfo.Recipe) output = WrongMealPrice;
            else if (order.ServingInfo.Meal.CookStatus == Cooking.ECookStatus.Overcooked) output = OvercookedMealPrice;
            else output = order.ServingInfo.Recipe.Price; // 10€ if served in less than 20s. 5€ if served in 50s

            return output;
        }
        #endregion
    }
}