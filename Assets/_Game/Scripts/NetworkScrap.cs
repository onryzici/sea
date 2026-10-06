using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace SalvageCrew
{
    [RequireComponent(typeof(NetworkObject), typeof(NetworkTransform), typeof(NetworkRigidbody))]
    public sealed class NetworkScrap : NetworkBehaviour
    {
        public const ulong Nobody = ulong.MaxValue;
        public readonly NetworkVariable<ulong> Holder = new(Nobody);
        public ScrapItem Item { get; private set; }
        private void Awake() { Item = GetComponent<ScrapItem>(); }
        public override void OnNetworkSpawn()
        {
            Item.enabled = IsServer;
            if (!IsServer) Item.Body.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }
        private void LateUpdate()
        {
            if (!IsSpawned || !IsServer) return;
            var holder = Item.Holder != null ? Item.Holder.GetComponent<NetworkCrewPlayer>() : null;
            Holder.Value = holder != null && holder.IsSpawned ? holder.OwnerClientId : Nobody;
        }
    }
}
