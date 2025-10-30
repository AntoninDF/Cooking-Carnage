using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    /// <summary>
    /// A Gamesystem used to manage Updates of non-monobehvaiour scripts
    /// </summary>
    public class GameLoop : MonoBehaviour,IManager
    {
        #region Properties
        
        public bool IsRunning { get; private set; }
        
        #endregion

        #region Fields

        private static readonly List<UpdatableBase> _updatables = new();

        #endregion

        #region Methods
        
        /// <summary>
        /// IManager Interface method to register itself to the ReferenceManager
        /// </summary>
        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }

        public void OnEnable()
        {
            Register();
            StartRunning();
        }

        /// <summary>
        /// Updates all the registered updatables if the GameLoop IsRunning
        /// </summary>
        void Update()
        {
            if (!IsRunning) return;
            
            float deltaTime = Time.deltaTime;
            foreach (var updatable in _updatables)
            {
                updatable.Update(deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (!IsRunning) return;
            
            foreach (var updatable in _updatables)
            {
                updatable.FixedUpdate();
            }
        }

        /// <summary>
        /// Starts the game loop
        /// </summary>
        public void StartRunning()
        {
            IsRunning = true;
        }

        
        /// <summary>
        /// Stops the game loop
        /// </summary>
        public void StopRunning()
        {
            IsRunning = false;
        }
        
        /// <summary>
        /// Registers classes that implements UpdatableBase to be updated each frame
        /// </summary>
        public void RegisterUpdatable(UpdatableBase updatable)
        {
            if (!_updatables.Contains(updatable))
                _updatables.Add(updatable);
        }

        /// <summary>
        /// Unregisters a previously registered updatable
        /// </summary>
        public void UnregisterUpdatable(UpdatableBase manager)
        {
            _updatables.Remove(manager);
        }
        
        #endregion
    }

}