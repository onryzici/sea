using System;
using UnityEngine;

namespace SalvageCrew
{
    // Solo authority adapter only. NetworkBoat remains the server's authority adapter.
    [RequireComponent(typeof(Rigidbody),typeof(CalmWaterBuoyancy))]
    public sealed class OfflineBoatController : MonoBehaviour
    {
        public static OfflineBoatController Instance {get;private set;}
        [SerializeField] private Transform helm;
        [SerializeField,Min(.1f)] private float maximumSpeed=2.5f,acceleration=.6f,waterResistance=.5f,lateralResistance=2;
        [SerializeField,Min(1)] private float turnSpeed=10,turnAcceleration=5;
        [SerializeField,Min(.1f)] private float interactionRange=2.5f;
        private Rigidbody body;
        private PhysicsCarry driver;
        private Vector2 drive;
        private float lastInput;
        private FirstPersonMotor player;
        public bool Departed {get;private set;}
        public bool Driving => driver!=null;
        public Vector2 Drive => drive;
        public string Feedback {get;private set;}="";
        private float feedbackUntil;
        public Rigidbody Body => body!=null?body:GetComponent<Rigidbody>();
        public Transform Helm => helm;
        private void Awake(){body=GetComponent<Rigidbody>();}
        private void OnEnable(){Instance=this;ResetToDock();}
        private void Start(){player=FindAnyObjectByType<FirstPersonMotor>();}
        private void OnDisable(){Release();if(Instance==this)Instance=null;}
        public void ResetToDock()
        {
            Release();Departed=false;Body.position=new Vector3(7,GetComponent<CalmWaterBuoyancy>().RestHeight,10);Body.rotation=Quaternion.identity;
            Body.linearVelocity=Body.angularVelocity=Vector3.zero;
            Body.constraints=RigidbodyConstraints.FreezePositionX|RigidbodyConstraints.FreezePositionZ|RigidbodyConstraints.FreezeRotationY;
            if(player!=null)player.ConfigureSpawn(new Vector3(0,1.25f,4),Quaternion.Euler(0,58,0));
        }
        public bool Target(PhysicsCarry carry)
        {
            if(carry==null||helm==null||Vector3.Distance(carry.View.position,helm.position)>interactionRange)return false;
            var hits=Physics.RaycastAll(carry.View.position,carry.View.forward,interactionRange,~0,QueryTriggerInteraction.Ignore);
            Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
            foreach(var h in hits){var target=h.collider.transform;if(target.IsChildOf(carry.transform))continue;if(carry.Held!=null&&h.collider.GetComponentInParent<ScrapItem>()==carry.Held)continue;return target==helm||target.IsChildOf(helm);}
            return false;
        }
        public bool HandleInput(PhysicsCarry carry,LocalPlayerInput.Sample sample)
        {
            bool target=Target(carry);
            if(sample.Interact&&(Driving||target)){
                if(Driving)Release();
                else if(carry.Held!=null){Feedback="Dümen için önce hurdayı bırak.";feedbackUntil=Time.unscaledTime+2;}
                else {driver=carry;player=carry.GetComponent<FirstPersonMotor>();player.MovementLocked=true;player.BeforeRespawn+=Release;Feedback="";}
                return true;
            }
            if(Driving){drive=Vector2.ClampMagnitude(sample.Move,1);lastInput=Time.unscaledTime;return true;}
            return false;
        }
        public void Release()
        {
            if(driver!=null){var motor=driver.GetComponent<FirstPersonMotor>();motor.MovementLocked=false;motor.BeforeRespawn-=Release;}
            driver=null;drive=Vector2.zero;
        }
        public bool Contains(Vector3 point){var p=transform.InverseTransformPoint(point);return Mathf.Abs(p.x)<2.4f&&Mathf.Abs(p.z)<5.5f&&p.y>.6f&&p.y<5;}
        public Vector3 RescuePoint(float side=0)=>transform.TransformPoint(new Vector3(side,1.6f,-3.4f));
        private void Update()
        {
            if(Time.unscaledTime>feedbackUntil)Feedback="";
            if(Departed&&player!=null&&player.gameObject.activeInHierarchy)player.ConfigureSpawn(RescuePoint(),Quaternion.Euler(0,transform.eulerAngles.y,0));
            if(Driving&&(driver==null||!driver.isActiveAndEnabled||Vector3.Distance(driver.transform.position,helm.position)>4))Release();
        }
        private void FixedUpdate()
        {
            if(!Driving||Time.unscaledTime-lastInput>.25f)drive=Vector2.zero;
            if(drive.sqrMagnitude>.001f&&!Departed){Departed=true;Body.constraints=RigidbodyConstraints.None;HarborSession.Instance?.SetRampActive(false);Body.WakeUp();}
            if(!Departed)return;
            var forward=Vector3.ProjectOnPlane(Body.rotation*Vector3.forward,Vector3.up).normalized;
            var planar=Vector3.ProjectOnPlane(Body.linearVelocity,Vector3.up);
            var lateral=planar-forward*Vector3.Dot(planar,forward);
            float thrust=Mathf.Abs(drive.y)<.01f?0:Mathf.Clamp((drive.y*maximumSpeed-Vector3.Dot(planar,forward))*1.5f,-acceleration,acceleration);
            Body.AddForce(forward*thrust-planar*waterResistance-lateral*lateralResistance,ForceMode.Acceleration);
            float turn=Mathf.Clamp(drive.x*turnSpeed*Mathf.Deg2Rad-Body.angularVelocity.y,-turnAcceleration*Mathf.Deg2Rad*Time.fixedDeltaTime,turnAcceleration*Mathf.Deg2Rad*Time.fixedDeltaTime);
            Body.AddTorque(Vector3.up*turn,ForceMode.VelocityChange);
        }
    }
}
