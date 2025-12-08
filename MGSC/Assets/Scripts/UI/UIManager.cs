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
                this.Log(LogType.Warning,"An error occured during the initialization of the UIManager : No gameloop found");
            if (_inputActionAsset == null)
                this.Log(LogType.Warning,"An error occured during the initialization of the UIManager : No input action asset found");
            if (_playerInputs == null)
                this.Log(LogType.Warning,"An error occured during the initialization of the UIManager : No player action map found");
            if (_UIInputs == null)
                this.Log(LogType.Warning,"An error occured during the initialization of the UIManager : No UI action map found");
            if (_player == null)
                this.Log(LogType.Warning,"An error occured during the initialization of the UIManager : No player found");

            if (_gameLoop == null || _inputActionAsset == null || _playerInputs == null || _UIInputs == null || _player == null)
                this.Log(LogType.Error,"UIManager cannot run because it has not been initialized correctly");

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
            
            this.Log(LogType.Log, $"{_menus.Count} UI menus has been setup");
        }

        public void EnableMenu(string menuName)
        {
            if (!_menusByName.ContainsKey(menuName))
                this.Log(LogType.Warning, $"No such menu could be found {menuName}");
            
            EnableMenu(_menusByName[menuName]);
        }


        public void EnableMenu(UIMenu menu)
        {
            DisableMenu(_currentMenu);
            
            this.Log(LogType.Log, $"Enabling menu {menu.name}");
            if (menu.menu == null)
            {
                this.Log(LogType.Warning,"Couldn't enable menu '" + menu.name + "' because the gameobject was null");
                return;
            }
            _currentMenu = menu;
            _currentMenu.menu.SetActive(true);
            _currentMenu.onEnable.Invoke();
            SetGameState(!_currentMenu.pauseGame);
            SetCursorFreedom(_currentMenu.mouseFreedom);
            
            this.Log(LogType.Log, $"{menu.name} menu has been enabled");
        }
        public void DisableMenu(UIMenu menu)
        {
            if (menu == default) return;
            this.Log(LogType.Log, $"Disabling menu {menu.name}");

            if (menu.menu == null)
            {
                this.Log(LogType.Warning,"Couldn't enable menu '" + menu.name + "' because the gameobject was null");
                return;
            }
            menu.menu.SetActive(false);
            menu.onDisable.Invoke();
            this.Log(LogType.Log, $"{menu.name} menu has been disabled");
        }

        public void DisableAllMenus()
        {
            this.Log(LogType.Log, $"Disabling all menus");
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
            
            this.Log(LogType.Log, $"Game has been paused");
        }

        public void ResumeGame()
        {
            _gameLoop.StartRunning();
            _playerInputs.Enable();
            _UIInputs.Enable();
            
            this.Log(LogType.Log, $"Game has been resumed");
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
            this.Log(LogType.Log, $"Toggleing pause");
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