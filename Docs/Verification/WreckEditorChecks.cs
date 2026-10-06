using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Unity.Netcode;
using SalvageCrew;
using SalvageCrew.EditorTools;
public static class WreckEditorChecks
{
 public static object Run()
 {
  if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");
  var root=GameObject.Find("HarborPrototypeGenerated");var session=UnityEngine.Object.FindAnyObjectByType<HarborSession>();
  var helm=UnityEngine.Object.FindAnyObjectByType<OfflineBoatController>().Helm;
  var s=new SerializedObject(session);var player=s.FindProperty("offlinePlayer").objectReferenceValue;
  int before=root.GetComponentsInChildren<Transform>(true).Length;
  WreckExpeditionSetup.Apply();
  int after=root.GetComponentsInChildren<Transform>(true).Length;
  var mission=UnityEngine.Object.FindAnyObjectByType<WreckExpedition>();
  var list=AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>("Assets/_Game/Settings/NetworkPrefabs.asset");
  var net=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/NetworkWreckExpedition.prefab");
  var wheel=UnityEngine.Object.FindAnyObjectByType<OfflineBoatController>().transform.Find("ShipWheelVisual");
  var renderers=wheel.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
  var result=new{before,after,noDuplicates=before==after,sites=mission.Sites.Length,spawns=mission.Sites.Sum(x=>x.cargoSpawns.Length),
   sameHelm=helm==UnityEngine.Object.FindAnyObjectByType<OfflineBoatController>().Helm,samePlayer=new SerializedObject(session).FindProperty("offlinePlayer").objectReferenceValue==player,
   registered=list.Contains(net),wheelWidth=bounds.size.x,wheelHeight=bounds.size.y,
   materialsValid=renderers.All(r=>r.sharedMaterials.All(m=>m!=null&&m.shader!=null&&m.shader.isSupported)),
   singleMissionHud=UnityEngine.Object.FindObjectsByType<WreckSearchHud>().Length==1};
  System.IO.File.WriteAllText("Docs/Verification/WreckEditorResults.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));
  return result;
 }
}
