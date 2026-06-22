using System;
using GameSystem;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Project.Scripts.GameSystems.Gameplay.Upgrades;
using UnityEngine;
using UI;
using UnityEditor.Compilation;
using Assembly = System.Reflection.Assembly;

namespace Upgrades
{

    public class UpgradeManager : IManager
    {
        #region Properties
        
        public List<Upgrade> Upgrades => _upgrades;
        
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
        /// Run before the initialization, make sure that the config is injected
        /// </summary>
        public void SetConfig(UpgradeManagerConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config),"[UpgradeManager] UpgradeManagerConfig cannot be null.");
        }
        
        /// <summary>
        /// Init all the Upgrades and give them their ScriptableObject
        /// </summary>
        public void Init()
        {
            if (_config == null)
                throw new InvalidOperationException("[UpgradeManager] SetConfig() must be called before Init().");
            
            _upgrades = new List<Upgrade>
            {
                new OvenCountUpgrade(_config.ovenCount),
                new CookingSpeedUpgrade(_config.cookingSpeed),
                new IngredientDropUpgrade(_config.ingredientDrop),
                new BlenderSpeedUpgrade(_config.blenderSpeed),
            };

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
            bool hasUI = Reference.TryGetManager(out UIManager uiManager);

            if (!upgrade.CanLevelUp())
            {
                if (hasUI) uiManager.OnUpgradeFail?.Invoke();
                return false;
            }
            //check if the player have enough money
            float price = upgrade.GetPrice();
            if (_moneyManager.MoneyInfo < price)
            {
                this.Log(LogType.Log, $"Not enough money for {upgrade.GetType().Name}");
                if (hasUI) uiManager.OnUpgradeFail?.Invoke();
                return false;
            }
            
            _moneyManager.AddMoney(-price);
            upgrade.LevelUp();
            this.Log(LogType.Log, $"{upgrade.GetType().Name} leveled up to {upgrade.Level}");
            if (hasUI) uiManager.OnUpgradeSuccess?.Invoke();
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