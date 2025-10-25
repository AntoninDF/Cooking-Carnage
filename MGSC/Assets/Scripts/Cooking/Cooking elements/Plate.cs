using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
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

        public float CookPercentage => CookDuration / (TargetCookDuration > 0 ? TargetCookDuration : 1.0f); // Between 0 and 1

        public UnityEvent<ICookable> OnCook => _onCook;
        public ECookStatus CookStatus => ECookStatus.Raw;

        #endregion

        #region Fields
        [Header("Plate references")]
        [Tooltip("Reference a slot container in which to put the base ingredient.")]
        [SerializeField] private SlotContainer _baseIngredientSlot;
        [Tooltip("Reference a multi slot container in which to put every ingredient.")]
        [SerializeField] private MultiSlotContainer _ingredientSlots;
        [Tooltip("The recipe book should contain every possible recipe for this object.")]
        [SerializeField] private SO_RecipeBook _recipeBook;
        //[Space]
        //[Header("Plate parameters")]
        //[Tooltip("How much time it will take to be 100% cooked.")]
        private float _targetCookDuration = 0.0f;
        [Space]
        [Header("Plate events")]
        [Tooltip("Invoked repeatedly when the plate is cooking")]
        [SerializeField] private UnityEvent<ICookable> _onCook;
        #endregion

        #region Methods

        protected override void Start()
        {
            base.Start();
            if (_baseIngredientSlot == null)
                Debug.LogError("[Plate] : Please reference the base ingredient slot in the inspector");
            if (_ingredientSlots == null)
                Debug.LogError("[Plate] : Please reference the ingredient multislot in the inspector");
            if (_ingredientSlots == null)
                Debug.LogError("[Plate] : Please reference the ingredient multislot in the inspector");

            SetupSlots();
        }

        private void SetupSlots()
        {
            _ingredientSlots.SetActive(false);
            _baseIngredientSlot.OnGrab.AddListener((item) => _ingredientSlots.SetActive(true));
            _baseIngredientSlot.OnDrop.AddListener((item) => _ingredientSlots.SetActive(false));

            _ingredientSlots.OnGrab.AddListener((item) => _baseIngredientSlot.IsLocked = true);
            _ingredientSlots.OnDrop.AddListener((item) => { if (_ingredientSlots.HeldItems.Count == 0) _baseIngredientSlot.IsLocked = false; });
        }

        public void Cook(float dT) 
        {
            SO_Recipe recipe = GetMatchingRecipe();
            _targetCookDuration = recipe.timeToCook;
            CookDuration += dT; 
            OnCook?.Invoke(this);
            UpdateTooltip();

            if (CookPercentage >= 1)
            {
                TransformToMeal(recipe);
            }
        }
        public void StopCooking()
        {
            CookDuration = 0f;
            UpdateTooltip();
        }
        /// <summary>
        /// Checks if the current ingredient list can make a recipe. If so, it passes said recipe as ref
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        private bool IsValidRecipe(ref SO_Recipe recipe)
        {
            recipe = GetMatchingRecipe();
            return recipe != _recipeBook.DefaultRecipe;
        }

        private SO_Recipe GetMatchingRecipe()
        {
            var ingredients = _ingredientSlots.HeldItems.Select((item) => ((Ingredient)item).Model);
            foreach (var r in _recipeBook.Recipes)
            {
                if (SO_Recipe.Compare(ingredients, r))
                {
                    return r;
                }
            }

            return _recipeBook.DefaultRecipe;
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

        private void UpdateTooltip()
        {
            string text;
            if (CookDuration == 0)
            {
                text = "Plate";
            }
            else
            {
                text = "Cooking (" + Mathf.RoundToInt(CookPercentage*100) +"%)";
            }
            if (_tooltipComponent != null)
            {
                _tooltipComponent.SetText(text);
            }
        }

        #endregion
    }

}