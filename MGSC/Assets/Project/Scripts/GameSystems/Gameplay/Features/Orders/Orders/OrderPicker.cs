using Clients;
using Cooking;
using GameSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Orders
{
    public static class OrderPicker
    {
        private const float UpgradeProbability = 1f / 3f; 
        private const int ForcedFullRecipeInterval = 15;   

        private static int _selectionCount = 0;
        private static Queue<SO_Recipe> _forcedRecipes = new();

        public static SO_Recipe Pick()
        {
            // If recipes have been explicitely enqueued, output them instead
            if (TryPickQueuedRecipe(out var recipe))
                return recipe;


            _selectionCount++;

            SO_RecipeBook         book         = FindCurrentRecipeBook();

            if (_selectionCount % ForcedFullRecipeInterval == 0
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
            IEnumerable<SO_Recipe> recipes = book.Recipes.Where(IsBaseRecipe).Where(r => RecipeUnlockChecker.IsUnlocked(r));

            if (!recipes.Any())
            {
                Debug.LogError($"[OrderPicker] Aucune recette de base débloquée (shift {Reference.GetManager<ShiftManager>().ShiftNumber}).");
                return book.FailedRecipe;
            }

            List<SO_Client> clientModels = Reference.AssetReferences.clientList;
            EnforceSeenClientsCountTypes(ref clientCount, clientModels.Select(model => model.clientType));
            Dictionary<SO_Recipe, float> weights = SetupRecipeWeights(recipes, clientCount, clientModels);

            SO_Recipe picked = PickRandomRecipe(weights);
            return ApplyUpgrade(picked, book, SO_RecipeUnlockConfig.Instance);
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

        private static void EnforceSeenClientsCountTypes(ref Dictionary<EClientType, int> clientCount, IEnumerable<EClientType> clients)
        {
            foreach (EClientType clientType in clients)
                if (!clientCount.ContainsKey(clientType))
                    clientCount[clientType] = 0;
        }
    }
}