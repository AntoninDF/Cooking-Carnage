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
            ReferenceManager.Instance.ScanHierarchy();
            _targetOvens = ReferenceManager.Instance.GetElementsOfType<Oven>();

            foreach (Oven o in _targetOvens)
            {
                o.SetStrength(o.Strength + 0.3f);
            }
        }
        public override void LevelUp()
        {
            _level++;
            OnLevelUp?.Invoke(_level);
        }

        public override float GetPrice()
        {
            return 5f;
        }
        #endregion
    }
}