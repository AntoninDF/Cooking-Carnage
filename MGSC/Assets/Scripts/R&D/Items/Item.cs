using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Item : MonoBehaviour, IInteractable, IGrabbable
{
    public ItemManager Manager { get; set; }
    public Action OnInteract { get; set; }
    public Transform Transform { get => this.transform; }

    public Collider Collider { get; set; }

    private List<Vector3> pointsToDebug = new();
    private Vector3 offsetToLowestPoint;
    [SerializeField] private GameObject previewValidObject;
    [SerializeField] private GameObject previewInvalidObject;
    [SerializeField] private EReleaseMode releaseMode;
    private ContainerSlot containerSlot;
    private Quaternion initialRotation;

    void Start()
    {
        Collider = GetComponent<Collider>();

        offsetToLowestPoint = FindLowestPoint();
        initialRotation = transform.rotation;
        

    }

    // Update is called once per frame
    void Update()
    {
        TryPreview(Manager.Interactor);
    }

    public void Interact(Interactor interactor)
    {
        OnInteract?.Invoke();
        Collider.isTrigger = true;
        bool didGrab = interactor.Inventory.Grab(this);
        if (containerSlot != null && didGrab)
        {
            containerSlot.Release();
            containerSlot = null;
        }
        Debug.Log("[Item] Interact : Interacting...");
    }

    public void TryInteract(Interactor interactor)
    {
        if (interactor.AllowInteraction(this) && interactor.LookInfo.collider.gameObject == this.gameObject)
        {
            Interact(interactor);
        }
    }

    public void Throw(Interactor interactor)
    {
        Manager.Inventory.Release();
        switch (releaseMode)
        {
            case EReleaseMode.NoPhysicsAndClip:
                Release1(interactor);
                break;
            case EReleaseMode.PhysicsAndClip:
                Release2(interactor);
                break;
            case EReleaseMode.PhysicsAndNoClip:
                Release3(interactor);
                break;
            case EReleaseMode.SlotsAndClip:
                Release4(interactor);
                break;
            case EReleaseMode.NoPhysicsAndNoClip:
                Release5(interactor);
                break;
            case EReleaseMode.NoPhysicsAndNoClipAndPreview:
                Release5(interactor);
                break;
            case EReleaseMode.SlotsAndNoClip:
                Release6(interactor);
                break;

        }

    }

    public void TryThrow(Interactor interactor) {if (interactor.AllowThrow(this)) Throw(interactor);}

    private void Release1(Interactor interactor)
    {
        if (interactor.LookTarget.GetComponent<ContainerSurface>() == null) return;
        Collider.isTrigger = true;
        transform.position = interactor.LookInfo.point;
        transform.rotation = initialRotation;
    }

    private void Release2(Interactor interactor)
    {
        if (interactor.LookTarget.GetComponent<ContainerSurface>() == null) return;
        transform.position = interactor.LookInfo.point;
        transform.rotation = initialRotation;
        Collider.isTrigger = false;
    }
    private void Release3(Interactor interactor)
    {
        if (interactor.LookTarget.GetComponent<ContainerSurface>() == null) return;
        pointsToDebug.Add(interactor.LookInfo.point);
        pointsToDebug.Add(interactor.LookInfo.point - offsetToLowestPoint);
        Debug.Log("[Item] Release3 : Closest point on the bounding box to the requested point to place : " + offsetToLowestPoint);
        SetPosition(interactor.LookInfo.point);
        Collider.isTrigger = false;
    }

    private void Release4(Interactor interactor)
    {
        ContainerSlot slot = interactor.LookTarget.GetComponent<ContainerSlot>();
        if (slot == null) return;
        if (slot.IsFree())
        {
            slot.Grab(this);
            containerSlot = slot;   
        }
        else
            Manager.Inventory.Grab(this);
    }

    private void Release5(Interactor interactor)
    {
        if (interactor.LookTarget.GetComponent<ContainerSurface>() == null) return;
        if (!IsValidSpot(interactor))
            Manager.Inventory.Grab(this);
        else
        {
            Collider.isTrigger = true;
            SetPosition(interactor.LookInfo.point);
        }
    }

    private void Release6(Interactor interactor)
    {
        Release4(interactor);
    }

    private void TryPreview(Interactor interactor) 
    {
        if (releaseMode != EReleaseMode.NoPhysicsAndNoClipAndPreview) return;
        if (previewValidObject == null || previewInvalidObject == null) InitPreviewObjects();
        if (Manager.Inventory.HeldItem == (IGrabbable)this) 
            Preview(interactor); 
        else 
            StopPreview(); 

    }
    private void Preview(Interactor interactor)
    {
        if (IsValidSpot(interactor))
        {
            previewValidObject.SetActive(true);
            previewInvalidObject.SetActive(false);
            previewValidObject.transform.position = interactor.LookInfo.point - offsetToLowestPoint;
        }
        else
        {
            previewInvalidObject.SetActive(true);
            previewValidObject.SetActive(false);
            previewInvalidObject.transform.position = interactor.LookInfo.point - offsetToLowestPoint;
        }
        
    }
    private void StopPreview()
    {
        if (Manager.Inventory.HeldItem == null)
        {
            previewValidObject.SetActive(false);
            previewInvalidObject.SetActive(false);
            
        }
    }

    private bool IsItem(GameObject go)
    {
        if (go == null) return false;
        if (go.GetComponent<Item>() == null) return false;
        return true;
    }

    private bool IsValidSpot(Interactor interactor)
    {
        List<Collider> colliders = Physics.OverlapBox(interactor.LookInfo.point, Collider.bounds.extents).ToList();

        if (colliders.Any((col) => IsItem(col.gameObject)))
        {
            return false;
        }
        if (interactor.LookInfo.collider.GetComponent<ContainerSurface>() == null) return false;
        return true;
    }

    private Vector3 FindLowestPoint()
    {
        Ray ray = new Ray(transform.position + Vector3.down * 3, Vector3.up);
        RaycastHit hitInfo;
        Collider.Raycast(ray, out hitInfo, 3);
        return hitInfo.point - transform.position;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position - offsetToLowestPoint;
        transform.rotation = initialRotation;
    }

    private void InitPreviewObjects()
    {
        MeshRenderer mR = GetComponent<MeshRenderer>();
        MeshFilter mF = GetComponent<MeshFilter>();
        if (mR == null || mF == null) return;
        if (previewValidObject == null)
        {
            previewValidObject = new GameObject(this.gameObject.name + "_ValidPreview");
            previewValidObject.transform.localScale = transform.localScale;
            previewValidObject.transform.rotation = initialRotation;
            MeshRenderer mr = previewValidObject.AddComponent<MeshRenderer>();
            MeshFilter mf = previewValidObject.AddComponent<MeshFilter>();
            Material mat = new Material(mR.material);
            mat.color = Color.green;
            mr.material = mat;
            mf.mesh = mF.mesh;
        }
        if (previewInvalidObject == null)
        {
            previewInvalidObject = new GameObject(this.gameObject.name + "_InvalidPreview");
            previewInvalidObject.transform.localScale = transform.localScale;
            previewInvalidObject.transform.rotation = initialRotation;
            MeshRenderer mr = previewInvalidObject.AddComponent<MeshRenderer>();
            MeshFilter mf = previewInvalidObject.AddComponent<MeshFilter>();
            Material mat = new Material(mR.material);
            mat.color = Color.red;
            mr.material = mat;

            mf.mesh = mF.mesh;
        }
    }

#if UNITY_EDITOR
    private void DebugPoints()
    {
        foreach(Vector3 point in pointsToDebug)
        {
            Gizmos.color = Color.yellow;
            Debug.Log("[Item] DebugPoints : Drawing points to debug");
            Gizmos.DrawSphere(point, 0.03f);
        }
    }

    void OnDrawGizmos()
    {
        DebugPoints();
    }
#endif
}
