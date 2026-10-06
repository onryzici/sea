using System;
using System.Linq;
using System.Threading.Tasks;
using SalvageCrew;
using UnityEngine;

// Play-only capture fixtures. Placement is for repeatable views, not a claim of player traversal.
public static class ArtVisualChecks
{
    public static async Task<object> View(string angle)
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Play required");
        HarborSession.Instance.SetPanelOpen(false);
        var p=UnityEngine.Object.FindAnyObjectByType<FirstPersonMotor>();var carry=p.GetComponent<PhysicsCarry>();var cc=p.GetComponent<CharacterController>();
        carry.Drop();p.ReturnToSpawn();p.enabled=false;cc.enabled=false;
        p.transform.SetPositionAndRotation(angle=="Start"?new Vector3(0,1.25f,4):angle=="Deck"?new Vector3(6.3f,1.53f,6.3f):new Vector3(6.6f,1.53f,6.2f),Quaternion.Euler(0,angle=="Start"?58:angle=="Deck"?15:0,0));
        cc.enabled=true;Physics.SyncTransforms();
        bool held=false;
        if(angle=="Carry") {var i=UnityEngine.Object.FindObjectsByType<ScrapItem>().First(x=>x.Mass==12);i.Body.position=new Vector3(6.6f,2.35f,7.65f);i.Body.linearVelocity=Vector3.zero;Physics.SyncTransforms();carry.View.LookAt(i.Body.worldCenterOfMass);held=carry.TryPickup(i);}
        await Task.Delay(500);
        return new {angle,position=p.transform.position.ToString(),rotation=p.transform.eulerAngles.ToString(),held,frames=Time.frameCount};
    }
    public static async Task<object> Metrics()
    {
        var deltas=new System.Collections.Generic.List<float>();int last=Time.frameCount;
        for(int i=0;i<160;i++){await Task.Delay(18);if(Time.frameCount!=last){deltas.Add(Time.unscaledDeltaTime*1000);last=Time.frameCount;}}
        deltas.Sort();var renderers=UnityEngine.Object.FindObjectsByType<Renderer>();
        var bad=renderers.Where(r=>r.enabled && r.sharedMaterials.Any(m=>m==null || m.shader==null || !m.shader.isSupported || m.shader.name=="Hidden/InternalErrorShader")).Select(r=>r.name).ToArray();
        return new {sampleFrames=deltas.Count,medianMs=deltas[deltas.Count/2],p95Ms=deltas[(int)(deltas.Count*.95f)],enabledRenderers=renderers.Count(r=>r.enabled),badMaterials=bad,background=Application.runInBackground};
    }
    public static async Task<object> CompareCost()
    {
        await View("Start");
        var roots=UnityEngine.Object.FindObjectsByType<Transform>().Where(t=>t.name=="ArtVisuals").ToArray();
        var all=UnityEngine.Object.FindObjectsByType<MeshRenderer>();var flags=all.Select(r=>r.enabled).ToArray();
        var sea=GameObject.Find("Sea_VisualOnly").GetComponent<Renderer>();var water=sea.sharedMaterial;
        object without;
        try
        {
            foreach(var t in roots)t.gameObject.SetActive(false);
            foreach(var r in all)if(!roots.Any(t=>r.transform.IsChildOf(t)))r.enabled=true;
            sea.sharedMaterial=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/SeaBlue.mat");
            await Task.Delay(1000);without=await Metrics();
        }
        finally
        {
            foreach(var t in roots)t.gameObject.SetActive(true);
            for(int i=0;i<all.Length;i++)all[i].enabled=flags[i];sea.sharedMaterial=water;
        }
        await Task.Delay(1000);
        return new {method="Same-camera capped Editor A/B: visual subtrees + water off/on. Not a HEAD/GPU benchmark.",withoutArt=without,withArt=await Metrics()};
    }
}
