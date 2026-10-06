using UnityEngine;
namespace SalvageCrew
{
    public sealed class HelmWheelVisual : MonoBehaviour
    {
        [SerializeField] private Transform wheel;
        [SerializeField] private float maximumAngle = 95;
        private Quaternion rest;
        private NetworkBoat network;
        private OfflineBoatController offline;
        private float angle;
        private void Awake()
        { rest=wheel.localRotation; network=GetComponentInParent<NetworkBoat>();offline=GetComponentInParent<OfflineBoatController>(); }
        private void LateUpdate()
        {
            float turn=network!=null&&network.IsSpawned?network.SteeringVisual:offline!=null?offline.Drive.x:0;
            angle=Mathf.MoveTowards(angle,-turn*maximumAngle,180*Time.deltaTime);
            wheel.localRotation=rest*Quaternion.AngleAxis(angle,Vector3.up);
        }
    }
}
