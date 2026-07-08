using Cooking;
using GameSystem;
using System;

namespace Upgrades
{

    public class OvenCountUpgrade : Upgrade<OvenCountUpgrade>
    {
        public int OvenCount => _level + 1;
    }
}