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
        
        public static SO_Recipe Pick()
        {
            ClientManager clientManager = ReferenceManager.Instance.GetManager<ClientManager>();
            if (clientManager == null)
                throw new NullReferenceException("[OrderPicker] : client manager not found");

            SO_Recipe output;
            Dictionary<EClientType, int> clientCount = clientManager.GetSeenClientTypesCount();
            IEnumerable<SO_Recipe> recipes = FindCurrentRecipeBook().Recipes;
            if (recipes == null)
                throw new NullReferenceException("[OrderPicker] : No recipe list found");
            List<SO_Client> clientModels = ReferenceManager.AssetReferences.clientList;
            EnforceSeenClientsCountTypes(ref clientCount, clientModels.Select((model) => model.clientType));
            Dictionary<SO_Recipe, float> recipeWeights = SetupRecipeWeights(recipes, clientCount, clientModels);

            Dictionary<SO_Recipe, float> weights = SetupRecipeWeights(recipes, clientCount, clientModels);

            output = PickRandomRecipe(weights);

            return output;
        }

        private static SO_RecipeBook FindCurrentRecipeBook()
        {
            var ar = ReferenceManager.AssetReferences;
            var sm = ReferenceManager.Instance.GetManager<ShiftManager>() ;
            SO_RecipeBook book = ar.defaultRecipeBook;

            if (sm != null)
                foreach (var elm in ar.recipeBookByShiftCount)
                    if (sm.ShiftCount >= elm.shift) book = elm.recipeBook;

            OrderManager om = ReferenceManager.Instance.GetManager<OrderManager>();
            if (om != null) om.Log("The current recipe book is '" + book.name + "'");
            return book;
        }

        private static SO_Recipe PickRandomRecipe(Dictionary<SO_Recipe, float> weights)
        {
            float sum = 0;
            foreach (var kvp  in weights)
            {
                sum += kvp.Value;
            }
            float randomTarget = UnityEngine.Random.Range(0f, sum);

            //MonoBehaviour.print("[OrderPicker] : Random target is " + randomTarget + ".");
            //DebugWeights(weights);
            float current = 0;
            foreach (var kvp in weights)
            {
                current += kvp.Value;
                if (current >= randomTarget)
                    return kvp.Key;
            }
            MonoBehaviour.print("[OrderPicker] : No recipe picked. There might be a problem with the picking algorithm");
            return null;
        }

        private static Dictionary<EClientType, int> CountClients(IEnumerable<EClientType> pastClients)
        {
            MonoBehaviour.print("pastClients count : " + pastClients.Count());
            Dictionary<EClientType, int> clientCount = new();

            foreach (EClientType client in pastClients)
            {
                if (!clientCount.ContainsKey(client))
                    clientCount[client] = 0;
                clientCount[client] ++;
            }

            return clientCount;
        }

        /// <summary>
        /// Specifies how likely a recipe is to be picked
        /// </summary>
        /// <param name="recipes"></param>
        /// <param name="clientCount"></param>
        /// <param name="clients"></param>
        /// <returns></returns>
        private static Dictionary<SO_Recipe, float> SetupRecipeWeights(IEnumerable<SO_Recipe> recipes, Dictionary<EClientType, int> clientCount, IEnumerable<SO_Client> clients)
        {
            Dictionary<SO_Recipe, float> recipeWeights = new();
            Dictionary<SO_Recipe, List<EClientType>> matchyMatchy = MatchRecipesToClients(recipes, clients);
            //DebugMatchingTypes(matchyMatchy);

            foreach (var recipe in recipes)
            {
                recipeWeights[recipe] = recipe.probabilityRate;
                float increment = float.MaxValue;
                foreach (var kvp in clientCount) 
                {
                    if (matchyMatchy[recipe].Contains(kvp.Key))
                    {
                        increment = Mathf.Min(increment, kvp.Value * recipe.probabilityRateIncrement);
                    }
                }
                if (increment < float.MaxValue) recipeWeights[recipe] += increment;
            }

            return recipeWeights;
        }

        /// <summary>
        /// Specifies to which types of client a recipe is associated, based on the ingredients needed and who can drop them
        /// </summary>
        /// <param name="recipes"></param>
        /// <param name="clients"></param>
        /// <returns></returns>
        private static Dictionary<SO_Recipe, List<EClientType>> MatchRecipesToClients(IEnumerable<SO_Recipe> recipes, IEnumerable<SO_Client> clients)
        {
            Dictionary<SO_Recipe, List<EClientType>> output = new();

            foreach(SO_Recipe recipe in recipes)
            {
                output[recipe] = new List<EClientType>();
                foreach(SO_Client client in clients)
                {
                    if (output[recipe].Contains(client.clientType)) continue;
                    if(!recipe.Ingredients.Contains(client.loot)) continue;
                    output[recipe].Add(client.clientType);
                }
            }
            return output;
        }

        private static void EnforceSeenClientsCountTypes(ref Dictionary<EClientType, int> clientCount, IEnumerable<EClientType> clients)
        {
            foreach (EClientType clientType in clients)
            {
                if (!clientCount.ContainsKey(clientType))
                    clientCount[clientType] = 0;
            }
        }

        private static void DebugWeights(Dictionary<SO_Recipe, float> weights)
        {
            string text = "Recipe weights : \n";
            foreach(var kvp in weights)
            {
                text += kvp.Key.Name;
                text += " : ";
                text += kvp.Value.ToString();
                text += "\n";
            }
            MonoBehaviour.print(text);
        }
        private static void DebugMatchingTypes(Dictionary<SO_Recipe, List<EClientType>> matchyMatchy)
        {
            string text = "Recipe types : \n";
            foreach (var kvp in matchyMatchy)
            {
                text += kvp.Key.Name;
                text += " : [";
                foreach(var t  in kvp.Value)
                {
                    text += t;
                    text += " ; ";
                }
                text += "]\n";
            }
            MonoBehaviour.print(text);
        }
    }
}