using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using SalvageCrew;
public static class CrewHarborChecks
{
    public static async Task<object> Animation()
    {
        var p=UnityEngine.Object.FindObjectsByType<NetworkCrewPlayer>().First(x=>!x.IsOwner);
        var a=p.GetComponentInChildren<Animator>();
        var bone=a.GetComponentsInChildren<Transform>().First(t=>t.name=="upperleg.l");
        var start=bone.localRotation;float t=a.GetCurrentAnimatorStateInfo(0).normalizedTime;
        await Task.Delay(480);
        return new {initialized=a.isInitialized,animatorTimeAdvanced=a.GetCurrentAnimatorStateInfo(0).normalizedTime!=t,boneAngle=Quaternion.Angle(start,bone.localRotation),rootMotion=a.applyRootMotion};
    }
    public static async Task<object> Paths()
    {
        var p=UnityEngine.Object.FindAnyObjectByType<FirstPersonMotor>();
        HarborSession.Instance.SetPanelOpen(false);p.ReturnToSpawn();p.enabled=false;
        var results=new Dictionary<string,object>();
        try
        {
            async Task Walk(string name,Vector3 target)
            {
                for(int i=0;i<550;i++)
                {
                    var d=Vector3.ProjectOnPlane(target-p.transform.position,Vector3.up);
                    if(d.magnitude<.13f)break;
                    p.transform.rotation=Quaternion.LookRotation(d);
                    p.Step(new LocalPlayerInput.Sample{Move=Vector2.up},.016f);
                    await Task.Delay(16);
                }
                results[name]=new {reached=Vector3.ProjectOnPlane(target-p.transform.position,Vector3.up).magnitude<.15f,position=p.transform.position.ToString(),grounded=p.Grounded};
            }
            await Walk("newSupplyBerth",new Vector3(0,0,11));
            await Walk("returnToPier",new Vector3(0,0,7.5f));
            await Walk("existingRampToDeck",new Vector3(6.9f,0,7.5f));
            for(int i=0;i<30;i++){p.Step(default,.016f);await Task.Delay(16);}
            results["supportedOnBoat"]=typeof(DeckPassenger).GetField("offlineSupport",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(p.Passenger)!=null;
            return results;
        }
        finally{p.ReturnToSpawn();p.enabled=true;}
    }
    public static object EditorChecks()
    {
        if(Application.isPlaying)throw new Exception("Stop Play");
        var root=GameObject.Find("CrewHarborDetails");
        var oldCount=root.GetComponentsInChildren<Transform>().Length;
        var helm=GameObject.Find("StaticBoat_10m_x_4m").transform.Find("Helm");
        SalvageCrew.EditorTools.CrewHarborSetup.Apply();
        var rs=root.GetComponentsInChildren<Renderer>();
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/NetworkFirstPersonPlayer.prefab");
        return new {before=oldCount,after=root.GetComponentsInChildren<Transform>().Length,details=root.transform.childCount,
            sameHelm=helm==GameObject.Find("StaticBoat_10m_x_4m").transform.Find("Helm"),
            addedRigidbodies=root.GetComponentsInChildren<Rigidbody>().Length,addedNetworkObjects=root.GetComponentsInChildren<Unity.Netcode.NetworkObject>().Length,
            missingMaterials=rs.Count(r=>r.sharedMaterials.Any(m=>m==null||m.shader==null||!m.shader.isSupported)),
            castShadows=rs.Count(r=>r.shadowCastingMode!=UnityEngine.Rendering.ShadowCastingMode.Off),renderers=rs.Length,
            crewCards=UnityEngine.Object.FindObjectsByType<CrewPresence>().Length,
            avatarController=prefab.GetComponentInChildren<Animator>(true).runtimeAnimatorController.name,
            sceneDirty=UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty};
    }
}
