using Cooking;
using GameSystem;
using System;

namespace Upgrades
{

    public class OvenCountUpgrade : Upgrade
    {
        public int OvenCount => _level + 1;
    }
}