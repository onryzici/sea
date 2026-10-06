using TMPro;
using UnityEngine;

namespace SalvageCrew
{
    public sealed class NetworkCarryHud : MonoBehaviour
    {
        [SerializeField] private NetworkCrewPlayer player;
        [SerializeField] private TextMeshProUGUI label;
        private void LateUpdate()
        {
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
