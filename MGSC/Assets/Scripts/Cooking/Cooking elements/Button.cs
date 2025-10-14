using Controller;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// Can be interacted with, triggers a unity event
    /// </summary>
    public class Button : Interactable
    {
        public UnityEvent OnPress;

        public override void Interact(PlayerInteractor interactor)
        {
            base.Interact(interactor);
            OnPress?.Invoke();
        }
    }

}