using System.Collections.Generic;
using System.Net;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SalvageCrew
{
    [RequireComponent(typeof(NetworkManager), typeof(UnityTransport))]
    public sealed class HarborSession : MonoBehaviour
    {
        public static HarborSession Instance { get; private set; }
        [SerializeField] private GameObject offlinePlayer;
        [SerializeField] private GameObject offlineHud;
        [SerializeField] private Transform offlineScraps;
        [SerializeField] private GameObject[] scrapPrefabs;
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_InputField address;
        [SerializeField] private TMP_InputField port;
        [SerializeField] private TextMeshProUGUI statusLabel;
        [SerializeField] private TextMeshProUGUI roleLabel;
        [SerializeField] private UnityEngine.UI.Button hostButton;
        [SerializeField] private UnityEngine.UI.Button clientButton;
        [SerializeField] private UnityEngine.UI.Button disconnectButton;
        [SerializeField] private UnityEngine.UI.Button resumeButton;
        private NetworkManager manager;
        private UnityTransport transport;
        private LocalPlayerInput currentInput;
        private NetworkCrewPlayer localPlayer;
        private InputAction panelAction;
        private bool sessionActive, restorePending, intentionalDisconnect;
        private readonly Dictionary<ulong, int> slots = new();
        public string Status { get; private set; } = "Tek oyuncu hazır. Host başlat veya Client bağlan.";
        public bool PanelOpen => panel.activeSelf;
        public NetworkCrewPlayer LocalPlayer => localPlayer;
        public NetworkManager Manager => manager;

        private void Awake()
        {
            Instance = this; manager = GetComponent<NetworkManager>(); transport = GetComponent<UnityTransport>();
            Application.runInBackground = true;
            Application.targetFrameRate = 60;
            manager.ConnectionApprovalCallback = Approve;
            manager.OnClientConnectedCallback += Connected;
            manager.OnClientDisconnectCallback += Disconnected;
            manager.OnClientStopped += Stopped;
            hostButton.onClick.AddListener(StartHost);
            clientButton.onClick.AddListener(StartClient);
            disconnectButton.onClick.AddListener(Disconnect);
            resumeButton.onClick.AddListener(() => SetPanelOpen(false));
            panelAction = new InputAction("ConnectionPanel", InputActionType.Button, "<Keyboard>/tab");
            panelAction.performed += _ => { if (Application.isFocused) SetPanelOpen(!PanelOpen); };
            panelAction.Enable();
            BindInput(offlinePlayer.GetComponent<LocalPlayerInput>());
            SetPanelOpen(true);
        }
        public void BindLocal(NetworkCrewPlayer player)
        { localPlayer = player; BindInput(player.Input); SetPanelOpen(true); }
        private void BindInput(LocalPlayerInput input)
        {
            currentInput = input;
        }
        public void SetPanelOpen(bool open)
        {
            panel.SetActive(open);
            if (currentInput != null) currentInput.SetPanelOpen(open);
            if (open) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        }
        public void StartHost() => StartSession(true);
        public void StartClient() => StartSession(false);
        public void ConfigureEndpoint(string ip, ushort endpointPort) { address.text = ip; port.text = endpointPort.ToString(); }
        private void StartSession(bool host)
        {
            if (manager.IsListening || manager.ShutdownInProgress || sessionActive) return;
            if (!IPAddress.TryParse(address.text.Trim(), out var ip) || ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork
                || !ushort.TryParse(port.text, out ushort number) || number == 0)
            { Status = "Geçerli bir IPv4 adresi ve 1–65535 port gir."; return; }
            offlinePlayer.GetComponent<PhysicsCarry>().Drop();
            offlinePlayer.SetActive(false); offlineHud.SetActive(false); offlineScraps.gameObject.SetActive(false);
            sessionActive = true; intentionalDisconnect = false; slots.Clear(); localPlayer = null;
            transport.SetConnectionData(ip.ToString(), number, host ? "0.0.0.0" : null);
            bool started = host ? manager.StartHost() : manager.StartClient();
            if (!started) { Status = "Bağlantı başlatılamadı; adres/port kullanımını kontrol et."; restorePending = true; return; }
            Status = host ? "Host hazır; client bekleniyor." : "Host'a bağlanılıyor…";
            SetPanelOpen(true);
            if (host)
            {
                for (int i = 0; i < scrapPrefabs.Length; i++)
                {
                    var source = offlineScraps.GetChild(i);
                    var obj = Instantiate(scrapPrefabs[i], source.position, source.rotation);
                    obj.GetComponent<NetworkObject>().Spawn();
                }
            }
        }
        private void Approve(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            int slot = 0; while (slots.ContainsValue(slot)) slot++;
            response.Approved = slot < 2;
            response.CreatePlayerObject = response.Approved;
            response.Pending = false;
            response.Reason = response.Approved ? "" : "Bu prototip en fazla iki oyuncu destekler.";
            if (!response.Approved) return;
            slots[request.ClientNetworkId] = slot;
            response.Position = slot == 0 ? new Vector3(0, 1.25f, 4) : new Vector3(-.9f, 1.25f, 4.9f);
            response.Rotation = Quaternion.Euler(0, slot == 0 ? 58 : 90, 0);
        }
        private void Connected(ulong id)
        { Status = manager.IsHost ? $"Host: {manager.ConnectedClients.Count}/2 oyuncu bağlı." : $"Client bağlı — {address.text}:{port.text}."; }
        private void Disconnected(ulong id)
        {
            slots.Remove(id);
            if (intentionalDisconnect) return;
            if (manager.IsServer && id != manager.LocalClientId)
            { Status = "Client ayrıldı; taşıdığı hurda server'da serbest bırakıldı."; return; }
            if (!manager.IsServer)
            {
                if (!intentionalDisconnect)
                    Status = "Host bağlantısı kapandı veya bağlantı başarısız. " + manager.DisconnectReason;
                restorePending = true;
            }
        }
        private void Stopped(bool wasHost) { restorePending = true; }
        public void Disconnect()
        {
            if (!sessionActive) { SetPanelOpen(true); return; }
            foreach (var player in FindObjectsByType<NetworkCrewPlayer>()) player.Carry.Drop();
            intentionalDisconnect = true;
            Status = "Bağlantı kesildi. Tek oyuncu moduna dönüldü.";
            manager.Shutdown(); restorePending = true;
        }
        private void RestoreOffline()
        {
            sessionActive = restorePending = false; localPlayer = null; slots.Clear();
            offlineScraps.gameObject.SetActive(true);
            foreach (var item in offlineScraps.GetComponentsInChildren<ScrapItem>()) item.Recover();
            offlinePlayer.SetActive(true); offlineHud.SetActive(true);
            offlinePlayer.GetComponent<FirstPersonMotor>().ReturnToSpawn();
            BindInput(offlinePlayer.GetComponent<LocalPlayerInput>()); SetPanelOpen(true);
        }
        private void Update()
        {
            if (restorePending && !manager.IsListening && !manager.ShutdownInProgress) RestoreOffline();
            statusLabel.text = Status;
            string role = manager.IsHost ? "HOST" : manager.IsConnectedClient ? "CLIENT" : sessionActive ? "CLIENT / CONNECTING" : "SOLO";
            roleLabel.text = role + "  |  Tab: Panel / Esc: İmleç";
            hostButton.interactable = clientButton.interactable = !sessionActive && !manager.ShutdownInProgress;
            disconnectButton.interactable = sessionActive;
            resumeButton.interactable = !sessionActive || localPlayer != null;
        }
        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.ConnectionApprovalCallback -= Approve;
                manager.OnClientConnectedCallback -= Connected;
                manager.OnClientDisconnectCallback -= Disconnected;
                manager.OnClientStopped -= Stopped;
            }
            panelAction?.Dispose();
            if (Instance == this) Instance = null;
        }
    }
}
