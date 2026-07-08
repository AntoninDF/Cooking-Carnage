
using System.Collections.Generic;
using UnityEngine;

namespace Upgrades
{

    public class CookingSpeedUpgrade : Upgrade<CookingSpeedUpgrade>
    {
        private const float ReductionPerLevel = 0.5f;
        
        public float TimeReduction => _level * ReductionPerLevel;

        public float GetFinalDuration(float baseDuration, float minDuration = 0.1f)
        {
            return Mathf.Max(minDuration, baseDuration - TimeReduction);
        }
    }
}