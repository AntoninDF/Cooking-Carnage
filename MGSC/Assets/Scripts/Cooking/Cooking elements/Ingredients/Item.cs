
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
        BindEvents();
    }

    protected void BindEvents()
    {
        Grabbable.OnGrab += TriggerAnim;
    }

    private void TriggerAnim()
    {
        if (Grabbable.Container == null || Grabbable.Container.transform == null)
        {
            Anim.GoTo(Vector3.down * Bounds.FindLocalLowestPoint());
        }
        if (Grabbable.Container.TryGetSibling(out LookAtTarget lookTarget))
        {
            Anim.GoTo(Grabbable.Container.transform, lookTarget.LookPosition + Vector3.down * Bounds.FindLocalLowestPoint());
        }
        else
        {
            Anim.GoTo(Grabbable.Container.transform);
        }
    }
    #endregion
}

