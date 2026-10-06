using System.Linq;
using TMPro;
using UnityEngine;

namespace SalvageCrew
{
    // Presentation only: reads spawned players and their authoritative carry/helm state.
    // No extra network objects, client-side ownership or invented crew members.
    public sealed class CrewPresence : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI heading, roster, marker;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform markerRect;
        [SerializeField] private RectTransform canvasRect;
        private NetworkCrewPlayer[] players = new NetworkCrewPlayer[0];
        private float nextRefresh;
        public string RosterText => roster != null ? roster.text : "";
        public static string Name(NetworkCrewPlayer p) => p.OwnerClientId == 0 ? "KAPTAN" : "TAYFA " + p.OwnerClientId;
        public static string Activity(NetworkCrewPlayer p) => p.Driving ? "DÜMENDE" : p.Held != null ? "TAŞIYOR · " + p.Held.Item.DisplayName : "GÜVERTE / KEŞİF";
        private void LateUpdate()
        {
            var session = HarborSession.Instance;
            if (session == null) return;
            bool online = session.Manager != null && session.Manager.IsListening && session.LocalPlayer != null;
            group.alpha = session.PanelOpen ? 0 : 1;
            if (Time.unscaledTime >= nextRefresh)
            {
                nextRefresh = Time.unscaledTime + .2f;
                players = online ? FindObjectsByType<NetworkCrewPlayer>().Where(p => p.IsSpawned).OrderBy(p => p.OwnerClientId).ToArray() : new NetworkCrewPlayer[0];
                heading.text = online ? $"MÜRETTEBAT   {players.Length}/2" : "TEK KİŞİLİK SEFER";
                roster.text = online ? string.Join("\n\n", players.Select(p =>
                    $"<color={(p.OwnerClientId == 0 ? "#67DAD5" : "#F0AD4A")}>{Name(p)}</color>" +
                    (p.IsOwner ? "  <size=14>SEN</size>" : $"  <size=14>{Vector3.Distance(p.transform.position, session.LocalPlayer.transform.position):0} m</size>") +
                    $"\n<size=15>{Activity(p)}</size>")) : "Birlikte enkaz aramak için\n<size=16>TAB → Host / Client</size>";
                if (online && players.Length == 1) roster.text += "\n\n<size=14>İkinci mürettebat bekleniyor</size>";
                ((RectTransform)transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, online ? 198 : 112);
            }
            marker.gameObject.SetActive(false);
            if (!online || session.PanelOpen) return;
            var other = players.FirstOrDefault(p => p != null && p.IsSpawned && !p.IsOwner);
            if (other == null) return;
            var view = session.LocalPlayer.Carry.View;
            var camera = view.GetComponentInChildren<Camera>();
            if (camera == null) camera = session.LocalPlayer.GetComponentInChildren<Camera>();
            Vector3 point = other.transform.position + Vector3.up * 2.12f;
            Vector3 delta = point - camera.transform.position;
            var screen = camera.WorldToViewportPoint(point);
            if (delta.magnitude > 60 || screen.z <= 0 || screen.x < .05f || screen.x > .95f || screen.y < .08f || screen.y > .85f) return;
            // Hide behind cabin/terrain; ignore local and remote player colliders.
            foreach (var hit in Physics.RaycastAll(camera.transform.position, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(other.transform) && !hit.transform.IsChildOf(session.LocalPlayer.transform)) return;
            markerRect.anchoredPosition = new Vector2((screen.x - .5f) * canvasRect.rect.width, (screen.y - .5f) * canvasRect.rect.height);
            marker.text = $"<b>{Name(other)}</b>  ·  {delta.magnitude:0} m\n<size=14>{Activity(other)}</size>";
            marker.color = other.OwnerClientId == 0 ? new Color(.4f,.86f,.84f) : new Color(1,.76f,.36f);
            marker.gameObject.SetActive(true);
        }
    }
}
