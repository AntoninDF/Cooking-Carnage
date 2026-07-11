using Upgrades;

namespace Features
{
    public class Cutter : TransformAfterTimer, IUpgradable<BlenderSpeedUpgrade>
    {
        private float _baseDuration;
        
        public Cutter(IFeatureCollection owner) : base(owner)
        {
            Conditions.AddCondition(item => item is F_Cuttable);
            Conditions.AddCondition(item => Globals.Gameplay.AllowCutting);
            _baseDuration = SpeedMultiplier;
            this.TrySubscribe<BlenderSpeedUpgrade>();
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
            SpeedMultiplier =  upgrade.GetFinalDuration(_baseDuration);
        }

        #endregion
        
    }
}