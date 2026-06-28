
using Cooking;
using Features;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The plate class manages the contained items, can cook, then transform in a completed recipeComponent
/// </summary>
[RequireComponent(typeof(MultiSlotContainer))]
[RequireComponent(typeof(Collider))]
public class Plate : Item
{
    #region Properties
    public F_Cookable Cookable;
    public F_OverCookable OverCookable;
    public F_MultiSlotContainer Container;
    #endregion

    #region Fields
                
    [Header("Plate references")]
    [Tooltip("The recipe book should contain every possible recipe for this object.")]
    [SerializeField] private SO_RecipeBook _recipeBook;
    [SerializeField] MultiSlotContainer ContainerComponent;


    private ModelController cookedVariant;
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
        OverCookable.OnTransformation += OnOverCook;
        
    }


    protected override void Start()
    {
        base.Start();
        BindExternalEvents();
        cookedVariant?.SetActive(false);
        cookedVariant?.gameObject.SetActive(false);
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
                    SwapWith(playerContainer.HeldItem);
                }
                // Or just take this one
                else
                {
                    playerContainer.RequestGrab(Grabbable);
                }
            }
        }

    }

    

    private void OnOverCook()
    {
        var recipe = Reference.AssetReferences.DefaultRecipeBook.FailedRecipe;
        EnableRecipeModel(recipe);
        // Assign the failed recipe, otherwise it might detect an empty recipe instead
        if (TryGetComponent<Recipe>(out var recipeComponent))
        {
            var feature = recipeComponent.RecipeFeature;
            feature.Recipe = recipe;
        }
    }
    private void OnCook()
    {
        Cookable.Enabled = false;
        OverCookable.Enabled = true;
        DetectRecipe(out var recipe);
        EnableRecipeModel(recipe);
        DeleteContent();
    }

    private void DeleteContent()
    {
        Reference.GetManager(out ItemManager manager);
        foreach(var slot in Container.SoloContainers)
        {
            if (slot.HeldItem == null) continue;
            Item item = slot.HeldItem.Owner as Item;
            if (item == null) continue;
            manager.Despawn(item);

        }
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
        ModelController mc = recipe.mealPrefab;
        if (mc == null)
        {
            Debug.LogWarning("The meal prefab on the recipe " + recipe.name + " was null");
            return;
        }

        // Setup
        GameObject go = Instantiate(mc.gameObject);
        go.transform.SetParent(defaultModel.transform.parent);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;
        cookedVariant = go.GetComponent<ModelController>();
        SetActiveModel(cookedVariant);


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
