using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Upgrades
{

    public abstract class Upgrade
    {
        #region Properties
        
        public int               Level     => _level;
        public UpgradeConfig     Config    => _config;
        
        internal event Action<Upgrade> OnLevelUpInternal;
        
        #endregion

        #region Fields

        protected UpgradeConfig _config;
        protected int           _level;          
        
        #endregion

        #region Methods

        protected Upgrade()
        { }

        internal void SetConfig(UpgradeConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config),$"[{GetType().Name}] UpgradeConfig cannot be null");
        }
        
        public virtual void LevelUp()
        {
            if (!CanLevelUp())
            {
                Debug.LogWarning($"[{GetType().Name}] Cannot level up further (level {_level}/{_config?.MaxLevel})");
                return;
            }
            _level++;
            OnLevelUpInternal?.Invoke(this);
        }
        
        public virtual float GetPrice()   => _config.PriceCurve.Evaluate(_level);
        public virtual bool  CanLevelUp() => _level < _config.MaxLevel;
        
        #endregion
    }
}