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
        
        public UnityEvent OnUpgradeFail;
        public UnityEvent OnUpgradeSuccess;
        
        #endregion

        #region Fields
        
        private List<Upgrade>        _upgrades;
        private MoneyManager         _moneyManager;
        private UpgradeManagerConfig _config;
        
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
            _config = UpgradeManagerConfig.instance;
            
            if (_config == null)
                throw new InvalidOperationException("[UpgradeManager] SetConfig() must be called before Init().");
            

            //Get all the inherited upgrades 
            var allUpgrade = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Upgrade)) && !t.IsAbstract);

            //Init every upgrades and add them to the upgrade list
            foreach (var upgrade in allUpgrade)
            {
                _upgrades.Add((Upgrade)Activator.CreateInstance(upgrade));
            }

            if (!Reference.TryGetManager(out _moneyManager))
            {
                this.Log(LogType.Warning, "No money manager found");
            }
            else
            {
                this.Log(LogType.Log, "UpgradeManager correctly initialized");
            }

            ApplyUpgrades();
        }

        public void ApplyUpgrades()
        {
            foreach (var upgrade in _upgrades)
            {
                upgrade.Apply();
            }
        }

        public T GetUpgradeByType<T>() where T : Upgrade
        {
            foreach (var upgrade in _upgrades)
                if (upgrade is T)
                    return upgrade as T;
            return null;
        }
        
        public Upgrade GetUpgradeByType(Type type)
        {
            return _upgrades.FirstOrDefault(u => u.GetType() == type);
        }

        /// <summary>
        /// To Get an Upgrade with the associated Config
        /// </summary>
        public Upgrade GetUpgradeByConfig(UpgradeConfig config)
            => _upgrades.FirstOrDefault(u => u.Config == config);
        
        
        
        public bool HasUpgradeOfType<T>(out T upgrade) where T : Upgrade
        {
            upgrade = GetUpgradeByType<T>();
            return upgrade != null;
        }

    
        public int LevelUp<T>() where T : Upgrade
        {
            T upgrade;
            if (!HasUpgradeOfType<T>(out upgrade))
            {
                this.Log(LogType.Error, "No upgrade of type " + nameof(T) + " found");
            }
        
            LevelUp(upgrade);
            return upgrade.Level;
        }

        private void LevelUp<T>(T upgrade) where T : Upgrade
        {
            upgrade?.LevelUp();
            if (upgrade != null)
            {
                this.Log(LogType.Log, $"{upgrade} level up");
            }
        }
        

        /// <summary>
        /// If there's enough money, spend it and apply the level up
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>True if the level up happens, false otherwise</returns>
        /// 
        public bool TryLevelUp<T>() where T : Upgrade
        {
            if (!HasUpgradeOfType<T>(out T upgrade))
            {
                this.Log(LogType.Error, $"No upgrade of type {typeof(T).Name} found");
                return false;
            }
            return TryLevelUpInternal(upgrade);
        }
        
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
        
        
        public void Register()
        {
            Reference.SetManager(this);
        }
        public void Unregister()
        {
            Reference.RemoveManager(this);
        }

        #endregion
    }
}