using TMPro;
using UnityEngine;

namespace SalvageCrew
{
    public sealed class NetworkCarryHud : MonoBehaviour
    {
        [SerializeField] private NetworkCrewPlayer player;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private HarborHudPresentation presentation;
        private void LateUpdate()
        {
            if (player == null || label == null) return;
            if (presentation != null) presentation.SetContext(player.Driving || player.HelmTarget, player.Driving);
            if (player.Driving)
            { label.text = "Dümen — W/S: İleri/Geri · A/D: Dönüş\nE — Bırak · Tab/Esc: Girdi durur"; return; }
            if (player.HelmTarget)
            {
                label.text = player.Held != null ? "Dümen için önce hurdayı bırak."
                    : NetworkBoat.Instance.Driver.Value != NetworkScrap.Nobody ? "Dümeni başka bir oyuncu kullanıyor." : "Dümen\nE — Kullan";
                return;
            }
            var held = player.Held;
            var target = held != null ? held : player.Target;
            if (target == null) { label.text = player.Feedback; return; }
            string action = held != null ? "E — Bırak"
                : target.Holder.Value != NetworkScrap.Nobody ? "Başka bir oyuncu taşıyor" : "E — Tut";
            if (!player.Input.GameplayActive) action = "Panel / Click — Devam et";
            label.text = $"{target.Item.DisplayName}  ({target.Item.Mass:0} kg)\n{action}";
        }
    }
}
