using UnityEngine;
using Cooking;
using System.Collections.Generic;
using System.Linq;

namespace Client
{

    public class Client : MonoBehaviour
    {
        #region Properties
        public EClientType Type { get; private set; }
        public SO_Recipe Order {  get; private set; }
        public List<SO_Ingredient> IngredientsToDrop { get; private set; }
        public SO_Client Model { get; private set; }

        #endregion

        #region Fields



        private BaseState _currentState;
        #endregion

        #region Methods

        /// <summary>
        /// This is the advised way to spawn a new client
        /// </summary>
        /// <param name="model">The client data model</param>
        /// <param name="order">What the client will order at the counter</param>
        /// <param name="position">The position at which the gameobject will be instantiated</param>
        /// <returns></returns>
        public static Client New(SO_Client model, SO_Recipe order, Vector3 position = default(Vector3))
        {
            if (model == null) {
                Debug.LogError("[Client] New : The specified client model was null. Please pass in a valid model");
                return null;
            }
            return 
                Client.New(model.prefab, model.clientType, model.loot, order, model.name, position)
                .SetModel(model);
        }
        /// <summary>
        /// If you spawn a client this way, you should probably use SetModel on it afterwards.
        /// </summary>
        /// <param name="prefab">The GameObject that will be spawned. It should be of the appropriate species</param>
        /// <param name="type">The client type</param>
        /// <param name="ingredientsDroppedOnKill">List of all the ingredients dropped on kill. It might be modified based on gameplay situations</param>
        /// <param name="order">What the client will order at the counter</param>
        /// <param name="name">The name that will be used for the gameobject</param>
        /// <param name="position">The position at which the gameobject will be instantiated</param>
        /// <returns></returns>
        public static Client New(GameObject prefab, EClientType type, List<SO_Ingredient> ingredientsDroppedOnKill, SO_Recipe order, string name=null, Vector3 position=default(Vector3))
        {
            if (prefab == null)
            {
                Debug.LogError("[Client] New : The argument 'prefab' given to Client.New() was null");
                return null;
            }

            GameObject go = GameObject.Instantiate(prefab, position, Quaternion.identity);
            Client client = go.GetComponent<Client>();

            if (client == null)
            {
                Debug.LogError("[Client] New : The given prefab '" + prefab.name + "' has no Client component. Please add one to the root object");
                return null;
            }

            // Setup the client
            client
                .SetType(type)
                .SetOrder(order)
                .SetLoot(ingredientsDroppedOnKill);

            if (name != null)
                go.name = name;

            return client;
        }

        void Start()
        {
        
        }

        void Update()
        {
        
        }

        public Client SetType(EClientType type)
        {
            Type = type;
            return this;
        }

        public Client SetOrder(SO_Recipe order)
        {
            Order = order;
            return this;
        }

        public Client SetLoot(List<SO_Ingredient> loot)
        {
            IngredientsToDrop = loot;
            return this;
        }

        public Client SetModel(SO_Client model)
        {
            Model = model;
            return this;
        }
        #endregion
    }
    
}