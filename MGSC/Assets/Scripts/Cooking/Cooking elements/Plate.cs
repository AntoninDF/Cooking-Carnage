using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// The plate class manages the contained items, can cook, then transform in a completed recipe
    /// </summary>
    public class Plate : Item, ICookable
    {
        #region Properties
        public float TargetCookDuration {get => _targetCookDuration; set => _targetCookDuration = value; }
        public float CookDuration { get; set; }

        public float CookPercentage => CookDuration / (TargetCookDuration > 0 ? TargetCookDuration : 1.0f);

        public UnityEvent<ICookable> OnCook => _onCook;

        #endregion

        #region Fields
        [Header("Plate references")]
        [Tooltip("Reference a multi slot container in which to put every ingredient.")]
        [SerializeField] private MultiSlotContainer _ingredientSlots;
        [Tooltip("The recipe book should contain every possible recipe for this object.")]
        [SerializeField] private SO_RecipeBook _recipeBook;
        [Space]
        [Header("Plate parameters")]
        [Tooltip("How much time it will take to be 100% cooked.")]
        [SerializeField] private float _targetCookDuration = 10.0f;
        [Space]
        [Header("Plate events")]
        [Tooltip("Invoked repeatedly when the plate is cooking")]
        [SerializeField] private UnityEvent<ICookable> _onCook;
        #endregion

        #region Methods

        public void Cook(float dT) 
        {
            CookDuration += dT; 
            OnCook?.Invoke(this);

            if (CookPercentage > 1)
            {
                SO_Recipe recipe = default(SO_Recipe);
                if (IsValidRecipe(ref recipe))
                {
                    TransformToMeal(recipe);
                }
            }
        }

        /// <summary>
        /// Checks if the current ingredient list can make a recipe. If so, it passes said recipe as ref
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        private bool IsValidRecipe(ref SO_Recipe recipe)
        {
            var ingredients = _ingredientSlots.HeldItems.Select((item) => ((Ingredient)item).Model);
            foreach (var r in _recipeBook.Recipes)
            {
                if (SO_Recipe.Compare(ingredients, r))
                {
                    recipe = r;
                    return true;
                }
            }

            return false;
        }

        private void TransformToMeal(SO_Recipe recipe)
        {
            var go = GameObject.Instantiate(recipe.mealPrefab, transform.parent);
            go.transform.localPosition = transform.localPosition;
            go.transform.rotation = transform.rotation;
            go.name = this.name + " as meal";
            Meal meal = go.GetComponent<Meal>();
            meal.Recipe = recipe;
            Container.Drop(this);
            Container.Grab(meal);
            Destroy(this.gameObject);
        }

        #endregion
    }

}