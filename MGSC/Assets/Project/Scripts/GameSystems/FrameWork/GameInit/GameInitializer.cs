using System;
using Controller;
using Features;
using NaughtyAttributes;
using UI;
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
        static Action OnReset;
        #endregion

        #region Methods
        public static void ResetAll() => OnReset?.Invoke();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            // Create all the pure managers
            new OrderManager();
            new ClientManager();
            new TimerManager();
            new ShiftManager(); 
            new MoneyManager();
            new UpgradeManager();
            //new CameraManager();
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
        void OnEnable()
        {
            OnReset += Init;
            OnReset += ResetScene;
            OnReset += SetupScene;
        }
        void OnDisable()
        {
            OnReset -= Init;
            OnReset -= ResetScene;
            OnReset -= SetupScene;
            Reference.GetManager<UpgradeManager>().ClearAllSubscribers();
        }
        /// <summary>
        /// Instantiate all objects specified in the scene preset, and put them all in place
        /// </summary>
        public void ResetScene()
        {
            if (_gameProfile == null)
                throw new MissingReferenceException("[GameInitializer] : Please reference a game profile in the inspector.");

            if (_gameProfile.DestroyEveryOtherObject) DestroyEverything();

            Reference.AssetReferences = _gameProfile.AssetReferences;

            // Instantiate the scene and create all the MonoBehaviour managers
            if (_gameProfile.ApplyScenePreset) ApplyScenePreset();

            if (_gameProfile.AddUI) new UISystem();

            if (_gameProfile.AddPlayer) PlayerSetup.TrySetupPlayer();

            SetupDebug();
            
            // Setup the managers' references
            InitManagers();
        }
        private void SetupScene()
        {
            CursorUtility.Update();
            // Reset the save file
            if (_gameProfile.ResetSaveFile) SaveManager.ResetSave();
            GameLoop.SetState<MainMenuState>();
            

            // Apply the base logic to ensure all managers are setup to work correctly
            PostInitManagers();


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