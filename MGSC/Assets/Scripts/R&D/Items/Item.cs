using System;
using UnityEngine;

public class Item : MonoBehaviour, IInteractable, IGrabbable
{
    public ItemManager Manager { get; set; }
    public Action OnInteract {  get; set; }
    public Transform Transform { get => this.transform; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact(Interactor interactor)
    {
        OnInteract?.Invoke();
        interactor.Inventory.Grab(this);
        Debug.Log("[Item] Interact : Interacting...");
    }

    public void TryInteract(Interactor interactor)
    {
        if (interactor.AllowInteraction(this))
        {
            Interact(interactor);
        }
    }

    public void Throw(Interactor interactor)
    {
        Manager.Inventory.Release();
        Release1(interactor);
    }

    public void TryThrow(Interactor interactor)
    {
        if (interactor.AllowThrow(this))
        {
            Throw(interactor);
        }
    }

    private void Release1(Interactor interactor)
    {
        transform.position = interactor.LookInfo.point;
    }
}
