using System;
using Upgrades;

namespace Features
{
    public class Cooker : TransformAfterTimer, IUpgradable<CookingSpeedUpgrade>
    {
        private float _baseDuration;
        private TransformableAfterTimer _currentTarget;
        public Action FailedLaunchCooking { get; set; }
        
        public Action OnCookingStart { get; set; }
        public Action<float> OnCookingProgress { get; set; }
        public Action OnCookingOver { get; set; }

        public Action OnOvercookingStart { get; set; }
        public Action<float> OnOvercookingProgress { get; set; }
        public Action OnOvercookingOver { get; set; }
        
        
        public Cooker(IFeatureCollection owner) : base(owner)
        {
            Conditions.AddCondition(item => item is F_Cookable || item is F_OverCookable);
            Conditions.AddCondition(item => Globals.Gameplay.AllowCooking);
            _baseDuration = SpeedMultiplier;
            
            OnTransformationStart += HandleStart;
            OnTransformationProgress += HandleProgress;
            OnTransformationOver += HandleOver;
            this.TrySubscribe<CookingSpeedUpgrade>();
        }

        #region CookerSpecificTransformerAction

        private void HandleStart(TransformableAfterTimer target)
        {
            _currentTarget = target;
            if(target is F_OverCookable) OnOvercookingStart?.Invoke();
            else if (target is F_Cookable) OnCookingStart?.Invoke();
        }

        private void HandleProgress(float progress01)
        {
            if(_currentTarget is F_OverCookable) OnOvercookingProgress?.Invoke(progress01);
            else if (_currentTarget is F_Cookable) OnCookingProgress?.Invoke(progress01);
        }
        
        private void HandleOver(TransformableAfterTimer target)
        {
            if(target is F_OverCookable) OnOvercookingOver?.Invoke();
            else if (target is F_Cookable) OnCookingOver?.Invoke();
            _currentTarget = null;
        }
        #endregion
        
        
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