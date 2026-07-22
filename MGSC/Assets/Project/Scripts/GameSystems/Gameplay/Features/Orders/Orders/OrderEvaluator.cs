using Cooking;
using UnityEngine;

namespace Orders
{
    public static class OrderEvaluator
    {
        public static float WrongRecipePrice { get; set; } = 5f;
        public static float PlainJellyPrice { get; set; } = 1f;

        private const float TipMultiplier0Star = 0f; // 0 star => No tip
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

            bool  isCorrectRecipe = IsCorrectRecipe(order);
            float basePrice       = CalculateBasePrice(order, isCorrectRecipe);
            
            int   stars           = isCorrectRecipe ? CalculateStars(order) : 0;
            float tip             = isCorrectRecipe ? CalculateTip(basePrice, stars) : 0f;
            float totalGained     = basePrice + tip;

            Debug.Log($"BasePrice is {basePrice}" +
                      $" stars is {stars}" +
                      $" tip is {tip}" +
                      $" total is {totalGained}" +
                      $" isCorrectRecipe is {isCorrectRecipe}");

            return new OrderRating(order, order.ServingInfo.Recipe, stars, totalGained);
        }

        private static bool IsCorrectRecipe(Order order)
        {
            return order.Recipe != null && order.Recipe == order.ServingInfo.Recipe;
        }

        private static float CalculateBasePrice(Order order, bool isCorrectRecipe)
        {
            Servable meal = order.ServingInfo.Meal;

            if (meal == null || meal.Recipe == null)
                return PlainJellyPrice;

            // NOTE : Les recettes vides et brûlées ont déjà leur prix de vente dans le ppté Price
            
            if (!isCorrectRecipe && meal.Recipe.name != "Failed recipe" && meal.Recipe.name != "Empty recipe")
                return WrongRecipePrice;
            
            return meal.Recipe.Price;
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
            0 => TipMultiplier0Star * basePrice,
            _ => 0f * basePrice
        };

    
    }
}