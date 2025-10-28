
using Clients;
using GameSystem;
using Orders;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

namespace Tutorials

{
    public class KillingTutorial : Tutorial
    {
        #region Fields
        [SerializeField] private Client _client;
        [SerializeField] private SittingCounter _counter;


        #endregion

        #region Methods

        private void Start()
        {
            Setup();
        }

        public override void Setup()
        {
            base.Setup();
            if (_client == null)
                Debug.LogError("[OrderTutorial] : Please reference the default client in the inspector");
            if (_counter == null)
                Debug.LogError("[OrderTutorial] : Please reference the default counter in the inspector");

            _client
                .SetModel(ReferenceManager.AssetReferences.clientList[0])
                .SetLoot(_client.Model.loot);
            _counter.OnUserUnregister.AddListener(Complete);
            OrderManager manager = ReferenceManager.Instance.GetManager<OrderManager>();
            Order order = manager.NewOrder(_client, ReferenceManager.AssetReferences.recipeBook.Recipes[0]);
            _client.AcceptOrder(order);
            _client.UseCounter(_counter);
        }

        #endregion
    }
}