using UnityEngine;

namespace SalvageCrew
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ScrapItem : MonoBehaviour
    {
        [SerializeField] private string displayName = "Hurda";
        [SerializeField, Min(.1f)] private float mass = 3f;
        [Header("Carry: bounded physical spring")]
        [SerializeField, Min(1f)] private float springForce = 180f;
        [SerializeField, Min(1f)] private float dampingForce = 32f;
        [SerializeField, Min(1f)] private float maximumForce = 180f;
        [SerializeField, Min(.1f)] private float maximumSpeed = 5f;
        [SerializeField, Min(.1f)] private float holdDistance = 1.5f;
        [SerializeField, Range(.2f, 1f)] private float movementMultiplier = .95f;
        [SerializeField] private bool allowSprint = true;
        [Header("Recovery: visual sea only")]
        [SerializeField] private float rescueHeight = .15f;
        [SerializeField, Min(.1f)] private float rescueDelay = 1.5f;
        [SerializeField, Min(1f)] private float rescueDistance = 100f;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private float submergedTime;
        private PhysicsCarry owner;
        public Rigidbody Body { get; private set; }
        public string DisplayName => displayName;
        public float Mass => mass;
        public float SpringForce => springForce;
        public float DampingForce => dampingForce;
        public float MaximumForce => maximumForce;
        public float MaximumSpeed => maximumSpeed;
        public float HoldDistance => holdDistance;
        public float MovementMultiplier => movementMultiplier;
        public bool AllowSprint => allowSprint;
        public bool IsHeld => owner != null;
        public PhysicsCarry Holder => owner;

        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
            Body.mass = mass;
            spawnPosition = Body.position;
            spawnRotation = Body.rotation;
        }
        private void OnValidate()
        {
            mass = Mathf.Max(.1f, mass);
            GetComponent<Rigidbody>().mass = mass;
        }
        public bool TryClaim(PhysicsCarry holder)
        {
            if (owner != null || holder == null || !isActiveAndEnabled || Body.isKinematic) return false;
            owner = holder;
            return true;
        }
        public void ReleaseClaim(PhysicsCarry holder) { if (owner == holder) owner = null; }
        private void FixedUpdate()
        {
            bool outside = Body.position.y < rescueHeight
                || Vector3.Distance(Body.position, spawnPosition) > rescueDistance;
            submergedTime = outside ? submergedTime + Time.fixedDeltaTime : 0f;
            if (submergedTime >= rescueDelay) Recover();
        }
        public void Recover()
        {
            if (owner != null) owner.Drop();
            Body.position = spawnPosition;
            Body.rotation = spawnRotation;
            Body.linearVelocity = Vector3.zero;
            Body.angularVelocity = Vector3.zero;
            submergedTime = 0f;
            // Let gravity settle the recovered body if its spawn was slightly above the pier.
            Body.WakeUp();
        }
        private void OnDisable() { if (owner != null) owner.Drop(); }
    }
}
