using Controller;
using System;
using UnityEngine;

namespace Cooking
{

    public interface IInteractable : IGeneric<IInteractable>
    {
        public void Interact(PlayerInteractor interactor);
        public Action OnInteract {  get; }
        public Collider Collider { get; }
    }

}