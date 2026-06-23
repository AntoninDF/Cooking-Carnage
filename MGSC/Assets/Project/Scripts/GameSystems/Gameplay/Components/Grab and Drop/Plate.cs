
using Cooking;
using Features;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public Servable Servable;
    #endregion

    #region Fields
                
    [Header("Plate references")]
    [Tooltip("The recipe book should contain every possible recipe for this object.")]
    [SerializeField] private SO_RecipeBook _recipeBook;
    [SerializeField] MultiSlotContainer ContainerComponent;
    [SerializeField] Recipe RecipeComponent;
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
        Servable = new(this);
        Servable.Enabled = false;
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
        if (context.TryGetFeature(out F_Container container))
        {
            // Try to take the player's ingredient
            if (container.CanDrop(container.HeldItem))
            {
                Container.RequestGrab(container.HeldItem);
            }
            // Or ask the player to grab this plate
            else
            {
                container.RequestGrab(Grabbable);
            }
        }

    }

    private void OnCook()
    {
        Cookable.Enabled = false;
        OverCookable.Enabled = true;
        Servable.Enabled = true;
        TryDetectRecipe(out var temp);
        SetActiveModel(CookedVariant);
    }
    
    /// <summary>
    /// REFACTO → Ne pas nécessiter de component de recette et tout faire self-contained
    /// </summary>
    /// <param name="recipe"></param>
    /// <returns></returns>
    public bool TryDetectRecipe(out SO_Recipe recipe)
    {
        recipe = null;
        if (RecipeComponent == null && !TryGetComponent(out RecipeComponent))
            return false;

        recipe = RecipeComponent.RecipeFeature.GetMatchingRecipe();

        RecipeComponent.RecipeFeature.SetIngredients(Container.Container.GetHeldItemsAs<Features.F_Ingredient>());

        if (recipe == null) return false;
        return true;
    }
    #endregion
}
