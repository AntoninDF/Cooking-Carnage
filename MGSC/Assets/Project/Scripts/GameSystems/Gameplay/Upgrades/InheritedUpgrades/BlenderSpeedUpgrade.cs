using UnityEngine;

namespace Upgrades
{

    public class BlenderSpeedUpgrade : Upgrade<BlenderSpeedUpgrade>
    {
        private const float ReductionPerLevel = 0.25f;
        public float TimeReduction => _level * ReductionPerLevel;
        
        public float GetFinalDuration(float baseDuration, float minDuration = 0.1f)
        {
            return Mathf.Max(minDuration, baseDuration - TimeReduction);
        }

        public override void LevelUp()
        {
            base.LevelUp();
            if (_level == _config.MaxLevel)
                Achievements.Unlock("ACH_U03");
        }
    }
}