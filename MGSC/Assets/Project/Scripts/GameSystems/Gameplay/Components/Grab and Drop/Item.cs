
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

        SetActiveModel(defaultModel);
        BindEvents();
    }
    protected virtual void Start()
    {
        Collection.LateSetup();
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
            // Go to world origin
            Anim.GoTo(Vector3.down * Bounds.FindLocalLowestPoint());
        }
        if (Grabbable.Container.TryGetSibling(out F_LookAtTarget lookTarget) && Grabbable.Container.AllowItemPlacedOnSurfaceRatherThanTransform)
        {
            // Go to the new container at the spot pointed at (for surface containers)
            Anim.GoTo(containerTransform, lookTarget.LookPosition + Vector3.down * Bounds.FindLocalLowestPoint());
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
    protected virtual void TryGetGrabbed(InteractionContext context)
    {
        if (context.TryGetFeature(out F_Container container))
        {
            container.RequestGrab(Grabbable);
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

        Debug.Log("Setting active...");
        activeModel.gameObject.SetActive(true);
        activeModel.SetActive(true);
        Debug.Log("Has set active");

        Debug.Log("MeshControllers : " + activeModel.MeshController.Controllers.Length);
        Mesh colliderMesh = activeModel.MeshController.GetCombinedMeshes(transform, true);
        Debug.Log("Collider mesh set");
        Collider.sharedMesh = colliderMesh;

        LookAtTarget.OnStartLookAt += activeModel.Outline.EnableOutline;
        LookAtTarget.OnStopLookAt += activeModel.Outline.DisableOutline;
    }
    #endregion

    
}

