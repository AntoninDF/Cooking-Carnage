using UnityEngine;
using UnityEngine.InputSystem;

namespace Features
{
    public class PlayerInputReader : BasePlayerInputs
    {
        public string Movement_ActionName { get; set; } = "Movement";
        public string Rotation_ActionName { get; set; } = "Rotation";
        public string Interact_ActionName { get; set; } = "Interact";
        public string Pause_ActionName { get; set; } = "Pause";
        public InputActionAsset InputActions { get; set; }


        private string _actionMapName = "Player";

        private InputAction _movementAction;
        private InputAction _rotationAction;
        private InputAction _interactAction;
        private InputAction _pauseAction;

        public PlayerInputReader(IFeatureCollection owner) : base(owner)
        {
        }

        public override void LateSetup()
        {
            base.LateSetup();
            InputActions = Reference.AssetReferences.InputActions;
            SetInputReferences();
        }

        protected void SetInputReferences()
        {
            if (InputActions == null)
                throw new MissingReferenceException("Please ensure that the player controls are assigned in the player input handler :)");
            InputActionMap mapReference = InputActions.FindActionMap(_actionMapName);

            _movementAction = mapReference.FindAction(Movement_ActionName);
            _rotationAction = mapReference.FindAction(Rotation_ActionName);
            _interactAction = mapReference.FindAction(Interact_ActionName);
            _pauseAction = mapReference.FindAction(Pause_ActionName);

            BindEvents();
            OnEnable += BindEvents;
            OnDisable += UnbindEvents;
        }

        protected void BindEvents()
        {
            _movementAction.performed += MovementAction;
            _movementAction.canceled += MovementActionReset;

            _rotationAction.performed += RotationAction;
            _rotationAction.canceled += RotationActionReset;

            _interactAction.performed += InteractAction;

            _pauseAction.performed += PauseAction;
        }
        protected void UnbindEvents()
        {
            _movementAction.performed -= MovementAction;
            _movementAction.canceled -= MovementActionReset;

            _rotationAction.performed -= RotationAction;
            _rotationAction.canceled -= RotationActionReset;

            _interactAction.performed -= InteractAction;

            _pauseAction.performed -= PauseAction;
        }

        void RotationActionReset(InputAction.CallbackContext ctx)
        {
            LookOffset = Vector2.zero;
            OnLook?.Invoke();
        }
        void RotationAction(InputAction.CallbackContext ctx)
        {
            LookOffset = ctx.ReadValue<Vector2>();
            OnLook?.Invoke();
        }
        void MovementActionReset(InputAction.CallbackContext ctx)
        {
            MovementDirection = Vector2.zero;
            OnMovement?.Invoke();
        }
        void MovementAction(InputAction.CallbackContext ctx)
        {
            MovementDirection = ctx.ReadValue<Vector2>();
            OnMovement?.Invoke();
        }
        void InteractAction(InputAction.CallbackContext ctx) => OnPrimaryAction?.Invoke();
        void PauseAction(InputAction.CallbackContext ctx) => OnPause?.Invoke();
    }
}