using Controller;
using UnityEngine;
namespace GameSystem
{

    /// <summary>
    /// The GameInitializer is responsible for instantiating and setting up every object at the start of the game.
    /// No other script should hold a reference to the GameInitializer
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        #region Properties
        public PlayerInterface Player {  get=>_player; private set => _player=value; }

        #endregion

        #region Fields

        // SERIALIZED FIELDS
        [Header("References")]

        [Tooltip("Pass in the scriptable object holding references to every element to spawn on game start")]
        [SerializeField] private SO_ScenePreset _scenePreset;

        [Tooltip("Pass in the scriptable object holding references to every important data asset for the game")]
        [SerializeField] private SO_AssetReferences _assetReferences;

        // Transform at which the player will be spawned
        private Transform _playerStart;

        // PRIVATE FIELDS
        private PlayerInterface _player;
        private bool _destroyAfterInitialization = true;
        
        private AdaptiveClientSpawner _adaptiveClientSpawner;
        #endregion

        #region Methods


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            ResetScene();
            if (_destroyAfterInitialization) Destroy(this.gameObject);
        }

        /// <summary>
        /// Instantiate all objects specified in the scene preset, and put them all in place
        /// </summary>
        /// <exception cref="MissingReferenceException"></exception>
        /// <exception cref="MissingComponentException"></exception>
        public void ResetScene()
        {
            // Check scene preset reference
            if (_scenePreset == null)
                throw new MissingReferenceException("Please assign a scene preset scriptable object in the game initializer's inspector, otherwise it cannot initialize anything :)");
            
            if (_assetReferences == null)
                throw new MissingReferenceException("Please assign an asset reference scriptable object in the game initializer's inspector, otherwise many references will be missing :)");
            
            // Create a player if none exist yet
            if (_player == null) SpawnPlayer();

            // Create all the objects as requested in the scene preset
            foreach (var go in _scenePreset.Objects)
                Instantiate(go);
            
            // Init GameLoop
            GameObject gameLoop = new GameObject("GameLoop");
            gameLoop.AddComponent<GameLoop>();

            // Initialize managers
            // Note : It's important to call the ReferenceManager as late as possible, because it scans the scene upon being created (when first called).
             new OrderManager();
             new TimerManager();
             _adaptiveClientSpawner = new AdaptiveClientSpawner();
             new DataManager();
             new MoneyManager();
            
             _adaptiveClientSpawner.LaunchDelay(); // Might be moved to the adaptive client spawner's constructor ?
            ReferenceManager.AssetReferences = _assetReferences;
             
            _playerStart = ReferenceManager.Instance.GetElementOfType<PlayerStart>().transform;
            ResetPlayer();
        }

        /// <summary>
        /// Instantiate the player prefab in the scene
        /// </summary>
        private void SpawnPlayer()
        {
            if (_scenePreset.Player == null)
                throw new MissingReferenceException("Please assign a player prefab in the scene preset's dedicated field :)");
            GameObject playerGo = GameObject.Instantiate(_scenePreset.Player);
            playerGo.name = "Player";

            if (!playerGo.HasComponent<PlayerInterface>())
            {
                throw new MissingComponentException("Please provide a PlayerInterface component on the player prefab :)");
            }
            _player = playerGo.GetComponent<PlayerInterface>();
        }

        /// <summary>
        /// Place the player at the player start position
        /// </summary>
        private void ResetPlayer()
        {
            if (_playerStart != null)
            {
                _player.transform.position = _playerStart.position;
                _player.transform.rotation = _playerStart.rotation;
            }
            else
            {
                Debug.LogWarning("[GameInitializer] ResetPlayer : Couldn't reset the player's position because the player start's transform's reference is missing.");
            }
            SetPlayerReferences();
        }
    
        /// <summary>
        /// Assign the input action from the scene preset to the player
        /// </summary>
        /// <exception cref="MissingReferenceException"></exception>
        private void SetPlayerReferences()
        {
            if (_scenePreset.InputActions == null)
                throw new MissingReferenceException("Please assign the input action asset in the scene preset SO");
            _player.InputHandler.PlayerControls = _scenePreset.InputActions;
            _player.Interactor.InputHandler = _player.InputHandler;
            _player.Controller.InputHandler = _player.InputHandler;
            _player.InputHandler.SetInputReferences();
        }
        #endregion
    }

}