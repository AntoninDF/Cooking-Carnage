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
        
        private const float Threshold1Star = 0.5f;  // 1 star => served in the first 50% of the time
        private const float Threshold2Star = 0.66f; // 2 star => served in the first 33% of the time
        public static OrderRating Evaluate(Order order)
        {
            if (order.ServingInfo == default(ServingInfo))
            {
                return new OrderRating(order, order.Recipe, 0);
            }
            
            float basePrice = CalculateBasePrice(order);
            int stars = CalculateStars(order);
            float tip = CalculateTip(basePrice, stars);
            float totalGained = basePrice + tip;

            OrderRating orderRating = new OrderRating(order, order.ServingInfo.Recipe, stars , totalGained);

            return orderRating;
        }

        private static float CalculateBasePrice(Order order)
        {
            Servable meal = order.ServingInfo.Meal;
            
            if(meal == null || meal.Recipe == null)
                return PlainJellyPrice;
            
            /*
            if(meal.OverCookable.Transformed)
                return RecipeFinder.AllRecipes.FailedRecipe.Price;
            Debug.Log("Calculate base price : Didn't overcook");
            
            if(order.Recipe != order.ServingInfo.Recipe)
                return Globals.Gameplay.WrongMealPrice;
            Debug.Log("Calculate base price : Meal was correct");
            */
            
            return meal.Recipe.Price;
        }

        private static int CalculateStars(Order order)
        {
            if (order.ServingInfo.Equals(default(ServingInfo)))
                return 0;
            
            float elapsedSeconds = order.ServingInfo.Timestamp - order.SeatedTimestamp;
            float patience = Mathf.Clamp01(1f - elapsedSeconds / order.PatienceDuration);

            
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