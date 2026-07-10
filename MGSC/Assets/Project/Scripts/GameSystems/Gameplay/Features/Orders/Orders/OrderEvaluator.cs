using Cooking;
using UnityEngine;

namespace Orders
{
    public static class OrderEvaluator
    {
        public  static float OvercookedMealPrice { get; set; }
        public static float PlainJellyPrice { get; set; }

        private const float TipMultiplier1Star = 0.2f; // 1 star => + 20% 
        private const float TipMultiplier2Star = 0.5f; // 2 star => + 50%

        private static float Threshold1Star => 1 - Globals.Gameplay.Threshold1Star;
        private static float Threshold2Star => 1 - Globals.Gameplay.Threshold2Star;
        
        
        public static OrderRating Evaluate(Order order)
        {
            if (!order.ServingInfo.HasBeenServed)
            {
                return new OrderRating(order, order.Recipe, 0);
            }

            float basePrice   = CalculateBasePrice(order);
            int   stars       = CalculateStars(order);
            float tip         = CalculateTip(basePrice, stars);
            float totalGained = basePrice + tip;

            return new OrderRating(order, order.ServingInfo.Recipe, stars, totalGained);
        }
        
        private static float CalculateBasePrice(Order order)
        {
            Servable meal = order.ServingInfo.Meal;

            if (meal == null || meal.Recipe == null)
                return PlainJellyPrice;

            float basePrice  = RecipePriceTable.GetBasePrice(meal.Recipe);

            return basePrice ;
        }

        private static int CalculateStars(Order order)
        {
            float elapsedSeconds = order.ServingInfo.Timestamp - order.SeatedTimestamp;
            float patience       = Mathf.Clamp01(1f - elapsedSeconds / order.PatienceDuration);

            if (patience >= Threshold2Star) return 2;
            if (patience >= Threshold1Star) return 1;
            return 0;
        }

        private static float CalculateTip(float basePrice, int stars) => stars switch
        {
            2 => TipMultiplier2Star * basePrice,
            1 => TipMultiplier1Star * basePrice,
            _ => 0f
        };
        
    }
}