using Controller;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{

    public interface IWatchable : IGeneric<IWatchable>
    {
        public bool IsLookedAt {get;}
        public void LookAt(PlayerInteractor interactor);
        public Collider Collider { get; }
        public UnityEvent OnLookAt { get; }
        public UnityEvent OnStopLookAt { get; }

    }

}