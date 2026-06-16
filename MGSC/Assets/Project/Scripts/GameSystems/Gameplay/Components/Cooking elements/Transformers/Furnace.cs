using Features;
using GameSystem;
using UnityEngine;

public class Furnace : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; set; } = new();
    public F_Interactable Interactable { get; set;  }
    public F_SoloContainer Container { get; set; }
    public Cooker Cooker { get; set; }


    #region Fields
    [SerializeField] private Transform itemSlot;
    [SerializeField] private InteractionButton button;

    #endregion

    void Awake()
    {
        Interactable = new(this);
        Container = new (this, itemSlot);
        Container.Filter.AddCondition(item => item is Feature feature && feature.TryGetSibling<F_Cookable>(out var temp));
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
            if (button.TryGetFeature(out F_Interactable interactable))
            {
                interactable.OnInteraction += TryStartCooking;
            }
        }
    }

    private void BindEvents()
    {
        Interactable.OnInteraction += TryInteract;
        Cooker.OnTransformationStart += _ => Container.IsLocked = true;
        Cooker.OnTransformationOver += _ => Container.IsLocked = false;

        Cooker.OnTransformationStart += _ => Debug.Log("Cooker.OnTransformationStart");
        Cooker.OnTransformationOver += _ => Debug.Log("Cooker.OnTransformationOver");

    }
    private void TryInteract(InteractionContext context)
    {
        if ( context.TryGetFeature(out F_Container container))
        {
            if (Container.RequestGrab(container.HeldItem))
                return;

            container.RequestGrab(Container.HeldItem);
        }
    }
    private void TryStartCooking(InteractionContext context)
    {
        if (Container.HeldItem == null) return;
        if (!Container.HeldItem.TryGetSibling(out F_Cookable cookable, true) || !cookable.CanBeTransformed) return;

        Cooker.RequestStartTransformation(cookable);
    }
    
}
