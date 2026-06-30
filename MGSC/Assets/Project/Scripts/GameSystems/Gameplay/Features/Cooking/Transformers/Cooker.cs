using Upgrades;

namespace Features
{
    public class Cooker : TransformAfterTimer, IUpgradable<CookingSpeedUpgrade>
    {
        private float _baseDuration;
        
        public Cooker(IFeatureCollection owner) : base(owner)
        {
            Conditions.AddCondition(item => item is F_Cookable || item is F_OverCookable);
            Conditions.AddCondition(item => Globals.Gameplay.AllowCooking);
            _baseDuration = SpeedMultiplier;
        }

        #region Upgrade

        public void OnUpgradeInit(CookingSpeedUpgrade upgrade)
        { 
            OnLevelUp(upgrade);
        }
        public void OnUpgraded(CookingSpeedUpgrade upgrade)
        {
            OnLevelUp(upgrade);
        }

        public void OnLevelUp(CookingSpeedUpgrade upgrade)
        {
            SpeedMultiplier = upgrade.GetFinalDuration(_baseDuration);
        }

        #endregion
       
    }
}