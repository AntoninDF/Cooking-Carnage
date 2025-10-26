using GameSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace UI
{
    public class UIManager : MonoBehaviour, IManager
    {
        [SerializeField] private UnityEvent _onStart;
        [SerializeField] private UnityEvent _onEnable;
        [SerializeField] private UnityEvent _onDisable;

        private GameLoop _gameLoop;
        private InputActionAsset _inputActionAsset;
        private InputActionMap _playerInputs;
        private InputActionMap _UIInputs;

        void Start()
        {
            Register();
            Init();
            EnableUI();
            _onStart?.Invoke();
        }

        private void Init()
        {
            _gameLoop = ReferenceManager.Instance.GetManager<GameLoop>();
            _inputActionAsset = ReferenceManager.AssetReferences.inputActionsAsset;
            _playerInputs = _inputActionAsset.FindActionMap("Player");
            _UIInputs = _inputActionAsset.FindActionMap("UI");

            if (_gameLoop == null)
                Debug.LogError("An error occured during the initialization of the UIManager : No gameloop found");
            if (_inputActionAsset == null)
                Debug.LogError("An error occured during the initialization of the UIManager : No input action asset found");
            if (_playerInputs == null)
                Debug.LogError("An error occured during the initialization of the UIManager : No player action map found");
            if (_UIInputs == null)
                Debug.LogError("An error occured during the initialization of the UIManager : No UI action map found");
        }

        public void EnableUI()
        {
            _gameLoop.StopRunning();
            _playerInputs.Disable();
            _UIInputs.Enable();
            FreeCursor();
            _onEnable?.Invoke();
        }

        public void DisableUI()
        {
            _gameLoop.StartRunning();
            _playerInputs.Enable();
            _UIInputs.Disable();
            LockCursor();
            _onDisable?.Invoke();
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

        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }
    }
}