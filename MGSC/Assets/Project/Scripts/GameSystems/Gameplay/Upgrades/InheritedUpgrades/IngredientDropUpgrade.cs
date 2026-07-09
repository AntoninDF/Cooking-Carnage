using UnityEngine;
using GameSystem;

namespace Upgrades
{

    public class IngredientDropUpgrade : Upgrade<IngredientDropUpgrade>
    {
        public float _dropRateIncrement => 0.15f;
        
        public float GetDropRate() => _dropRateIncrement * _level;
        
    }
}