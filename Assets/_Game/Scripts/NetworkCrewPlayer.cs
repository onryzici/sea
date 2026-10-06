using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace SalvageCrew
{
    public struct CrewPose : INetworkSerializable, IEquatable<CrewPose>
    {
        public Vector3 Position;
        public float Yaw, Pitch;
        public uint Epoch;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        { serializer.SerializeValue(ref Position); serializer.SerializeValue(ref Yaw); serializer.SerializeValue(ref Pitch); serializer.SerializeValue(ref Epoch); }
        public bool Equals(CrewPose other) => Position == other.Position && Yaw == other.Yaw && Pitch == other.Pitch && Epoch == other.Epoch;
    }

    // Local CharacterController prediction, server-validated 20 Hz pose replication.
    // This is a co-op prototype, not a complete prediction/reconciliation or anti-cheat system.
    public sealed class NetworkCrewPlayer : NetworkBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private GameObject remoteVisual;
        [SerializeField] private GameObject localHud;
        [SerializeField] private Transform lookMarker;
        [SerializeField, Min(1f)] private float sendRate = 20;
        [SerializeField, Min(.01f)] private float commandInterval = .10f;
        [SerializeField, Min(.1f)] private float poseTimeout = 1.5f;
        private readonly NetworkVariable<CrewPose> pose = new();
        public readonly NetworkVariable<ulong> HeldId = new(NetworkScrap.Nobody);
        private FirstPersonMotor motor;
        private LocalPlayerInput input;
        private PhysicsCarry carry;
        private CharacterController controller;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private float nextSend, lastPoseTime, lastCommandTime = -100;
        private float lastReceivedPose = -100;
        private uint epoch;
        private bool resetPending;
        private readonly List<(Collider item, Collider player, bool ignored)> localIgnoredPairs = new();
        public string Feedback { get; private set; } = "";
        public PhysicsCarry Carry => carry;
        public FirstPersonMotor Motor => motor;
        public LocalPlayerInput Input => input;
        public NetworkScrap Held => Resolve(HeldId.Value);
        public NetworkScrap Target => carry.FindTarget(true)?.GetComponent<NetworkScrap>();

        private void Awake()
        {
            motor = GetComponent<FirstPersonMotor>(); input = GetComponent<LocalPlayerInput>();
            carry = GetComponent<PhysicsCarry>(); controller = GetComponent<CharacterController>();
            motor.enabled = input.enabled = carry.enabled = false;
            GetComponent<LocalCarryInteraction>().enabled = false;
            playerCamera.enabled = false; playerCamera.GetComponent<AudioListener>().enabled = false;
            localHud.SetActive(false); controller.enabled = false;
        }
        public override void OnNetworkSpawn()
        {
            spawnPosition = transform.position; spawnRotation = transform.rotation;
            motor.ConfigureSpawn(spawnPosition, spawnRotation);
            controller.enabled = IsOwner || IsServer;
            carry.enabled = IsServer;
            motor.enabled = input.enabled = IsOwner;
            playerCamera.enabled = IsOwner; playerCamera.GetComponent<AudioListener>().enabled = IsOwner;
            localHud.SetActive(IsOwner); remoteVisual.SetActive(!IsOwner);
            var color = OwnerClientId == NetworkManager.ServerClientId ? new Color(.2f, .7f, .9f) : new Color(.95f, .55f, .2f);
            foreach (var renderer in remoteVisual.GetComponentsInChildren<Renderer>()) renderer.material.color = color;
            if (IsServer)
            {
                pose.Value = new CrewPose { Position = spawnPosition, Yaw = spawnRotation.eulerAngles.y };
                lastPoseTime = Time.unscaledTime;
            }
            HeldId.OnValueChanged += HeldChanged;
            if (IsOwner)
            {
                motor.InputSampled += Sample;
                motor.RespawnGuard = RequestRescue;
                HarborSession.Instance.BindLocal(this);
            }
            ApplyMovement();
        }
        private void Sample(LocalPlayerInput.Sample sample) { if (sample.Interact && input.GameplayActive) RequestToggle(); }
        public void RequestToggle()
        {
            if (!IsSpawned || !IsOwner || resetPending) return;
            var target = Target;
            CarryRequestRpc(HeldId.Value != NetworkScrap.Nobody, target != null ? target.NetworkObjectId : NetworkScrap.Nobody);
        }
        private bool RequestRescue()
        {
            if (!resetPending) { resetPending = true; RescueRequestRpc(); }
            return false;
        }
        private void LateUpdate()
        {
            if (!IsSpawned) return;
            if (IsOwner && !resetPending && Time.unscaledTime >= nextSend)
            {
                nextSend = Time.unscaledTime + 1f / Mathf.Clamp(sendRate, 5, 20);
                float pitch = Mathf.Asin(Mathf.Clamp(-carry.View.forward.y, -1, 1)) * Mathf.Rad2Deg;
                SubmitPoseRpc(transform.position, transform.eulerAngles.y, pitch, epoch);
            }
            if (!IsOwner && !IsServer)
            {
                float blend = 1f - Mathf.Exp(-18f * Time.deltaTime);
                if (Vector3.Distance(transform.position, pose.Value.Position) > 3)
                    transform.position = pose.Value.Position;
                else transform.position = Vector3.Lerp(transform.position, pose.Value.Position, blend);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, pose.Value.Yaw, 0), blend);
                cameraPivot.localRotation = Quaternion.Euler(pose.Value.Pitch, 0, 0);
            }
            lookMarker.localRotation = Quaternion.Euler(pose.Value.Pitch, 0, 0);
            if (IsServer)
            {
                HeldId.Value = carry.Held != null ? carry.Held.GetComponent<NetworkObject>().NetworkObjectId : NetworkScrap.Nobody;
                if (carry.Held != null && Time.unscaledTime - lastPoseTime > poseTimeout) carry.Drop();
                if (transform.position.y < .2f && !resetPending) RescueServer();
            }
        }
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner, Delivery = RpcDelivery.Unreliable)]
        private void SubmitPoseRpc(Vector3 position, float yaw, float pitch, uint sentEpoch, RpcParams rpc = default)
        {
            if (!IsServer || rpc.Receive.SenderClientId != OwnerClientId || sentEpoch != epoch) return;
            float now = Time.unscaledTime;
            if (now - lastReceivedPose < .035f) return;
            lastReceivedPose = now;
            if (!Finite(position.x) || !Finite(position.y) || !Finite(position.z) || !Finite(yaw) || !Finite(pitch)) return;
            float dt = Mathf.Clamp(now - lastPoseTime, .02f, .5f);
            var delta = position - pose.Value.Position;
            float speed = carry.Held != null ? 6.5f * carry.Held.MovementMultiplier : 6.5f;
            if (carry.Held != null && !carry.Held.AllowSprint) speed = 4f * carry.Held.MovementMultiplier;
            bool valid = new Vector2(delta.x, delta.z).magnitude <= speed * dt + .22f
                && Mathf.Abs(delta.y) <= 40 * dt + .3f && Mathf.Abs(position.x) < 50
                && position.z > -40 && position.z < 50 && position.y > -15 && position.y < 20;
            if (!valid) { CorrectOwnerRpc(pose.Value.Position, pose.Value.Yaw); return; }
            lastPoseTime = now;
            if (!IsOwner)
            {
                controller.Move(position - transform.position);
                position = transform.position;
                transform.rotation = Quaternion.Euler(0, yaw, 0);
                cameraPivot.localRotation = Quaternion.Euler(Mathf.Clamp(pitch, -80, 80), 0, 0);
            }
            pose.Value = new CrewPose { Position = position, Yaw = yaw, Pitch = Mathf.Clamp(pitch, -80, 80), Epoch = epoch };
        }
        [Rpc(SendTo.Owner)]
        private void CorrectOwnerRpc(Vector3 position, float yaw)
        {
            if (!IsOwner) return;
            controller.enabled = false; transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            controller.enabled = true;
        }
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void CarryRequestRpc(bool drop, ulong targetId, RpcParams rpc = default)
        {
            if (!IsServer || rpc.Receive.SenderClientId != OwnerClientId) return;
            if (drop) { carry.Drop(); HeldId.Value = NetworkScrap.Nobody; ReplyRpc(""); return; }
            if (Time.unscaledTime - lastCommandTime < commandInterval) { ReplyRpc("İstek çok hızlı; yeniden dene."); return; }
            lastCommandTime = Time.unscaledTime;
            if (carry.Held != null) { ReplyRpc("Zaten bir eşya taşıyorsun."); return; }
            var item = Resolve(targetId);
            if (item == null || Time.unscaledTime - lastPoseTime > poseTimeout || carry.FindTarget(true) != item.Item)
            { ReplyRpc("Hurda menzil dışında veya görüş engelli."); return; }
            if (item.Item.IsHeld) { ReplyRpc("Başka bir oyuncu taşıyor"); return; }
            if (!carry.TryPickup(item.Item)) { ReplyRpc("Tutma isteği reddedildi."); return; }
            HeldId.Value = targetId; item.Holder.Value = OwnerClientId; ReplyRpc("");
        }
        [Rpc(SendTo.Owner)]
        private void ReplyRpc(string text) { Feedback = text; ApplyMovement(); }
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void RescueRequestRpc(RpcParams rpc = default)
        { if (IsServer && rpc.Receive.SenderClientId == OwnerClientId) RescueServer(); }
        private void RescueServer()
        {
            carry.Drop(); HeldId.Value = NetworkScrap.Nobody;
            epoch++;
            motor.CompleteRespawn();
            pose.Value = new CrewPose { Position = spawnPosition, Yaw = spawnRotation.eulerAngles.y, Epoch = epoch };
            lastPoseTime = Time.unscaledTime;
            RescueOwnerRpc(epoch);
        }
        [Rpc(SendTo.Owner)]
        private void RescueOwnerRpc(uint newEpoch)
        {
            epoch = newEpoch; resetPending = false;
            motor.CompleteRespawn(); motor.SetCarryMovement(1, true); Feedback = "";
        }
        private void HeldChanged(ulong before, ulong after) { ApplyMovement(); }
        private void ApplyMovement()
        {
            if (!IsOwner) return;
            var held = Held;
            motor.SetCarryMovement(held != null ? held.Item.MovementMultiplier : 1, held == null || held.Item.AllowSprint);
            // NetworkRigidbody replicas are kinematic, but must not obstruct their carrier's CC.
            // The server's PhysicsCarry already handles its own collision pairs.
            if (!IsServer)
            {
                RestoreLocalCollisions();
                if (held != null)
                    foreach (var itemCollider in held.GetComponentsInChildren<Collider>())
                        foreach (var playerCollider in GetComponentsInChildren<Collider>())
                        {
                            localIgnoredPairs.Add((itemCollider, playerCollider, Physics.GetIgnoreCollision(itemCollider, playerCollider)));
                            Physics.IgnoreCollision(itemCollider, playerCollider, true);
                        }
            }
        }
        private void RestoreLocalCollisions()
        {
            foreach (var pair in localIgnoredPairs)
                if (pair.item != null && pair.player != null) Physics.IgnoreCollision(pair.item, pair.player, pair.ignored);
            localIgnoredPairs.Clear();
        }
        private NetworkScrap Resolve(ulong id)
        {
            return IsSpawned && NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(id, out var obj)
                ? obj.GetComponent<NetworkScrap>() : null;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public override void OnNetworkDespawn()
        {
            carry.Drop(); HeldId.OnValueChanged -= HeldChanged;
            RestoreLocalCollisions();
            motor.InputSampled -= Sample; motor.RespawnGuard = null;
        }
    }
}
