using System;
using UnityEngine;

namespace SalvageCrew
{
    // No parenting: a CC receives one rigid platform delta, then its own local walking step.
    [RequireComponent(typeof(CharacterController))]
    public sealed class DeckPassenger : MonoBehaviour
    {
        [SerializeField, Min(.1f)] private float supportProbe = .32f;
        private CharacterController controller;
        private Vector3 previousPosition;
        private Quaternion previousRotation;
        private CalmWaterBuoyancy offlineSupport;
        private Transform Platform => Support != null && Support.IsSpawned ? Support.transform : offlineSupport != null ? offlineSupport.transform : null;
        public NetworkBoat Support { get; private set; }
        public NetworkBoat Reference { get; private set; }
        public Vector3 AirVelocity { get; private set; }
        public Vector3 PlatformVelocity => Reference != null ? Reference.PointVelocity(transform.position) : offlineSupport != null ? offlineSupport.Body.GetPointVelocity(transform.position) : Vector3.zero;
        private void Awake() { controller = GetComponent<CharacterController>(); }
        public void BeginStep()
        {
            var root = Platform;if(root==null)return;
            Vector3 local = Quaternion.Inverse(previousRotation) * (transform.position - previousPosition);
            Vector3 target = root.position + root.rotation * local;
            controller.Move(target - transform.position);
            transform.Rotate(0, Mathf.DeltaAngle(previousRotation.eulerAngles.y, root.eulerAngles.y), 0, Space.World);
            previousPosition = root.position; previousRotation = root.rotation;
        }
        public void TakeOff()
        { AirVelocity = PlatformVelocity; Support = null;offlineSupport=null; }
        public void EndStep(bool ascending)
        {
            if (!ascending)
            {
                var hits = Physics.RaycastAll(transform.position + Vector3.up * .18f, Vector3.down, supportProbe + .18f, ~0, QueryTriggerInteraction.Ignore);
                Array.Sort(hits, (a,b) => a.distance.CompareTo(b.distance));
                NetworkBoat found = null;
                CalmWaterBuoyancy localFound=null;
                foreach (var hit in hits)
                {
                    if (hit.collider.transform.IsChildOf(transform)) continue;
                    if (hit.normal.y > .7f) found = hit.collider.GetComponentInParent<NetworkBoat>();
                    if(found==null && hit.normal.y>.7f)localFound=hit.collider.GetComponentInParent<CalmWaterBuoyancy>();
                    if (found == null && hit.normal.y > .7f)
                    {
                        var scrap = hit.collider.GetComponentInParent<NetworkScrap>();
                        var boat = NetworkBoat.Instance;
                        if (scrap != null && scrap.Holder.Value == NetworkScrap.Nobody && boat != null
                            && boat.ContainsPassenger(boat.transform.InverseTransformPoint(scrap.Item.Body.position))) found = boat;
                    }
                    break;
                }
                if (found != null) { Support = Reference = found;offlineSupport=null; AirVelocity = Vector3.zero; }
                else if(localFound!=null){offlineSupport=localFound;Support=Reference=null;AirVelocity=Vector3.zero;}
                else if (Platform != null) { TakeOff(); }
                if (controller.isGrounded && found == null && localFound==null) Clear();
            }
            if (Platform != null) { previousPosition = Platform.position; previousRotation = Platform.rotation; }
            if (Reference != null && !Reference.ContainsPassenger(Reference.transform.InverseTransformPoint(transform.position))) Reference = null;
        }
        public void Clear() { Support = Reference = null;offlineSupport=null; AirVelocity = Vector3.zero; }
        public void Reacquire() { Clear(); EndStep(false); }
        public void SetNetworkReference(NetworkBoat boat) { Reference = boat; }
    }
}
