using Clients;
using Controller;
using GameSystem;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UI
{
    public class UIManager : MonoBehaviour, IManager
    {
        public UIMenu CurrentMenu => _currentMenu;
        public string CurrentMenuName => _currentMenu.name;
        
        public UnityEvent OnUpgradeFail => _onUpgradeFail;
        public UnityEvent OnUpgradeSuccess => _onUpgradeSuccess;

        [Header("Info")]
        [InfoBox("Recommended UI tags are :\n" +
            "Main\n" +
            "Pause\n" +
            "Upgrades\n" +
            "Game\n" +
            "ShiftRecap", EInfoBoxType.Normal)]

        //[Header("References")]
        [SerializeField] private GameObject _canvas;
        [SerializeField] private string _pauseMenuName = "Pause";
        [SerializeField] private string _gameMenuName = "Game";
        [SerializeField] private string _mainMenuName = "Main";
        [SerializeField] private List<UIMenu> _menus = new List<UIMenu>();
        [SerializeField] private GameObject _skipTutoButton;
        [SerializeField] private GameObject _playButton;
        [SerializeField] private Subtitle _subtitles;

        [Header("UI Manager events")]

        [SerializeField] private UnityEvent _onStart;

        [SerializeField] private UnityEvent _onTutorialSetup;
        [SerializeField] private UnityEvent _onTutorialSkip;
        [SerializeField] private UnityEvent _onFade;
        [SerializeField] private UnityEvent _onEnable;
        [SerializeField] private UnityEvent _onDisable;
        [SerializeField] private UnityEvent _onUpgradeFail;
        [SerializeField] private UnityEvent _onUpgradeSuccess;
        [SerializeField] private UnityEvent _onPause;
        [SerializeField] private UnityEvent _onResume;

        private GameLoop _gameLoop;
        private TimerManager _timerManager;
        private ShiftManager _shiftManager;
        private DataManager _dataManager;
        private CameraManager _cameraManager;

        private InputActionAsset _inputActionAsset;
        private InputActionMap _playerInputs;
        private InputActionMap _UIInputs;
        private PlayerInterface _player;
        private UIMenu _currentMenu;
        private Dictionary<string, UIMenu> _menusByName = new();

        [Header("UI Resolution Settings")] 
        [SerializeField] private int widthRatio = 16;
        [SerializeField] private int heightRatio = 9;

        // Managers

        void Awake()
        {
            Register();
        }

        public void Unregister()
        {
            Reference.RemoveManager(this);
        }
        public void Init()
        {
            Reference.GetManager(out _gameLoop);
            Reference.GetManager(out _shiftManager);
            Reference.GetManager(out _dataManager);
            Reference.GetManager(out _timerManager);

            _inputActionAsset = Reference.AssetReferences.InputActions;
            _playerInputs = _inputActionAsset.FindActionMap("Player");
            _UIInputs = _inputActionAsset.FindActionMap("UI");
            Reference.GetObject(out _player);

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
            if (_shiftManager == null)
                this.Log(LogType.Warning,"An error occured during the initialization of the UIManager : No shift manager found");

            if (_gameLoop == null || _inputActionAsset == null || _playerInputs == null || _UIInputs == null || _player == null)
                this.Log(LogType.Error,"UIManager cannot run because it has not been initialized correctly");

            SetupMenuDictionary();
            DisableAllMenus();

            SubscribeToEvents();

            Vector2 resolution = GetAdaptedFullScreenResolution(widthRatio,heightRatio);
            
            Screen.SetResolution((int)resolution.x, (int)resolution.y, FullScreenMode.FullScreenWindow , new RefreshRate { numerator = 60, denominator = 1 });
            
            _onStart?.Invoke();
        }        
        public void PostInit()
        {
            if (_dataManager.ShiftCount >= 1)
            {
                SkipTutorial();
            }
            else
            {
                _onTutorialSetup?.Invoke();
            }
        }        
        Vector2  GetAdaptedFullScreenResolution(float ratioX, float ratioY)
        {
            float x = Screen.currentResolution.width / ratioX;
            float y = Screen.currentResolution.height / ratioY;

            float mult = x >= y ? y : x;
            
            return new Vector2(Mathf.Round(ratioX*mult), Mathf.Round(ratioY*mult));
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
            
            _currentMenu = menu;
            if (menu.menu != null) _currentMenu.menu.SetActive(true);
            _currentMenu.onEnable.Invoke();
            SetGameState(!_currentMenu.pauseGame);
            SetCursorFreedom(_currentMenu.mouseFreedom);


            
            this.Log(LogType.Log, $"{menu.name} menu has been enabled");
        }
        public void DisableMenu(UIMenu menu)
        {
            if (menu == default) return;
            this.Log(LogType.Log, $"Disabling menu {menu.name}");

            if (menu.menu != null) menu.menu.SetActive(false);
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
            bool isPauseMenu = _currentMenu.name == _pauseMenuName;
            
            _gameLoop.StopRunning(isPauseMenu);
            _playerInputs.Disable();
            _UIInputs.Enable();
            
            if (isPauseMenu)
            {
                _onPause?.Invoke();
            }

            this.Log(LogType.Log, $"Game has been paused");
        }
        public void StartGame()
        {
            if (_shiftManager == null) return;
            _shiftManager.SetupShift(_shiftManager.ShiftCount);
            _shiftManager.LaunchShift();
        }
        public void ResumeGame(bool skiptutorial=false)
        {
            if (skiptutorial)
            {
                SkipTutorial();
            }
            
            
            _gameLoop.StartRunning();
            _playerInputs.Enable();
            _UIInputs.Enable();
            _onResume?.Invoke();
            
            SetCursorFreedom(false);
            
            this.Log(LogType.Log, $"Game has been resumed");
        }
        public void PlaySubtitles(string name)
        {
            if (_subtitles == null) return;
            _subtitles.Play(name);
        }
        public void TryPlayTutorialSubtitles()
        {
            if (_shiftManager == null) return;
            if (_shiftManager.ShiftCount == 0)
            {
                PlaySubtitles("Start");
            }
        }
        public void SkipTutorial()
        {
            //_playButton.GetComponent<Button>().onClick = _skipTutoButton.GetComponent<Button>().onClick;
            //_skipTutoButton?.gameObject.SetActive(false);
            _shiftManager.SkipTutorialShift(); 
            foreach (var b in Reference.GetAll<SittingCounter>())
            {
                b.GetComponentInChildren<EventLinker>().UnBindAll();
                b.OnSkipTutorial?.Invoke();
            }
            _onTutorialSkip?.Invoke();
        }
        public void TutorialEndTransition()
        {
            _onFade?.Invoke();
            if (_shiftManager != null && _timerManager != null)
                _timerManager.StartTimer("Tutorial ended. Moving to the next shift", 1.5f, () => _shiftManager.TryEndShift(true));
        }
        public void SwitchPlayerInputs(bool enabled)
        {
            if (enabled)
            {
                _playerInputs.Enable();
            }
            else
            {
                _playerInputs.Disable();
            }
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
            {
                _UIInputs.FindAction("Pause").performed += (ctx) => TogglePause();
                _UIInputs.FindAction("ToggleUI").performed += (ctx) => ToggleAllUI();
                _UIInputs.FindAction("Screenshot").performed += (ctx) => TakeScreenshot();
            }
        }
        private void TogglePause()
        {
            if (_currentMenu.name == _mainMenuName) return;
            //Debug.Log("[UIManager] : Current menu is : " + _currentMenu.name);
            this.Log(LogType.Log, $"Toggleing pause");
            if (_currentMenu.name == _pauseMenuName)
                EnableMenu(_gameMenuName);
            else if (_currentMenu.name != _mainMenuName)
                EnableMenu(_pauseMenuName);
        }
        public void ReloadScene()
        {
            _gameLoop.StartRunning();
            _timerManager.StartTimer("reloadscene", 1,
                () => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex), true);
        }      
        public void ResetEverything()
        {
            _dataManager.ResetSave();
            ReloadScene();
        }
        public void Register()
        {
            Reference.SetManager(this);
        }
        public void RequestCameraSwitch(Transform target)
        {
            _cameraManager.LookAt(target, 0, true);
        }
        public void RequestCameraSwitchToFirstUITarget()
        {
            UITarget target = Reference.GetObject<UITarget>();
            RequestCameraSwitch(target.transform);
        }
        public void ResetCameraSwitch()
        {
            _cameraManager.EnableFirstPersonView();
        }
        public void HideAllUI() => _canvas?.SetActive(false);
        public void ShowAllUI() => _canvas?.SetActive(true);
        public void ToggleAllUI() => _canvas?.SetActive(!_canvas.activeSelf);
        public void TakeScreenshot()
        {
            string path = Application.persistentDataPath + "/Screenshots";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] info = dir.GetFiles("*.*");
            int fileAmount = info.Length;
            ScreenCapture.CaptureScreenshot(path + "/Screenshot " + fileAmount + ".png", 2);
            Debug.Log("Screenshot taken, saved at path " + path + "/Screenshot " + fileAmount);
            this.Log("Screenshot taken, saved at path " + path + "/Screenshot " + fileAmount);
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