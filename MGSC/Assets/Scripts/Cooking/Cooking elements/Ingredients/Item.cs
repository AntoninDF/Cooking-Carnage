
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
    #endregion

    #region Fields


    #endregion

    #region Methods

    protected virtual void Start()
    {
        Collider = GetComponent<Collider>();
        Grabbable = new(this);
        Anim = new(this, transform);
        LookAtTarget = new(this, transform);
        BindEvents();
    }

    protected void BindEvents()
    {
        Grabbable.OnGrab += () => Anim.GoTo(Grabbable.Container?.transform, setAsParent: true);
    }
    #endregion
}

