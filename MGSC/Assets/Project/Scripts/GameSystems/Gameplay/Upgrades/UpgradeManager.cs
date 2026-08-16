using System;
using GameSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


namespace Upgrades
{

    public class UpgradeManager : IManager, IDisposable
    {
        #region Properties
        
        public List<Upgrade> Upgrades => _upgrades;
        public List<UpgradeLevelUpInfo> LevelUpHistory = new();

        [GlobalEvent("Upgrades/OnTryLevelUpFailed")]
        public static UnityEvent OnUpgradeFail    = new UnityEvent();
        [GlobalEvent("Upgrades/OnLevelUp")]
        public static UnityEvent OnUpgradeSuccess = new UnityEvent();
        
        #endregion

        #region Fields
        
        private List<Upgrade> _upgrades = new();
        private MoneyManager  _moneyManager;

        private bool          _isInitialized = false;
        private readonly List<Action> _pendingSubscriptions = new();
        
        #endregion

        #region Methods

        public UpgradeManager()
        {
            Register();
            GameLoop.OnAfterReset += ActualizeAllUpgrade;
            OnUpgradeSuccess.AddListener(CheckIfAllUpgradesAreUnlocked);
        }
        
        void CheckIfAllUpgradesAreUnlocked()
        {
            bool valid = true;
            foreach (var upgrade in _upgrades)
            {
                if (upgrade.Level != upgrade.Config.MaxLevel)
                {
                    valid = false;
                    break;
                }
            }
            if (valid)
                Achievements.Unlock("ACH_UMAX");
        }

        /// <summary>
        /// Init all the Upgrades and give them their ScriptableObject
        /// </summary>
        public void Init()
        {
            var index = UpgradeBindingIndex.Instance;
            if (index == null)
            {
                this.Log(LogType.Error, "UpgradeBindingIndex not found — cannot initialize upgrades");
                return;
            }

            _upgrades = new List<Upgrade>();

            var concreteTypes = AppDomain.CurrentDomain.GetAssemblies()
                                         .SelectMany(a => a.GetTypes())
                                         .Where(t => t.IsSubclassOf(typeof(Upgrade)) && !t.IsAbstract)
                                         .ToDictionary(t => t.Name);

            foreach (var entry in index.Bindings)
            {
                if (entry.Config == null)
                {
                    this.Log(LogType.Warning, $"Config is null for '{entry.UpgradeTypeName}' — skipping");
                    continue;
                }

                if (!concreteTypes.TryGetValue(entry.UpgradeTypeName, out var type))
                {
                    this.Log(LogType.Warning, $"No type found for '{entry.UpgradeTypeName}' — skipping");
                    continue;
                }

#if DEMO
                // Force specific upgrades to be enabled, and only these upgrades
                Type[] demoOnlyUpgrades = new[] {
                    typeof(BlenderSpeedUpgrade),
                    typeof(CookingSpeedUpgrade)
                };
                foreach (Type demoType in demoOnlyUpgrades)
                {
                    if (type == demoType)
                    {
                        entry.Config.IsLocked = false;
                        break;
                    }
                    else
                        entry.Config.IsLocked = true;
                }
#endif

                var upgrade = (Upgrade)Activator.CreateInstance(type);
                upgrade.SetConfig(entry.Config);

                _upgrades.Add(upgrade);
            }

            if (!Reference.TryGetManager(out _moneyManager))
                this.Log(LogType.Warning, "No MoneyManager found");
            else
                this.Log(LogType.Log, "UpgradeManager correctly initialized");
            
            _isInitialized = true;
            FlushPendingSubscriptions();
        }

        private void FlushPendingSubscriptions()
        {
            if (_pendingSubscriptions.Count == 0) return;

            var pending = _pendingSubscriptions.ToList();
            _pendingSubscriptions.Clear();

            foreach (var subscribeAction in pending)
                subscribeAction.Invoke();
        }

        public void ActualizeAllUpgrade()
        {
            foreach (var upgrade in _upgrades)
                upgrade.NotifyAllSubscribers();
        }
        

        #endregion
        
        #region Public Mehtods

        /// <summary>
        /// Try to level up but by using a config to access the correct upgrade
        /// </summary>
        public bool TryLevelUp(UpgradeConfig config)
        {
            var upgrade = GetUpgradeByConfig(config);
            if (upgrade == null)
            {
                this.Log(LogType.Error, $"No upgrade found for config '{config.name}'");
                return false;
            }
            return TryLevelUpInternal(upgrade);
        }
        
        private bool TryLevelUpInternal(Upgrade upgrade)
        {

            if (!upgrade.CanLevelUp())
            {
                OnUpgradeFail?.Invoke();
                return false;
            }
            
            //check if the player have enough money
            float price = upgrade.GetPrice();
            if (_moneyManager.MoneyInfo < price)
            {
                this.Log(LogType.Log, $"Not enough money for {upgrade.GetType().Name}");
                OnUpgradeFail?.Invoke();
                return false;
            }
            
            _moneyManager.AddMoney(-price);
            upgrade.LevelUp();
            this.Log(LogType.Log, $"{upgrade.GetType().Name} leveled up to {upgrade.Level}");
            return true;
        }
        
        public T       GetUpgradeByType<T>() where T : Upgrade  => _upgrades.OfType<T>().FirstOrDefault();
        public Upgrade GetUpgradeByType(Type            type)   => _upgrades.FirstOrDefault(u => u.GetType() == type);
        public Upgrade GetUpgradeByConfig(UpgradeConfig config) => _upgrades.FirstOrDefault(u => u.Config == config);
        
        public bool HasUpgradeOfType<T>(out T upgrade) where T : Upgrade
        {
            upgrade = GetUpgradeByType<T>();
            return upgrade != null;
        }

        public Dictionary<Type, int> GetUpgradeLevelsOnShift(int shiftNumber)
        {
            // Retrieve the highest level reached for each upgrade, before this shift
            Dictionary<Type, int> previousUpgradeLevels = new();
            foreach (var info in LevelUpHistory)
            {
                if (!previousUpgradeLevels.ContainsKey(info.TargetUpgrade))
                    previousUpgradeLevels[info.TargetUpgrade] = 0;
                if (info.ShiftNumber <= shiftNumber && info.NewUpgradeLevel > previousUpgradeLevels[info.TargetUpgrade])
                    previousUpgradeLevels[info.TargetUpgrade] = info.NewUpgradeLevel;
            }
            return previousUpgradeLevels;
        }
        public void ApplyLevels(Dictionary<Type, int> upgradeLevels)
        {
            foreach (var upgrade in Upgrades)
            {
                Type type = upgrade.GetType();
                upgrade.SetLevel(0);
                if (upgradeLevels.ContainsKey(type))
                    upgrade.SetLevel(upgradeLevels[type]);
            }
        }

        #endregion
        
        #region IManager

        public void Register()
        {
            Reference.SetManager(this);
        }
        public void Unregister()
        {
            Debug.Log("Unregistered " + GetType().Name);
            Reference.RemoveManager(this);
        }

        #endregion
        
        #region Subscription

     
        public void Subscribe<T>(IUpgradable<T> subscriber) where T : Upgrade<T>
        {
            if (!_isInitialized)
            {
                _pendingSubscriptions.Add(() => Subscribe(subscriber));
                return;
            }

            if (GetUpgradeByType<T>() is T upgrade)
                upgrade.Subscribe(subscriber);
            else
                this.Log(LogType.Warning, $"[UpgradeManager] Aucune upgrade de type {typeof(T).Name} trouvée pour {subscriber.GetType().Name}");
        }

        public void UnsubscribeFromAll(object subscriber)
        {
            foreach (var upgrade in _upgrades)
                upgrade.UnsubscribeObject(subscriber);
            
            _pendingSubscriptions.RemoveAll(_ => false); 
        }

        public void ClearAllSubscribers()
        {
            foreach (var upgrade in _upgrades)
                upgrade.ClearSubscribers();
        }

        #endregion

        public void Dispose()
        {
            Debug.Log($"Disposing : {GetType().Name} & Clearing All Subscribers" );
            ClearAllSubscribers();
            _pendingSubscriptions.Clear();
        }

        [Serializable]
        public struct UpgradeLevelUpInfo
        {
            public SerializableSystemType TargetUpgrade;
            public int PreviousUpgradeLevel;
            public int NewUpgradeLevel;
            public float Price;
            public int ShiftNumber;
        }
        
    }
}