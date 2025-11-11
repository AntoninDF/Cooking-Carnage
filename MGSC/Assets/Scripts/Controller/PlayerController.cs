using UnityEngine;

namespace Controller
{
    /* PlayerController applies the supposed actions linked to the inputs on the player Prefab */
    public class PlayerController : MonoBehaviour
    {
        #region Properties
        public PlayerInterface Player { get => _playerInterface; set => _playerInterface = value; }
        public PlayerInputHandler InputHandler => Player.InputHandler;
        public float CurrentSpeed => _walkSpeed;
        #endregion

        #region Fields
        [Header("Parameters")]
        [Tooltip("Base movement speed when walking.")]
        [Range(1f, 30f)]
        [SerializeField] private float _walkSpeed = 3.0f;
        
        [Tooltip("Sensitivity of the mouse when looking around.")]
        [Range(0.01f, 2f)]
        [SerializeField] private float _mouseSensitivity = 0.1f;
        
        [Tooltip("Height size of the Character")]
        [Range(0.1f, 8f)]
        [SerializeField] private float _characterSize = 2f;

        [Tooltip("Maximum vertical angle the player can look up or down.")]
        [Range(0, 90)]
        [SerializeField] private float _upDownLookRange = 80f;

        [Header ("References")] /*REFACTO : needs to be referenced automatically by GameInitializer*/
        [SerializeField] private CharacterController _characterController; 
        [SerializeField] private Camera _mainCamera;
        private PlayerInterface _playerInterface;

        private Vector3 _currentMovement;
        private float _verticalRotation;
        #endregion

        #region Methods
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        void Update()
        {
            if (!Mathf.Approximately(_characterController.height, _characterSize)) /*REFACTO : Cest uniquement pour du parametrage, a jarter assez vite*/
            {
                _characterController.height = _characterSize;
            }
            
            HandleMovement();
            HandleRotation();
        }

        private Vector3 CalculateWorldDirection()
        {
            Vector3 inputDirection = new Vector3(InputHandler.MovementInput.x, 0f, InputHandler.MovementInput.y);
            Vector3 worldDirection = transform.TransformDirection(inputDirection);
            return worldDirection.normalized;
        }
        
        private void HandleJumping()
        {
            if (_characterController.isGrounded)
            {
                _currentMovement.y = -0.5f;
            }
            else
            {
                _currentMovement.y += Physics.gravity.y * Time.deltaTime;
            }
        }

        private void HandleMovement()
        {
            Vector3 worldDirection = CalculateWorldDirection();
            _currentMovement.x = worldDirection.x * CurrentSpeed;
            _currentMovement.z = worldDirection.z * CurrentSpeed;

            HandleJumping();
            _characterController.Move(_currentMovement * Time.deltaTime);
        }

        private void ApplyHorizontalRotation(float rotationAmount)
        {
            transform.Rotate(0, rotationAmount, 0);
        }

        private void ApplyVerticalRotation(float rotationAmount)
        {
            _verticalRotation = Mathf.Clamp(_verticalRotation - rotationAmount, -_upDownLookRange, _upDownLookRange);
            _mainCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
        }

        private void HandleRotation()
        {
            float mouseXRotation = InputHandler.RotationInput.x * _mouseSensitivity;
            float mouseYRotation = InputHandler.RotationInput.y * _mouseSensitivity;

            ApplyHorizontalRotation(mouseXRotation);
            ApplyVerticalRotation(mouseYRotation);
        }
        #endregion
    }
}
