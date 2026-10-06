using UnityEngine;

namespace SalvageCrew
{
    // Four bounded buoyancy forces on the actual hull body. Clients remain kinematic.
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CalmWaterBuoyancy : MonoBehaviour
    {
        [SerializeField,Min(0)] private float waveResponse=.65f;
        [SerializeField,Min(.1f)] private float spring=12;
        [SerializeField,Min(.1f)] private float damping=6;
        [SerializeField,Range(.1f,8)] private float maximumTilt=3;
        [SerializeField,Min(0),Tooltip("Hull root depth below mean water. Visual hull and compound colliders move together.")] private float draft=.65f;
        [SerializeField] private bool mooredOffline;
        [SerializeField] private Collider[] boardingColliders;
        private Rigidbody body;
        public Rigidbody Body => body != null ? body : GetComponent<Rigidbody>();
        public float RestHeight => -draft;
        private static readonly Vector3[] Points={new(-1.3f,0,-3.4f),new(1.3f,0,-3.4f),new(-1.3f,0,3.4f),new(1.3f,0,3.4f)};
        private void Awake()
        {
            body=GetComponent<Rigidbody>();
            if(mooredOffline){body.isKinematic=false;body.constraints=RigidbodyConstraints.FreezePositionX|RigidbodyConstraints.FreezePositionZ|RigidbodyConstraints.FreezeRotationY;}
            body.maxAngularVelocity=.35f;
            body.inertiaTensorRotation=Quaternion.identity;
            body.inertiaTensor=body.mass/12f*new Vector3(101.44f,116,17.44f);
            if(boardingColliders==null || boardingColliders.Length==0)
                boardingColliders=System.Array.ConvertAll(new[]{"BoardingRamp","RampGuideFront","RampGuideRear"},name=>GameObject.Find(name)?.GetComponent<Collider>());
            if(boardingColliders!=null)foreach(var a in GetComponentsInChildren<Collider>())foreach(var b in boardingColliders)if(b!=null)Physics.IgnoreCollision(a,b,true);
        }
        private void FixedUpdate()
        {
            if(body.isKinematic)return;
            foreach(var local in Points){
                Vector3 point=body.position+body.rotation*local;
                float error=RestHeight+HarborWater.Height(point)*waveResponse-point.y;
                float response=Mathf.Clamp(error*spring-body.GetPointVelocity(point).y*damping,-4,4);
                body.AddForceAtPosition(Vector3.up*(body.mass*.25f*(-Physics.gravity.y+response)),point,ForceMode.Force);
            }
            var up=body.rotation*Vector3.up;var axis=Vector3.Cross(up,Vector3.up);
            float tilt=Vector3.Angle(up,Vector3.up);
            var angular=body.angularVelocity;angular.y=0;
            body.AddTorque(axis*(tilt>maximumTilt?20:4)-angular*3,ForceMode.Acceleration);
        }
    }
}
