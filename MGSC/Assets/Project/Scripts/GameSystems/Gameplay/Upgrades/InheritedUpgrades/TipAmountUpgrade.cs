using UnityEngine;

namespace Upgrades
{

    public class TipAmountUpgrade : Upgrade<TipAmountUpgrade>
    {
        public float PercentageIncreasePerLevel => 0.05f;
        public float GetTipPercentageIncrease() => PercentageIncreasePerLevel * Level;

        public override void LevelUp()
        {
            base.LevelUp();
            if (_level == _config.MaxLevel)
                Achievements.Unlock("ACH_U10");
        }
    }
} 
