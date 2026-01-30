using UnityEngine;
using GameSystem;

namespace Upgrades
{

    public class IngredientDropUpgrade : Upgrade
    {
        #region Properties
        #endregion

        #region Fields
        private float dropRateIncrement = 1f;

        private ClientManager _clientManager;
        #endregion

        #region Methods

        public IngredientDropUpgrade() : base()
        {
        }

        public override void Apply()
        {
            Reference.GetManager(out _clientManager);
            if (_clientManager == null) return;
            var curr = _clientManager.CurrentClients;
            if (curr == null) return;
            foreach (var client in curr)
            {
                Apply(client);
            }
        }

        public void Apply(Clients.Client client)
        {
            if (client == null) return;
            Vector2 newRate = client.Model.dropRate + Vector2.one * dropRateIncrement * _level;
            client.SetLoot(client.Model.loot, newRate);
        }
        public override void LevelUp()
        {
            _level++;
            OnLevelUp?.Invoke(_level);
        }

        public override float GetPrice()
        {
            return 100f;
        }
        public override bool CanLevelUp() => _level < 3;
        #endregion
    }
}