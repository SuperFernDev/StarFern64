using KBCore.Refs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RailShooter
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputReader : ValidatedMonoBehaviour
    {
        [SerializeField, Self] PlayerInput playerInput;
        [SerializeField] float doubleTapTime = 0.5f;

        InputAction moveAction;
        InputAction aimAction;
        InputAction fireAction;
        InputAction rollAction;
        InputAction pauseAction;

        float lastMoveTime;
        float lastMoveDirection;

        public event Action LeftTap;
        public event Action RightTap;
        public event Action Fire;
        public event Action Roll;
        public event Action Pause;

        public Vector2 Move => moveAction.ReadValue<Vector2>();
        public Vector2 Aim => aimAction.ReadValue<Vector2>();

        void Awake()
        {
            moveAction = playerInput.actions["Move"];
            aimAction = playerInput.actions["Aim"];
            fireAction = playerInput.actions["Fire"];
            rollAction = playerInput.actions["Roll"];
            pauseAction = playerInput.actions["Pause"];
        }

        void OnEnable()
        {
            moveAction.performed += OnMovePerformed;
            fireAction.performed += OnFire;
            rollAction.performed += OnRoll;
            pauseAction.performed += OnPause;
        }

        void OnDisable()
        {
            moveAction.performed -= OnMovePerformed;
            fireAction.performed -= OnFire;
            rollAction.performed -= OnRoll;
            pauseAction.performed -= OnPause;
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            float currentDirection = Move.x;

            if (Time.time - lastMoveTime < doubleTapTime && playerInput.currentControlScheme != "Gamepad" && currentDirection == lastMoveDirection)
            {
                if (currentDirection < 0)
                {
                    LeftTap?.Invoke();
                }
                else if (currentDirection > 0)
                {
                    RightTap?.Invoke();
                }
            }

            lastMoveTime = Time.time;
            lastMoveDirection = currentDirection;
        }
        private void OnFire(InputAction.CallbackContext context) => Fire?.Invoke();
        private void OnRoll(InputAction.CallbackContext context) => Roll?.Invoke();
        private void OnPause(InputAction.CallbackContext context) => Pause?.Invoke();

    }
}
