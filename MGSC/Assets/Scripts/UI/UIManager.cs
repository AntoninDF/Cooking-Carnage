using Controller;
using GameSystem;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace UI
{
    public class UIManager : MonoBehaviour, IManager
    {
        public UIMenu CurrentMenu => _currentMenu;
        public string CurrentMenuName => _currentMenu.name;

        [Header("Info")]
        [InfoBox("Recommended UI tags are :\n" +
            "Main\n" +
            "Pause\n" +
            "Upgrades\n" +
            "Game\n" +
            "ShiftRecap", EInfoBoxType.Normal)]

        //[Header("References")]
        [SerializeField] private string _pauseMenuName = "Pause";
        [SerializeField] private string _gameMenuName = "Game";
        [SerializeField] private List<UIMenu> _menus = new List<UIMenu>();

        [Header("UI Manager events")]

        [SerializeField] private UnityEvent _onStart;
        [SerializeField] private UnityEvent _onEnable;
        [SerializeField] private UnityEvent _onDisable;

        private GameLoop _gameLoop;
        private InputActionAsset _inputActionAsset;
        private InputActionMap _playerInputs;
        private InputActionMap _UIInputs;
        private PlayerInterface _player;
        private UIMenu _currentMenu;
        private Dictionary<string, UIMenu> _menusByName = new();

        void Awake()
        {
            Register();
        }

        public void Unregister()
        {
            ReferenceManager.Instance.RemoveManager(this);
        }

        public void Init()
        {
            _gameLoop = ReferenceManager.Instance.GetManager<GameLoop>();
            _inputActionAsset = ReferenceManager.AssetReferences.InputActions;
            _playerInputs = _inputActionAsset.FindActionMap("Player");
            _UIInputs = _inputActionAsset.FindActionMap("UI");
            _player = ReferenceManager.Instance.GetElementOfType<PlayerInterface>();

            if (_gameLoop == null)
                Debug.LogWarning("An error occured during the initialization of the UIManager : No gameloop found");
            if (_inputActionAsset == null)
                Debug.LogWarning("An error occured during the initialization of the UIManager : No input action asset found");
            if (_playerInputs == null)
                Debug.LogWarning("An error occured during the initialization of the UIManager : No player action map found");
            if (_UIInputs == null)
                Debug.LogWarning("An error occured during the initialization of the UIManager : No UI action map found");
            if (_player == null)
                Debug.LogWarning("An error occured during the initialization of the UIManager : No player found");

            if (_gameLoop == null || _inputActionAsset == null || _playerInputs == null || _UIInputs == null || _player == null)
                Debug.LogError("[UIManager] : The UIManager cannot run because it has not been initialized correctly");

            SetupMenuDictionary();
            DisableAllMenus();

            SubscribeToEvents();
            _onStart?.Invoke();
        }

        private void SetupMenuDictionary()
        {
            _menusByName.Clear();
            foreach (var menu in _menus)
                _menusByName[menu.name] = menu;
        }

        public void EnableMenu(string menuName)
        {
            if (!_menusByName.ContainsKey(menuName))
                Debug.LogError("[UIManager] : No such menu could be found : " + menuName + ". Please make sure you have the correct spelling");
            
            EnableMenu(_menusByName[menuName]);
        }


        public void EnableMenu(UIMenu menu)
        {
            DisableMenu(_currentMenu);
            //Debug.Log("Enabling menu '" + menu.name + "'");
            if (menu.menu == null)
            {
                Debug.LogWarning("[UIManager] : Couldn't enable the menu '" + menu.name + "' because the gameobject was null");
                return;
            }
            _currentMenu = menu;
            _currentMenu.menu.SetActive(true);
            _currentMenu.onEnable.Invoke();
            SetGameState(!_currentMenu.pauseGame);
            SetCursorFreedom(_currentMenu.mouseFreedom);
        }
        public void DisableMenu(UIMenu menu)
        {
            if (menu == default) return;
            //Debug.Log("Disabling menu '" + menu.name + "'");

            if (menu.menu == null)
            {
                Debug.LogWarning("[UIManager] : Couldn't enable the menu '" + menu.name + "' because the gameobject was null");
                return;
            }
            menu.menu.SetActive(false);
            menu.onDisable.Invoke();
        }

        public void DisableAllMenus()
        {
            foreach(var menu in _menus)
            {
                DisableMenu(menu);
            }
        }

        /// <summary>
        /// Wether or not the game should be paused
        /// </summary>
        /// <param name="running"></param>
        public void SetGameState(bool running) { if (running) ResumeGame(); else PauseGame(); }
        public void PauseGame()
        {
            _gameLoop.StopRunning();
            _playerInputs.Disable();
            _UIInputs.Enable();
        }

        public void ResumeGame()
        {
            _gameLoop.StartRunning();
            _playerInputs.Enable();
            _UIInputs.Enable();
        }

        private void SetCursorFreedom(bool freedom)
        {
            if (freedom)
                FreeCursor();
            else LockCursor();
        }
        private void FreeCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void SubscribeToEvents()
        {
            if (_UIInputs != null)
                _UIInputs.FindAction("Pause").performed += (ctx) => TogglePause();
        }

        private void TogglePause()
        {
            //Debug.Log("[UIManager] : Current menu is : " + _currentMenu.name);
            if (_currentMenu.name == _pauseMenuName)
                EnableMenu(_gameMenuName);
            else
                EnableMenu(_pauseMenuName);
        }

        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }

        [System.Serializable]
        public struct UIMenu
        {
            public string name;
            public GameObject menu;
            public bool mouseFreedom;
            public bool pauseGame;
            public UnityEvent onEnable;
            public UnityEvent onDisable;

            public override bool Equals(object obj)
            {
                return obj is UIMenu menu &&
                       name == menu.name &&
                       EqualityComparer<GameObject>.Default.Equals(this.menu, menu.menu) &&
                       mouseFreedom == menu.mouseFreedom &&
                       pauseGame == menu.pauseGame;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(name, menu, mouseFreedom, pauseGame);
            }

            public static bool operator ==(UIMenu a, UIMenu b) => (a.name == b.name) && (a.menu == b.menu) && (a.mouseFreedom == b.mouseFreedom) && (a.pauseGame == b.pauseGame);
            public static bool operator !=(UIMenu a, UIMenu b) => !(a==b);
        }
    }
}