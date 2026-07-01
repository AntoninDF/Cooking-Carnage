using System;
using GameSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using UnityEngine.Events;


namespace Upgrades
{

    public class UpgradeManager : IManager
    {
        #region Properties
        
        public List<Upgrade> Upgrades => _upgrades;
        
        public UnityEvent OnUpgradeFail    = new UnityEvent();
        public UnityEvent OnUpgradeSuccess = new UnityEvent();
        
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
            var type = upgrade.GetType();
            if (_subscribers.TryGetValue(type, out var list))
                list.Dispatch(upgrade);
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
        
        public T GetUpgradeByType<T>() where T : Upgrade => _upgrades.OfType<T>().FirstOrDefault();
        public Upgrade GetUpgradeByType(Type type) => _upgrades.FirstOrDefault(u => u.GetType() == type);
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
                if (upgrade is not T typed) return;
                foreach (var sub in Subscribers)
                    sub.OnUpgraded(typed);
            }
        }

        #endregion
        
    }
}