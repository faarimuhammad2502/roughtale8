using UnityEngine;
using UnityEngine.InputSystem;

namespace Roughtale8.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class EidonPlayerController : MonoBehaviour
    {
        [SerializeField] private float walkSpeed = 4.25f;
        [SerializeField] private float jogSpeed = 6.85f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private Transform cameraTransform;

        private CharacterController controller;
        private float verticalVelocity;
        private bool isMovementLocked;

        public bool IsJogging { get; private set; }
        public Vector3 Velocity => controller != null ? controller.velocity : Vector3.zero;

        public void SetCameraTransform(Transform value)
        {
            cameraTransform = value;
        }

        public void SetMovementLocked(bool locked)
        {
            isMovementLocked = locked;
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (isMovementLocked)
            {
                if (controller.isGrounded && verticalVelocity < 0f)
                {
                    verticalVelocity = -2f;
                }
                verticalVelocity += gravity * Time.deltaTime;
                controller.Move(Vector3.up * (verticalVelocity * Time.deltaTime));
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null || cameraTransform == null)
            {
                return;
            }

            var input = Vector2.zero;
            if (keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed) input.y -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;
            input = Vector2.ClampMagnitude(input, 1f);

            IsJogging = keyboard.leftShiftKey.isPressed && input.sqrMagnitude > 0.01f;
            var currentSpeed = IsJogging ? jogSpeed : walkSpeed;

            var forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            var right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            var planarMove = (forward * input.y) + (right * input.x);

            if (planarMove.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(planarMove), 14f * Time.deltaTime);
            }

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;
            controller.Move(((planarMove * currentSpeed) + (Vector3.up * verticalVelocity)) * Time.deltaTime);
        }
    }
}
