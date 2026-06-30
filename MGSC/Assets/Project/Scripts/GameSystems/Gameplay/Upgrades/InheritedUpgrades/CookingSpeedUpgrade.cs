
using System.Collections.Generic;

namespace Upgrades
{

    public class CookingSpeedUpgrade : Upgrade
    {
        public float SpeedMultiplier => 1f + _level * 0.25f;
    }
}