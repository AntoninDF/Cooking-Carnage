using Controller;
using NaughtyAttributes;
using System.Collections.Generic;
using TMPro;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using Upgrades;
namespace GameSystem
{

    /// <summary>
    /// The GameInitializer is responsible for instantiating and setting up every object at the start of the game.
    /// No other script should hold a reference to the GameInitializer
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        #region Fields

        // SERIALIZED FIELDS
        [Tooltip("The game profile specifies what  will be created on start, and how")]
        [Expandable]
        [SerializeField] private SO_GameProfile _gameProfile;

        // PRIVATE FIELDS

        #endregion

        #region Methods


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            ResetScene();
        }

        /// <summary>
        /// Instantiate all objects specified in the scene preset, and put them all in place
        /// </summary>
        /// <exception cref="MissingReferenceException"></exception>
        /// <exception cref="MissingComponentException"></exception>
        public void ResetScene()
        {
            ReferenceManager.Instance.RemoveAllManagers();
            if (_gameProfile == null)
                Debug.LogError("[GameInitializer] : Please reference a game profile in the inspector.");

            if (_gameProfile.DestroyEveryOtherObject) DestroyEverything();

            ReferenceManager.AssetReferences = _gameProfile.AssetReferences;

            if (_gameProfile.RequireGameLoop) MakeGameLoop();

            // Instantiate the scene and create all the MonoBehaviour managers
            if (_gameProfile.ApplyScenePreset) ApplyScenePreset();

            // Create all the pure managers
            new OrderManager();
            new ClientManager();
            new TimerManager();
            if (_gameProfile.AddClientSpawner) new AdaptiveClientSpawner();
            new ShiftManager(_gameProfile.ShiftDuration, _gameProfile.RentPrice);
            new MoneyManager();
            new UpgradeManager();
            new DataManager();
            if (_gameProfile.AddUI) AddUI();

            // Create the player
            ReferenceManager.Instance.ScanHierarchy();
            if (_gameProfile.AddPlayer) PlayerSetup.TrySetupPlayer();

            // Setup the managers' references
            InitManagers();
            // Apply the base logic to ensure all managers are setup to work correctly
            PostInitManagers();

            if (_gameProfile.ResetSaveFile) ReferenceManager.Instance.GetManager<DataManager>().ResetSave();
            SetupUI();

            if (_gameProfile.DestroyAfterInitialization) Destroy(this.gameObject);
        }

        private void InitManagers()
        {
            foreach (var kvp in ReferenceManager.Instance.Managers)
            {
                //Debug.Log("[GameInitializer] : Initializing " + kvp.Key + ", object is " + kvp.Value);
                (kvp.Value).Init();
            }
        }
        private void PostInitManagers()
        {
            foreach (var kvp in ReferenceManager.Instance.Managers)
            {
                (kvp.Value).PostInit();
            }
        }
        private void ApplyScenePreset()
        {
            // Check scene preset reference
            if (_gameProfile.ScenePreset == null)
                Debug.LogError("[GameInitializer] : No scene preset referenced. Couldn't instantiate the scene.");
            // Create all the objects as requested in the scene preset
            foreach (var go in _gameProfile.ScenePreset.Objects)
                Instantiate(go);
        }

        private void MakeGameLoop()
        {
            // Init GameLoop
            GameObject gameLoop = new GameObject("GameLoop");
            gameLoop.AddComponent<GameLoop>();
        }

        private void AddUI()
        {
            if (ReferenceManager.AssetReferences.UI == null)
                Debug.LogWarning("[GameInitializer] : Couldn't add the UI prefab because it was not referenced in the asset references object");
            else
            {
                Instantiate(ReferenceManager.AssetReferences.UI);
            }
        }

        private void SetupUI()
        {
            var ui = ReferenceManager.Instance.GetManager<UIManager>();
            ui.EnableMenu(_gameProfile.DefaultMenu);
            ReferenceManager.Instance.GetManager<ShiftManager>().OnShiftEnded += () => ui.EnableMenu("ShiftRecap");
        }
    

        private void DestroyEverything()
        {
            foreach(GameObject go in FindObjectsByType(typeof(GameObject), FindObjectsSortMode.None))
            {
                if (go != gameObject) Destroy(go);
            }
        }

        #endregion
    }

}