using Unity.Netcode;
using UnityEngine;

namespace SalvageCrew
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class NetworkWreckExpedition : NetworkBehaviour
    {
        public readonly NetworkVariable<int> Discovered = new();
        public readonly NetworkVariable<int> Secured = new();
        public readonly NetworkVariable<bool> Complete = new();
        private float nextLocalRequest;
        public override void OnNetworkSpawn() { WreckExpedition.Instance.Attach(this); }
        public void RequestScan()
        {
            if (!IsSpawned || Time.unscaledTime < nextLocalRequest) return;
            nextLocalRequest = Time.unscaledTime + .5f; ScanRpc();
        }
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ScanRpc(RpcParams rpc = default)
        {
            if (!IsServer || !NetworkManager.ConnectedClients.TryGetValue(rpc.Receive.SenderClientId, out var client)
                || client.PlayerObject == null) return;
            var player = client.PlayerObject.GetComponent<NetworkCrewPlayer>();
            if (player == null || !player.FreshPose) return;
            string message = WreckExpedition.Instance.Scan(player.transform);
            FeedbackRpc(message, RpcTarget.Single(rpc.Receive.SenderClientId, RpcTargetUse.Temp));
        }
        [Rpc(SendTo.SpecifiedInParams)]
        private void FeedbackRpc(string message, RpcParams rpc = default) { WreckExpedition.Instance?.SetFeedback(message); }
        public override void OnNetworkDespawn() { WreckExpedition.Instance?.EndSession(); }
    }
}
