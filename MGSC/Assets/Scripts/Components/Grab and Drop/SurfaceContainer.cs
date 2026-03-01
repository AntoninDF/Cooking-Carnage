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
    public MultiContainer Container { get; set; }
    public Conditions Filter { get; set; }
    public LookAtTarget LookAtTarget { get; set; }
    public Interactable Interactable { get; set; }
    #endregion

    #region Fields
        


    #endregion

    #region Methods

    void Start()
    {
        Container = new(this, transform);
        Filter = new(this);
        Container.Filter = Filter;
        LookAtTarget = new LookAtTargetByRaycast(this, transform);
        Interactable = new (this);

        BindEvents();
        Collection.LateSetup();
    }

    private void BindEvents()
    {
        Interactable.OnInteraction += TryGrabObject;
    }

    private void TryGrabObject(InteractionContext context)
    {
        Container container = null;
        if (context.Interactor is Container)
        {
            container = (Container)context.Interactor;
        }
        else if (context.Interactor is IFeatureCollection col)
        {
            col.Get(out container);
        }

        if (container != null)
        {
            if (Container.RequestGrab(container.HeldItem))
                return;
        }
    }

    void Update()
    {
        Collection.Update();
    }
        
    #endregion
}