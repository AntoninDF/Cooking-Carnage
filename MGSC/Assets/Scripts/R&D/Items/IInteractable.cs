using System;
using UnityEngine;

public interface IInteractable 
{
    public void Interact(Interactor interactor);
    public Action OnInteract {  get; }
}
