using Controller;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cooking
{

    /// <summary>
    /// The cooking interface holds references to all the interactable cooking elements, and relays their events
    /// </summary>
    public class CookingInterface : MonoBehaviour
    {
        #region Properties
        public PlayerInterface Player { get => _player; set => _player = value; }
        public List<Component> Items { get => _items; private set => _items = value; }
        #endregion

        #region Fields
        [Header("References")]
        [Tooltip("Specify if the scene should be scanned on start to list every cooking element")]
        [SerializeField] private bool _getItemsFromScene = false;
        [Tooltip("List here all the items that are instantiated on game start, so that they are referenced somewhere")]
        [SerializeField] private List<Component> _items;

        private PlayerInterface _player;
        #endregion


        #region Methods
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_player == null) throw new MissingReferenceException("No reference to the player could be found. Make sure the GameInitializer sets it.");

            if (_getItemsFromScene)
            {
                // Not implemented yet
            }
            
            // Loop through all interactables, and assign them the player reference
        }

        public void AddItem(Component item)
        {
            if (_items.Contains(item)) return;
            _items.Add(item);
        }
        #endregion
    }

}