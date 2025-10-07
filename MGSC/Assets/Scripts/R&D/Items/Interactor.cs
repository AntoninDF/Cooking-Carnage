/*
    The interactor is a player component that enables interaction with items.
    It provides information about the player in the context of an interaction
*/

using UnityEngine;
using System;
public class Interactor : MonoBehaviour
{
    public Inventory Inventory { get => inventory; set => inventory = value; }
    [SerializeField] private Inventory inventory;

    public GameObject LookTarget { get; set; }
    public RaycastHit LookInfo { get; set; }

    private Transform lookTransform;
    [SerializeField] private float reachDistance = 10f;
    public Action<Interactor> OnTryInteract {  get; set; }
    public Action<Interactor> OnTryThrow {  get; set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (inventory == null) inventory = FindAnyObjectByType<Inventory>();
    }

    // Update is called once per frame
    void Update()
    {

        if (CheckForThrowInput())
        {
            Debug.Log("[Interactor] Update : Tring to throw an object");
            OnTryThrow?.Invoke(this);
        }

        if (CheckForInteractInput())
        {
            Debug.Log("[Interactor] Update : Trying to interact");
            OnTryInteract?.Invoke(this);
        }

    }

    private bool CheckForInteractInput()
    {
        return Input.GetMouseButtonDown(0);
    }

    private bool CheckForThrowInput()
    {
        return Input.GetMouseButtonDown(0);
    }

    public bool AllowInteraction<T>(T interactable) where T : IInteractable
    {
        RaycastHit hit;
        SetLookTransform();
        if (Physics.Raycast(lookTransform.position, lookTransform.rotation * Vector3.forward, out hit, reachDistance))
        {
            if (hit.collider.gameObject.GetComponent<T>() != null)
            {
                LookTarget = hit.collider.gameObject;
                LookInfo = hit;
                return true;
            }
        }
        return false;
    }

    public bool AllowThrow<T>(T interactable) where T : IGrabbable
    {
        RaycastHit hit;
        SetLookTransform();
        if (Physics.Raycast(lookTransform.position, lookTransform.rotation * Vector3.forward, out hit, reachDistance))
        {
            if (hit.collider.gameObject.GetComponent<ContainerSurface>() != null)
            {
                LookTarget = hit.collider.gameObject;
                LookInfo = hit;
                return true;
            }
        }
        return false;
    }

    public void SetLookTransform()
    {
        Camera cam = Camera.main;
        lookTransform = cam.transform;
    }
}
