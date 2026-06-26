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
            //Can Get his config 
            _config = UpgradeManagerConfig.Instance.BlenderSpeed;
        }

        public override void Apply()
        {
            /*
            _targetBlenders = Reference.GetAll<Blender>();

            foreach (Blender b in _targetBlenders)
            {
                //b.CutDuration = b.DefaultCutDuration / (_level + 1);
            }
            */
        }
        
        #endregion
    }
}