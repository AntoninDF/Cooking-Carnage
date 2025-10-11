using Controller;
using UnityEngine;

namespace Cooking
{

    public interface IWatchable : IGeneric<IWatchable>
    {
        public bool IsLookedAt {get;}
        public void LookAt(PlayerInteractor interactor);
        public Collider Collider { get; }
    }

}