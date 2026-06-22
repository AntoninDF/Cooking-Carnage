using Cooking;
using GameSystem;
using System.Collections.Generic;
using Project.Scripts.GameSystems.Gameplay.Upgrades;

namespace Upgrades
{

    public class BlenderSpeedUpgrade : Upgrade
    {
        #region Properties
        #endregion

        #region Fields
        private List<Blender> _targetBlenders;
        #endregion

        #region Methods

        public BlenderSpeedUpgrade(UpgradeConfig config ) : base(config) { }

        public override void Apply()
        {
            _targetBlenders = Reference.GetAll<Blender>();

            foreach (Blender b in _targetBlenders)
            {
                //b.CutDuration = b.DefaultCutDuration / (_level + 1);
            }
        }

        public override float GetPrice()
        {
            return 100f;
        }
        
        public override bool CanLevelUp() => _level <2;

        #endregion
    }
}