
using Cooking;
using Features;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The plate class manages the contained items, can cook, then transform in a completed recipe
/// </summary>
[RequireComponent(typeof(MultiSlotContainer))]
[RequireComponent(typeof(Collider))]
public class Plate : Item
{
    #region Properties
    public F_Cookable Cookable;
    public F_Cookable OverCookable;
    #endregion

    #region Fields
                
    [Header("Plate references")]
    [Tooltip("The recipe book should contain every possible recipe for this object.")]
    [SerializeField] private SO_RecipeBook _recipeBook;
    [SerializeField] MultiSlotContainer ContainerComponent;
    [SerializeField] ModelController CookedVariant;


    F_MultiSlotContainer Container;
    #endregion
        
        

    #region Methods
    public void Reset()
    {
        ContainerComponent = GetComponent<MultiSlotContainer>();
    }
    protected override void Awake()
    {
        if (ContainerComponent == null) ContainerComponent = GetComponent<MultiSlotContainer>();
        Cookable = new(this);
        OverCookable = new(this);
        OverCookable.Enabled = false;
        base.Awake();
    }
    protected override void BindEvents()
    {
        base.BindEvents();
        Cookable.OnTransformation += OnCook;
        
    }
    protected override void Start()
    {
        base.Start();
        BindExternalEvents();
        CookedVariant.SetActive(false);
        CookedVariant.gameObject.SetActive(false);
        SetActiveModel(defaultModel);
    }

    private void BindExternalEvents()
    {
        if (ContainerComponent.TryGetFeature(out Container))
        {
            Container.OnGrab += TryLock;
            // Make sure only cut ingredients can be put in the plate
            Container.Filter.AddCondition(GrabbableCondition);
            Collection.Add(Container);
        }
        if (ContainerComponent.TryGetFeature(out F_Interactable interactable))
        {
            interactable.Enabled = false;
        }
    }
    bool GrabbableCondition(object item)
    {
        F_Grabbable grabbable = (F_Grabbable)item;
        if (grabbable == null) return false;
        if (!grabbable.TryGetSibling(out F_Cuttable cuttable)) return false;
        if (!cuttable.Transformed) return false;
        return true;
    }

    private void TryLock(F_Grabbable item)
    {
        if (item != null)
        {
            item.IsLocked = true;
        }
    }
    protected override void TryInteract(InteractionContext context)
    {
        if (context.TryGetFeature(out F_Container playerContainer))
        {
            // Try to take the player's ingredient, otherwise, try to get grabbed
            if (!Container.RequestGrab(playerContainer.HeldItem))
            {
                // Try to swap the plates
                if (playerContainer.HeldItem != null && playerContainer.HeldItem.Owner is Plate playerPlate)
                {
                    var otherItem = playerContainer.HeldItem;
                    var currentContainer = Grabbable.Container;
                    var targetPos = transform.position;
                    playerContainer.RequestDrop(otherItem);
                    playerContainer.RequestGrab(Grabbable);
                    if (currentContainer != null)
                    {
                        currentContainer.RequestGrab(otherItem);
                        var otherPlate = otherItem.Owner as Plate;
                        otherPlate.Anim.GoTo(currentContainer.transform, targetPos);
                    }
                }
                // Or just take this one
                else
                {
                    playerContainer.RequestGrab(Grabbable);
                }
            }
        }

    }

    private void OnCook()
    {
        Cookable.Enabled = false;
        OverCookable.Enabled = true;
        DetectRecipe(out var recipe);
        EnableRecipeModel(recipe);
        SetActiveModel(CookedVariant);
    }
    
    public void DetectRecipe(out SO_Recipe recipe)
    {
        recipe = null;
        var recipeComponent = gameObject.AddComponent<Recipe>();
        var recipeFeature = recipeComponent.GetFeature<F_Recipe>();

        recipeFeature.Ingredients = GetContainedIngredients();
        recipe = recipeFeature.Recipe;
    }

    void EnableRecipeModel(SO_Recipe recipe)
    {
        // TODO


    }

    List<F_Ingredient> GetContainedIngredients()
    {
        List<F_Ingredient> result = new();
        foreach (var slot in Container.SoloContainers)
        {
            if (slot == null) continue;
            if (slot.HeldItem == null) continue;
            if (!slot.HeldItem.TryGetSibling(out F_Ingredient ingredient)) continue;
            result.Add(ingredient);
        }
        return result;
    }
    #endregion
}
