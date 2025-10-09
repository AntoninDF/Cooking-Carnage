using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller
{
    /* PlayerInputHandler is the class handling the player's inputs and calling events when needed */
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Input Action Asset")]  /*REFACTO : needs to be referenced automatically by GameInitializer*/
        [SerializeField] private InputActionAsset playerControls;
        
        [Header("Action Map Name Reference")]
        private string actionMapName = "Player";
        
        [Header("Action Name References")]
        private string movement = "Movement";
        private string rotation = "Rotation";
        
        private InputAction movementAction;
        private InputAction rotationAction;
        
        public Vector2 MovementInput { get; private set; }
        public Vector2 RotationInput { get; private set; }

        private void Awake()
        {
            InputActionMap mapReference = playerControls.FindActionMap(actionMapName);

            movementAction = mapReference.FindAction(movement);
            rotationAction = mapReference.FindAction(rotation);

            SubscribeActionValuesToInputEvents();
        }

        private void SubscribeActionValuesToInputEvents()
        {
            movementAction.performed += inputInfo => MovementInput = inputInfo.ReadValue<Vector2>();
            movementAction.canceled += inputInfo => MovementInput = Vector2.zero;

            rotationAction.performed += inputInfo => RotationInput = inputInfo.ReadValue<Vector2>();
            rotationAction.canceled += inputInfo => RotationInput = Vector2.zero;
        }

        private void OnEnable()
        {
            playerControls.FindActionMap(actionMapName).Enable();
        }

        private void OnDisable()
        {
            playerControls.FindActionMap(actionMapName).Disable();
        }

    }
}
