
using Features;
using UnityEngine;


/// <summary>
/// Adds every necessary item behaviour
/// </summary>
[RequireComponent(typeof(Collider))]
public class Item : MonoBehaviour, IFeatureCollection
{
    #region Properties
    public bool IsLocked { get;set; }

    public bool EnableCallbacks { get; set; }


    public FeatureCollection Collection { get; set; } = new();
    public Grabbable Grabbable {get; set;}
    public GoToAnimation Anim { get; set; }
    public Collider Collider { get; set; }
    public LookAtTarget LookAtTarget { get; set; }
    public Interactable Interactable { get; set; }
    public BoundsFinder Bounds { get; set; }
    #endregion

    #region Fields

    protected Bounds bounds;

    #endregion

    #region Methods

    protected virtual void Awake()
    {
        Collider = GetComponent<Collider>();


        Grabbable = new(this);
        Anim = new(this, transform);
        LookAtTarget = new(this, transform);
        Bounds = new(this, transform);
        Bounds.Get(out bounds);
        Interactable = new(this);

        BindEvents();
    }
    /// <summary>
    /// BindEvents is called in Awake. It should not depend on other objects being initialized.
    /// </summary>
    protected virtual void BindEvents()
    {
        Grabbable.OnGrab += TriggerAnim;
        Interactable.OnInteraction += TryInteract;
    }

    protected void TriggerAnim()
    {
        Transform containerTransform = Grabbable.Container.transform;
        if (containerTransform == null) Grabbable.Container.GetParentTransform();
        if (Grabbable.Container == null || containerTransform == null)
        {
            Anim.GoTo(Vector3.down * Bounds.FindLocalLowestPoint());
        }
        if (Grabbable.Container.TryGetSibling(out LookAtTarget lookTarget))
        {
            Anim.GoTo(containerTransform, lookTarget.LookPosition + Vector3.down * Bounds.FindLocalLowestPoint());
        }
        else
        {
            Anim.GoTo(containerTransform);
        }
    }

    protected virtual void TryInteract(InteractionContext context)
    {
        TryGetGrabbed(context);
    }
    protected virtual void TryGetGrabbed(InteractionContext context)
    {
        if (context.TryGetFeature(out Container container))
        {
            container.RequestGrab(Grabbable);
        }
    }
    #endregion
}

