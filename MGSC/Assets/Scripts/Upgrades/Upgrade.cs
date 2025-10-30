using System;
using System.Diagnostics;
using UnityEngine;

namespace Upgrades
{

    public abstract class Upgrade
    {
        #region Properties
        public int Level => _level;
        
        public Action<int> OnLevelUp { get; set; }
        #endregion

        #region Fields


        protected int _level; // Starts at 0
        protected int _maxLevel = 2; // => Means 3 levels in total
        #endregion

        #region Methods

        public Upgrade()
        {
            OnLevelUp += (level) => Apply();
        }

        public abstract void Apply();
        public abstract void LevelUp();
        public abstract float GetPrice();
        #endregion
    }
}