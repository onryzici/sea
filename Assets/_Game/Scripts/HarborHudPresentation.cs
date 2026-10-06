using TMPro;
using UnityEngine;

namespace SalvageCrew
{
    // Presentation only: never handles input, physics or network authority.
    [DefaultExecutionOrder(100)]
    public sealed class HarborHudPresentation : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI prompt;
        [SerializeField] private CanvasGroup promptGroup;
        [SerializeField] private CanvasGroup controlsGroup;
        [SerializeField] private UnityEngine.UI.Image icon;
        [SerializeField] private UnityEngine.UI.Image reticle;
        [SerializeField] private Sprite cargoIcon;
        [SerializeField] private Sprite helmIcon;
        [SerializeField] private TextMeshProUGUI cursorHint;
        [SerializeField] private TextMeshProUGUI controlsLabel;
        private bool helm, driving;

        public void SetContext(bool atHelm, bool isDriving = false) { helm = atHelm; driving = isDriving; }

        private void LateUpdate()
        {
            bool panel = HarborSession.Instance != null && HarborSession.Instance.PanelOpen;
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            bool show = !panel && prompt != null && !string.IsNullOrWhiteSpace(prompt.text);
            if (promptGroup != null) promptGroup.alpha = show ? 1 : 0;
            if (prompt != null) prompt.enabled = show;
            if (icon != null) icon.sprite = helm ? helmIcon : cargoIcon;
            if (reticle != null) reticle.enabled = !panel && locked;
            if (controlsGroup != null) controlsGroup.alpha = panel ? 0 : .94f;
            if (cursorHint != null) cursorHint.enabled = !panel && !locked;
            if (controlsLabel != null)
                controlsLabel.text = driving
                    ? "<b><color=#F0AD4A>W / S</color></b>  İleri / Geri    <b>A / D</b>  Dönüş\n<b><color=#F0AD4A>E</color></b>  Dümeni bırak    <b>R</b>  Kurtar    <b>TAB</b>  Oturum"
                    : "<b><color=#F0AD4A>WASD</color></b>  Yürü    <b>SHIFT</b>  Koş    <b>SPACE</b>  Zıpla\n<b><color=#F0AD4A>E</color></b>  Etkileşim    <b>R</b>  Kurtar    <b>TAB</b>  Oturum";
        }
    }
}
