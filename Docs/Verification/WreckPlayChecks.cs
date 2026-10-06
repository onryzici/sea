using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using SalvageCrew;

public static class WreckPlayChecks
{
    static void Place(FirstPersonMotor p,Vector3 point,Quaternion rotation)
    {var c=p.GetComponent<CharacterController>();c.enabled=false;p.Passenger?.Clear();p.transform.SetPositionAndRotation(point,rotation);c.enabled=true;Physics.SyncTransforms();p.Passenger?.Reacquire();}
    public static object HelmView()
    {
        var b=OfflineBoatController.Instance;var p=UnityEngine.Object.FindAnyObjectByType<FirstPersonMotor>();
        HarborSession.Instance.SetPanelOpen(false);p.enabled=false;
        Place(p,b.transform.TransformPoint(new Vector3(0,1.6f,1.4f)),b.transform.rotation);
        p.GetComponent<PhysicsCarry>().View.LookAt(b.Helm.position);
        return new{b.Helm.position,target=b.Target(p.GetComponent<PhysicsCarry>())};
    }
    public static object WreckView()
    {
        var m=WreckExpedition.Instance;var b=OfflineBoatController.Instance;var p=UnityEngine.Object.FindAnyObjectByType<FirstPersonMotor>();
        b.Release();b.Body.position=new Vector3(24.5f,b.Body.position.y,43.6f);b.Body.rotation=Quaternion.identity;b.Body.linearVelocity=Vector3.zero;
        b.transform.SetPositionAndRotation(b.Body.position,b.Body.rotation);Physics.SyncTransforms();
        Place(p,b.transform.TransformPoint(new Vector3(-1.4f,1.6f,-1.6f)),Quaternion.Euler(0,270,0));
        p.enabled=false;p.GetComponent<PhysicsCarry>().View.LookAt(m.Cargo[1].transform.position);m.RequestScan(p.transform);
        return new{cargo=m.Cargo.Select(x=>new{x.name,pos=x.Body.position.ToString(),speed=x.Body.linearVelocity.magnitude}).ToArray()};
    }
    public static async Task<object> Run()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Play required");
        var checks=new Dictionary<string,bool>();var m=WreckExpedition.Instance;var b=OfflineBoatController.Instance;
        var p=UnityEngine.Object.FindAnyObjectByType<FirstPersonMotor>();var carry=p.GetComponent<PhysicsCarry>();
        HarborSession.Instance.SetPanelOpen(false);p.enabled=false;
        await Task.Delay(1200);
        checks["sixPhysicalCargo"]=m.Cargo.Count==6&&m.Cargo.All(x=>!x.Body.isKinematic);
        checks["stableOnWreck"]=m.Cargo.All(x=>x.Body.position.y>.15f&&x.Body.linearVelocity.magnitude<.2f);
        checks["shoreScanRejected"]=m.Scan(p.transform).Contains("güverte");
        Place(p,b.transform.TransformPoint(new Vector3(0,1.6f,-2)),b.transform.rotation);
        checks["scanDiscoversInRange"]=m.Scan(p.transform).Contains("bulundu")&&m.DiscoveredMask==1;
        checks["cooldown"]=m.Scan(p.transform).Contains("hazırlanıyor");
        WreckView();await Task.Delay(700);
        // Fixture positions the carrier at the wreck; pickup/drop still use the real raycast and physics spring.
        bool pickup=true;
        for(int i=0;i<3;i++)
        {
            var item=m.Cargo[i];
            Place(p,item.Body.position+new Vector3(-1.3f,.15f,0),Quaternion.Euler(0,90,0));
            carry.View.LookAt(item.Body.position);Physics.SyncTransforms();
            bool got=carry.TryPickup(item);pickup &= got;
            carry.Drop();
            item.Body.position=b.transform.TransformPoint(new Vector3(-1.05f+i*1.05f,2.1f,-2.4f));
            item.Body.rotation=b.Body.rotation;item.Body.linearVelocity=Vector3.zero;item.Body.angularVelocity=Vector3.zero;
        }
        checks["allWeightsPickupDrop"]=pickup;
        Place(p,b.transform.TransformPoint(new Vector3(0,1.6f,1.4f)),b.transform.rotation);
        await Task.Delay(4000);
        checks["loadedThree"]=m.Secured==3;
        checks["notCompleteAwayFromHarbor"]=!m.Complete;
        var delta=new Vector3(7,0,10)-new Vector3(b.Body.position.x,0,b.Body.position.z);
        b.Body.position+=delta;foreach(var item in m.Cargo.Take(3)){item.Body.position+=delta;item.Body.linearVelocity=Vector3.zero;}
        b.transform.SetPositionAndRotation(b.Body.position,b.Body.rotation);Physics.SyncTransforms();
        Place(p,b.transform.TransformPoint(new Vector3(0,1.6f,1.4f)),b.transform.rotation);
        await Task.Delay(3000);
        checks["returnCompletes"]=m.Complete;
        var last=m.Cargo[4];last.Body.position=new Vector3(0,-3,0);await Task.Delay(2100);
        checks["uncollectedCargoRescuesToWreck"]=Vector3.Distance(last.Body.position,m.Sites[1].location.position)<7&&last.Body.position.y>.15f;
        return new{checks,secured=m.Secured,complete=m.Complete};
    }
    public static async Task<object> Transfer()
    {
        var m=WreckExpedition.Instance;var b=OfflineBoatController.Instance;
        var p=UnityEngine.Object.FindAnyObjectByType<FirstPersonMotor>();var carry=p.GetComponent<PhysicsCarry>();
        var results=new List<object>();p.enabled=false;
        await Task.Delay(1500);
        // Boat placement is a fixture. Items are not teleported: ray pickup, spring lift, walking and drop are real.
        for(int i=0;i<3;i++)
        {
            var item=m.Cargo[i];var local=b.transform.InverseTransformPoint(item.Body.position);
            Place(p,b.transform.TransformPoint(new Vector3(-1.4f,1.6f,local.z)),Quaternion.Euler(0,270,0));
            carry.View.LookAt(item.Body.position);Physics.SyncTransforms();
            bool targeted=carry.FindTarget(true)==item;carry.Toggle();
            bool picked=carry.Held==item;
            await Task.Delay(450);
            carry.View.LookAt(carry.View.position+Vector3.left*3+Vector3.up*.4f);
            await Task.Delay(800);
            float until=Time.time+2,last=Time.time;
            while(Time.time<until && b.transform.InverseTransformPoint(p.transform.position).x<.35f)
            {await Task.Delay(16);float now=Time.time;p.Step(new LocalPlayerInput.Sample{Move=Vector2.down},Mathf.Min(now-last,.035f));last=now;}
            await Task.Delay(900);
            bool retained=carry.Held==item;
            carry.Drop();await Task.Delay(1300);
            var final=b.transform.InverseTransformPoint(item.Body.position);
            results.Add(new{mass=item.Mass,targeted,picked,retained,final=final.ToString(),onDeck=Mathf.Abs(final.x)<1.8f&&final.y>1.2f&&final.y<3});
        }
        return results;
    }
}
