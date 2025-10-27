
using Clients;
using Cooking;
using GameSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace GameSystem
{
    /// <summary>
    /// Spawning Manager used to request appearance of a specific new client at precise timing
    /// </summary>
    public class AdaptiveClientSpawner : IManager
    {
        #region Properties
        
        // Public Properties
        public int NextClientsCount => nextClients.Count;
        public SO_Client GetNextClient() => nextClients.Count > 0 ? nextClients.Peek() : null;
        
        // Private Properties
        float GetRandomizedDelay => (delayLength + Random.Range(-randomDelayRange/2, randomDelayRange/2));
        bool CanSpawnClient => OrderingCounterAvailable() && _gameLoop.IsRunning;
        
        #endregion

        #region Fields
        
        // Setup Fields
        private float delayLength = 5;
        private float randomDelayRange = 10;
        private float retryDelay = 2;
        private string timerName = "clientSpawn";
        private List<SO_Client> models;
        private SO_RecipeBook recipes;
        private SplineContainer spline;
        
        // Reference Fields
        private TimerManager _timerManager;
        private GameLoop _gameLoop;
        
        private Queue<SO_Client> nextClients = new();
        
        #endregion

        #region Methods
        
        /// <summary>
        /// Constructor of the script, waiting for the GameInitializer to be ready
        /// </summary>
        public AdaptiveClientSpawner()
        {
            Register();
            _timerManager = ReferenceManager.Instance.GetManager<TimerManager>();
            _timerManager.StartTimer("AdaptiveClientSpawner Init", 0.01f, () => Init(), false);
        }
        
        
        /// <summary>
        /// IManager Interface method to register itself to the ReferenceManager
        /// </summary>
        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }
        
        
        /// <summary>
        /// Setup the fields according to referenced data and then lauches the cycle
        /// </summary>
        void Init() 
        {
            // WIP : should also setup floats according to a Settings Data
            
            _gameLoop = ReferenceManager.Instance.GetManager<GameLoop>();
            models = ReferenceManager.AssetReferences.clientList;
            recipes = ReferenceManager.AssetReferences.recipeBook;
            spline = ReferenceManager.Instance.GetManager<PathManager>().SplineComponent;

            Launch();
        }
        
        
        /// <summary>
        /// Check if a counter is free to order
        /// </summary>
        bool OrderingCounterAvailable() 
        {
            // WIP : counter occupation system might change
            
            var counters = ReferenceManager.Instance.GetElementsOfType<OrderingCounter>();
            
            if (counters.Count == 0)
            {
                Debug.LogWarning("No ordering counter in the scene");
                return false;
            }

            return counters.Any(counter => counter.IsFree);
        }

        
        /// <summary>
        /// Launch spawn timer with a delay (or not)
        /// </summary>
        void Launch(float delay = 2)
        {
            _timerManager.StartTimer(timerName, delay, () => SpawnClient()(), false);
        }

        
        /// <summary>
        /// Spawn a new Client if it is possible
        /// </summary>
        Action SpawnClient(SO_Client clientSO = null)
        {
            // WIP : Choice of the recipe asked is fully random for now
            return () =>
            {
                if (CanSpawnClient || clientSO != null)
                {
                    // If the queue is empty createsa new client else it dequeues the first one
                    clientSO ??= nextClients.Count > 0 ? nextClients.Dequeue() : NewClient();
                    
                    // Picks a random recipe through the list of recipes
                    var randomRecipe = recipes.Recipes[Random.Range(0, recipes.Recipes.Count)];
                    
                    // Spawns the client in the scene via ClientGenerator method Client.New()
                    Client client = Client.New(clientSO, randomRecipe, Vector3.zero);
                    client.SetSplinePath(spline);

                    Launch(GetRandomizedDelay);
                }
                else
                {
                    Launch(retryDelay);
                }
            };
        }
        
        /// <summary>
        /// Select a random Client of the right type through a referenced list
        /// </summary>
        SO_Client NewClient() 
        {
            // WIP : should check clients history
            // WIP : could generate the entirety of the client instead of selecting
            
            //Picks a random client type
            int indexType = Random.Range(1, Enum.GetValues(typeof(EClientType)).Length); // not 0 cause => None
            EClientType chosenType = (EClientType)indexType;
            
            // Creates a filtered array according to chosenType
            var clientsOfType = models.Where(m => m.clientType == chosenType).ToArray();

            if (clientsOfType.Length == 0)
            {
                Debug.LogWarning($"No client of type {chosenType} found in models!");
                return null;
            }
            
            return clientsOfType[Random.Range(0, clientsOfType.Length)];
        }

        
        /// <summary>
        /// Add a client to the queue
        /// </summary>
        private void EnqueueClient(SO_Client client)
        {
            nextClients.Enqueue(client);
        }

        #endregion
        
    } 
}