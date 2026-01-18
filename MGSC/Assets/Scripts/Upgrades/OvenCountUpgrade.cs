using Cooking;
using GameSystem;
using System;

namespace Upgrades
{

    public class OvenCountUpgrade : Upgrade
    {
        #region Properties
        #endregion

        #region Fields
        private OvenSetup _targetOvenSetup;
        #endregion

        #region Methods

        public OvenCountUpgrade() : base()
        {
            _targetOvenSetup = ReferenceManager.Instance.GetElementOfType<OvenSetup>();
        }

        public override void Apply()
        {
            if (_targetOvenSetup == null) return;
            _targetOvenSetup.Setup(_level +1);
        }
        public override void LevelUp()
        {
            _level++;
            OnLevelUp?.Invoke(_level);
        }

        public override float GetPrice()
        {
            return 150f;
        }
        public override bool CanLevelUp() => _level < 1;
        #endregion
    }
}