using UnityEngine;
using Cooking;
using System.Collections.Generic;
using Features;
using UnityEngine.Splines;
using Upgrades;
using GameSystem;

namespace Clients
{
    /// <summary>
    /// This part of the client class handles the Client generation.
    /// For client behaviour, see Client.cs
    /// </summary>
    public partial class Client : MonoBehaviour, IUpgradable<IngredientDropUpgrade>, IUpgradable<GainedBloodUpgrade>
    { 
        #region Properties

        //public ClientMovement Movement => _movement;
        public EClientType Type { get; private set; }
        public SO_Client Model { get; private set; }
        public SO_Ingredient IngredientToDrop { get; private set; }
        /// <summary>
        /// Very confusing because you're never sure that what you get is what you've set
        /// (But it works for now I think)
        /// </summary>
        public Vector2 IngredientDropRate
        {
            get => _ingredientUpgrade != null ? _ingredientUpgrade.GetUpgradedDropRate(_defaultIngredientDropRate) : _defaultIngredientDropRate;
            private set => _defaultIngredientDropRate = value;
        }
        public Vector2 SecondaryIngredientDropRate
        {
            get => _ingredientUpgrade != null ? _ingredientUpgrade.GetUpgradedDropRate(_defaultSecondaryIngredientDropRate) : _defaultSecondaryIngredientDropRate;
            private set => _defaultSecondaryIngredientDropRate = value;
        }
        public Vector2 BloodDropRate
        {
            get
            {
                var value = _bloodUpgrade != null ? _bloodUpgrade.GetUpgradedDropRate(_defaultBloodDropRate) : _defaultBloodDropRate;
                Debug.Log($"[Client] BloodDropRate getter called. Default: {_defaultBloodDropRate}, Returned: {value}, _bloodUpgrade: {_bloodUpgrade}");
                return value;
            }
            private set => _defaultBloodDropRate = value;
        }
        public Vector2 SecondaryBloodDropRate
        {
            get => _bloodUpgrade != null ? _bloodUpgrade.GetUpgradedDropRate(_defaultSecondaryBloodDropRate) : _defaultSecondaryBloodDropRate;
            private set => _defaultSecondaryBloodDropRate = value;
        }
        public EventVariable EventInvokedUponBlending { get; set; }

        public SplineContainer SplineComp => _splineComp;
        public float           Speed      => _speed;
        
        #endregion
        
        
        #region Fields

        //private bool _useUpgrades   = true;
        private float _speed         = 5f;
        private float _reachDistance = 1f;
        private SplineContainer _splineComp;
        public Vector2 _defaultIngredientDropRate = Vector2.one;
        public Vector2 _defaultSecondaryIngredientDropRate = Vector2.one;
        public Vector2 _defaultBloodDropRate = new Vector2(30f, 60f);
        public Vector2 _defaultSecondaryBloodDropRate = new Vector2(10f, 30f);
        private IngredientDropUpgrade _ingredientUpgrade;
        private GainedBloodUpgrade _bloodUpgrade;
        //private ClientMovement _movement;

        #endregion


        #region Client creation
        /// <summary>
        /// This is the advised way to spawn a new client
        /// </summary>
        /// <param name="model">The client data model</param>
        /// <param name="order">What the client will order at the counter</param>
        /// <param name="position">The position at which the gameobject will be instantiated</param>
        /// <returns></returns>
        public static Client New(SO_Client model, Vector3 position = default(Vector3), float speed=1.0f)
        {
            if (model == null)
            {
                Debug.LogError("[Client] New : The specified client model was null. Please pass in a valid model");
                return null;
            }

            return
                Client
                    .New(model.prefab, model.clientType, model.loot, model.ingredientDropRate, model.secondaryIngredientDropRate, model.bloodDropRate, model.secondaryBloodDropRate, model.EventInvokedUponBlending, model.name, position, model.movementSpeed)
                    .SetModel(model);
            //.TryUseUpgrades();
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
        public static Client New(GameObject prefab, EClientType type, SO_Ingredient ingredientDroppedOnKill, Vector2 ingredientDropRate, Vector2 secondaryIngredientDropRate, Vector2 bloodDropRate, Vector2 secondaryBloodDropRate, EventVariable eventWhenBlended, string name = null, Vector3 position = default(Vector3), float speed = 1.0f)
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
                .SetIngredientLoot(ingredientDroppedOnKill, ingredientDropRate, secondaryIngredientDropRate)
                .SetBloodLoot(eventWhenBlended, bloodDropRate, secondaryBloodDropRate)
                .SetSpeed(speed);

            if (name != null)
                go.name = name;
            
            return client;
        }
        #endregion

        // private Client TryUseUpgrades()
        // {
        //     if (!_useUpgrades) return this;
        //     if (!Reference.TryGetManager(out UpgradeManager uMan)) return this;
        //     // IngredientDropUpgrade upgrade = uMan.GetUpgradeByType<IngredientDropUpgrade>();
        //     // if (upgrade == null) return this;
        //     // upgrade.Apply(this);
        //     return this;
        // }


        #region Client properties
        public Client SetType(EClientType type)
        {
            Type = type;
            return this;
        }

        public Client SetOrder(SO_Recipe recipe)
        {
            //RequestedRecipe = recipe;
            return this;
        }

        public Client SetIngredientLoot(SO_Ingredient loot, Vector2 dropRate, Vector2 secondaryBloodRate)
        {
            IngredientToDrop = loot;           
            IngredientDropRate = dropRate;

            return this;
        }
        public Client SetBloodLoot(EventVariable eventInvokedUponBlending, Vector2 dropRate, Vector2 secondaryBloodRate)
        {          
            BloodDropRate = dropRate;
            EventInvokedUponBlending = eventInvokedUponBlending;
            return this;
        }

        public Client SetModel(SO_Client model)
        {
            Model = model;
            return this;
        }
        

        public Client SetReachDistance(float distance)
        {
            _reachDistance = distance;
            return this;
        }
        
        public Client SetSpeed(float speed)
        {
            _speed = speed;
            return this;
        }
        
        public Client SetSplinePath(SplineContainer splineObject)
        {
            _splineComp = splineObject;
            return this;
        }

        public Client SubscribeToUpgrade()
        {
            (this as IUpgradable<IngredientDropUpgrade>).TrySubscribe();
            (this as IUpgradable<GainedBloodUpgrade>).TrySubscribe();
            return this;
        }
        #endregion

        #region Upgrade

        public void OnUpgradeInit(IngredientDropUpgrade upgrade) => OnUpgrade(upgrade);
        public void OnUpgraded(IngredientDropUpgrade upgrade) => OnUpgrade(upgrade);
        public void OnUpgrade(IngredientDropUpgrade upgrade) => _ingredientUpgrade = upgrade;

        public void OnUpgradeInit(GainedBloodUpgrade upgrade) => OnUpgrade(upgrade);
        public void OnUpgraded(GainedBloodUpgrade upgrade) => OnUpgrade(upgrade);
        public void OnUpgrade(GainedBloodUpgrade upgrade) => _bloodUpgrade = upgrade;
        #endregion


    }

}