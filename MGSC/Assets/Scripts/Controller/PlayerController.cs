using UnityEngine;

namespace Controller
{
    /* PlayerController applies the supposed actions linked to the inputs on the player Prefab */
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Parameters")]
        [Tooltip("Base movement speed when walking.")]
        [Range(1f, 30f)]
        [SerializeField] private float walkSpeed = 3.0f;

        [Header("Look Parameters")]
        [Tooltip("Sensitivity of the mouse when looking around.")]
        [Range(0.01f, 2f)]
        [SerializeField] private float mouseSensitivity = 0.1f;

        [Tooltip("Maximum vertical angle the player can look up or down.")]
        [Range(0, 90)]
        [SerializeField] private float upDownLookRange = 80f;

        [Header ("References")] /*REFACTO : needs to be referenced automatically by GameInitializer*/
        [SerializeField] private CharacterController characterController; 
        [SerializeField] private Camera mainCamera;
        [SerializeField] private PlayerInputHandler playerInputHandler;

        private Vector3 currentMovement;
        private float verticalRotation;
        private float CurrentSpeed => walkSpeed ;
        
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        void Update()
        {
            HandleMovement();
            HandleRotation();
        }

        private Vector3 CalculateWorldDirection()
        {
            Vector3 inputDirection = new Vector3(playerInputHandler.MovementInput.x, 0f, playerInputHandler.MovementInput.y);
            Vector3 worldDirection = transform.TransformDirection(inputDirection);
            return worldDirection.normalized;
        }
        
        private void HandleJumping()
        {
            if (characterController.isGrounded)
            {
                currentMovement.y = -0.5f;
            }
            else
            {
                currentMovement.y += Physics.gravity.y * Time.deltaTime;
            }
        }

        private void HandleMovement()
        {
            Vector3 worldDirection = CalculateWorldDirection();
            currentMovement.x = worldDirection.x * CurrentSpeed;
            currentMovement.z = worldDirection.z * CurrentSpeed;

            HandleJumping();
            characterController.Move(currentMovement * Time.deltaTime);
        }

        private void ApplyHorizontalRotation(float rotationAmount)
        {
            transform.Rotate(0, rotationAmount, 0);
        }

        private void ApplyVerticalRotation(float rotationAmount)
        {
            verticalRotation = Mathf.Clamp(verticalRotation - rotationAmount, -upDownLookRange, upDownLookRange);
            mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }

        private void HandleRotation()
        {
            float mouseXRotation = playerInputHandler.RotationInput.x * mouseSensitivity;
            float mouseYRotation = playerInputHandler.RotationInput.y * mouseSensitivity;

            ApplyHorizontalRotation(mouseXRotation);
            ApplyVerticalRotation(mouseYRotation);
        }

    }
}
