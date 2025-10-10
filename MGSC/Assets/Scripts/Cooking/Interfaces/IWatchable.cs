using Controller;

namespace Cooking
{

    public interface IWatchable
    {
        public bool IsLookedAt { get; set; }
        public void LookAt(PlayerInteractor interactor);
    }

}