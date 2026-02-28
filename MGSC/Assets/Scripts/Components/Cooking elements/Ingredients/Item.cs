
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

    private Bounds bounds;

    #endregion

    #region Methods

    protected virtual void Start()
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

    protected void BindEvents()
    {
        Grabbable.OnGrab += TriggerAnim;
        Interactable.OnInteraction += TryGetGrabbed;
    }

    private void TriggerAnim()
    {
        Transform containerTransform = Grabbable.Container.GetParentTransform();
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

    private void TryGetGrabbed(InteractionContext context)
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
            container.RequestGrab(Grabbable);
        }
    }
    #endregion
}

