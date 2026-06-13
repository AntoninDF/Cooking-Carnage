
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
    public Cookable Cookable;
    public Cookable OverCookable;
    public Servable Servable;
    #endregion

    #region Fields
                
    [Header("Plate references")]
    [Tooltip("The recipe book should contain every possible recipe for this object.")]
    [SerializeField] private SO_RecipeBook _recipeBook;
    [SerializeField] MultiSlotContainer ContainerComponent;
    [SerializeField] Recipe RecipeComponent;



    Features.MultiSlotContainer Container;
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
    protected virtual void Start()
    {
        BindExternalEvents();
    }

    private void BindExternalEvents()
    {
        if (ContainerComponent.TryGetFeature(out Container))
        {
            Container.OnGrab += TryLock;
            // Make sure only cut ingredients can be put in the plate
            Container.Filter.AddCondition(item => item.TryGetFeature(out Cuttable cuttable) && cuttable.Transformed);
        }
        if (ContainerComponent.TryGetFeature(out Interactable interactable))
        {
            interactable.Enabled = false;
        }
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
        if (context.TryGetFeature(out Container container))
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
    }
    
    public bool TryDetectRecipe(out SO_Recipe recipe)
    {
        recipe = null;
        if (RecipeComponent == null && !TryGetComponent(out RecipeComponent))
            return false;

        recipe = RecipeComponent.RecipeFeature.GetMatchingRecipe();

        RecipeComponent.RecipeFeature.SetIngredients(Container.Container.GetHeldItemsAs<Features.Ingredient>());

        if (recipe == null) return false;
        return true;
    }
    #endregion
}
