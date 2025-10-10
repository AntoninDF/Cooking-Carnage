using Controller;
using System;

namespace Cooking
{

    public interface IInteractable
    {
        public void Interact(PlayerInteractor interactor);
        public Action OnInteract {  get; }
    }

}