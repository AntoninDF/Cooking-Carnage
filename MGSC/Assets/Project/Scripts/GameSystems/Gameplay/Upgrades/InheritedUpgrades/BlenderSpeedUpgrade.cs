using Cooking;
using GameSystem;
using System.Collections.Generic;
using UnityEngine;

namespace Upgrades
{

    public class BlenderSpeedUpgrade : Upgrade
    {
        private const float ReductionPerLevel = 0.25f;
        public float TimeReduction => _level * ReductionPerLevel;
        
        public float GetFinalDuration(float baseDuration, float minDuration = 0.1f)
        {
            return Mathf.Max(minDuration, baseDuration - TimeReduction);
        }
    }
}