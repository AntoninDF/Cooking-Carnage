using GameSystem;
using UnityEngine;
using UnityEngine.Events;
using Upgrades;

namespace Assets.Scripts.Upgrades
{
    public class UpgradeRequester : MonoBehaviour
    {
        #region Fields
        [SerializeField] private UnityEvent _onUpgradeDenied; 
        [SerializeField] private UnityEvent _onUpgradeGranted; 
        [SerializeField] private UnityEvent _onNewOvenUpgradeDenied; 
        [SerializeField] private UnityEvent _onNewOvenUpgradeGranted; 
        [SerializeField] private UnityEvent _onCookingSpeedUpgradeDenied; 
        [SerializeField] private UnityEvent _onCookingSpeedUpgradeGranted; 
        [SerializeField] private UnityEvent _onCuttingSpeedUpgradeDenied; 
        [SerializeField] private UnityEvent _onCuttingSpeedUpgradeGranted; 
        [SerializeField] private UnityEvent _onIngredientDropsUpgradeDenied; 
        [SerializeField] private UnityEvent _onIngredientDropsUpgradeGranted; 
        [SerializeField] private UnityEvent _onBlenderSpeedUpgradeDenied; 
        [SerializeField] private UnityEvent _onBlenderSpeedUpgradeGranted; 


        private UpgradeManager _upgradeManager;
        #endregion
        // Use this for initialization
        void Start()
        {
            _upgradeManager = ReferenceManager.Instance.GetManager<UpgradeManager>();
            if (_upgradeManager == null)
                Debug.LogError("[UpgradeRequester] : No Upgrade Manager found");
        }

        private bool TryLevelUp<T>() where T : Upgrade
        {
            bool levelUpGranted = _upgradeManager.TryLevelUp<T>();
            if (levelUpGranted)
                _onUpgradeGranted?.Invoke();
            else
                _onUpgradeDenied?.Invoke();
            return levelUpGranted;
        }

        public void TryLevelUpOvenCount()
        {
            bool levelUpGranted = TryLevelUp<OvenCountUpgrade>();
            if (levelUpGranted)
                _onNewOvenUpgradeGranted?.Invoke();
            else
                _onNewOvenUpgradeDenied?.Invoke();
            //return levelUpGranted;
        }
        public void TryLevelUpCookingSpeed()
        {
            bool levelUpGranted = TryLevelUp<CookingSpeedUpgrade>();
            if (levelUpGranted)
                _onCookingSpeedUpgradeGranted?.Invoke();
            else
                _onCookingSpeedUpgradeDenied?.Invoke();
            //return levelUpGranted;
        }
        public void TryLevelUpIngredientCount()
        {
            bool levelUpGranted = TryLevelUp<IngredientDropUpgrade>();
            if (levelUpGranted)
                _onIngredientDropsUpgradeGranted?.Invoke();
            else
                _onIngredientDropsUpgradeDenied?.Invoke();
            //return levelUpGranted;
        }
        public void TryLevelUpBlenderSpeed()
        {
            bool levelUpGranted = TryLevelUp<BlenderSpeedUpgrade>();
            if (levelUpGranted)
                _onBlenderSpeedUpgradeGranted?.Invoke();
            else
                _onBlenderSpeedUpgradeDenied?.Invoke();
            //return levelUpGranted;
        }

    }
}