using UnityEngine;
using Unity.Netcode;

namespace SalvageCrew
{
    [ExecuteAlways]
    public sealed class HarborWater : MonoBehaviour
    {
        [SerializeField] private Material waterMaterial;
        public static HarborWater Instance { get; private set; }
        public static float Clock => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening
            ? (float)NetworkManager.Singleton.ServerTime.Time : Time.time;
        private void OnEnable() { Instance=this; }
        private void OnDisable() { if(Instance==this)Instance=null; }
        private void Update() { Shader.SetGlobalFloat("_HarborWaterTime",Clock); }
        private void LateUpdate()
        {
            // Keep dense geometry near the local camera while sampling waves in world coordinates.
            if(!Application.isPlaying)return;
            var camera=Camera.main;var surface=transform.Find("Ocean surface");
            if(camera==null||surface==null)return;
            var p=camera.transform.position;surface.position=new Vector3(Mathf.Floor(p.x/8)*8,-.05f,Mathf.Floor(p.z/8)*8);
        }
        public static float Exposure(Vector3 world)
        {
            float d=new Vector2((world.x-7)*.8f,world.z-10).magnitude;
            float r=Mathf.SmoothStep(0,1,Mathf.InverseLerp(18,100,d));
            return Mathf.Lerp(.28f,1,r)*(.9f+.1f*Mathf.Sin(world.x*.003f+world.z*.008f));
        }
        public static float Height(Vector3 world)
        {
            var w=Instance;if(w==null || w.waterMaterial==null)return 0;
            float a=w.waterMaterial.GetFloat("_WaveHeight"),t=Clock*w.waterMaterial.GetFloat("_Speed");
            return a*Exposure(world)*(Mathf.Sin(world.x*.26f+world.z*.13f+t*.8f)*.50f
                +Mathf.Sin(world.x*-.17f+world.z*.31f-t*.93f)*.28f
                +Mathf.Sin(world.x*.62f+world.z*.42f+t*1.31f)*.14f
                +Mathf.Sin(world.x*-.91f+world.z*.32f-t*1.71f)*.08f);
        }
    }
}
