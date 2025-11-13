
using Clients;
using Cooking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
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
        bool CanSpawnClient => OrderingCounterAvailable() && canSpawnClient && spline!=null;
        
        #endregion

        #region Fields
        
        public bool canSpawnClient = true;
        
        // Setup Fields
        private float delayLength = 5;
        private float randomDelayRange = 10;
        private float retryDelay = 2;
        private string timerName = "clientSpawn";
        private float clientSpeed = 1;
        private List<SO_Client> models;
        private SO_RecipeBook recipes;
        private SplineContainer spline;
        private List<OrderingCounter> counters;
        
        // Reference Fields
        private TimerManager _timerManager;
        
        private Queue<SO_Client> nextClients = new();
        
        #endregion

        #region Methods
        
        /// <summary>
        /// Constructor of the script, waiting for the GameInitializer to be ready
        /// </summary>
        public AdaptiveClientSpawner()
        {
            Register();
        }
        
        
        /// <summary>
        /// IManager Interface method to register itself to the ReferenceManager
        /// </summary>
        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }
        public void Unregister()
        {
            ReferenceManager.Instance.RemoveManager(this);
        }


        /// <summary>
        /// Setup the fields according to referenced data and then lauches the cycle
        /// </summary>
        public void Init()
        {
            // WIP : should also setup floats according to a Settings Data

            models = ReferenceManager.AssetReferences.clientList;
            recipes = ReferenceManager.AssetReferences.recipeBook;
            spline = ReferenceManager.Instance.GetManager<PathManager>()?.SplineComponent;
            counters = ReferenceManager.Instance.GetElementsOfType<OrderingCounter>();

            _timerManager = ReferenceManager.Instance.GetManager<TimerManager>();
            _timerManager.StartTimer( "AdaptiveClientSpawner Init", 1, () => Launch(),true);
        }
        
        
        /// <summary>
        /// Check if a counter is free to order
        /// </summary>
        bool OrderingCounterAvailable() 
        {
            if (!ShiftOngoing())
            {
                return false;
            }
            
            if (counters.Count == 0)
            {
                Debug.LogError("No ordering counter in the scene");
                return false;
            }

            return counters.Any(counter => counter.IsFree);
        }

        bool ShiftOngoing()
        {
            return !ReferenceManager.Instance.GetManager<TimerManager>().IsTimerRunning("shiftEndDelay");
        }
        
        /// <summary>
        /// Assign a Client to the ordering counter when it spawns
        /// </summary>
        void AssignClientOrderingCounter(Client client)
        {
            foreach (var counter in counters)
            {
                if (!counter.IsFree) continue;
                client.ReserveCounter(counter);
                return;
            }
            Debug.LogWarning("Ordering Counter Occupation error");
        }

        
        /// <summary>
        /// Launch spawn timer with a delay (or not)
        /// </summary>
        void Launch(float delay = 2)
        {
            _timerManager.StartTimer(timerName, delay, () => TrySpawnClient());
        }

        bool TrySpawnClient(SO_Client clientSO = null)
        {
            if (CanSpawnClient || clientSO != null)
            {
                SpawnClient(clientSO);
                return true;
            }
            else
            {
                Launch(retryDelay);
                return false;
            }
        }

        /// <summary>
        /// Spawn a new Client if it is possible
        /// </summary>
        void SpawnClient(SO_Client clientSO = null)
        {     
            if (clientSO == null)
            {
                // If the queue is empty enqueues a new client then it dequeues the first one
                if (nextClients.Count == 0) { EnqueueClient(NewClient()); }
                clientSO = nextClients.Dequeue();
            }
                    
            // Spawns the client in the scene via ClientGenerator method Client.New()
            Client client = Client.New(clientSO, Vector3.zero,clientSpeed);
            client.SetSplinePath(spline);
            AssignClientOrderingCounter(client);
                    
            ReferenceManager.Instance.GetManager<ClientManager>().AddClientData(client,false);
                    
            Launch(GetRandomizedDelay);
        }

        /// <summary>
        /// Select a random Client of the right type through a referenced list
        /// </summary>
        SO_Client NewClient()
        {
            // WIP : could generate the entirety of the client instead of selecting
            
            EClientType chosenType = GetRandomWeightedType();
            
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
        /// Picks a random client type according to GD algorithm
        /// </summary>
        EClientType GetRandomWeightedType()
        {
            Dictionary<EClientType, float> clientTypeAmount =
                ReferenceManager.Instance.GetManager<ClientManager>().ClientTypeAmount;
            Dictionary<EClientType, float> weightedTypes = new Dictionary<EClientType, float>();

            var keys = clientTypeAmount.Keys.ToList();
            int keyCount = keys.Count;
            
            //WIP : Works only for 3 types of Alien
            for (int typeIndex = 0; typeIndex < keyCount; typeIndex++)
            {
                int prevIndex = (typeIndex - 1) < 0 ? keyCount - 1 : typeIndex - 1;
                int nextIndex = (typeIndex + 1) >= keyCount ? 0 : typeIndex + 1;

                EClientType type = keys[typeIndex];
                weightedTypes[type] =
                    35 + clientTypeAmount[keys[prevIndex]] * 5 + clientTypeAmount[keys[nextIndex]] * 5;
            }
            float sum = weightedTypes[keys[0]] + weightedTypes[keys[1]]+weightedTypes[keys[2]];
            
            for (int typeIndex = 0; typeIndex < keyCount; typeIndex++)
            {
                EClientType type = keys[typeIndex];
                weightedTypes[type] /= sum;
            }

            float randomValue = Random.value;
            
            float cumulative = 0f;
            
            foreach (var pair in weightedTypes)
            {
                cumulative += pair.Value;
                if (randomValue <= cumulative)
                {
                    return pair.Key;
                }
            }
            Debug.LogError("Error with the selection of new client's type");
            return EClientType.None;
        }

        public void SetClientSpeed(float speed)
        {
            clientSpeed = speed;
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