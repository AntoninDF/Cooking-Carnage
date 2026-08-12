using System;
using Upgrades;

namespace Features
{
    public class Cooker : TransformAfterTimer, IUpgradable<CookingSpeedUpgrade>
    {
        private TransformableAfterTimer _currentTarget;
        private CookingSpeedUpgrade _cookingSpeedUpgrade;

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
            Conditions.AddCondition(item => Globals.Authorizations.AllowCooking);
            
            OnTransformationStart += HandleStart;
            OnTransformationProgress += HandleProgress;
            OnTransformationOver += HandleOver;
            (this as IUpgradable<CookingSpeedUpgrade>).TrySubscribe();
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

        public override float GetTransformationDuration(TransformableAfterTimer target)
        {
            if (target is F_Cookable && _cookingSpeedUpgrade != null)
                return _cookingSpeedUpgrade.GetFinalDuration(target.TransformationDuration);

            return target.TransformationDuration;
        }
        
        #region Upgrade

        public void OnUpgradeInit(CookingSpeedUpgrade upgrade)
        { 
            _cookingSpeedUpgrade = upgrade;
        }
        public void OnUpgraded(CookingSpeedUpgrade upgrade)
        {
            _cookingSpeedUpgrade = upgrade;
        }

        #endregion
       
    }
}