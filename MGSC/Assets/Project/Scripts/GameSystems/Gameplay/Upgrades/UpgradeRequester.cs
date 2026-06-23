using UnityEngine;
using UnityEngine.Events;

namespace Upgrades
{
    public class UpgradeRequester : MonoBehaviour
    {
        #region Properties
        [SerializeField] private UpgradeConfig _targetUpgrade;
        [Space] 
        [SerializeField] private UnityEvent _onUpgradeGranted;
        [SerializeField] private UnityEvent _onUpgradeDenied;
        #endregion
        
        #region Fields
        private UpgradeManager _upgradeManager;
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            Reference.GetManager(out _upgradeManager);
            
            if (_upgradeManager == null)
                Debug.LogError("[UpgradeRequester] No UpgradeManager found", this);
            if (_targetUpgrade == null)
                Debug.LogError("[UpgradeRequester] No UpgradeConfig assigned", this);
            
        }

        #endregion

        #region Methods

        public void TryLevelUp()
        {
            if (_upgradeManager == null || _targetUpgrade == null) return;

            bool granted = _upgradeManager.TryLevelUp(_targetUpgrade);
            (granted ? _onUpgradeGranted : _onUpgradeDenied)?.Invoke();
        }

        #endregion
        

    }
}