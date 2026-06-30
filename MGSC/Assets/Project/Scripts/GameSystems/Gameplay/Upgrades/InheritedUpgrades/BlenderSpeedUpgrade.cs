using Cooking;
using GameSystem;
using System.Collections.Generic;

namespace Upgrades
{

    public class BlenderSpeedUpgrade : Upgrade
    {
        public float SpeedMultiplier => 1f + _level * 0.25f;
    }
}