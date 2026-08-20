using Clients;
using Cooking;
using GameSystem;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Upgrades;
using Random = UnityEngine.Random;

namespace Orders
{
    public static class OrderPicker
    {
        private const float UpgradeProbability = 1f / 3f; 
        private static int ForcedFullRecipeInterval
        {
            get
            {
                if (Reference.TryGetManager(out UpgradeManager um))
                {
                    var upgrade = um.GetUpgradeByType<MixtRecipePityThresholdUpgrade>();
                    if (upgrade != null)
                        return upgrade.CurrentPityThreshold;
                }
                return 15;
            }
        }

        public static float DefaultDrinkProbability = 0.5f; // 1 means there'll always be a drink ordered (if possible)
        public static float DrinkProbability => Mathf.Clamp01(DefaultDrinkProbability + Globals.Gameplay.DrinkProbabilityIncrease);

        private static int _selectionCount = 0; 
        private static Queue<SO_Recipe> _forcedRecipes = new();
        
        public static SO_Recipe Pick()
        {
            // If recipes have been explicitely enqueued, output them instead
            if (TryPickQueuedRecipe(out var recipe))
                return recipe;


            _selectionCount++;

            SO_RecipeBook         book         = FindCurrentRecipeBook();

            if ((ForcedFullRecipeInterval == 0 || _selectionCount % ForcedFullRecipeInterval == 0)
                && RecipeUnlockChecker.IsUnlocked(RecipeCategory.MixteTriple))
            {
                SO_Recipe forced = FindTripleDistinctRecipe(book);
                if (forced != null) return forced;
                Debug.LogWarning("[OrderPicker] Recette ABC introuvable pour le tirage forcé, retour au tirage normal.");
            }

            ClientManager clientManager = Reference.GetManager<ClientManager>();
            if (clientManager == null)
                throw new NullReferenceException("[OrderPicker] : client manager not found");

            Dictionary<EClientType, int> clientCount = clientManager.GetSeenClientTypesCount();
            List<SO_Recipe> recipes = new();
            string recipeSelectionDebug = "[OrderPicker] Recipe selection yields :\n";
            foreach (var curr in book.Recipes)
            {
                recipeSelectionDebug += curr.name;
                if (curr.TryGetRequirements(out var req))
                    recipeSelectionDebug += " (requires shift " + req.minShift + " and total money " + req.minMoney + "$)" ;
                recipeSelectionDebug += " : ";
                if (IsBaseRecipe(curr) && RecipeUnlockChecker.IsUnlocked(curr))
                {
                    recipes.Add(curr);
                    recipeSelectionDebug += "valid";
                }
                else
                    recipeSelectionDebug += "NOT valid";
                recipeSelectionDebug += "\n";


            } 

            if (!recipes.Any())
            {
                Debug.LogError($"[OrderPicker] Aucune recette de base débloquée (shift {Reference.GetManager<ShiftManager>().ShiftNumber}).");
                return book.FailedRecipe;
            }

            List<SO_Client> clientModels = Reference.AssetReferences.clientList;
            Dictionary<SO_Recipe, float> weights = SetupRecipeWeights(recipes, clientCount, clientModels);
            SO_Recipe picked = PickRandomRecipe(weights);
            return ApplyUpgrade(picked, book, SO_RecipeUnlockConfig.Instance);
        }

        public static bool CanOrderDrink()
        {
            if (!Globals.Authorizations.AllowMakingDrinks) return false;
            if (!RecipeUnlockChecker.IsUnlocked(RecipeCategory.Simple)) return false;
            return true;
        }
        /// <summary> Decides whether or not a drink should be served. If not, returns <see cref="EClientType.None"/>.
        /// Otherwise, returns the type of drink, computed based on the weights given by <see cref="GetDrinkProbabilityWeights"/> </summary>
        public static EClientType PickDrink()
        {
            if (!CanOrderDrink()) return EClientType.None;

            // Decide wether this order will contain a drink or not
            if (Random.value > DrinkProbability)
                return EClientType.None;

            // If it contains a drink, decide which one
            var weights = GetDrinkProbabilityWeights();
            float val = Random.value;
            float accumulatedVal = 0f;
            EClientType drink = EClientType.None;
            foreach (var kvp in weights)
            {
                if (kvp.Value == 0) continue; 
                if (val > accumulatedVal)
                    drink = kvp.Key;
                accumulatedVal += kvp.Value;
            }
            return drink;
        }
        public static void Test_PickDrink()
        {
            // Run the picking algorithm a few times to test it
            System.Collections.Generic.List<EClientType> drinks = new();
            int max = 1000;
            for (int i = 0; i < max; i++) drinks.Add(PickDrink());
            System.Collections.Generic.Dictionary<EClientType, int> dict = new();
            foreach (EClientType type in drinks)
            {
                if (!dict.ContainsKey(type)) dict[type] = 0;
                dict[type]++;
            }
            UnityEngine.Debug.Log("Ran drink picking for " + max + " times. Yielded the following result : " + dict.ToString_Detailed());
        }

        private static bool TryPickQueuedRecipe(out SO_Recipe recipe)
        {
            recipe = null;
            if (_forcedRecipes.Count == 0) return false;
            recipe = _forcedRecipes.Dequeue();
            return true;
        }
        public static void AddRecipeToQueue(SO_Recipe recipe)
        {
            if (recipe == null) return;
            _forcedRecipes.Enqueue(recipe);
        }

        private static bool IsBaseRecipe(SO_Recipe recipe)
        {
            return recipe.Ingredients.Count == recipe.Ingredients.Distinct().Count();
        }

        private static SO_Recipe FindTripleDistinctRecipe(SO_RecipeBook book)
        {
            return book.Recipes.FirstOrDefault(r => r.Ingredients.Count == 3 && r.Ingredients.Distinct().Count() == 3);
        }
        
        private static SO_Recipe ApplyUpgrade(SO_Recipe picked, SO_RecipeBook book, SO_RecipeUnlockConfig unlockConfig)
        {
            int total    = picked.Ingredients.Count;
            int distinct = picked.Ingredients.Distinct().Count();

            if (total == 1)
            {
                if (RecipeUnlockChecker.IsUnlocked(RecipeCategory.DoubleSupplement)
                    && UnityEngine.Random.value < UpgradeProbability)
                {
                    SO_Ingredient x       = picked.Ingredients[0];
                    SO_Recipe     doubled = book.FindByIngredients(new List<SO_Ingredient> { x, x });
                    if (doubled != null) return doubled;
                    Debug.LogWarning($"[OrderPicker] Recette doublée introuvable pour {picked.name}.");
                }
                return picked;
            }

            if (total == 2 && distinct == 2)
            {
                if (RecipeUnlockChecker.IsUnlocked(RecipeCategory.MixteSupplement)
                    && UnityEngine.Random.value < UpgradeProbability)
                {
                    SO_Ingredient x     = picked.Ingredients[0];
                    SO_Ingredient y     = picked.Ingredients[1];
                    bool          isXXY = UnityEngine.Random.value < 0.5f;

                    List<SO_Ingredient> tripled = isXXY
                        ? new List<SO_Ingredient> { x, x, y }
                        : new List<SO_Ingredient> { y, y, x };

                    SO_Recipe upgraded = book.FindByIngredients(tripled);
                    if (upgraded != null) return upgraded;
                    Debug.LogWarning($"[OrderPicker] Recette triplée introuvable pour {picked.name}.");
                }
                return picked;
            }

            return picked;
        }

        public static SO_RecipeBook FindCurrentRecipeBook()
        {
            var ar = Reference.AssetReferences;
            var sm = Reference.GetManager<ShiftManager>();
            SO_RecipeBook book = ar.DefaultRecipeBook;

            if (sm != null)
                foreach (var elm in ar.recipeBookByShiftCount)
                    if (sm.ShiftNumber >= elm.shift) book = elm.recipeBook;

            OrderManager om = Reference.GetManager<OrderManager>();
            if (om != null) om.Log("The current recipe book is '" + book.name + "'");
            return book;
        }

        private static SO_Recipe PickRandomRecipe(Dictionary<SO_Recipe, float> weights)
        {
            bool emptyWeights = false;
            float sum = 0;
            foreach (var kvp in weights)
                sum += kvp.Value;

            if (sum == 0)
            {
                sum = weights.Count;
                emptyWeights = true;
            }
            float randomTarget = UnityEngine.Random.Range(0f, sum);

            float current = 0;
            foreach (var kvp in weights)
            {
                current += (!emptyWeights) ? kvp.Value : 1;
                if (current >= randomTarget)
                    return kvp.Key;
            }
            Debug.LogWarning("[OrderPicker] : No recipe picked. There might be a problem with the picking algorithm");
            return null;
        }

        private static Dictionary<SO_Recipe, float> SetupRecipeWeights(IEnumerable<SO_Recipe> recipes, Dictionary<EClientType, int> clientCount, IEnumerable<SO_Client> clients)
        {
            Dictionary<SO_Recipe, float> recipeWeights = new();
            Dictionary<SO_Recipe, List<EClientType>> matchyMatchy = MatchRecipesToClients(recipes, clients);

            foreach (var recipe in recipes)
            {
                recipeWeights[recipe] = recipe.probabilityRate;
                float increment = float.MaxValue;
                foreach (var kvp in clientCount)
                {
                    if (matchyMatchy[recipe].Contains(kvp.Key))
                        increment = Mathf.Min(increment, kvp.Value * recipe.probabilityRateIncrement);
                }
                if (increment < float.MaxValue) recipeWeights[recipe] += increment;
            }

            return recipeWeights;
        }

        private static Dictionary<SO_Recipe, List<EClientType>> MatchRecipesToClients(IEnumerable<SO_Recipe> recipes, IEnumerable<SO_Client> clients)
        {
            Dictionary<SO_Recipe, List<EClientType>> output = new();

            foreach (SO_Recipe recipe in recipes)
            {
                output[recipe] = new List<EClientType>();
                foreach (SO_Client client in clients)
                {
                    if (output[recipe].Contains(client.clientType)) continue;
                    if (!recipe.Ingredients.Contains(client.loot)) continue;
                    output[recipe].Add(client.clientType);
                }
            }
            return output;
        }

        /// <summary>
        /// WIP, returns probability weights for each type of drink. <see cref="EClientType.None"/> will always have a probability of zero, and the sum of the other probabilities will always be 1.
        /// </summary>
        /// <returns></returns>
        static Dictionary<EClientType, float> GetDrinkProbabilityWeights()
        {
            Dictionary<EClientType, float> defaultWeights = new() { { EClientType.None, 0 }, { EClientType.A, 1f / 3f }, { EClientType.B, 1f / 3f }, { EClientType.C, 1f / 3f } };
            if (!Reference.TryGetManager(out ClientManager cmgr)) return defaultWeights;

            var clientWeights = cmgr.GetSeenClientTypesCount();
            float clientWeightSum = clientWeights.Values.Sum();

            Dictionary<EClientType, float> result = new();
            // Apply weights and increments
            foreach (EClientType enumValue in Enum.GetValues(typeof(EClientType)))
            {
                result[enumValue] = enumValue == EClientType.None ? 0 : 35;
                result[enumValue] += clientWeights[enumValue] * 20;
            }
            // Normalize everything so that the sum of values is equal to 1
            float sum = result.Values.Sum();
            foreach (EClientType enumValue in Enum.GetValues(typeof(EClientType)))
            {
                result[enumValue] = result[enumValue] / sum;
            }
            Debug.Assert(Mathf.Approximately(result.Values.Sum(), 1f), "OrderPicker.GetDrinkProbabilityWeights() didn't return a dict which's weights' sum is equl to one : " + result.ToString_Detailed());
            return result;
        }
    }
}