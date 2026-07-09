using System;
using GameSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Object = System.Object;


namespace Upgrades
{

    public class UpgradeManager : IManager, IDisposable
    {
        #region Properties
        
        public List<Upgrade> Upgrades => _upgrades;

        [GlobalEvent("Upgrades/OnTryLevelUpFailed")]
        public static UnityEvent OnUpgradeFail    = new UnityEvent();
        [GlobalEvent("Upgrades/OnLevelUp")]
        public static UnityEvent OnUpgradeSuccess = new UnityEvent();
        
        #endregion

        #region Fields
        
        private List<Upgrade>                    _upgrades = new();
        private MoneyManager                     _moneyManager;
        private Dictionary<Type, SubscriberList> _subscribers = new();
        
        #endregion

        #region Methods

        public UpgradeManager()
        {
            Register();
           // ( as MonoBehaviour).enabled = true;
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

            _upgrades    = new List<Upgrade>();
            _subscribers = new Dictionary<Type, SubscriberList>();

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

                var upgrade = (Upgrade)Activator.CreateInstance(type);
                upgrade.SetConfig(entry.Config);

                upgrade.OnLevelUpInternal += DispatchLevelUp;

                _upgrades.Add(upgrade);
            }

            if (!Reference.TryGetManager(out _moneyManager))
                this.Log(LogType.Warning, "No MoneyManager found");
            else
                this.Log(LogType.Log, "UpgradeManager correctly initialized");
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="upgrade"></param>
        private void DispatchLevelUp(Upgrade upgrade)
        {
            Debug.Log("Applying upgrade " + upgrade?.GetType().Name);
            var type = upgrade.GetType();
            if (_subscribers.TryGetValue(type, out var list))
                list.Dispatch(upgrade);
        }

        public void ActualizeAllUpgrade()
        {
            foreach (var upgrade in _upgrades)
            {
                DispatchLevelUp(upgrade);
            }
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
            OnUpgradeSuccess?.Invoke();
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
        
        #region Subscriber Wrapper

        private abstract class SubscriberList
        {
            public abstract void Dispatch(Upgrade upgrade);
        }

        private class SubscriberList<T> : SubscriberList where T : Upgrade
        {
            public List<IUpgradable<T>> Subscribers = new();

            public override void Dispatch(Upgrade upgrade)
            {
                Debug.Log("Subscribers are :\n" + Subscribers.ToString_EasyRead(separator: "\n"));
                if (upgrade is not T typed) return;
                foreach (var sub in Subscribers)
                {
                    Debug.Log("Upgrading subscriber " + sub);
                    sub.OnUpgraded(typed);
                }
            }
        }
        
        public void Subscribe<T>(IUpgradable<T> subscriber) where T : Upgrade<T>
        {
            if (GetUpgradeByType<T>() is T upgrade)
                upgrade.Subscribe(subscriber);
            else
                this.Log(LogType.Warning, $"[UpgradeManager] Aucune upgrade de type {typeof(T).Name} trouvée pour {subscriber.GetType().Name}");
        }

        public void UnsubscribeFromAll(object subscriber)
        {
            foreach (var upgrade in _upgrades)
                upgrade.UnsubscribeObject(subscriber);
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
        }

        public void SaveUpgrades()
        {
            
        }
    }
}