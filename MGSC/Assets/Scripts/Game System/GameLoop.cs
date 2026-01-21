using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

        private List<UpdatableBase> _updatables = new();
        public UnityEvent OnGameStart = new ();
        public UnityEvent OnPause = new();
        public UnityEvent OnResume = new();
        
        
        public UnityEvent ShiftBegin = new();
        public UnityEvent ShiftEnd = new();
        public UnityEvent ShiftNoon = new();
        public UnityEvent RentGameOver = new();

        #endregion

        #region Methods
        
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
        public void Init() { }

        public void OnEnable()
        {
            Register();
            OnGameStart?.Invoke();
        }

        /// <summary>
        /// Updates all the registered updatables if the GameLoop IsRunning
        /// </summary>
        void Update()
        {
            if (!IsRunning) return;
            //Debug.Log("GameLoop updating");
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
        /// Resume the game loop
        /// </summary>
        public void StartRunning()
        {
            IsRunning = true;
            OnResume?.Invoke();
            
            this.Log("is now running");
        }

        
        /// <summary>
        /// Pauses the game loop
        /// </summary>
        public void StopRunning(bool pauseMenu = true)
        {
            IsRunning = false;
            OnPause?.Invoke();
            if (!pauseMenu)
            {
                OnResume?.Invoke();
            }
            this.Log("is not running anymore");
        }
        
        /// <summary>
        /// Registers classes that implements UpdatableBase to be updated each frame
        /// </summary>
        public void RegisterUpdatable(UpdatableBase updatable)
        {
            if (!_updatables.Contains(updatable))
            {
                _updatables.Add(updatable);
                
                this.Log(updatable.ToString() + " has been registered");
            }
            else
            {
                this.Log(LogType.Warning ,updatable.ToString() + " has already been registered");
            }
        }

        /// <summary>
        /// Unregisters a previously registered updatable
        /// </summary>
        public void UnregisterUpdatable(UpdatableBase manager)
        {
            _updatables.Remove(manager);
            
            this.Log(manager.ToString() + " has been removed");
        }
        
        #endregion
    }

}