using Upgrades;

namespace Features
{
    public class Cutter : TransformAfterTimer, IUpgradable<BlenderSpeedUpgrade>
    {
        public Cutter(IFeatureCollection owner) : base(owner)
        {
            Conditions.AddCondition(item => item is F_Cuttable);
            Conditions.AddCondition(item => Globals.Gameplay.AllowCutting);
        }

        #region Upgrade

        public void OnUpgradeInit(BlenderSpeedUpgrade upgrade)
        {
            OnLevelUp(upgrade);
        }

        public void OnUpgraded(BlenderSpeedUpgrade upgrade)
        {
            OnLevelUp(upgrade);
        }

        public void OnLevelUp(BlenderSpeedUpgrade upgrade)
        {
            SpeedMultiplier = upgrade.SpeedMultiplier;
        }

        #endregion
        
    }
}