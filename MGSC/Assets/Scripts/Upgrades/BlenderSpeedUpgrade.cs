using Cooking;
using GameSystem;
using System.Collections.Generic;

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

        public BlenderSpeedUpgrade() : base()
        {
            _maxLevel = 1;
        }

        public override void Apply()
        {
            Reference.ScanHierarchy();
            _targetBlenders = Reference.GetAll<Blender>();

            foreach (Blender b in _targetBlenders)
            {
                b.CutDuration = b.DefaultCutDuration / (_level + 1);
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
        
        public override bool CanLevelUp() => _level <2;

        #endregion
    }
}