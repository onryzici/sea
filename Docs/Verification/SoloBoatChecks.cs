using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using SalvageCrew;
public static class SoloBoatChecks
{
 public static async Task<object> Run()
 {
  if(!Application.isPlaying)throw new InvalidOperationException("Play required");
  var boat=OfflineBoatController.Instance;var p=UnityEngine.Object.FindAnyObjectByType<FirstPersonMotor>();var cc=p.GetComponent<CharacterController>();var carry=p.GetComponent<PhysicsCarry>();
  HarborSession.Instance.SetPanelOpen(false);carry.Drop();boat.ResetToDock();p.enabled=false;cc.enabled=false;p.transform.SetPositionAndRotation(boat.transform.TransformPoint(new Vector3(0,1.6f,1.8f)),Quaternion.identity);cc.enabled=true;Physics.SyncTransforms();p.Passenger.Reacquire();carry.View.LookAt(boat.Helm.position);
  async Task Tick(float seconds,Vector2 move){float until=Time.time+seconds,last=Time.time;while(Time.time<until){await Task.Delay(16);float now=Time.time;boat.HandleInput(carry,new LocalPlayerInput.Sample{Move=move});p.Step(default,Mathf.Min(now-last,.04f));last=now;}}
  var checks=new System.Collections.Generic.Dictionary<string,bool>();var ramp=GameObject.Find("HarborPrototypeGenerated/Harbor/BoardingRamp");
  try{
   checks["helmTarget"]=boat.Target(carry);
   var item=UnityEngine.Object.FindObjectsByType<ScrapItem>().First(i=>i.Mass==3);item.Body.position=carry.View.position+carry.View.forward*1.1f;item.Body.linearVelocity=Vector3.zero;Physics.SyncTransforms();bool held=carry.TryPickup(item);carry.View.LookAt(boat.Helm.position);
   boat.HandleInput(carry,new LocalPlayerInput.Sample{Interact=true});checks["heldScrapRejected"]=held&&!boat.Driving&&boat.Feedback!="";carry.Drop();item.Recover();
   carry.View.LookAt(boat.Helm.position);boat.HandleInput(carry,new LocalPlayerInput.Sample{Interact=true});checks["helmEntered"]=boat.Driving&&p.MovementLocked;
   var start=boat.Body.position;await Tick(5,Vector2.up);float forward=boat.Body.position.z-start.z;checks["forwardAndRampDetached"]=forward>1&&boat.Departed&&!ramp.activeSelf;
   float beforeYaw=boat.transform.eulerAngles.y;await Tick(3,new Vector2(1,1));float turn=Mathf.Abs(Mathf.DeltaAngle(beforeYaw,boat.transform.eulerAngles.y));checks["steering"]=turn>5;
   float speed=boat.Body.linearVelocity.magnitude;await Tick(3,Vector2.zero);checks["neutralSlows"]=boat.Body.linearVelocity.magnitude<speed;
   await Tick(5,Vector2.down);checks["reverse"]=Vector3.Dot(boat.Body.linearVelocity,boat.transform.forward)<-.2f;
   checks["passengerStaysOnDeck"]=boat.Contains(p.transform.position)&&p.Grounded;
   p.ReturnToSpawn();checks["recoveryReleasesHelm"]=!boat.Driving&&!p.MovementLocked&&Vector3.Distance(p.transform.position,boat.RescuePoint())<.2f;
   return new{checks,forwardMetres=forward,turnDegrees=turn,finalSpeed=boat.Body.linearVelocity.magnitude};
  }finally{boat.Release();carry.Drop();boat.ResetToDock();HarborSession.Instance.SetRampActive(true);p.ConfigureSpawn(new Vector3(0,1.25f,4),Quaternion.Euler(0,58,0));p.ReturnToSpawn();p.enabled=true;}
 }
}
