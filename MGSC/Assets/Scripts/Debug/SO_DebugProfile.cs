using System;
using System.Collections.Generic;
using GameSystem;
using NaughtyAttributes;
using UnityEngine;

namespace GameSystem
{

    [Serializable]
    public class DebugManagerSetting
    {
        public string managerTypeName;
        public bool debugInfo;
        public bool debugWarning;
        public bool debugError;
    }
    
    [CreateAssetMenu(fileName = "New DebugProfile", menuName = "Scriptable Objects/Debug Profile")]
    public class SO_DebugProfile : ScriptableObject
    {
        #region Fields
        
        [Header("Debug Settings Per Manager")]        
        [Tooltip("Debug settings for each manager type dynamically used by the editor.")]
        public List<DebugManagerSetting> managerSettings = new();
        
        #endregion
        
        #region Methods
        /// <summary>
        /// Returns the entry for a given manager type, creates it if not found.
        /// </summary>
        public DebugManagerSetting GetOrCreateManagerSetting(Type type)
        {
            string name = type.FullName;
            
            var entry = managerSettings.Find(s => s.managerTypeName == name);

            if (entry == null)
            {
                entry = new DebugManagerSetting()
                {
                    managerTypeName = name,
                    debugInfo = false,
                    debugWarning = false,
                    debugError = false
                };

                managerSettings.Add(entry);
            }

            return entry;
        }

        #endregion
    }
}