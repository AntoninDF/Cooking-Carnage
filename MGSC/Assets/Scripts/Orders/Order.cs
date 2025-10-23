using UnityEngine;
using Clients;
using Cooking;

namespace Orders
{
    public class Order : MonoBehaviour
    {
        #region Properties

        public Client Owner => _owner;
        public SO_Recipe Recipe => _recipe;
        public float Timestamp => _timestamp;

        #endregion

        #region Fields


        private Client _owner;
        private SO_Recipe _recipe;
        private float _timestamp; // Time at which the order was taken (accepted by the player)

        #endregion

        #region Methods

        public Order(Client owner, SO_Recipe recipe)
        {
            _owner = owner;
            _recipe = recipe;
            _timestamp = Time.time;
        }

        #endregion

    }
}