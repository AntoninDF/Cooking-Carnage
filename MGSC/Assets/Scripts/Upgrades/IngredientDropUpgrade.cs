using Cooking;
using GameSystem;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Upgrades
{

    public class IngredientDropUpgrade : Upgrade
    {
        #region Properties
        #endregion

        #region Fields
        private float dropRateIncrement = 1f;
        #endregion

        #region Methods

        public IngredientDropUpgrade() : base()
        {
        }

        public override void Apply()
        {
            ClientManager clientManager = ReferenceManager.Instance.GetManager<ClientManager>();
            if (clientManager == null) return;
            var curr = clientManager.CurrentClients;
            if (curr == null) return;
            foreach (var client in curr)
            {
                Apply(client);
            }
        }

        public void Apply(Clients.Client client)
        {
            client.SetLoot(client.Model.loot, client.Model.dropRate + UnityEngine.Vector2.one * dropRateIncrement * _level);
        }
        public override void LevelUp()
        {
            _level++;
            OnLevelUp?.Invoke(_level);
        }

        public override float GetPrice()
        {
            return 50f;
        }
        #endregion
    }
}