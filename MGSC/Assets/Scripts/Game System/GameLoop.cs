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
        
        private static readonly List<UpdatableBase> _updatables = new();
        
        #endregion

        #region Fields

        public bool IsRunning { get; private set; }

        #endregion

        #region Methods
        
        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }

        public void OnEnable()
        {
            Register();
            StartRunning();
        }

        void Update()
        {
            if (IsRunning)
            {
                float deltaTime = Time.deltaTime;
                foreach (var manager in _updatables)
                {
                    manager.Update(deltaTime);
                }
            }
        }
        
        public void StartRunning()
        {
            IsRunning = true;
        }

        public void StopRunning()
        {
            IsRunning = false;
        }

        public void RegisterUpdatable(UpdatableBase manager)
        {
            if (!_updatables.Contains(manager))
                _updatables.Add(manager);
        }

        public void UnregisterUpdatable(UpdatableBase manager)
        {
            _updatables.Remove(manager);
        }
        
        #endregion
    }

}