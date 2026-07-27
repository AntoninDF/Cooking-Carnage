using UnityEngine;
using Features;

/// <summary>
/// The surface container is a surface that can receive any IGrabbable object on top of it.
/// </summary>
[RequireComponent(typeof(Collider))]
public class SurfaceContainer : MonoBehaviour, IFeatureCollection
{
    #region Properties
    public bool IsLocked { get => Container.IsLocked; set => Container.IsLocked = value; }
    public bool Passthrough => false;

    public FeatureCollection Collection { get; set; } = new();
    public F_MultiContainer Container { get; set; }
    public Conditions Filter { get; set; }
    public F_LookAtTarget LookAtTarget { get; set; }
    public F_Interactable Interactable { get; set; }
    #endregion

    #region Fields
        


    #endregion

    #region Methods

    void Start()
    {
        Container = new(this, transform);
        Filter = new(this);
        Container.Filter = Filter;
        Container.AllowItemPlacedOnSurfaceRatherThanTransform = true;
        LookAtTarget = new F_LookAtTarget(this, transform);
        Interactable = new (this);

        BindEvents();
        Collection.LateSetup();
    }

    private void BindEvents()
    {
        Interactable.OnInteraction += TryGrabObject;
        Interactable.AddCondition(CanInteract);
    }

    private void TryGrabObject(InteractionContext context)
    {
        F_Container container = null;
        if (context.Interactor is F_Container)
        {
            container = (F_Container)context.Interactor;
        }
        else if (context.Interactor is IFeatureCollection col)
        {
            col.Get(out container);
        }

        if (container == null) return;

        Container.RequestGrab(container.HeldItem);
    }

    bool CanInteract(InteractionContext context)
    {
        F_Container container = null;
        if (context.Interactor is F_Container)
        {
            container = (F_Container)context.Interactor;
        }
        else if (context.Interactor is IFeatureCollection col)
        {
            col.Get(out container);
        }

        if (container == null) return false;
        if (container.HeldItem == null) return false;
        if (!Container.CanGrab(container.HeldItem)) return false;

        return true;
    }

    void Update()
    {
        Collection.Update();
    }
        
    #endregion
}