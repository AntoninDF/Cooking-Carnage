
using System.Collections.Generic;
using Project.Scripts.GameSystems.Gameplay.Upgrades;

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

        public CookingSpeedUpgrade(UpgradeConfig config) : base(config)
        {
        }

        public override void Apply()
        {
            _targetOvens = Reference.GetAll<Oven>();

            foreach (Oven o in _targetOvens)
            {
                //o.SetStrength(_level * 2f);
            }
        }
        

        public override float GetPrice()
        {
            return 100f;
        }

        public override bool CanLevelUp() => _level < 3;
        #endregion
    }
}