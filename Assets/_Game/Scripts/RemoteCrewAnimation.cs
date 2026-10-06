using UnityEngine;

namespace SalvageCrew
{
    // Animation derives from replicated motion, never drives the CharacterController.
    [DefaultExecutionOrder(60)]
    public sealed class RemoteCrewAnimation : MonoBehaviour
    {
        [SerializeField] private NetworkCrewPlayer player;
        [SerializeField] private Animator animator;
        private Vector3 previous;
        private Transform reference;
        private bool initialized;
        public float VisualSpeed { get; private set; }
        private void OnEnable() { initialized = false; }
        private void LateUpdate()
        {
            if (player == null || !player.IsSpawned || player.IsOwner || animator == null) return;
            var boat = NetworkBoat.Instance;
            Transform current = boat != null && boat.ContainsPassenger(boat.transform.InverseTransformPoint(player.transform.position)) ? boat.transform : null;
            Vector3 position = current != null ? current.InverseTransformPoint(player.transform.position) : player.transform.position;
            float speed = initialized && current == reference ? new Vector2(position.x - previous.x, position.z - previous.z).magnitude / Mathf.Max(Time.deltaTime,.001f) : 0;
            VisualSpeed = Mathf.Lerp(VisualSpeed, player.Driving ? 0 : Mathf.Min(speed,6.5f), 1 - Mathf.Exp(-10 * Time.deltaTime));
            animator.SetFloat("Speed", VisualSpeed);
            previous = position; reference = current; initialized = true;
        }
    }
}
