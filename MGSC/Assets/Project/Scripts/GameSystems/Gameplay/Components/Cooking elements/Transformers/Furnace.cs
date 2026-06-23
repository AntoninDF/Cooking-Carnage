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
    [SerializeField] private SlotContainer _itemSlot;
    [SerializeField] private InteractionButton _button;

    #endregion

    void Awake()
    {
        Interactable = new(this);
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
        if (_itemSlot != null)
        {
            Container = _itemSlot.Container;
            Container.Filter.AddCondition(
                item =>
                {
                    Debug.Log("Filter evaluation");
                    if (item is not Feature feature) return false;
                    Debug.Log("Feature detected");
                    if (!feature.TryGetSibling<F_Cookable>(out var temp)) return false;
                    Debug.Log("Cookable detected");
                    return true;
                }
                );
        }
        if (_button != null)
        {
            if (_button.TryGetFeature(out F_Interactable interactable))
            {
                interactable.OnInteraction += TryStartCooking;
            }
        }
    }

    private void BindEvents()
    {
        Interactable.OnInteraction += TryInteract;
        Cooker.OnTransformationStart += OnStartCooking;
        Cooker.OnTransformationOver += OnStopCooking;

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
    private void OnStartCooking(TransformableAfterTimer target)
    {
        Debug.Log("Cooker.OnTransformationStart");
        Container.IsLocked = true;
    }
    private void OnStopCooking(TransformableAfterTimer target)
    {
        Debug.Log("Cooker.OnTransformationStop");
        Container.IsLocked = false;
    }
}
