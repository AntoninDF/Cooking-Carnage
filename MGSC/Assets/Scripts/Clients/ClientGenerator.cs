using UnityEngine;
using Cooking;
using System.Collections.Generic;
using UnityEngine.Splines;
using Upgrades;
using GameSystem;

namespace Clients
{
    /// <summary>
    /// This part of the client class handles the Client generation.
    /// For client behaviour, see Client.cs
    /// </summary>
    public partial class Client : MonoBehaviour
    {

        #region Client creation
        /// <summary>
        /// This is the advised way to spawn a new client
        /// </summary>
        /// <param name="model">The client data model</param>
        /// <param name="order">What the client will order at the counter</param>
        /// <param name="position">The position at which the gameobject will be instantiated</param>
        /// <returns></returns>
        public static Client New(SO_Client model, SO_Recipe order, Vector3 position = default(Vector3), float speed=1.0f)
        {
            if (model == null)
            {
                Debug.LogError("[Client] New : The specified client model was null. Please pass in a valid model");
                return null;
            }
            return
                Client
                .New(model.prefab, model.clientType, model.loot, model.dropRate, order, model.name, position, speed)
                .SetModel(model)
                .TryUseUpgrades();
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
        public static Client New(GameObject prefab, EClientType type, SO_Ingredient ingredientDroppedOnKill, Vector2 dropRate, SO_Recipe order, string name = null, Vector3 position = default(Vector3), float speed = 1.0f)
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
                .SetLoot(ingredientDroppedOnKill, dropRate)
                .SetSpeed(speed)
                .TryUseUpgrades();

            if (name != null)
                go.name = name;

            return client;
        }
        #endregion

        private Client TryUseUpgrades()
        {
            if (!_useUpgrades) return this;
            UpgradeManager uMan = ReferenceManager.Instance.GetManager<UpgradeManager>();
            if (uMan == null) return this;
            IngredientDropUpgrade upgrade = uMan.GetUpgradeByType<IngredientDropUpgrade>();
            if (upgrade == null) return this;
            upgrade.Apply(this);
            return this;
        }


        #region Client properties
        public Client SetType(EClientType type)
        {
            Type = type;
            return this;
        }

        public Client SetOrder(SO_Recipe recipe)
        {
            RequestedRecipe = recipe;
            return this;
        }

        public Client SetLoot(SO_Ingredient loot, Vector2 dropRate)
        {
            IngredientToDrop = loot;
            DropRate = dropRate;
            return this;
        }

        public Client SetModel(SO_Client model)
        {
            Model = model;
            return this;
        }

        public Client SetSplinePath(SplineContainer splineObject)
        {
            _splineComp = splineObject;
            if (_movement != null) _movement.SplineComponent = splineObject;
            return this;
        }

        public Client SetSpeed(float speed)
        {
            _speed = speed;
            return this;
        }

        public Client SetReachDistance(float distance)
        {
            _reachDistance = distance;
            return this;
        }
        #endregion


    }

}