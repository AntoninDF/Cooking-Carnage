
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
            config = UpgradeManagerConfig.Instance.CookingSpeed;
        }

        public override void Apply()
        {
            /*
            _targetOvens = Reference.GetAll<Oven>();

            foreach (Oven o in _targetOvens)
            {
                //o.SetStrength(level * 2f);
            }
            */
        }

        #endregion
    }
}