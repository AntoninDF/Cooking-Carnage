using UnityEngine;
using UnityEngine.Events;

namespace Upgrades
{
    /// <summary>
    /// Script that request an upgrade for a specific Upgrade
    /// UpgradeManager will use the config as a key to allow the good upgrade to update
    /// </summary>
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

        /// <summary>
        /// Method that request an upgrade to the UpgradeManager
        /// </summary>
        public void TryLevelUp()
        {   
            if (!Globals.Gameplay.AllowUpgrades) return;
            if (_upgradeManager == null || _targetUpgrade == null) return;

            bool granted = _upgradeManager.TryLevelUp(_targetUpgrade);
            (granted ? _onUpgradeGranted : _onUpgradeDenied)?.Invoke();
        }

        #endregion
        

    }
}