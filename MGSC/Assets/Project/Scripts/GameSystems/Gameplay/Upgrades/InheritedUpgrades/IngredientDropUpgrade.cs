using UnityEngine;
using GameSystem;

namespace Upgrades
{

    public class IngredientDropUpgrade : Upgrade<IngredientDropUpgrade>
    {
        public float _dropRateIncrement => 0.2f;
        
        public float GetDropRate() => _dropRateIncrement * _level;
        public override void LevelUp()
        {
            base.LevelUp();
            if (_level == _config.MaxLevel)
                Achievements.Unlock("ACH_U04");
        }
    }
}