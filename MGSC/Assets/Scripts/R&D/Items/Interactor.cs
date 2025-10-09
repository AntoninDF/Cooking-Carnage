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


    private RaycastHit hit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (inventory == null) inventory = FindAnyObjectByType<Inventory>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLookInfo();

        if (CheckForThrowInput())
        {
            OnTryThrow?.Invoke(this);
        }

        if (CheckForInteractInput())
        {
            OnTryInteract?.Invoke(this);
            InteractWithLookTarget();
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

    private void InteractWithLookTarget()
    {
        IInteractable interactable;
        if (AllowInteraction(out interactable))
        {
            interactable.Interact(this);
        }
    }

    /// <summary>
    /// Check if the looked at target has a component implementing IInteractable, and if so, passes it back as out parameter. Otherwise it will be null.
    /// </summary>
    /// <param name="interactable">The interactable that the player is looking at</param>
    /// <returns></returns>
    private bool AllowInteraction(out IInteractable interactable)
    {
        if (LookTarget != null)
        {
            foreach (var comp in LookTarget.GetComponents<Component>())
            {
                if (comp is IInteractable)
                {
                    interactable = (IInteractable)comp;
                    return true;
                }
            }
        }
        interactable = null;
        return false;
    }

    public bool AllowThrow<T>(T interactable) where T : IGrabbable
    {
        
        if (LookTarget.HasComponent<ContainerSurface>() &&
            LookTarget.HasComponent<ContainerSlot>())
            return false;
        return true;
    }

    public void SetLookTransform()
    {
        Camera cam = Camera.main;
        lookTransform = cam.transform;
    }

    private void UpdateLookInfo()
    {
        SetLookTransform();
        if (Physics.Raycast(lookTransform.position, lookTransform.rotation * Vector3.forward, out hit, reachDistance))
        {
            LookInfo = hit;
            LookTarget = LookInfo.collider.gameObject;
        }
        // The case when the player looks at nothing (the sky) isn't handled
    }
}
