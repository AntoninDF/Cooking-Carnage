using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller
{
    /* PlayerInputHandler is the class handling the player's inputs and calling events when needed */
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Input Action Asset")]  /*REFACTO : needs to be referenced automatically by GameInitializer*/ // Tkt mon gars je suis dessus <3
        public PlayerInterface Player { get => _playerInterface; set => _playerInterface = value; }
        public InputActionAsset PlayerControls
        {
            get => _playerControls;
            set
            {
                _playerControls = value;
                SetInputReferences();
            }
        }


        private PlayerInterface _playerInterface;
        private InputActionAsset _playerControls;
        
        [Header("Action Map Name Reference")]
        private string _actionMapName = "Player";
        
        [Header("Action Name References")]
        private string _movement = "Movement";
        private string _rotation = "Rotation";
        private string _interact = "Interact";
        private string _grab = "Grab";
        private string _pause = "Pause";
        
        private InputAction _movementAction;
        private InputAction _rotationAction;
        private InputAction _interactAction;
        private InputAction _grabAction;
        private InputAction _pauseAction;
        
        public Vector2 MovementInput { get; private set; }
        public Vector2 RotationInput { get; private set; }
        public Action<InputAction.CallbackContext> OnInteract { get; set; }
        public Action<InputAction.CallbackContext> OnGrab { get; set; }
        public Action<InputAction.CallbackContext> OnPause { get; set; }

        public void SetInputReferences()
        {
            if (_playerControls == null)
                throw new MissingReferenceException("Please ensure that the player controls are assigned in the player input handler :)");
            InputActionMap mapReference = _playerControls.FindActionMap(_actionMapName);

            _movementAction = mapReference.FindAction(_movement);
            _rotationAction = mapReference.FindAction(_rotation);
            _interactAction = mapReference.FindAction(_interact);
            _grabAction = mapReference.FindAction(_grab);
            _pauseAction = mapReference.FindAction(_pause);

            SubscribeActionValuesToInputEvents();
        }

        private void SubscribeActionValuesToInputEvents()
        {
            _movementAction.performed += inputInfo => MovementInput = inputInfo.ReadValue<Vector2>();
            _movementAction.canceled += inputInfo => MovementInput = Vector2.zero;

            _rotationAction.performed += inputInfo => RotationInput = inputInfo.ReadValue<Vector2>();
            _rotationAction.canceled += inputInfo => RotationInput = Vector2.zero;

            _interactAction.performed += inputInfo => OnInteract?.Invoke(inputInfo);

            _grabAction.performed += inputInfo => OnGrab?.Invoke(inputInfo);

            _pauseAction.performed += inputInfo => OnPause?.Invoke(inputInfo);
        }

        private void OnEnable()
        {
            if (_playerControls == null) return;
            _playerControls.FindActionMap(_actionMapName).Enable();
        }

        private void OnDisable()
        {
            if (_playerControls == null) return;
            _playerControls.FindActionMap(_actionMapName).Disable();
        }

    }
}
