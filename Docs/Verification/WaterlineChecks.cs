using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using SalvageCrew;
public static class WaterlineChecks
{
 public static async Task<object> Run()
 {
  if(!Application.isPlaying)throw new InvalidOperationException("Play required");
  var p=UnityEngine.Object.FindAnyObjectByType<FirstPersonMotor>();var cc=p.GetComponent<CharacterController>();
  var hull=UnityEngine.Object.FindAnyObjectByType<CalmWaterBuoyancy>();
  HarborSession.Instance.SetPanelOpen(false);p.GetComponent<PhysicsCarry>().Drop();
  foreach(var item in UnityEngine.Object.FindObjectsByType<ScrapItem>())item.Recover();
  p.ReturnToSpawn();p.enabled=false;cc.enabled=false;p.transform.SetPositionAndRotation(new Vector3(0,1.25f,7.5f),Quaternion.Euler(0,90,0));cc.enabled=true;Physics.SyncTransforms();
  async Task Walk(float seconds,Vector2 move){float until=Time.time+seconds,last=Time.time;while(Time.time<until){await Task.Delay(16);float now=Time.time;p.Step(new LocalPlayerInput.Sample{Move=move},Mathf.Min(now-last,.04f));last=now;}}
  try{
   await Walk(1.8f,Vector2.up);var deck=p.transform.position;var local=hull.transform.InverseTransformPoint(deck);bool onDeck=p.Grounded&&local.y>1.4f&&local.y<1.75f&&Mathf.Abs(local.x)<1.9f;
   p.transform.rotation=Quaternion.Euler(0,270,0);await Walk(1.8f,Vector2.up);var pier=p.transform.position;bool back=p.Grounded&&pier.x<2.2f&&pier.y>1.1f;
   return new{checks=new{rootAtWaterline=Mathf.Abs(hull.Body.position.y+.65f)<.15f,walkRampToDeck=onDeck,walkBackToPier=back},rootY=hull.Body.position.y,deck=deck.ToString(),deckLocal=local.ToString(),pier=pier.ToString()};
  }finally{p.ReturnToSpawn();p.enabled=true;}
 }
}
