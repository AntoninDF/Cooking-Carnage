using System.Collections.Generic;
using Game_System.Interface;
using UnityEngine;

namespace GameSystem
{
    /// <summary>
    /// A Gamesystem used to manage Updates of non-monobehvaiour scripts
    /// </summary>
    public class GameLoop : MonoBehaviour,IManager
    {
        #region Properties
        
        private static readonly List<IUpdatable> _updatables = new();
        
        #endregion

        #region Fields

        #endregion

        #region Methods
        
        void Awake()
        {
            Register();
        }
        
        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }

        void Update()
        {
            float deltaTime = Time.deltaTime;
            foreach (var manager in _updatables)
            {
                manager.Update(deltaTime);
            }
        }

        public void RegisterUpdatable(IUpdatable manager)
        {
            if (!_updatables.Contains(manager))
                _updatables.Add(manager);
        }

        public void UnregisterUpdatable(IUpdatable manager)
        {
            _updatables.Remove(manager);
        }
        
        #endregion
    }

}