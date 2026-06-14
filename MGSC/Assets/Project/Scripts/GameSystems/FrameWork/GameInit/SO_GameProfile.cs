using GameSystem;
using NaughtyAttributes;
using UnityEngine;

namespace GameSystem
{

    [CreateAssetMenu(fileName = "New GameProfile", menuName = "Scriptable Objects/Game Profile")]
    public class SO_GameProfile : ScriptableObject
    {
        #region Fields

        [Header("Profile parameters")]
        [Tooltip("Should this gameobject be destroyed juste after the game starts ?")]
        [SerializeField] private bool _destroyAfterInitialization = true;
        public bool DestroyAfterInitialization => _destroyAfterInitialization;

        [Tooltip("Should every other object already present in the scene be destroyed on start ?")]
        [SerializeField] private bool _destroyEveryOtherObject = false;
        public bool DestroyEveryOtherObject => _destroyEveryOtherObject;

        [Tooltip("Whether or not the scene preset should be used to instantiate scene elements")]
        [SerializeField] private bool _applyScenePreset;
        public bool ApplyScenePreset => _applyScenePreset;

        [Tooltip("Wether or not the save file should be reset upon entering play mode")]
        [SerializeField] private bool _resetSaveFile;
        public bool ResetSaveFile => _resetSaveFile;

        [Tooltip("Wether or not to reset the ReferenceManager upon reloading")]
        [SerializeField] private bool _resetRefManager = true;
        public bool ResetRefManager => _resetRefManager;


        [Space]
        [Header("Game settings")]
        [Tooltip("How long should a shift last ?")]
        [SerializeField] private float _shiftDuration = 30f;
        public float ShiftDuration => _shiftDuration;
        
        [Tooltip("How much time left is considered noon/the end of the shift ?")]
        [SerializeField] private float _shiftNoonDuration = 30f;
        public float ShiftNoonDuration => _shiftNoonDuration;
        
        [Tooltip("How much money does the rent cost (Y axis), depending on the shift count (X axis)")]
        [SerializeField] private AnimationCurve _rentPrice;
        public AnimationCurve RentPrice => _rentPrice;
        [Tooltip("The name of the menu that will be enabled by default. It should match with a menu from the UIManager.")]
        [SerializeField] private string _defaultMenu;
        public string DefaultMenu => _defaultMenu;
        [Tooltip("Failed recipe prices")]
        [SerializeField] private DefaultRecipePrices _defaultRecipePrices;
        public DefaultRecipePrices DefaultRecipePrices => _defaultRecipePrices;

        [Space]
        [Header("Managers & customization")]

        [Tooltip("Whether or not a player is required. It will try to snap to the playerStart")]
        [SerializeField] private bool _addPlayer;
        public bool AddPlayer => _addPlayer;
        /*
        [Tooltip("Whether or not a gameloop is required.")]
        [SerializeField] private bool _addGameLoop;
        public bool RequireGameLoop => _addGameLoop;
        
        [Tooltip("Whether or not a client spawner is required.")]
        [SerializeField] private bool _addClientSpawner;
        public bool AddClientSpawner => _addClientSpawner;
        */
        [Tooltip("Whether or not the UI is required.")]
        [SerializeField] private bool _addUI;
        public bool AddUI => _addUI;



        [Space]
        [Header("References")]
        [Expandable]
        [Tooltip("Pass in the scriptable object holding references to every element to spawn on game start")]
        [SerializeField] private SO_ScenePreset _scenePreset;
        public SO_ScenePreset ScenePreset => _scenePreset;

        [Expandable]
        [Tooltip("Pass in the scriptable object holding references to every important data asset for the game")]
        [SerializeField] private SO_AssetReferences _assetReferences;
        public SO_AssetReferences AssetReferences => _assetReferences;

        #endregion

        
    }

    [System.Serializable]
    public struct DefaultRecipePrices
    {
        [Tooltip("How much money does an overcooked meal give")]
        public float overcookedMealPrice;
        [Tooltip("How much money does a raw meal give")]
        public float rawMealPrice;
        [Tooltip("How much money does a meal give if it's not the one ordered")]
        public float wrongMealPrice;
    }
}