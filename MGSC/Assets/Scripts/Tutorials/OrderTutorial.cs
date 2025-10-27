
using Clients;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

namespace Tutorials

{
    public class OrderTutorial : Tutorial
    {
        #region Fields
        [SerializeField] private Client _client;
        [SerializeField] private OrderingCounter _counter;


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
            _client.UseCounter(_counter);
        }

        #endregion
    }
}