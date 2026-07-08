using UnityEngine;
using GameSystem;

namespace Upgrades
{

    public class IngredientDropUpgrade : Upgrade<IngredientDropUpgrade>
    {
        public float DropRateIncrement => 0.15f;
    }
}