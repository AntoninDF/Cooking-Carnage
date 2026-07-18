
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
    public SO_Recipe Recipe => DetectRecipe();
    public F_Cookable Cookable;
    public F_OverCookable OverCookable;
    public F_MultiSlotContainer Container;
    #endregion

    #region Fields
                
    [Header("Plate references")]
    [Tooltip("The recipe book should contain every possible recipe for this object.")]
    [SerializeField] private SO_RecipeBook _recipeBook;
    [SerializeField] MultiSlotContainer ContainerComponent;
    [SerializeField] EventVariable _onIngredientAdded;
    
    [Header("Feedback")]
    [SerializeField] private Animation _animation;
    [SerializeField] private AnimationClip _animationClip;

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
            if (_onIngredientAdded != null) Container.OnGrab += _onIngredientAdded.Raise;
            // Make sure only cut ingredients can be put in the plate
            Container.Filter.AddCondition(GrabbableCondition);
            Container.Filter.AddCondition(_ => Globals.Gameplay.AllowPuttingIngredientsInPlates);
            Collection.Add(Container);
        }
        if (ContainerComponent.TryGetFeature(out F_Interactable interactable))
        {
            interactable.Enabled = false;
        }

        ContainerComponent.Container.OnGrab += BindIngredient;
        ContainerComponent.Container.OnDrop += UnbindIngredient;
    }

    private void BindIngredient(F_Grabbable grabbable)
    {
        if (grabbable.Owner is Item item)
        {
            LookAtTarget.OnStartLookAt += item.ActiveModel.Outline.EnableOutline;
            LookAtTarget.OnStopLookAt  += item.ActiveModel.Outline.DisableOutline;

            if (CurrentAppliedLayer.HasValue)
            {
                item.SetLayerRecursive(CurrentAppliedLayer.Value);
                item.SetHeld(true);
            }
                
        }
    }
    private void UnbindIngredient(F_Grabbable grabbable)
    {
        if (grabbable.Owner is Item item)
        {
            LookAtTarget.OnStartLookAt -= item.ActiveModel.Outline.EnableOutline;
            LookAtTarget.OnStopLookAt  -= item.ActiveModel.Outline.DisableOutline;
        }
    }

    bool GrabbableCondition(object item)
    {
        F_Grabbable grabbable = (F_Grabbable)item;
        if (grabbable == null) return false;
        if(!Cookable.CanBeTransformed) return false;
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
            // Try to take the player's item, otherwise, try to get grabbed
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

                if (cookedVariant != null)
                {
                    Grabbable.OnJellyGrabSFX?.Invoke(); // son de gelée 
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
        DeleteContent();
        Container.IsLocked = true;
    }
    private void OnCook()
    {
        Cookable.Enabled = false;
        OverCookable.Enabled = true;
        DetectRecipe(out var recipe);
        EnableRecipeModel(recipe);
        DeleteContent();
        Container.IsLocked = true;
    }

    private void DeleteContent()
    {
        Reference.GetManager(out ItemManager manager);
        foreach(var slot in Container.SoloContainers)
        {
            if (slot.HeldItem == null) continue;
            Item item = slot.HeldItem.Owner as Item;
            if (item == null) continue;
            UnbindIngredient(item.Grabbable);
            slot.RequestDrop();
            Destroy(item.gameObject);
        }
    }

    public void DetectRecipe(out SO_Recipe recipe) => recipe = DetectRecipe();
    public SO_Recipe DetectRecipe()
    {
        SO_Recipe recipe = null;
        var recipeComponent = gameObject.AddComponent<Recipe>();
        var recipeFeature = recipeComponent.GetFeature<F_Recipe>();

        recipeFeature.Ingredients = GetContainedIngredients();
        recipe = recipeFeature.Recipe;
        
        Collection.Add(recipeComponent.Servable);
        return recipe;
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
        GameObject go = Instantiate(mc.gameObject, defaultModel.transform.parent, true);
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
    
    public override void SetLayerRecursive(int layer)
    {
        base.SetLayerRecursive(layer);

        foreach (var slot in Container.SoloContainers)
        {
            if (slot?.HeldItem == null) continue;
            if (slot.HeldItem.Owner is Item item)
            {
                item.SetLayerRecursive(layer);
                item.SetHeld(true);
            }
                
        }
    }
    
    public override void StoreLayerRecursive()
    {
        base.StoreLayerRecursive();
        foreach (var slot in Container.SoloContainers)
        {
            if (slot?.HeldItem?.Owner is Item item)
            {
                item.StoreLayerRecursive();
                
            }
                
        }
    }

    public override void RestoreLayerRecursive()
    {
        base.RestoreLayerRecursive();
        foreach (var slot in Container.SoloContainers)
        {
            if (slot?.HeldItem?.Owner is Item item)
            {
                item.RestoreLayerRecursive();
                item.SetHeld(false);
            }
                
            
        }
    }
    #endregion

    #region Animation

    public void PlayAnimation()
    {
        _animation?.Play(_animationClip?.name);
    }

    #endregion
}
