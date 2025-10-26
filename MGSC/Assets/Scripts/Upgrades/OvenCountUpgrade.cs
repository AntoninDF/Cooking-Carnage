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
            _targetOvenSetup.Setup(_level);
        }
        public override void LevelUp()
        {
            _level++;
            OnLevelUp?.Invoke(_level);
        }
        #endregion
    }
}