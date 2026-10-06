using TMPro;
using UnityEngine;

namespace SalvageCrew
{
    public sealed class CarryHud : MonoBehaviour
    {
        [SerializeField] private PhysicsCarry carry;
        [SerializeField] private LocalPlayerInput input;
        [SerializeField] private TextMeshProUGUI label;
        private void LateUpdate()
        {
            if (carry == null || label == null || input == null) return;
            var item = carry.Held != null ? carry.Held : carry.Target;
            label.text = item == null ? "" : $"{item.DisplayName}  ({item.Mass:0} kg)\n"
                + (!input.GameplayActive ? "Click — Devam et" : carry.Held != null ? "E — Bırak" : "E — Tut");
        }
    }
}
