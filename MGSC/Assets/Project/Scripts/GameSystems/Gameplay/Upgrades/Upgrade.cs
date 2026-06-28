using System;
using System.Diagnostics;
using UnityEngine;

namespace Upgrades
{

    public abstract class Upgrade
    {
        #region Properties
        
        public int           Level     => level;
        public Action<int>   OnLevelUp { get; set; }
        public UpgradeConfig Config    => config;
        
        #endregion

        #region Fields

        protected UpgradeConfig config;
        protected int           level;          // Starts at 0
        
        #endregion

        #region Methods

        //Initialize the config in the constructor
        protected Upgrade()
        {
            
            //config = config ?? throw new ArgumentNullException(nameof(config), $"[{GetType().Name}] UpgradeConfig cannot be null");
            OnLevelUp += (level) => Apply();
        }

        public abstract void Apply();

        //Defined here because all the inherited class use the same implementation
        public virtual void LevelUp()
        {
            level++;
            OnLevelUp?.Invoke(level);
        }
        public virtual float GetPrice() => config.PriceCurve.Evaluate(level);
        public virtual bool CanLevelUp() => level < config.MaxLevel;
        #endregion
    }
}