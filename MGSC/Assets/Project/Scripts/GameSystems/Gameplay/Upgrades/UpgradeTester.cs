using GameSystem;
using System.Collections;
using System.Linq;
using UnityEngine;
using Upgrades;

namespace Assets.Scripts.Upgrades
{
    public class UpgradeTester : MonoBehaviour
    {

        // Use this for initialization
        public void Upgrade()
        {
            (Reference.GetManager<UpgradeManager>().Upgrades.Where((u) => u is OvenCountUpgrade).FirstOrDefault() as OvenCountUpgrade).LevelUp();
        }
    }
}