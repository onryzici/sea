using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using SalvageCrew;
public static class BoatWaveChecks
{
 public static async Task<object> Run()
 {
  if(!Application.isPlaying)throw new InvalidOperationException("Play required");
  var boat=UnityEngine.Object.FindAnyObjectByType<CalmWaterBuoyancy>();var p=UnityEngine.Object.FindAnyObjectByType<FirstPersonMotor>();var cc=p.GetComponent<CharacterController>();
  HarborSession.Instance.SetPanelOpen(false);p.GetComponent<PhysicsCarry>().Drop();p.enabled=false;cc.enabled=false;
  var start=new Vector3(0,1.58f,-3.8f);p.transform.position=boat.transform.TransformPoint(start);cc.enabled=true;Physics.SyncTransforms();p.Passenger.Reacquire();
  var items=UnityEngine.Object.FindObjectsByType<ScrapItem>().OrderBy(i=>i.Mass).ToArray();
  for(int i=0;i<items.Length;i++){items[i].Body.position=boat.transform.TransformPoint(new Vector3((i-1)*1.1f,2.3f,-1.8f));items[i].Body.linearVelocity=Vector3.zero;items[i].Body.angularVelocity=Vector3.zero;}
  float min=100,max=-100,tilt=0,speed=0,drift=0;int grounded=0,n=0;float until=Time.time+14,last=Time.time;
  while(Time.time<until){await Task.Delay(16);float now=Time.time;float dt=now-last;if(dt<=0)continue;last=now;p.Step(default,Mathf.Min(dt,.05f));n++;if(p.Grounded)grounded++;
   min=Mathf.Min(min,boat.Body.position.y);max=Mathf.Max(max,boat.Body.position.y);tilt=Mathf.Max(tilt,Vector3.Angle(boat.transform.up,Vector3.up));
   var local=boat.transform.InverseTransformPoint(p.transform.position);drift=Mathf.Max(drift,Vector2.Distance(new Vector2(local.x,local.z),new Vector2(start.x,start.z)));
   foreach(var item in items)speed=Mathf.Max(speed,item.Body.linearVelocity.magnitude);
  }
  var positions=items.Select(i=>new {mass=i.Mass,local=boat.transform.InverseTransformPoint(i.Body.position).ToString(),speed=i.Body.linearVelocity.magnitude}).ToArray();
  bool cargo=items.All(i=>{var l=boat.transform.InverseTransformPoint(i.Body.position);return Mathf.Abs(l.x)<1.9f&&Mathf.Abs(l.z)<4.8f&&l.y>1.45f&&l.y<2.8f;});
  p.ReturnToSpawn();p.enabled=true;foreach(var item in items)item.Recover();
  return new {checks=new {dynamicHull=!boat.Body.isKinematic,visibleHeave=max-min>.025f,boundedTilt=tilt<5,passengerStable=drift<.35f&&grounded>n*.85f,cargoStable=cargo},heave=max-min,maximumTilt=tilt,passengerDrift=drift,groundedFraction=grounded/(float)n,maximumCargoSpeed=speed,items=positions,samples=n};
 }
}
