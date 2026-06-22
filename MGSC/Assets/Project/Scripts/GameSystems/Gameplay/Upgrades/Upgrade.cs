using System;
using System.Diagnostics;
using Project.Scripts.GameSystems.Gameplay.Upgrades;
using UnityEngine;

namespace Upgrades
{

    public abstract class Upgrade
    {
        #region Properties
        
        public int           Level     => _level;
        public Action<int>   OnLevelUp { get; set; }
        public UpgradeConfig Config    => _config;
        
        #endregion

        #region Fields

        protected UpgradeConfig _config;
        protected int           _level;          // Starts at 0
        
        #endregion

        #region Methods

        //Initialize the config in the constructor
        protected Upgrade(UpgradeConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config), $"[{GetType().Name}] UpgradeConfig cannot be null");
            OnLevelUp += (level) => Apply();
        }

        public abstract void Apply();

        //Defined here because all the inherited class use the same implementation
        public virtual void LevelUp()
        {
            _level++;
            OnLevelUp?.Invoke(_level);
        }
        public virtual float GetPrice() => _config.priceCurve.Evaluate(_level) * _config.basePrice;
        public virtual bool CanLevelUp() => _level < _config.maxLevel;
        #endregion
    }
}