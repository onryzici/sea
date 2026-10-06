using System;
using System.Collections.Generic;
using UnityEngine;

namespace SalvageCrew
{
    // Shared rules run offline or on the server; input and RPCs are separate adapters.
    public sealed class WreckExpedition : MonoBehaviour
    {
        [Serializable] public sealed class Site
        {
            public string title;
            public Transform location;
            public Transform[] cargoSpawns;
        }
        public static WreckExpedition Instance { get; private set; }
        [SerializeField] private Site[] sites;
        [SerializeField] private GameObject[] offlineCargo;
        [SerializeField] private GameObject[] networkCargo;
        [Header("Search / return rules")]
        [SerializeField, Min(5)] private float scanRange = 60;
        [SerializeField, Min(1)] private float scanCooldown = 5;
        [SerializeField, Min(1)] private int cargoRequired = 3;
        [SerializeField] private Vector3 harborPoint = new Vector3(7, 0, 10);
        [SerializeField, Min(2)] private float returnRadius = 12;
        [SerializeField, Min(.1f)] private float cargoSettleSeconds = 2;
        private readonly List<ScrapItem> cargo = new();
        private readonly List<int> cargoSites = new();
        private readonly List<float> settled = new();
        private NetworkWreckExpedition network;
        private bool authority, started;
        private float nextScan, nextTick, lastTick;
        private int discovered, secured;
        private bool complete;
        public int DiscoveredMask => network != null && network.IsSpawned ? network.Discovered.Value : discovered;
        public int Secured => network != null && network.IsSpawned ? network.Secured.Value : secured;
        public bool Complete => network != null && network.IsSpawned ? network.Complete.Value : complete;
        public int Required => cargoRequired;
        public Site[] Sites => sites;
        public IReadOnlyList<ScrapItem> Cargo => cargo;
        public float Range => scanRange;
        public string Feedback { get; private set; } = "";
        public float FeedbackUntil { get; private set; }
        public Transform Boat => NetworkBoat.Instance != null && NetworkBoat.Instance.IsSpawned
            ? NetworkBoat.Instance.transform : OfflineBoatController.Instance != null ? OfflineBoatController.Instance.transform : null;
        private void Awake() { Instance = this; }
        private void Start() { if (!started) BeginOffline(); }
        public void BeginOffline() { EndSession(); Begin(null); }
        public void Attach(NetworkWreckExpedition adapter) { EndSession(); Begin(adapter); }
        private void Begin(NetworkWreckExpedition adapter)
        {
            network = adapter; authority = adapter == null || adapter.IsServer; started = true;
            discovered = secured = 0; complete = false; nextScan = -100; lastTick = Time.time;
            if (!authority) return;
            for (int s = 0; s < sites.Length; s++)
                for (int i = 0; i < sites[s].cargoSpawns.Length; i++)
                {
                    var point = sites[s].cargoSpawns[i];
                    var prefabs = adapter == null ? offlineCargo : networkCargo;
                    var obj = Instantiate(prefabs[i % prefabs.Length], point.position, point.rotation);
                    obj.name = $"WreckCargo_{s}_{i}";
                    if (adapter != null) obj.GetComponent<Unity.Netcode.NetworkObject>().Spawn();
                    cargo.Add(obj.GetComponent<ScrapItem>()); cargoSites.Add(s); settled.Add(0);
                }
            Publish();
        }
        public void EndSession()
        {
            foreach (var item in cargo)
            {
                if (item == null) continue;
                if (item.Holder != null) item.Holder.Drop();
                var net = item.GetComponent<Unity.Netcode.NetworkObject>();
                if (net != null && net.IsSpawned)
                { if (authority) net.Despawn(); }
                else { item.gameObject.SetActive(false); Destroy(item.gameObject); }
            }
            cargo.Clear(); cargoSites.Clear(); settled.Clear(); network = null;
            authority = started = false; Feedback = "";
        }
        public void RequestScan(Transform actor)
        {
            if (!started) return;
            if (network != null && network.IsSpawned) { network.RequestScan(); return; }
            SetFeedback(Scan(actor));
        }
        public string Scan(Transform actor)
        {
            if (!authority || !started || actor == null || Boat == null) return "Tarama hazır değil.";
            Vector3 p = Boat.InverseTransformPoint(actor.position);
            if (Mathf.Abs(p.x) > 2.6f || Mathf.Abs(p.z) > 5.8f || p.y < .5f || p.y > 5)
                return "Sonar için teknenin güvertesine çık.";
            if (Time.time < nextScan) return $"Sonar hazırlanıyor · {Mathf.CeilToInt(nextScan-Time.time)} sn";
            nextScan = Time.time + scanCooldown;
            int found = 0;
            for (int i = 0; i < sites.Length; i++)
                if ((discovered & (1 << i)) == 0 && PlanarDistance(Boat.position, sites[i].location.position) <= scanRange)
                { discovered |= 1 << i; found++; }
            Publish();
            return found > 0 ? $"SONAR · {found} enkaz sinyali bulundu." : $"{scanRange:0} m içinde yeni sinyal yok. Açık denize ilerle.";
        }
        public void SetFeedback(string text) { Feedback = text; FeedbackUntil = Time.unscaledTime + 4; }
        private void Update()
        {
            if (!started || !authority || Boat == null || Time.time < nextTick) return;
            float dt = Mathf.Min(Time.time-lastTick, .5f); lastTick = Time.time; nextTick = Time.time + .2f;
            int count = 0;
            var body = Boat.GetComponent<Rigidbody>();
            for (int i = 0; i < cargo.Count; i++)
            {
                var item = cargo[i];
                if (item == null) continue;
                var p = Boat.InverseTransformPoint(item.Body.position);
                bool onDeck = (discovered & (1 << cargoSites[i])) != 0 && !item.IsHeld
                    && Mathf.Abs(p.x) < 1.75f && p.z > -4.5f && p.z < .6f && p.y > 1.25f && p.y < 3.1f
                    && (item.Body.linearVelocity-body.GetPointVelocity(item.Body.position)).magnitude < 1.4f;
                settled[i] = onDeck ? settled[i] + dt : 0;
                if (settled[i] >= cargoSettleSeconds) count++;
            }
            secured = count;
            if (!complete && secured >= cargoRequired && PlanarDistance(Boat.position, harborPoint) <= returnRadius
                && Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude < .45f)
            { complete = true; SetFeedback("SEFER TAMAMLANDI · Enkaz yükü limana getirildi."); }
            Publish();
        }
        private void Publish()
        {
            if (network == null || !network.IsSpawned || !network.IsServer) return;
            network.Discovered.Value = discovered; network.Secured.Value = secured; network.Complete.Value = complete;
        }
        public Vector3 HarborPoint => harborPoint;
        public static float PlanarDistance(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x,a.z),new Vector2(b.x,b.z));
        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
