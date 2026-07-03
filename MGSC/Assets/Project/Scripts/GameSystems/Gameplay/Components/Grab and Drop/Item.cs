
using Features;
using UnityEngine;


/// <summary>
/// Adds every necessary item behaviour
/// </summary>
[RequireComponent(typeof(MeshCollider))]
public class Item : MonoBehaviour, IFeatureCollection
{
    #region Properties
    public bool IsLocked { get;set; }

    public bool EnableCallbacks { get; set; }


    public FeatureCollection Collection { get; set; } = new();
    public F_Grabbable Grabbable {get; set;}
    public F_GoToAnimation Anim { get; set; }
    public F_LookAtTarget LookAtTarget { get; set; }
    public F_Interactable Interactable { get; set; }
    public MeshCollider Collider { get; set; }
    public F_BoundsFinder Bounds { get; set; }
    #endregion

    #region Fields

    [SerializeField] protected ModelController defaultModel;
    protected Bounds bounds;

    protected ModelController activeModel;

    #endregion

    #region Methods

    protected virtual void Awake()
    {
        Collider = GetComponent<MeshCollider>();
        if (defaultModel == null)
            defaultModel = GetComponent<ModelController>();

        Grabbable = new(this);
        Anim = new(this, transform);
        LookAtTarget = new(this, transform);
        Bounds = new(this, transform);
        Bounds.Get(out bounds);
        Interactable = new(this);

        BindEvents();
    }
    protected virtual void Start()
    {
        SetActiveModel(defaultModel);
        Collection.LateSetup();
    }
    /// <summary>
    /// BindEvents is called in Awake. It should not depend on other objects being initialized.
    /// </summary>
    protected virtual void BindEvents()
    {
        Grabbable.OnGrab += TriggerAnim;
        Grabbable.OnLock += () => Collider.enabled = false;
        Grabbable.OnUnlock += () => Collider.enabled = true;
        Interactable.OnInteraction += TryInteract;
    }

    protected void TriggerAnim()
    {
        if (Grabbable.Container == null)
        {
            // Go to world origin
            Anim.GoTo(Vector3.down * Bounds.FindLocalLowestPoint());
            return;
        }

        // Find target transform
        Transform containerTransform = Grabbable.Container.transform;
        if (containerTransform == null) containerTransform = Grabbable.Container.GetParentTransform();

        if (containerTransform == null)
        {
            // Go to world origin
            Anim.GoTo(Vector3.down * Bounds.FindLocalLowestPoint());
        }
        else if (Grabbable.Container.TryGetSibling(out F_LookAtTarget lookTarget) && Grabbable.Container.AllowItemPlacedOnSurfaceRatherThanTransform)
        {
            // Go to the new container at the spot pointed at (for surface containers)
            Anim.GoTo(containerTransform, lookTarget.LookPosition ); // + Vector3.down * Bounds.FindLocalLowestPoint()
        }
        else
        {
            // Go to 0,0,0 in the new container
            Anim.GoTo(containerTransform);
        }
    }

    protected virtual void TryInteract(InteractionContext context)
    {
        TryGetGrabbed(context);
    }
    protected virtual bool TryGetGrabbed(InteractionContext context)
    {
        if (!context.TryGetFeature(out F_Container container)) return false;

        return container.RequestGrab(Grabbable);
    }

    public void SwapWith(F_Grabbable other)
    {
        if (other == null) return;
        var otherContainer = other.Container;
        var currentContainer = Grabbable.Container;
        
        if (currentContainer != null && currentContainer.IsLocked) return;
        if (otherContainer != null && otherContainer.IsLocked) return;
        
        var targetPos = transform.position;
        var parent = transform.parent;

        otherContainer?.RequestDrop(other);
        otherContainer?.RequestGrab(Grabbable);

        currentContainer?.RequestGrab(other);
        if (other.TryGetSibling(out F_GoToAnimation anim))
        {
            if (currentContainer != null)
            {
                anim.GoTo(currentContainer.transform, targetPos);
            }
            else if (parent != null)
            {
                anim.GoTo(parent, targetPos);
            }
        }

    }
    public void SetActiveModel(ModelController model)
    {
        if (activeModel != null)
        {
            activeModel.SetActive(false);
            activeModel.gameObject.SetActive(false);
            LookAtTarget.OnStartLookAt -= activeModel.Outline.EnableOutline;
            LookAtTarget.OnStopLookAt -= activeModel.Outline.DisableOutline;
        }

        activeModel = model;
        if (activeModel == null)
            return;

        activeModel.gameObject.SetActive(true);
        activeModel.SetActive(true);

        Mesh colliderMesh = activeModel.MeshController.GetCombinedMeshes(transform, true);
        Collider.sharedMesh = colliderMesh;

        LookAtTarget.OnStartLookAt += activeModel.Outline.EnableOutline;
        LookAtTarget.OnStopLookAt += activeModel.Outline.DisableOutline;
    }
    #endregion

    
}

