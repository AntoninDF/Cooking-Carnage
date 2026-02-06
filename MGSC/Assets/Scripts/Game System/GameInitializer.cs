using System;
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
        
        [Expandable]
        [SerializeField] private SO_DebugProfile _debugProfile;

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
            if (_gameProfile.ResetRefManager) Reference.Reset();
            Reference.RemoveAllManagers();
            if (_gameProfile == null)
                throw new MissingReferenceException("[GameInitializer] : Please reference a game profile in the inspector.");

            if (_gameProfile.DestroyEveryOtherObject) DestroyEverything();

            Reference.AssetReferences = _gameProfile.AssetReferences;

            if (_gameProfile.RequireGameLoop) MakeGameLoop();

            // Instantiate the scene and create all the MonoBehaviour managers
            if (_gameProfile.ApplyScenePreset) ApplyScenePreset();

            // Create all the pure managers
            new OrderManager();
            new ClientManager();
            new TimerManager();
            if (_gameProfile.AddClientSpawner) new AdaptiveClientSpawner();
            new ShiftManager(_gameProfile.ShiftDuration, _gameProfile.ShiftNoonDuration, _gameProfile.RentPrice);
            new MoneyManager();
            new UpgradeManager();
            new DataManager();
            new CameraManager();
            new ItemManager();
            if (_gameProfile.AddUI) AddUI();

            // Create the player
            Reference.ScanHierarchy();
            if (_gameProfile.AddPlayer) PlayerSetup.TrySetupPlayer();

            SetupDebug();
            
            // Setup the managers' references
            InitManagers();
            
            // Reset the save file
            if (_gameProfile.ResetSaveFile) Reference.GetManager<DataManager>().ResetSave();

            // Apply the base logic to ensure all managers are setup to work correctly
            PostInitManagers();

            // Enable the default UI menu and bind it's events
            if (_gameProfile.AddUI) SetupUI();

            SetupMealPrices();

            // Remove the GameInitializer
            if (_gameProfile.DestroyAfterInitialization) Destroy(this.gameObject);
        }

        private void InitManagers()
        {
            foreach (var kvp in Reference.Managers)
            {
                //Debug.Log("[GameInitializer] : Initializing " + kvp.Key + ", object is " + kvp.Value);
                (kvp.Value).Init();
            }
        }
        private void PostInitManagers()
        {
            foreach (var kvp in Reference.Managers)
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
            if (Reference.AssetReferences.UI != null)
            {
                Instantiate(Reference.AssetReferences.GameLoop);
            }
            else
            {
                Debug.LogWarning("[GameInitializer] : Couldn't add the GameLoop prefab because it was not referenced in the asset references object");
            }
        }

        private void AddUI()
        {
            if (Reference.AssetReferences.UI == null)
                Debug.LogWarning("[GameInitializer] : Couldn't add the UI prefab because it was not referenced in the asset references object");
            else
            {
                Instantiate(Reference.AssetReferences.UI);
            }
        }

        private void SetupUI()
        {
            if (Reference.TryGetManager(out UIManager ui))
                ui.EnableMenu(_gameProfile.DefaultMenu);
            if (Reference.TryGetManager(out ShiftManager sm))
            {
                sm.OnShiftSucceeded += () => ui.EnableMenu("ShiftRecap");
                sm.OnShiftFailed += () => ui.EnableMenu("FailedShiftRecap");
            }
        }

        private void SetupMealPrices()
        {
            MoneyEvaluator.RawMealPrice = _gameProfile.DefaultRecipePrices.rawMealPrice;
            MoneyEvaluator.WrongMealPrice = _gameProfile.DefaultRecipePrices.wrongMealPrice;
            MoneyEvaluator.OvercookedMealPrice = _gameProfile.DefaultRecipePrices.overcookedMealPrice;
        }

        private void SetupDebug()
        {
            var refManagers = Reference.Managers;
            
            if(_debugProfile == null)
                return;
            
            foreach (var debugSetting in _debugProfile.managerSettings)
            {
                Type type = null;
                
                foreach (var manager in refManagers)
                {
                    if (manager.Key.Name == debugSetting.managerTypeName)
                    {
                        type = manager.Key;
                        break;
                    }
                }
                
                if (type == null)
                {
                    continue;
                }
                
                if (debugSetting.debugInfo)
                    Debugger.ToggleDebugManager(type, LogType.Log);
                
                if (debugSetting.debugWarning)
                    Debugger.ToggleDebugManager(type, LogType.Warning);
                
                if (debugSetting.debugError)
                    Debugger.ToggleDebugManager(type, LogType.Error);
            }
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