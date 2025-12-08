using System;
using GameSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Upgrades
{

    public class UpgradeManager : IManager
    {
        #region Properties
        public List<Upgrade> Upgrades => _upgrades;
        #endregion

        #region Fields
        
        private List<Upgrade> _upgrades;

        private MoneyManager _moneyManager;
        #endregion

        #region Methods

        public UpgradeManager()
        {
            Register();
        }

        public void Init()
        {
            _upgrades = new List<Upgrade>();
            _upgrades.Add(new OvenCountUpgrade());
            _upgrades.Add(new CookingSpeedUpgrade());

            _moneyManager = ReferenceManager.Instance.GetManager<MoneyManager>();
            if (_moneyManager == null)
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

        public bool HasUpgradeOfType<T>(out T upgrade) where T : Upgrade
        {
            upgrade = GetUpgradeByType<T>();
            return upgrade != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>New level</returns>
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
        public bool TryLevelUp<T>() where T: Upgrade
        {
            T upgrade;
            if (!HasUpgradeOfType<T>(out upgrade))
            {
                this.Log(LogType.Error, "No upgrade of type " + nameof(T) + " found");
            }
            float price = upgrade.GetPrice();
            if (_moneyManager.MoneyInfo < price)
            {
                this.Log(LogType.Log, $"No enough money for {upgrade}");
                return false;
            }
                
            _moneyManager.AddMoney(-price);
            LevelUp(upgrade);
            return true;
        }

        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }
        public void Unregister()
        {
            ReferenceManager.Instance.RemoveManager(this);
        }


        #endregion
    }
}