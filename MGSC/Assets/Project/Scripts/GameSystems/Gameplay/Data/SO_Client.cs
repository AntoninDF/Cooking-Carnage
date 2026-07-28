using Cooking;
using System.Collections.Generic;
using UnityEngine;

namespace Clients
{
    /// <summary>
    /// Defines the base identity for a client's alien species
    /// </summary>
    [CreateAssetMenu(fileName = "New Client", menuName = "Scriptable Objects/Client")]
    public class SO_Client : ScriptableObject
    {
        [Tooltip("The name will be used if needed in UI, or other places. It is the species' name rather than an individual name.")]
        public new string name;
        [Tooltip("The movement speed of the customer")]
        public float movementSpeed = 0.65f;
        [Tooltip("This is the specific prefab for this species. It should have the client component attached to the root object.")]
        public GameObject prefab;
        [Tooltip("The client type identifies the client's species")]
        public EClientType clientType;
        [Tooltip("The loot is the list of ingredients dropped upon death.")]
        public SO_Ingredient loot;
        [Tooltip("How much ingredients will be spawned ? (generates a random value between min and max, rounded to the nearest whole number.")]
        public Vector2 dropRate;
   
    }
}