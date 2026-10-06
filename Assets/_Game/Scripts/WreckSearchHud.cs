using TMPro;
using UnityEngine;

namespace SalvageCrew
{
    public sealed class WreckSearchHud : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI heading, detail;
        [SerializeField] private CanvasGroup group;
        private float nextUpdate;
        private void Update()
        {
            group.alpha = HarborSession.Instance != null && HarborSession.Instance.PanelOpen ? 0 : 1;
            if (Time.unscaledTime < nextUpdate) return;
            nextUpdate = Time.unscaledTime + .15f;
            var mission = WreckExpedition.Instance;
            if (mission == null || mission.Boat == null) return;
            heading.text = mission.Complete ? "SEFER TAMAMLANDI" : $"ENKAZ SEFERİ   <color=#F0AD4A>{mission.Secured}/{mission.Required}</color> YÜK";
            if (Time.unscaledTime < mission.FeedbackUntil) { detail.text = mission.Feedback; return; }
            if (mission.Complete) { detail.text = "Kurtarma yükü limana ulaştı.\nKalan enkazları keşfetmeye devam edebilirsin."; return; }
            if (mission.Secured >= mission.Required)
            { detail.text = "<b>LİMANA DÖN</b>   " + Direction(mission.Boat, mission.HarborPoint) + "\nYük güvertede kalsın; limanda yavaşla."; return; }
            int nearest = -1; float distance = float.MaxValue;
            for (int i=0;i<mission.Sites.Length;i++)
            {
                if ((mission.DiscoveredMask & (1<<i))==0) continue;
                float d=WreckExpedition.PlanarDistance(mission.Boat.position,mission.Sites[i].location.position);
                if(d<distance){nearest=i;distance=d;}
            }
            detail.text = nearest<0 ? "<b>F</b>  SONAR TARAMASI · Teknedeyken\nEnkazı bul → yükü güverteye al → limana dön"
                : $"<b>{mission.Sites[nearest].title}</b>   {Direction(mission.Boat,mission.Sites[nearest].location.position)}\n"+
                (distance<12 ? "Yavaşça yanaş · E ile yükleri al / bırak" : "F  Yeni tarama · Keşfedilen enkaza ilerle");
        }
        static string Direction(Transform boat, Vector3 point)
        {
            Vector3 d=point-boat.position;d.y=0;
            float angle=Vector3.SignedAngle(Vector3.ProjectOnPlane(boat.forward,Vector3.up),d,Vector3.up);
            string dir=Mathf.Abs(angle)<15?"İLERİ":Mathf.Abs(angle)>160?"GERİ":angle>0?"SAĞ":"SOL";
            return $"{dir} {Mathf.Abs(angle):0}° · {d.magnitude:0} m";
        }
    }
}
