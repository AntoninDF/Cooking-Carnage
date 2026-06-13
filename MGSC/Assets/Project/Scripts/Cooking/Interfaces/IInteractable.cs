using Controller;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{

    public interface IInteractable : IGeneric<IInteractable>
    {
        public bool RequestInteraction(PlayerInteractor interactor);
        public UnityEvent OnInteract {  get; }
        public Collider Collider { get; }
        public bool IsLocked { get; }
    }

}