
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
        public Action<Client> OnNewClient;
        
        // Private Properties
        float GetRandomizedDelay => (delayLength + Random.Range(-randomDelayRange/2, randomDelayRange/2));
        bool  CanSpawnClient     => canSpawnClient && spline != null && SeatsOrQueueAvailable()&& SpawnPointIsClear();        
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
        private SplineContainer spline;
        private List<object> counters;
        
        // Reference Fields
        private TimerManager _timerManager;
        private ClientManager _clientManager;
        
        private Queue<SO_Client> nextClients = new();
        
        private int   maxQueueSize    = 3;
        private float _spawnSafeZoneT = 0.1f;
        
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
            Reference.SetManager(this);
        }
        public void Unregister()
        {
            Reference.RemoveManager(this);
        }


        /// <summary>
        /// Setup the fields according to referenced data and then lauches the cycle
        /// </summary>
        public void Init()
        {
            // WIP : should also setup floats according to a Settings Data

            models = Reference.AssetReferences.clientList;
            spline = Reference.GetManager<PathManager>()?.SplineComponent;
            counters = Reference.GetAll<object>();

            Reference.GetManager(out _timerManager);
            Reference.GetManager(out _clientManager);
            _timerManager.StartTimer( "AdaptiveClientSpawner Init", 1, () => Launch(),true);

            if (models.Count <= 0 || !spline || counters.Count <= 0)
            {
                this.Log(LogType.Warning," Initialization is incomplete");
            }
            else
            {
                this.Log(" has been initialized");
            }
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
                this.Log(LogType.Error," No ordering counter has been detected");
                return false;
            }

            return false;
        }

        bool ShiftOngoing()
        {
            return !_timerManager.IsTimerRunning("shiftEndDelay");
        }
        
        /// <summary>
        /// Assign a Client to the ordering counter when it spawns
        /// </summary>
        void AssignClientOrderingCounter(Client client)
        {
            /*
            foreach (var counter in counters)
            {
                if (!counter.IsFree) continue;
                //client.ReserveCounter(counter);
                return;
            }
            */
            this.Log(LogType.Warning," No counter has been found to assign a client to");
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
                this.Log(LogType.Log," Client couldn't be spawned so it will try again in "+retryDelay+" seconds");
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
                if (nextClients.Count == 0)
                {
                    EnqueueClient(NewClient());
                    
                    this.Log(LogType.Log," No client in queue so, a new one has been generated");
                }
                clientSO = nextClients.Dequeue();
            }
                    
            // Spawns the client in the scene via ClientGenerator method Client.New()
            Vector3 spawnPosition = spline.Spline.Evaluate(0, out var pos, out _, out _) ? (Vector3)pos + spline.transform.position : Vector3.zero;
            Client client = Client.New(clientSO, spawnPosition,clientSpeed);
            client.SetSplinePath(spline);
            
            this.Log(LogType.Log,clientSO.name +" has been spawned into scene");
            
            AssignClientOrderingCounter(client);
            _clientManager.AddClientData(client,false);
            //Reference.Get<Entrance>().OnClientEnter.Invoke();
                    
            Launch(GetRandomizedDelay);
            OnNewClient?.Invoke(client);
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
                this.Log(LogType.Warning,$"No client of type {chosenType} found in models!");
                return null;
            }
            
            return clientsOfType[Random.Range(0, clientsOfType.Length)];
        }
        
        /// <summary>
        /// Picks a random client type according to GD algorithm
        /// </summary>
        EClientType GetRandomWeightedType()
        {
            Dictionary<EClientType, float> clientTypeAmount = _clientManager.ClientTypeAmount;
            Dictionary<EClientType, float> weightedTypes = new Dictionary<EClientType, float>();

            var keys = clientTypeAmount.Keys.ToList();
            int keyCount = keys.Count;
            
            //WIP : Works only for 3 types of Alien
            /*
            for (int typeIndex = 0; typeIndex < keyCount; typeIndex++)
            {
                int prevIndex = (typeIndex - 1) < 0 ? keyCount - 1 : typeIndex - 1;
                int nextIndex = (typeIndex + 1) >= keyCount ? 0 : typeIndex + 1;

                EClientType type = keys[typeIndex];
                weightedTypes[type] =
                    35 + clientTypeAmount[keys[prevIndex]] * 5 + clientTypeAmount[keys[nextIndex]] * 5;
            }
            */
            weightedTypes = new Dictionary<EClientType, float>();
            foreach (var kvp in clientTypeAmount)
            {
                EClientType type = kvp.Key;
                weightedTypes[type] = 35;
                foreach (var kvp2 in clientTypeAmount)
                {
                    if (kvp.Equals(kvp2)) continue;
                    weightedTypes[type] += 5 * clientTypeAmount[kvp2.Key];
                }
            }
            float sum = 0;
            for (int i=0; i < weightedTypes.Count; i++)
            {
                sum += weightedTypes[keys[i]];
            }
            
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
            this.Log(LogType.Error, "Error with the selection of new client's type");
            
            return EClientType.None;
        }

        public void SetClientSpeed(float speed)
        {
            clientSpeed = speed;
        }

        public void OnQueueSlotFreed()
        {
            TrySpawnClient();
        }
        
        bool SeatsOrQueueAvailable()
        {
            var cm = Reference.GetManager<ClientManager>();
            bool seatFree  = cm.GetClientSeats(mustBeAvailable: true).Count > 0;
            bool queueFree = cm.WaitingQueue?.Count < maxQueueSize;
            return seatFree || queueFree;
        }
        
        bool SpawnPointIsClear()
        {
            var clients = Reference.GetManager<ClientManager>().CurrentClients;
            foreach (var client in clients)
            {
                if (client.Movement == null) continue;
                if (client.Movement.SplineComponent == null) continue; 
                if (client.Movement.SplinePosition < _spawnSafeZoneT)
                    return false;
            }
            return true;
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