
using Cooking;
using Features;
using UnityEngine;

/// <summary>
/// The plate class manages the contained items, can cook, then transform in a completed recipe
/// </summary>
[RequireComponent(typeof(MultiSlotContainer))]
[RequireComponent(typeof(Collider))]
public class Plate : Item
{
    #region Properties

    #endregion

    #region Fields
                
    [Header("Plate references")]
    [Tooltip("The recipe book should contain every possible recipe for this object.")]
    [SerializeField] private SO_RecipeBook _recipeBook;
    [SerializeField] MultiSlotContainer ContainerComponent;

    private ItemManager _itemManager => Reference.GetManager<ItemManager>();
    Features.MultiSlotContainer Container;
    #endregion
        
        

    #region Methods
    public void Reset()
    {
        ContainerComponent = GetComponent<MultiSlotContainer>();
    }
    protected virtual void Start()
    {
        Debug.Log("Get COntainer. Success : " + ContainerComponent.TryGetFeature(out Container));

        if (ContainerComponent.TryGetFeature(out Interactable interactable))
        {
            Debug.Log("Interactions cut");
            interactable.Enabled = false;
        }

        Container.OnGrab += TryLock; // This is not in BindEvents because it depends on other components being initialized
    }
    private void TryLock(Grabbable item)
    {
        if (item != null)
        {
            item.IsLocked = true;
        }
    }
    protected override void TryInteract(InteractionContext context)
    {
        Debug.Log("Container is " + Container);
        if (context.TryGetFeature(out Container container))
        {
            // Try to take the player's ingredient
            if (container.CanDrop(container.HeldItem))
            {
                Debug.Log("Take ingredient");
                Debug.Log("Plate tries to grab. Success : " + Container.RequestGrab(container.HeldItem));
            }
            // Or ask the player to grab this plate
            else
            {
                Debug.Log("Plate tries to get grabbed");
                container.RequestGrab(Grabbable);
            }
        }

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
