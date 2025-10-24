
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Clients;
using Cooking;
using GameSystem;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

namespace GameSystem
{
    /// <summary>
    /// Used to make appear a specific new client at precise timing
    /// </summary>
    public class AdaptiveClientSpawner : IManager
    {
        #region Properties

        private float delayLength = 5;
        private float randomDelayRange = 10;
        private float retryDelay = 2;
        private string timerName = "clientSpawn";
        
        private List<SO_Client> models;
        private SO_RecipeBook recipes;
        private SplineContainer spline;
        
        private TimerManager _timerManager;
        private GameLoop _gameLoop;
        
        Queue<SO_Client> nextClients = new();
        
        #endregion

        #region Fields
        
        
        #endregion

        #region Methods

        public AdaptiveClientSpawner() //WIP should set the properties base values according to a Settings Data passed by GameInitializer
        {
            _timerManager = ReferenceManager.Instance.GetManager<TimerManager>();
            _gameLoop = ReferenceManager.Instance.GetManager<GameLoop>();
            
            models = ReferenceManager.Instance.GetElementOfType<SimpleClientSpawner>().models;
            recipes = ReferenceManager.Instance.GetElementOfType<SimpleClientSpawner>().recipes;
            spline = ReferenceManager.Instance.GetElementOfType<SimpleClientSpawner>().spline;
            
            EnqueueClient(NewClient());
        }
        
        bool CanSpawnClient()
        {
            return OrderingCounterAvailable() && _gameLoop.IsRunning;

        }

        bool OrderingCounterAvailable()
        {
            var counters = ReferenceManager.Instance.GetElementsOfType<OrderingCounter>();
            if (counters.Count == 0)
            {
                Debug.LogWarning("No ordering counter in the scene");
                return false;
            }

            foreach (var counter in counters)
            {
                if (counter.IsFree)
                    return true;
            }
            return false;

        }

        public void LaunchDelay(float length = 2)
        {
            _timerManager.StartTimer(timerName, length, () => SpawnClient(), false);
        }

        float GetRandomizedDelay()
        {
            return delayLength + Random.Range(-randomDelayRange/2, randomDelayRange/2);
        }

        Action SpawnClient(SO_Client clientSO = null)
        {
            if (CanSpawnClient() || clientSO != null)
            {
                if (clientSO == null)
                {
                    if (nextClients.Count == 0)
                        clientSO = NewClient(); 
                    else
                        clientSO = nextClients.Dequeue();
                }
                
                Client client = Client.New(clientSO, recipes.Recipes[Random.Range(0,recipes.Recipes.Count)], ReferenceManager.Instance.GetElementOfType<SimpleClientSpawner>().transform.position);

                client.SetSplinePath(spline);
                
                //EnqueueClient(NewClient());
                
                LaunchDelay(GetRandomizedDelay());
            }
            else
            {
                //Debug.Log("Can't spawn client");
                LaunchDelay(retryDelay);
            }

            return null;
        }

        SO_Client NewClient() //WIP should check clients history
        {
            int indexType = Random.Range(1, Enum.GetValues(typeof(EClientType)).Length-1); // not 0 cause => None
            
            var filtered = models.Where(m => m.clientType == (EClientType)indexType).ToList();

            if (filtered.Count == 0)
            {
                Debug.LogWarning("No client of type " + (EClientType)indexType + " found in models!");
                return null;
            }
            
            return filtered[Random.Range(0, filtered.Count)];
        }

        SO_Client GetNextClient()
        {
            return nextClients.Peek();
        }

        private void EnqueueClient(SO_Client client)
        {
            nextClients.Enqueue(client);
        }
        
        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }

        #endregion
        
    } 
}