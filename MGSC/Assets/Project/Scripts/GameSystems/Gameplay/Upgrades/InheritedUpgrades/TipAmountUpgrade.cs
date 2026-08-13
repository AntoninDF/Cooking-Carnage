using UnityEngine;

namespace Upgrades
{

    public class TipAmountUpgrade : Upgrade<TipAmountUpgrade>
    {
        public float PercentageIncreasePerLevel => 0.05f;
        public float GetTipPercentageIncrease() => PercentageIncreasePerLevel * Level;
    }
} 
