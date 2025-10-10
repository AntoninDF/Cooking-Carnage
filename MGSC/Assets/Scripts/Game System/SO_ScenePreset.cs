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
        public GameObject Player { get =>_player; private set=>_player=value; }
        [Tooltip("Player prefab to spawn on scene start")]
        [SerializeField] private GameObject _player;
        public InputActionAsset InputActions { get => _inputActions; private set => _inputActions = value; }
        [Tooltip("The input action asset for the player")]
        [SerializeField] private InputActionAsset _inputActions;

        public List<GameObject> Objects {  get=>_objects; private set=>_objects=value; }
        [Tooltip("List of gameobjects to instantiate on scene start (Awake)")]
        [SerializeField] private List<GameObject> _objects;


    }

}