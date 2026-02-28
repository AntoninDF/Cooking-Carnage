using Features;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// The plate class manages the contained items, can cook, then transform in a completed recipe
    /// </summary>
    public class Plate : Item
    {
        #region Properties

        public ECookStatus CookStatus => ECookStatus.Raw;

        #endregion

        #region Fields
                
        [Header("Plate references")]
        [Tooltip("The recipe book should contain every possible recipe for this object.")]
        [SerializeField] private SO_RecipeBook _recipeBook;

        private ItemManager _itemManager;
        protected MultiContainer _slots;
        #endregion
        
        

        #region Methods

        protected override void Start()
        {
            base.Start();
            
            SetupSlots();
            Reference.GetManager(out _itemManager);
        }
        

        public  void Reset()
        {
            //base.Reset();
            //DespawnContent();
        }

        private void SetupSlots()
        {
            //_ingredientSlots.SetActive(true);

            //_ingredientSlots.OnGrab.AddListener((item) => _onIngredientAdded?.Invoke());

            //_ingredientSlots.OnDrop.AddListener((item) => _onIngredientRemoved?.Invoke());
        }

        /*
        public void Cook(float dT) 
        {
            SO_Recipe recipe = GetMatchingRecipe();
            _targetCookDuration = recipe.timeToCook;
            CookDuration += dT; 
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
        */

        /// <summary>
        /// Checks if the current ingredient list can make a recipe. If so, it passes said recipe as ref
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        private bool IsValidRecipe(ref SO_Recipe recipe)
        {
            recipe = GetMatchingRecipe();
            return recipe != _recipeBook.EmptyRecipe;
        }

        public SO_Recipe GetMatchingRecipe()
        {
            /*
            var ingredients = _ingredientSlots.HeldItems.Select((item) => ((Ingredient)item).Model);
            if (ingredients.Count() == 0) return _recipeBook.EmptyRecipe; // If empty recipe
            foreach (var r in _recipeBook.Recipes)
            {
                if (SO_Recipe.Compare(ingredients, r))
                {
                    return r;
                }
            }
            */
            return _recipeBook.FailedRecipe;
        }

        private void TransformToMeal(SO_Recipe recipe)
        {
            var go = _itemManager.TrySpawn(recipe.mealPrefab, transform.position, transform.rotation, transform.parent);
            go.name = this.name + " as meal";
            Meal meal = go.GetComponent<Meal>();
            meal.Recipe = recipe;
            //Container.Empty();
            //Container.RequestGrab(meal);
            //DespawnContent();
            //_ingredientSlots.Reset();

            //StopCooking();
            _itemManager.Despawn(this);
        }
        /*
        public void DespawnContent()
        {

            foreach (var slot in _ingredientSlots.Slots)
            {
                //if (slot == null || slot.HeldItem == null)
                {

                    continue;
                }

                var obj = slot.HeldItem;
                var ingredient = obj.Owner as Item;
                if (ingredient != null)
                {

                    _itemManager.Despawn(ingredient);
                }
                else
                    _itemManager.Log(LogType.Error,"Ingredient was null when trying to get despawned");
            }
            //_ingredientSlots.HeldItems.Clear();
        }
        */

        /*
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
            //if (_tooltipComponent != null)
            {
                //_tooltipComponent.SetText(text);
            }
        }
        */

        public void SetItemColliders(bool active)
        {
            //foreach(var item in _ingredientSlots.HeldItems)
            {
                //item.Collider.enabled = active;
            }
        }
        #endregion
    }

}