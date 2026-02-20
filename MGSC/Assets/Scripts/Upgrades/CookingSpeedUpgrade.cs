using Cooking;
using GameSystem;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Upgrades
{

    public class CookingSpeedUpgrade : Upgrade
    {
        #region Properties
        #endregion

        #region Fields
        private List<Oven> _targetOvens;
        #endregion

        #region Methods

        public CookingSpeedUpgrade() : base()
        {
        }

        public override void Apply()
        {
            Reference.ScanHierarchy();
            _targetOvens = Reference.GetAll<Oven>();

            foreach (Oven o in _targetOvens)
            {
                //o.SetStrength(_level * 2f);
            }
        }
        public override void LevelUp()
        {
            _level++;
            OnLevelUp?.Invoke(_level);
        }

        public override float GetPrice()
        {
            return 100f;
        }

        public override bool CanLevelUp() => _level < 3;
        #endregion
    }
}