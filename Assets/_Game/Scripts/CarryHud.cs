using TMPro;
using UnityEngine;

namespace SalvageCrew
{
    public sealed class CarryHud : MonoBehaviour
    {
        [SerializeField] private PhysicsCarry carry;
        [SerializeField] private LocalPlayerInput input;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private HarborHudPresentation presentation;
        private void LateUpdate()
        {
            if (carry == null || label == null || input == null) return;
            var boat=OfflineBoatController.Instance;
            if (presentation != null) presentation.SetContext(boat != null && (boat.Driving || boat.Target(carry)), boat != null && boat.Driving);
            if(boat!=null){
                if(boat.Feedback!=""){label.text=boat.Feedback;return;}
                if(boat.Driving){label.text="DÜMEN  •  W/S İleri/Geri  •  A/D Dön\nE — Dümeni bırak";return;}
                if(boat.Target(carry)){label.text=carry.Held!=null?"Dümen için önce hurdayı bırak.":"DÜMEN\nE — Kullan";return;}
            }
            var item = carry.Held != null ? carry.Held : carry.Target;
            label.text = item == null ? "" : $"{item.DisplayName}  ({item.Mass:0} kg)\n"
                + (!input.GameplayActive ? "Click — Devam et" : carry.Held != null ? "E — Bırak" : "E — Tut");
        }
    }
}
