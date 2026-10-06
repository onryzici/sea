using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using SalvageCrew;

public static class CabinPlayChecks
{
 public static async Task<object> Run()
 {
  var boat=OfflineBoatController.Instance;var p=UnityEngine.Object.FindAnyObjectByType<FirstPersonMotor>();var cc=p.GetComponent<CharacterController>();var carry=p.GetComponent<PhysicsCarry>();
  HarborSession.Instance.SetPanelOpen(false);carry.Drop();boat.ResetToDock();p.ReturnToSpawn();p.enabled=false;cc.enabled=false;
  p.transform.SetPositionAndRotation(boat.transform.TransformPoint(new Vector3(0,1.58f,0)),Quaternion.identity);carry.View.localRotation=Quaternion.identity;cc.enabled=true;Physics.SyncTransforms();p.Passenger.Reacquire();
  async Task Step(float seconds,Vector2 move){float until=Time.time+seconds,last=Time.time;while(Time.time<until){await Task.Delay(16);float now=Time.time;p.Step(new LocalPlayerInput.Sample{Move=move},Mathf.Min(now-last,.035f));last=now;}}
  var results=new Dictionary<string,bool>();
  try{
   await Step(1.2f,Vector2.up);var inside=boat.transform.InverseTransformPoint(p.transform.position);
   results["enterThroughDoor"]=inside.z>1.7f&&inside.z<2.8f&&p.Grounded;
   float eye=boat.transform.InverseTransformPoint(carry.View.position).y;results["standingHeadroom"]=eye<3.53f&&eye>3;
   await Step(.4f,Vector2.right);var side=boat.transform.InverseTransformPoint(p.transform.position);results["sideWallCollision"]=side.x<.43f&&side.x>.2f;
   await Step(.1f,Vector2.left);carry.View.LookAt(boat.Helm.position);results["insideHelmTarget"]=boat.Target(carry);
   boat.HandleInput(carry,new LocalPlayerInput.Sample{Interact=true});results["insideHelmEnter"]=boat.Driving;
   boat.HandleInput(carry,new LocalPlayerInput.Sample{Interact=true});results["insideHelmExit"]=!boat.Driving&&!p.MovementLocked;
   p.transform.rotation=Quaternion.identity;await Step(.11f,Vector2.left);await Step(.8f,Vector2.down);
   var outside=boat.transform.InverseTransformPoint(p.transform.position);results["leaveThroughDoor"]=outside.z<1.1f&&p.Grounded;
   return new {checks=results,inside=inside.ToString(),side=side.ToString(),outside=outside.ToString(),eyeHeight=eye,renderWidth=Camera.main.pixelWidth,renderHeight=Camera.main.pixelHeight};
  }finally{boat.Release();p.ReturnToSpawn();p.enabled=true;carry.View.localRotation=Quaternion.identity;}
 }
}
