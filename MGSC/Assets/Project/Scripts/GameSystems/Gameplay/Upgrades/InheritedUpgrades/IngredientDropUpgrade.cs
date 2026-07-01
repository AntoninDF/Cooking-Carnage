using UnityEngine;
using GameSystem;

namespace Upgrades
{

    public class IngredientDropUpgrade : Upgrade
    {
        public float DropRateIncrement => _level * 1f;
    }
}