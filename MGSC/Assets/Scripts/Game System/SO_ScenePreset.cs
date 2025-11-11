using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameSystem
{
    /// <summary>
    /// This scriptable object lists every object and parameter to set up on game start. It will be set up by the GameInitializer
    /// </summary>
    [CreateAssetMenu(fileName = "ScenePreset", menuName = "Scriptable Objects/Scene Preset")]
    public class SO_ScenePreset : ScriptableObject
    {
        [Header("References")]

        public List<GameObject> Objects {  get=>_objects; private set=>_objects=value; }
        [Tooltip("List of gameobjects to instantiate on scene start (Awake)")]
        [SerializeField] private List<GameObject> _objects;


    }

}