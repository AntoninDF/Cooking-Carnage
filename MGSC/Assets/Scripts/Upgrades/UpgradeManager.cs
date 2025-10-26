using GameSystem;
using System.Collections.Generic;
using UnityEngine;

namespace Upgrades
{

    public class UpgradeManager : IManager
    {
        #region Properties
        public List<Upgrade> Upgrades => _upgrades;
        #endregion

        #region Fields


        private List<Upgrade> _upgrades;
        #endregion

        #region Methods

        public UpgradeManager()
        {
            Register();
            Init();
        }


        public void Init()
        {
            MonoBehaviour.print("Upgrade manager made");
            _upgrades = new List<Upgrade>();
            _upgrades.Add(new OvenCountUpgrade());
        }

        public void ApplyUpgrades()
        {
            foreach (var upgrade in _upgrades)
            {
                upgrade.Apply();
            }
        }
        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }
        #endregion
    }
}