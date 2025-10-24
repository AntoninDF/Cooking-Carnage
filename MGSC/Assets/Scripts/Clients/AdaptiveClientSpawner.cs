
using System.Collections.Generic;
using Clients;
using Cooking;
using GameSystem;
using UnityEngine;
using UnityEngine.Splines;

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
        
        [SerializeField] private List<SO_Client> models;
        [SerializeField] private SO_RecipeBook recipes;
        [SerializeField] private SplineContainer spline;
        private int nextSpawnedClient;
        
        #endregion

        #region Fields
        
        
        #endregion

        #region Methods

        void Initialize() { } //WIP should set the properties base values according to a Settings Data passed by GameInitializer
        
        bool CanSpawnClient() //missing info on if the game is currently running or not
        {
            if (OrderingCounterAvailable() && ReferenceManager.Instance.GetManager<GameLoop>().IsRunning)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        bool OrderingCounterAvailable()
        {
            if (ReferenceManager.Instance.GetElementsOfType<OrderingCounter>().Count > 0)
            {
                foreach (OrderingCounter orderingCounter in ReferenceManager.Instance.GetElementsOfType<OrderingCounter>())
                {
                    if (orderingCounter.IsFree)
                    {
                        //Debug.LogWarning("Ordering counter is free");
                        return true;
                    }
                }
            }
            else
            {
                Debug.LogWarning("No ordering counter in the scene");
            }
            
            return false;
        }

        public void LaunchDelay(float length = 2)
        {
            ReferenceManager.Instance.GetManager<TimerManager>().StartTimer(timerName, length, SpawnClient, false);
        }

        float GetRandomizedDelay()
        {
            return delayLength + Random.Range(-randomDelayRange/2, randomDelayRange/2);
        }

        void SpawnClient()
        {
            if (CanSpawnClient())
            {
                /*Client client = Client.New(models[nextSpawnedClient], recipes.Recipes[nextSpawnedClient% recipes.Recipes.Count], ReferenceManager.Instance.GetElementOfType<SimpleClientSpawner>().transform.position);
                nextSpawnedClient = (nextSpawnedClient + 1)% models.Count;

                client.SetSplinePath(spline);*/
                //Debug.Log("Spawning client");
                
                LaunchDelay(GetRandomizedDelay());
            }
            else
            {
                //Debug.Log("Can't spawn client");
                LaunchDelay(retryDelay);
            }
        }
    
        #endregion

        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }
    }
}