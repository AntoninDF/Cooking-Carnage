using Features;
using GameSystem;
using UnityEngine;

public class Furnace : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; set; } = new();
    public Interactable Interactable { get; set;  }
    public SoloContainer Container { get; set; }
    public Cooker Cooker { get; set; }


    #region Fields
    [SerializeField] private Transform itemSlot;
    [SerializeField] private Button button;

    #endregion

    void Awake()
    {
        Interactable = new(this);
        Container = new (this, itemSlot);
        Container.Filter.AddCondition((item) => item.TryGetSibling<Cookable>(out var temp));
        Cooker = new(this);

        BindEvents();
    }
    void Start()
    {
        Collection.LateSetup();
        BindExternalEvents();
    }

    private void BindExternalEvents()
    {
        if (button != null)
        {
            if (button.TryGetFeature(out Interactable interactable))
            {
                interactable.OnInteraction += TryStartCooking;
            }
        }
    }

    private void BindEvents()
    {
        Interactable.OnInteraction += TryInteract;
        Cooker.OnCookingStart += _ => Container.IsLocked = true;
        Cooker.OnCookingOver += _ => Container.IsLocked = false;
    }
    private void TryInteract(InteractionContext context)
    {
        if ( context.TryGetFeature(out Container container))
        {
            if (Container.RequestGrab(container.HeldItem))
                return;

            container.RequestGrab(Container.HeldItem);
        }
    }
    private void TryStartCooking(InteractionContext context)
    {
        if (Container.HeldItem == null) return;
        if (!Container.HeldItem.TryGetSibling(out Cookable cookable, true) || !cookable.CanBeCooked()) return;
        if (!Cooker.CanStartCooking(cookable)) return;

        Cooker.RequestStartCooking(cookable);
    }
    
}
