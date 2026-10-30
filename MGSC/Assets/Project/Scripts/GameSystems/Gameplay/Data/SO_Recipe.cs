using Orders;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cooking
{

    /// <summary>
    /// Defines the set of data needed for a meal
    /// </summary>
    [CreateAssetMenu(fileName = "Recipe", menuName = "Scriptable Objects/Recipe")]
    public class SO_Recipe : ScriptableObject
    {
        #region Properties
        public string Name { get => name; set { name = value; } }
        #endregion

        #region Fields
        [Tooltip("The name of the recipe that will be displayed in game")]
        [SerializeField] new private string name;

        [Tooltip("The item prefab should contain the graphics and a meal component")]
        public ModelController mealPrefab;
        [Tooltip("How much money does a recipe give")]
        [FormerlySerializedAs("Price")]
        public float DefaultPrice;
        public float DefaultDrinkBonus;
        public float Price => DefaultPrice * Globals.BalancingWeights.RecipePriceMultiplier ;

        [Tooltip("How likely this recipe is to be ordered by default. The unit is arbitrary and depends on the other recipes' probability rates.")]
        public float probabilityRate = 1f;
        [Tooltip("How much higher the probability rate will get each time a corresponding client is seen.")]
        public float probabilityRateIncrement = 1f;
        [Tooltip("The list of ingredients that are needed to complete the recipe")]
        public List<SO_Ingredient> Ingredients;
        [Tooltip("Do the ingredients need to be ordered to complete the recipe ?")]
        [HideInInspector] public bool ordered = false; // Disabled for now
        [Tooltip("Does the recipe need to have all and only these ingredients, or can other ingredients be in it too ?")]
        [HideInInspector] public bool exact = false; // Disabled for now

        #endregion

        #region Methods
        public bool TryGetRequirements(out SO_RecipeUnlockConfig.UnlockRequirement requirements)
        {
            requirements = default;
            var cat = RecipeCategoryHelper.GetCategory(this);
            var config = SO_RecipeUnlockConfig.Instance;
            if (config == null) return false;

            if (!config.TryGetRequirement(cat, out requirements))
            {
                return true;
            }
            return false;
        }
        public static bool Compare(SO_Recipe a, SO_Recipe b) => ScrambledEquals(a.Ingredients, b.Ingredients);
        public static bool Compare(IEnumerable<SO_Ingredient> a, SO_Recipe b) => ScrambledEquals(a, b.Ingredients);
        public static bool Compare(SO_Recipe a, IEnumerable<SO_Ingredient> b) => ScrambledEquals(b, a.Ingredients);
        public static bool Compare(IEnumerable<SO_Ingredient> a, IEnumerable<SO_Ingredient> b) => ScrambledEquals(b, a);

        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                    cnt[s]++;
                else
                    cnt.Add(s, 1);
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                    cnt[s]--;
                else
                    return false;
            }
            return cnt.Values.All(c => c == 0);
        }
        #endregion
    }

}