using System;
using Unity.Netcode;
using UnityEngine;

namespace SalvageCrew
{
    public struct BoatPose : INetworkSerializable, IEquatable<BoatPose>
    {
        public Vector3 Position, Velocity;
        public float Yaw, TurnRate;
        public Quaternion Rotation;
        public Vector3 AngularVelocity;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        { s.SerializeValue(ref Position); s.SerializeValue(ref Velocity); s.SerializeValue(ref Yaw); s.SerializeValue(ref TurnRate);s.SerializeValue(ref Rotation);s.SerializeValue(ref AngularVelocity); }
        public bool Equals(BoatPose b) => Position == b.Position && Velocity == b.Velocity && Yaw == b.Yaw && TurnRate == b.TurnRate && Rotation==b.Rotation && AngularVelocity==b.AngularVelocity;
    }

    // One server-owned dynamic compound body. Client replicas render snapshots, never simulate propulsion.
    [DefaultExecutionOrder(-200), RequireComponent(typeof(NetworkObject), typeof(Rigidbody))]
    public sealed class NetworkBoat : NetworkBehaviour
    {
        public static NetworkBoat Instance { get; private set; }
        [SerializeField] private Transform helm;
        [Header("Calm-water prototype (metres, seconds, degrees)")]
        [SerializeField, Min(.1f)] private float maximumSpeed = 2.5f;
        [SerializeField, Min(.1f)] private float acceleration = .6f;
        [SerializeField, Min(.1f)] private float waterResistance = .5f;
        [SerializeField, Min(.1f)] private float lateralResistance = 2f;
        [SerializeField, Min(1f)] private float turnSpeed = 10f;
        [SerializeField, Min(1f)] private float turnAcceleration = 5f;
        [SerializeField] private float waterline = 0f;
        [SerializeField, Min(.1f)] private float buoyancySpring = 12f;
        [SerializeField, Min(.1f)] private float buoyancyDamping = 7f;
        [SerializeField, Min(.1f)] private float driverRange = 2.5f;
        [SerializeField, Min(.1f)] private float inputTimeout = .35f;
        [SerializeField, Min(1f)] private float interpolationRate = 14f;
        public readonly NetworkVariable<ulong> Driver = new(NetworkScrap.Nobody);
        public readonly NetworkVariable<bool> Departed = new(false);
        private readonly NetworkVariable<BoatPose> state = new();
        private Vector2 drive;
        private float lastInput = -100, lastAcceptedInput = -100, nextPublish, receivedAt;
        public Rigidbody Body { get; private set; }
        public Transform Helm => helm;
        public Vector2 Drive => drive;
        public float SteeringVisual => IsServer ? drive.x : Mathf.Clamp(state.Value.TurnRate / turnSpeed, -1, 1);
        public Vector3 Velocity => IsServer ? Body.linearVelocity : state.Value.Velocity;

        private void Awake() { Body = GetComponent<Rigidbody>(); Body.isKinematic = true; }
        public override void OnNetworkSpawn()
        {
            Instance = this;
            // Docking clamp: avoid the boarding ramp imparting impulses before departure.
            Body.isKinematic = !IsServer;
            Body.interpolation = IsServer ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
            Body.collisionDetectionMode = IsServer ? CollisionDetectionMode.ContinuousDynamic : CollisionDetectionMode.Discrete;
            if (IsServer)
            {
                var floating=GetComponent<CalmWaterBuoyancy>();
                if(floating!=null){var spawn=Body.position;spawn.y=floating.RestHeight;Body.position=spawn;}
                // Moored hull can heave/pitch/roll; the server alone simulates wave forces.
                Body.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationY;
                // Compound cabin/rails tilt principal inertia axes; use the calm-water hull approximation.
                Body.inertiaTensorRotation = Quaternion.identity;
                Body.inertiaTensor = Body.mass / 12f * new Vector3(101.44f, 116f, 17.44f);
                Publish(); NetworkManager.OnClientDisconnectCallback += Disconnected;
            }
            else Apply(state.Value);
            state.OnValueChanged += Changed; receivedAt = Time.unscaledTime;
        }
        private void Changed(BoatPose before, BoatPose after) { receivedAt = Time.unscaledTime; }
        private void Apply(BoatPose pose)
        { transform.SetPositionAndRotation(pose.Position, pose.Rotation); Physics.SyncTransforms(); }
        private void Update()
        {
            if (!IsSpawned || IsServer) return;
            float age = Mathf.Clamp(Time.unscaledTime - receivedAt, 0, .12f);
            float blend = 1 - Mathf.Exp(-interpolationRate * Time.deltaTime);
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, state.Value.Position + state.Value.Velocity * age, blend),
                Quaternion.Slerp(transform.rotation, Quaternion.AngleAxis(state.Value.AngularVelocity.magnitude*Mathf.Rad2Deg*age,state.Value.AngularVelocity.normalized)*state.Value.Rotation, blend));
            Physics.SyncTransforms();
        }
        private void FixedUpdate()
        {
            if (!IsSpawned || !IsServer) return;
            if (Driver.Value != NetworkScrap.Nobody && !NetworkManager.ConnectedClients.ContainsKey(Driver.Value)) ReleaseDriver();
            if (Driver.Value == NetworkScrap.Nobody || Time.unscaledTime - lastInput > inputTimeout) drive = Vector2.zero;
            if (drive.sqrMagnitude > .001f && !Departed.Value)
            { Departed.Value = true; HarborSession.Instance.SetRampActive(false); Body.constraints=RigidbodyConstraints.None;Body.isKinematic = false; Body.WakeUp(); }
            if (!Departed.Value){if(Time.unscaledTime>=nextPublish){nextPublish=Time.unscaledTime+.05f;Publish();}return;}
            Vector3 velocity = Body.linearVelocity;
            Vector3 forward = Body.rotation * Vector3.forward;
            Vector3 planar = Vector3.ProjectOnPlane(velocity, Vector3.up);
            Vector3 lateral = planar - forward * Vector3.Dot(planar, forward);
            float desired = drive.y * maximumSpeed;
            float thrust = Mathf.Clamp((desired - Vector3.Dot(planar, forward)) * 1.5f, -acceleration, acceleration);
            if (Mathf.Abs(drive.y) < .01f) thrust = 0;
            Vector3 force = forward * thrust - planar * waterResistance - lateral * lateralResistance;
            // Spring around the calm waterline; cancel gravity without using cargo-dependent buoyancy.
            force.y = GetComponent<CalmWaterBuoyancy>() != null ? 0 : -Physics.gravity.y + Mathf.Clamp((waterline - Body.position.y) * buoyancySpring - velocity.y * buoyancyDamping, -3, 3);
            Body.AddForce(force, ForceMode.Acceleration);
            float targetTurn = drive.x * turnSpeed * Mathf.Deg2Rad;
            float turnDelta = Mathf.Clamp(targetTurn - Body.angularVelocity.y, -turnAcceleration * Mathf.Deg2Rad * Time.fixedDeltaTime,
                turnAcceleration * Mathf.Deg2Rad * Time.fixedDeltaTime);
            Body.AddTorque(Vector3.up * turnDelta, ForceMode.VelocityChange);
            if (Time.unscaledTime >= nextPublish) { nextPublish = Time.unscaledTime + .05f; Publish(); }
        }
        private void Publish() => state.Value = new BoatPose { Position = Body.position, Yaw = Body.rotation.eulerAngles.y,
            Velocity = Body.linearVelocity, TurnRate = Body.angularVelocity.y * Mathf.Rad2Deg,Rotation=Body.rotation,AngularVelocity=Body.angularVelocity };
        public Vector3 PointVelocity(Vector3 point) => IsServer ? Body.GetPointVelocity(point)
            : state.Value.Velocity + Vector3.Cross(state.Value.AngularVelocity, point - transform.position);
        public Vector3 PhysicsPoint(Vector3 local) => Body.position + Body.rotation * local;
        public Vector3 RescuePoint(ulong client)
        {
            float side = client == 0 ? -.9f : .9f;
            foreach (var local in new[] { new Vector3(side,1.58f,-2.8f), new Vector3(-side,1.58f,-2.8f),
                new Vector3(side,1.58f,-1.2f), new Vector3(-side,1.58f,-1.2f), new Vector3(0,1.58f,-.2f) })
            {
                Vector3 point = transform.TransformPoint(local);
                bool occupied = false;
                foreach (var collider in Physics.OverlapCapsule(point + Vector3.up * .32f, point + Vector3.up * 1.5f, .28f, ~0, QueryTriggerInteraction.Ignore))
                { var player = collider.GetComponentInParent<NetworkCrewPlayer>(); if (player == null || player.OwnerClientId != client) { occupied = true; break; } }
                if (!occupied) return point;
            }
            // Congested-deck fallback is above the safe aft area, never inside the hull.
            return transform.TransformPoint(new Vector3(side, 3.2f, -2.8f));
        }
        public Vector3 ScrapRescuePoint(ScrapItem item)
        {
            float side = item.Mass < 5 ? -1.1f : item.Mass < 20 ? 0 : 1.1f;
            foreach (float z in new[] { -1.5f, -3.6f, -.1f })
            {
                Vector3 point = PhysicsPoint(new Vector3(side, 2.2f, z)); bool occupied = false;
                foreach (var collider in Physics.OverlapBox(point, new Vector3(.65f,.4f,.65f), Body.rotation, ~0, QueryTriggerInteraction.Ignore))
                    if (collider.GetComponentInParent<ScrapItem>() != item) { occupied = true; break; }
                if (!occupied) return point;
            }
            return PhysicsPoint(new Vector3(side, 4.4f, -3.6f));
        }
        public bool ContainsPassenger(Vector3 local) => Mathf.Abs(local.x) <= 3 && Mathf.Abs(local.z) <= 6 && local.y > .6f && local.y < 8;
        public bool CanSeeHelm(PhysicsCarry carry)
        {
            if (carry == null || helm == null || Vector3.Distance(carry.View.position, helm.position) > driverRange) return false;
            var hits = Physics.RaycastAll(carry.View.position, carry.View.forward, driverRange, ~0, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (a,b) => a.distance.CompareTo(b.distance));
            var held = carry.Held != null ? carry.Held : carry.GetComponent<NetworkCrewPlayer>()?.Held?.Item;
            foreach (var hit in hits)
            {
                if (hit.collider.transform.IsChildOf(carry.transform)) continue;
                // A carrier's own held body must not hide the helm rejection prompt.
                if (held != null && hit.collider.GetComponentInParent<ScrapItem>() == held) continue;
                return hit.collider.transform == helm || hit.collider.transform.IsChildOf(helm);
            }
            return false;
        }
        public void RequestHelm() => HelmRequestRpc();
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void HelmRequestRpc(RpcParams rpc = default)
        {
            if (!IsServer || !NetworkManager.ConnectedClients.TryGetValue(rpc.Receive.SenderClientId, out var client)) return;
            var crew = client.PlayerObject != null ? client.PlayerObject.GetComponent<NetworkCrewPlayer>() : null;
            if (crew == null) return;
            if (Driver.Value == crew.OwnerClientId) { ReleaseDriver(); crew.SendFeedback(""); return; }
            if (crew.Carry.Held != null) { crew.SendFeedback("Dümen için önce hurdayı bırak."); return; }
            if (Driver.Value != NetworkScrap.Nobody) { crew.SendFeedback("Dümeni başka bir oyuncu kullanıyor."); return; }
            if (!crew.FreshPose || !CanSeeHelm(crew.Carry)) { crew.SendFeedback("Dümen menzil dışında veya görüş engelli."); return; }
            Driver.Value = crew.OwnerClientId; drive = Vector2.zero; lastInput = Time.unscaledTime;
            crew.SendFeedback("");
        }
        public void SubmitDrive(Vector2 input) => DriveRequestRpc(input);
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone, Delivery = RpcDelivery.Unreliable)]
        private void DriveRequestRpc(Vector2 input, RpcParams rpc = default)
        {
            if (!IsServer || rpc.Receive.SenderClientId != Driver.Value || Time.unscaledTime - lastAcceptedInput < .07f) return;
            if (float.IsNaN(input.x) || float.IsNaN(input.y) || float.IsInfinity(input.x) || float.IsInfinity(input.y)) return;
            if (!NetworkManager.ConnectedClients.TryGetValue(Driver.Value, out var client) || client.PlayerObject == null) { ReleaseDriver(); return; }
            var crew = client.PlayerObject.GetComponent<NetworkCrewPlayer>();
            if (!crew.FreshPose || Vector3.Distance(crew.transform.position, helm.position) > driverRange + 1) { ReleaseDriver(); return; }
            drive = Vector2.ClampMagnitude(input, 1); lastInput = lastAcceptedInput = Time.unscaledTime;
        }
        public void ReleaseFor(ulong client) { if (IsServer && Driver.Value == client) ReleaseDriver(); }
        private void ReleaseDriver() { Driver.Value = NetworkScrap.Nobody; drive = Vector2.zero; lastAcceptedInput = -100; }
        private void Disconnected(ulong id) => ReleaseFor(id);
        public override void OnNetworkDespawn()
        { if (IsServer) NetworkManager.OnClientDisconnectCallback -= Disconnected; if (Instance == this) Instance = null; }
    }
}
