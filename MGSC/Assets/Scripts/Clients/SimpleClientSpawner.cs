using System.Collections.Generic;
using UnityEngine;
using Cooking;
using UnityEngine.Splines;

namespace Client
{
    /// <summary>
    /// Just for tests. Pharrell, it belongs to you to make it clean <3
    /// </summary>
    public class SimpleClientSpawner : MonoBehaviour
    {
        [SerializeField] private List<SO_Client> models;
        [SerializeField] private SO_RecipeBook recipes;
        [SerializeField] private SplineContainer spline;

        [SerializeField] private float timeBetweenSpawn = 10f;
        private float timeOfLastSpawn = float.MinValue;
        private int nextSpawnedClient;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //timeOfLastSpawn = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time - timeOfLastSpawn > timeBetweenSpawn)
            {
                Spawn();
                timeOfLastSpawn = Time.time;
            }
        }

        private void Spawn()
        {
            Client client = Client.New(models[nextSpawnedClient], recipes.Recipes[nextSpawnedClient% recipes.Recipes.Count], transform.position);
            nextSpawnedClient = (nextSpawnedClient + 1)% models.Count;

            client.SetSplinePath(spline);
        }
    }

}