using UnityEngine;

namespace SalvageCrew
{
    [RequireComponent(typeof(CharacterController), typeof(LocalPlayerInput))]
    public sealed class FirstPersonMotor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Transform spawnPoint;
        [Header("Movement (metres / seconds)")]
        [SerializeField, Min(0.1f)] private float walkSpeed = 4f;
        [SerializeField, Min(0.1f)] private float sprintSpeed = 6.5f;
        [SerializeField, Min(0f)] private float jumpHeight = 1f;
        [SerializeField, Min(0.1f)] private float gravity = 20f;
        [Header("Look (degrees / mouse pixel)")]
        [SerializeField, Min(0.001f)] private float mouseSensitivity = 0.12f;
        [SerializeField, Range(10f, 89f)] private float pitchLimit = 80f;
        [Header("Recovery")]
        [SerializeField] private float rescueHeight = 0.2f;

        private CharacterController controller;
        private LocalPlayerInput input;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private float verticalSpeed, pitch;
        public float VerticalSpeed => verticalSpeed;
        public bool Grounded => controller != null && controller.isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<LocalPlayerInput>();
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        private void Update()
        {
            var sample = input.Read();
            if (sample.Reset || transform.position.y < rescueHeight)
            {
                ReturnToSpawn();
                return;
            }
            Step(sample, Time.deltaTime);
        }

        public void Step(LocalPlayerInput.Sample sample, float deltaTime)
        {
            if (deltaTime <= 0f) return;
            // Mouse delta is already a per-frame displacement: do not multiply by deltaTime.
            transform.Rotate(0f, sample.Look.x * mouseSensitivity, 0f);
            pitch = Mathf.Clamp(pitch - sample.Look.y * mouseSensitivity, -pitchLimit, pitchLimit);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            bool grounded = controller.isGrounded;
            if (grounded && verticalSpeed < 0f) verticalSpeed = -2f;
            if (grounded && sample.Jump) verticalSpeed = Mathf.Sqrt(2f * gravity * jumpHeight);
            verticalSpeed = Mathf.Max(verticalSpeed - gravity * deltaTime, -40f);
            Vector2 movement = Vector2.ClampMagnitude(sample.Move, 1f);
            Vector3 planar = (transform.right * movement.x + transform.forward * movement.y)
                * (sample.Sprint ? sprintSpeed : walkSpeed);
            CollisionFlags flags = controller.Move((planar + Vector3.up * verticalSpeed) * deltaTime);
            if ((flags & CollisionFlags.Above) != 0 && verticalSpeed > 0f) verticalSpeed = 0f;
            if (transform.position.y < rescueHeight) ReturnToSpawn();
        }

        public void ReturnToSpawn()
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(spawnPoint != null ? spawnPoint.position : initialPosition,
                spawnPoint != null ? spawnPoint.rotation : initialRotation);
            verticalSpeed = 0f;
            pitch = 0f;
            cameraPivot.localRotation = Quaternion.identity;
            controller.enabled = true;
            Physics.SyncTransforms();
        }
    }
}
