using Cooking;
using GameSystem;
using System;
using Project.Scripts.GameSystems.Gameplay.Upgrades;

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

        public OvenCountUpgrade(UpgradeConfig config) : base(config)
        {
            _targetOvenSetup = Reference.Get<OvenSetup>();
        }

        public override void Apply()
        {
            if (_targetOvenSetup == null) return;
            _targetOvenSetup.Setup(_level +1);
        }

        public override float GetPrice()
        {
            return 150f;
        }
        public override bool CanLevelUp() => _level < 1;
        #endregion
    }
}