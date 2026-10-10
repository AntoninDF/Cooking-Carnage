using UnityEngine;

namespace Upgrades
{

    public class IngredientDropUpgrade : Upgrade<IngredientDropUpgrade>
    {
        public float _dropRateIncrement => 0.075f;
        
        /// <summary>The upgraded drop rate adds a percentage of the max base value to the base range for each upgrade level/// </summary>
        /// <param name="baseDropRate"></param>
        /// <returns></returns>
        public Vector2 GetUpgradedDropRate(Vector2 baseDropRate) => new Vector2(baseDropRate.x + baseDropRate.y * _dropRateIncrement * _level, baseDropRate.y + baseDropRate.y * _dropRateIncrement * _level);
        public override void LevelUp()
        {
            base.LevelUp();
            if (_level == _config.MaxLevel)
                Achievements.Unlock("ACH_U04");
        }
    }
}