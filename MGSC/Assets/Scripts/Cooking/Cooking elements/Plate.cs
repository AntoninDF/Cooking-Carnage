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
    public class Plate : Item, ITrashable, ICookable
    {
        #region Properties
        public float TargetCookDuration {get => _targetCookDuration; set => _targetCookDuration = value; }
        public float CookDuration { get; set; }

        public float CookPercentage => CookDuration / (TargetCookDuration > 0 ? TargetCookDuration : 1.0f); // Between 0 and 1

        public UnityEvent<ICookable> OnCook => _onCook;
        public ECookStatus CookStatus => ECookStatus.Raw;

        #endregion

        #region Fields
        
        [SerializeField] string _itemID;
        
        [Header("Plate references")]
        //[Tooltip("Reference a slot container in which to put the base ingredient.")]
        //[SerializeField] private SlotContainer _baseIngredientSlot;
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
        [Tooltip("Invoked when the base ingredient is added to the plate")]
        [SerializeField] private UnityEvent _onBaseIngredientAdded;
        [Tooltip("Invoked when the base ingredient is removed from the plate")]
        [SerializeField] private UnityEvent _onBaseIngredientRemoved;
        [Tooltip("Invoked when a normal ingredient is added to the plate")]
        [SerializeField] private UnityEvent _onIngredientAdded;
        [Tooltip("Invoked when a normal ingredient is removed from the plate")]
        [SerializeField] private UnityEvent _onIngredientRemoved;

        private ItemManager itemManager;
        #endregion
        
        

        #region Methods

        protected override void Start()
        {
            base.Start();
            //if (_baseIngredientSlot == null)
                //Debug.LogError("[Plate] : Please reference the base ingredient slot in the inspector");
            if (_ingredientSlots == null)
                Debug.LogError("[Plate] : Please reference the ingredient multislot in the inspector");
            if (_ingredientSlots == null)
                Debug.LogError("[Plate] : Please reference the ingredient multislot in the inspector");

            SetupSlots();
            itemManager = GameSystem.ReferenceManager.Instance.GetManager<ItemManager>();
        }
        
        protected override string UpdateID()
        {
            base.SetID(_itemID);
            return _itemID;
        }

        public override void Reset()
        {
            base.Reset();
            
        }

        private void SetupSlots()
        {
            _ingredientSlots.SetActive(true);
            //_baseIngredientSlot.OnGrab.AddListener((item) => _ingredientSlots.SetActive(true));
            //_baseIngredientSlot.OnGrab.AddListener((item) => _onBaseIngredientAdded?.Invoke());
            //_baseIngredientSlot.OnDrop.AddListener((item) => _ingredientSlots.SetActive(false));
            //_baseIngredientSlot.OnDrop.AddListener((item) => _onBaseIngredientRemoved?.Invoke());

            //_ingredientSlots.OnGrab.AddListener((item) => _baseIngredientSlot.IsLocked = true);
            _ingredientSlots.OnGrab.AddListener((item) => _onIngredientAdded?.Invoke());
            //_ingredientSlots.OnDrop.AddListener((item) => { if (_ingredientSlots.HeldItems.Count == 0) _baseIngredientSlot.IsLocked = false; });
            _ingredientSlots.OnDrop.AddListener((item) => _onIngredientRemoved?.Invoke());
        }

        public void Cook(float dT) 
        {
            SO_Recipe recipe = GetMatchingRecipe();
            _targetCookDuration = recipe.timeToCook;
            CookDuration += dT; 
            OnCook?.Invoke(this);
            UpdateTooltip();
            SetItemColliders(false);

            if (CookPercentage >= 1)
            {
                TransformToMeal(recipe);
            }
        }
        public void StopCooking()
        {
            CookDuration = 0f;
            UpdateTooltip();
            SetItemColliders(true);
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
            if (ingredients.Count() == 0) return _recipeBook.DefaultRecipe; // If empty recipe
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
            var go = itemManager.TrySpawn(recipe.mealPrefab, transform.position, transform.rotation, transform.parent);
            go.name = this.name + " as meal";
            Meal meal = go.GetComponent<Meal>();
            meal.Recipe = recipe;
            Container.Drop(this);
            Container.Grab(meal);
            
            foreach (var obj in _ingredientSlots.HeldItems)
            {
                var ingredient = obj as Item;
                if (ingredient != null)
                    itemManager.Despawn(ingredient);
            }
            _ingredientSlots.Reset();

            itemManager.Despawn(this);
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

        public void SetItemColliders(bool active)
        {
            foreach(var item in _ingredientSlots.HeldItems)
            {
                item.Collider.enabled = active;
            }
        }

        #endregion
    }

}