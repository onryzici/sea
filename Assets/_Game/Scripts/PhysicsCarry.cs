using System.Collections.Generic;
using UnityEngine;

namespace SalvageCrew
{
    // No local device APIs here: future host authority can call the same commands.
    [RequireComponent(typeof(FirstPersonMotor))]
    public sealed class PhysicsCarry : MonoBehaviour
    {
        [SerializeField] private Transform view;
        [SerializeField, Min(.1f)] private float interactionRange = 2.5f;
        [SerializeField, Min(.1f)] private float releaseDistance = 3.2f;
        [SerializeField, Min(0f)] private float obstructionGrace = .45f;
        [SerializeField, Min(.1f)] private float releaseSpeedLimit = 2f;
        [SerializeField, Min(.1f)] private float angularSpeedLimit = 3f;
        private FirstPersonMotor motor;
        private float obstructedTime;
        private RigidbodyInterpolation previousInterpolation;
        private CollisionDetectionMode previousDetection;
        private readonly List<(Collider item, Collider player, bool ignored)> ignoredPairs = new();
        public ScrapItem Held { get; private set; }
        public ScrapItem Target { get; private set; }
        public Transform View => view;

        private void Awake() { motor = GetComponent<FirstPersonMotor>(); }
        private void OnEnable() { GetComponent<FirstPersonMotor>().BeforeRespawn += Drop; }
        private void OnDisable()
        {
            Drop();
            GetComponent<FirstPersonMotor>().BeforeRespawn -= Drop;
        }
        private void Update() { Target = Held == null ? FindTarget() : null; }
        public ScrapItem FindTarget(bool includeHeld = false)
        {
            if (view == null) return null;
            foreach (var hit in SortedHits(view.position, view.forward, interactionRange))
            {
                if (hit.collider.transform.IsChildOf(transform)) continue;
                var item = hit.collider.GetComponentInParent<ScrapItem>();
                return item != null && (includeHeld || !item.IsHeld) ? item : null;
            }
            return null;
        }
        public void Toggle()
        {
            if (Held != null) Drop();
            else TryPickup(FindTarget());
        }
        public bool TryPickup(ScrapItem item)
        {
            // Commands are validated here as well, not only by the local input adapter.
            if (Held != null || item == null || FindTarget() != item || !item.TryClaim(this)) return false;
            Held = item;
            Target = null;
            var body = item.Body;
            previousInterpolation = body.interpolation;
            previousDetection = body.collisionDetectionMode;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            foreach (var collider in item.GetComponentsInChildren<Collider>())
                foreach (var player in GetComponentsInChildren<Collider>())
                {
                    ignoredPairs.Add((collider, player, Physics.GetIgnoreCollision(collider, player)));
                    Physics.IgnoreCollision(collider, player, true);
                }
            obstructedTime = 0f;
            motor.SetCarryMovement(item.MovementMultiplier, item.AllowSprint);
            body.WakeUp();
            return true;
        }
        private void FixedUpdate()
        {
            if (Held == null) return;
            var body = Held.Body;
            Vector3 toItem = body.worldCenterOfMass - view.position;
            if (toItem.magnitude > releaseDistance) { Drop(); return; }
            bool blocked = false;
            foreach (var hit in SortedHits(view.position, toItem.normalized, toItem.magnitude))
            {
                if (hit.collider.transform.IsChildOf(transform)) continue;
                if (hit.collider.GetComponentInParent<ScrapItem>() == Held) break;
                blocked = true; break;
            }
            obstructedTime = blocked ? obstructedTime + Time.fixedDeltaTime : 0f;
            if (obstructedTime > obstructionGrace) { Drop(); return; }

            Vector3 target = view.position + view.forward * Held.HoldDistance - Vector3.up * .25f;
            var boat = motor.Passenger != null ? motor.Passenger.Reference : null;
            Vector3 platformVelocity = boat != null ? boat.PointVelocity(target) : Vector3.zero;
            // Render/CC support follows interpolated poses; server forces use the matching physics pose.
            if (boat != null && boat.IsServer) target = boat.PhysicsPoint(boat.transform.InverseTransformPoint(target));
            // A capped PD force, with no accumulated integral or stored spring energy.
            Vector3 force = (target - body.worldCenterOfMass) * Held.SpringForce
                - (body.linearVelocity - platformVelocity) * Held.DampingForce;
            if (body.useGravity) force -= Physics.gravity * body.mass;
            body.AddForce(Vector3.ClampMagnitude(force, Held.MaximumForce), ForceMode.Force);
            body.linearVelocity = platformVelocity + Vector3.ClampMagnitude(body.linearVelocity - platformVelocity, Held.MaximumSpeed);
            body.angularVelocity = Vector3.ClampMagnitude(body.angularVelocity, angularSpeedLimit);
        }
        public void Drop()
        {
            if (Held == null) { if (motor != null) motor.SetCarryMovement(1f, true); return; }
            var item = Held;
            Held = null;
            var body = item.Body;
            body.interpolation = previousInterpolation;
            body.collisionDetectionMode = previousDetection;
            Vector3 platformVelocity = motor.Passenger != null ? motor.Passenger.PlatformVelocity : Vector3.zero;
            body.linearVelocity = platformVelocity + Vector3.ClampMagnitude(body.linearVelocity - platformVelocity, releaseSpeedLimit);
            body.angularVelocity = Vector3.ClampMagnitude(body.angularVelocity, angularSpeedLimit);
            foreach (var pair in ignoredPairs)
                if (pair.item != null && pair.player != null)
                    Physics.IgnoreCollision(pair.item, pair.player, pair.ignored);
            ignoredPairs.Clear();
            item.ReleaseClaim(this);
            motor.SetCarryMovement(1f, true);
            obstructedTime = 0f;
        }
        private static RaycastHit[] SortedHits(Vector3 origin, Vector3 direction, float distance)
        {
            var hits = Physics.RaycastAll(origin, direction, distance, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            return hits;
        }
    }
}
