
using Clients;
using GameSystem;
using Orders;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

namespace Tutorials

{
    public class ServingTutorial : Tutorial
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

            _counter.OnUserUnregister.AddListener(Complete);
            OrderManager manager= ReferenceManager.Instance.GetManager<OrderManager>();
            Order order = manager.NewOrder(_client, ReferenceManager.AssetReferences.defaultRecipeBook.Recipes[0]);
            _client.AcceptOrder(order);
            _client.ReserveCounter(_counter);
            Debug.Log(_client.Order);
        }

        #endregion
    }
}