using UnityEngine;
using UnityEngine.InputSystem;

namespace SalvageCrew
{
    // Only local device/cursor handling lives here; the motor consumes a simple sample.
    public sealed class LocalPlayerInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actions;
        private InputActionMap map;
        private InputAction move, look, jump, sprint, reset, release, capture;
        private bool focused = true;
        private bool jumpRequested, resetRequested, releaseRequested, captureRequested;

        public struct Sample
        {
            public Vector2 Move, Look;
            public bool Jump, Sprint, Reset;
        }

        private void Awake()
        {
            // Each player owns its enabled action map, without changing the shared asset.
            actions = Instantiate(actions);
            map = actions.FindActionMap("Player", true);
            move = map.FindAction("Move", true);
            look = map.FindAction("Look", true);
            jump = map.FindAction("Jump", true);
            sprint = map.FindAction("Sprint", true);
            reset = map.FindAction("Reset", true);
            release = map.FindAction("ReleaseCursor", true);
            capture = map.FindAction("CaptureCursor", true);
            jump.performed += _ => jumpRequested = true;
            reset.performed += _ => resetRequested = true;
            release.performed += _ => releaseRequested = true;
            capture.performed += _ => captureRequested = true;
        }

        private void OnEnable() { map.Enable(); SetCursor(true); }
        private void OnDisable() { map.Disable(); ClearRequests(); SetCursor(false); }
        private void OnDestroy() { if (actions != null) Destroy(actions); }
        private void OnApplicationFocus(bool hasFocus)
        {
            focused = hasFocus;
            if (!hasFocus) { ClearRequests(); SetCursor(false); }
        }

        public Sample Read()
        {
            // Latch events until the motor consumes them, including multiple input updates per frame.
            if (releaseRequested) SetCursor(false);
            else if (focused && captureRequested) SetCursor(true);
            bool active = focused && Cursor.lockState == CursorLockMode.Locked;
            var sample = new Sample
            {
                Move = active ? Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1f) : Vector2.zero,
                Look = active ? look.ReadValue<Vector2>() : Vector2.zero,
                Jump = active && jumpRequested,
                Sprint = active && sprint.IsPressed(),
                Reset = focused && resetRequested
            };
            ClearRequests();
            return sample;
        }

        private void ClearRequests()
        { jumpRequested = resetRequested = releaseRequested = captureRequested = false; }

        private static void SetCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
