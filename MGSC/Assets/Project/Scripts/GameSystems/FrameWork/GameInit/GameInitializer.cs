using System;
using Controller;
using Features;
using NaughtyAttributes;
using Orders;
using UI;
using Unity.AI.Navigation;
using UnityEngine;
using Upgrades;
namespace GameSystem
{

    /// <summary>
    /// The GameInitializer is responsible for instantiating and setting up every object at the start of the game.
    /// No other script should hold a reference to the GameInitializer
    /// </summary>
    [HideScriptField]
    public class GameInitializer : MonoBehaviour, IFeatureCollection
    {
        #region Fields

        // SERIALIZED FIELDS
        [Tooltip("The game profile specifies what  will be created on start, and how")]
        [Expandable]
        [SerializeField] private SO_GameProfile _gameProfile;
        
        [Expandable]
        [SerializeField] private SO_DebugProfile _debugProfile;

        public FeatureCollection Collection { get; set; } = new FeatureCollection();

        // PRIVATE FIELDS
        
        #endregion

        #region Methods

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            MakeGameLoop();

            // Create all the pure managers
            new OrderManager();
            new ClientManager();
            new TimerManager();
            new ShiftManager(); // (_gameProfile.ShiftDuration, _gameProfile.ShiftNoonDuration, _gameProfile.RentPrice);
            new MoneyManager();
            new UpgradeManager();
            new DataManager();
            new CameraManager();
            new ItemManager();
            new FeatureManager();
            new AdaptiveClientSpawner();

           
        }
        void Awake()
        {
            ResetScene();
        }
        void Start()
        {
            SetupScene();
        }
        /// <summary>
        /// Instantiate all objects specified in the scene preset, and put them all in place
        /// </summary>
        public void ResetScene()
        {
            if (_gameProfile == null)
                throw new MissingReferenceException("[GameInitializer] : Please reference a game profile in the inspector.");

            if (_gameProfile.DestroyEveryOtherObject) DestroyEverything();

            //Debug.Log("Asset references set");
            Reference.AssetReferences = _gameProfile.AssetReferences;

            // Instantiate the scene and create all the MonoBehaviour managers
            if (_gameProfile.ApplyScenePreset) ApplyScenePreset();

            if (_gameProfile.AddUI) AddUI();

            GameSettings.Instance?.Apply();

            if (_gameProfile.AddPlayer) PlayerSetup.TrySetupPlayer();

            SetupDebug();
            
            // Setup the managers' references
            InitManagers();
        }
        private void SetupScene()
        {
            // Reset the save file
            if (_gameProfile.ResetSaveFile) Reference.GetManager<DataManager>().ResetSave();

            // Apply the base logic to ensure all managers are setup to work correctly
            PostInitManagers();


            SetupMealPrices();

            // Remove the GameInitializer
            if (_gameProfile.DestroyAfterInitialization) Destroy(this.gameObject);

            CursorUtility.Update();
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

        private static void MakeGameLoop()
        {
            GameObject go = new();
            var gameLoop = go.AddComponent<GameLoop>();
            gameLoop.StartRunning();
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

        private void SetupMealPrices()
        {
            //OrderEvaluator.WrongMealPrice = _gameProfile.DefaultRecipePrices.rawMealPrice;
            OrderEvaluator.WrongMealPrice = _gameProfile.DefaultRecipePrices.wrongMealPrice;
            OrderEvaluator.OvercookedMealPrice = _gameProfile.DefaultRecipePrices.overcookedMealPrice;
            OrderEvaluator.PlainJellyPrice = _gameProfile.DefaultRecipePrices.emptyMealPrice;
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