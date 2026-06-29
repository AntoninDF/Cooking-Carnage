using System;
using UnityEngine;

namespace Features
{
    public class PlayerMovement : Feature
    {
        public float WalkSpeed { get; set; } = 3.0f;
        public float LookSpeed { get; set; } = 0.1f;
        public float VerticalLookRange { get; set; } = 80f;
        public float CharacterSize { get; set; } = 2f;
        public Vector2 MovementDirection { get; set; }
        public Vector3 LookDirection
        {
            get => _lookDirection;
            set
            {
                _lookDirection = value;
                HandleRotation();
            }
        }
        public Vector2 LookOffset
        { 
            get => _lastLookOffset; 
            set
            {
                _lastLookOffset = value;
                UpdateLookDirectionFromOffset(value);
            }
        }
        public Transform ControlledTransform { get; set; }
        public Transform HeadTransform { get; set; }
        public CharacterController CharacterController { get; set; }

        public Action OnStartWalking { get; set; }
        public Action OnStopWalking { get; set; }

        private Vector3 _currentMovement;
        private Vector3 _lookDirection = Vector3.forward;
        private Vector2 _lastLookOffset;
        private float _verticalRotation;
        private bool isMoving;

        public PlayerMovement(IFeatureCollection owner, Transform controlledTransform, Transform headTransform, CharacterController characterController) : base(owner)
        {
            CharacterController = characterController;
            ControlledTransform = controlledTransform;
            HeadTransform = headTransform;
        }

        public override void Update()
        {
            if (!Globals.Gameplay.Player.AllowControllerMovement) return;
            if (!Mathf.Approximately(CharacterController.height, CharacterSize)) /*REFACTO : Cest uniquement pour du parametrage, a jarter assez vite*/
            {
                CharacterController.height = CharacterSize;
            }

            HandleMovement();
            HandleRotation();
        }

        private Vector3 CalculateWorldDirection()
        {
            Vector3 inputDirection = new Vector3(MovementDirection.x, 0f, MovementDirection.y);
            Vector3 worldDirection = ControlledTransform.TransformDirection(inputDirection);
            return worldDirection.normalized;
        }


        private void HandleMovement()
        {
            Vector3 worldDirection = CalculateWorldDirection();

            _currentMovement.x = worldDirection.x * WalkSpeed;
            _currentMovement.z = worldDirection.z * WalkSpeed;

            CharacterController.Move(_currentMovement * Time.deltaTime);

            if (!isMoving)
            {
                if (CharacterController.velocity.magnitude >= WalkSpeed * 0.75)
                {
                    isMoving = true;
                    OnStartWalking?.Invoke();
                }
            }
            else
            {
                if (CharacterController.velocity.magnitude < WalkSpeed * 0.75)
                {
                    isMoving = false;
                    OnStopWalking?.Invoke();
                }
            }
        }

        /*
        private void ApplyHorizontalRotation(float rotationAmount)
        {
            ControlledTransform.Rotate(0, rotationAmount, 0);
        }

        private void ApplyVerticalRotation(float rotationAmount)
        {
            _verticalRotation = Mathf.Clamp(_verticalRotation - rotationAmount, -VerticalLookRange, VerticalLookRange);
            if (HeadTransform != null)
                HeadTransform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
        }
        */
        private void ApplyLookDirection(Vector3 direction)
        {
            var yawDirection = new Vector3(direction.x, 0, direction.z); 
            
            if(yawDirection.sqrMagnitude > 0.01f)
                ControlledTransform.rotation = Quaternion.LookRotation(yawDirection, Vector3.up);
            
            if (HeadTransform != null)
                HeadTransform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
        }

        private void UpdateLookDirectionFromOffset(Vector2 offset)
        {
            if (!Globals.Gameplay.Player.AllowControllerMovement) return;
            LookDirection = Quaternion.Euler(0, offset.x * LookSpeed, 0) * LookDirection;
            _verticalRotation = Mathf.Clamp(_verticalRotation - offset.y *LookSpeed, -VerticalLookRange, VerticalLookRange);
        }
        private void HandleRotation()
        {
            if (!Globals.Gameplay.Player.AllowControllerMovement) return;
            ApplyLookDirection(LookDirection);
        }
    }
}