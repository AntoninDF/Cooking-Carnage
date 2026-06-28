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
            config = UpgradeManagerConfig.Instance.OvenCount;
            // _targetOvenSetup = Reference.Get<OvenSetup>();
        }

        public override void Apply()
        {
            if (_targetOvenSetup == null) return;
            _targetOvenSetup.Setup(level +1);
        }
        
        #endregion
    }
}