using GameSystem;
using UnityEngine;
using Upgrades;

namespace Assets.Scripts.Upgrades
{
    public class UpgradeRequester : MonoBehaviour
    {
        #region Fields
        private UpgradeManager _upgradeManager;
        #endregion
        // Use this for initialization
        void Start()
        {
            _upgradeManager = ReferenceManager.Instance.GetManager<UpgradeManager>();
            if (_upgradeManager == null)
                Debug.LogError("[UpgradeRequester] : No Upgrade Manager found");
        }

        private void TryLevelUp<T>() where T : Upgrade => _upgradeManager.TryLevelUp<T>();

        public void TryLevelUpOvenCount() => TryLevelUp<OvenCountUpgrade>();
        public void TryLevelUpCookingSpeed() => TryLevelUp<CookingSpeedUpgrade>();

    }
}