using UnityEngine;

namespace Upgrades
{
	public class GainedBloodUpgrade : Upgrade<GainedBloodUpgrade>
	{
		private const float BloodPerLevel = 0.1f;
        /// <summary>The upgraded drop rate adds a percentage of the max base value to the base range for each upgrade level/// </summary>
        /// <param name="baseDropRate"></param>
        /// <returns></returns>
        public Vector2 GetUpgradedDropRate(Vector2 baseDropRate) => new Vector2(baseDropRate.x + baseDropRate.y * BloodPerLevel * _level, baseDropRate.y + baseDropRate.y * BloodPerLevel * _level);
        public override void LevelUp()
        {
            base.LevelUp();
            if (_level == _config.MaxLevel)
                Achievements.Unlock("ACH_U12");
        }
    }
}