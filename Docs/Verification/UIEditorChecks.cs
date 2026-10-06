using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using SalvageCrew;

public static class UIEditorChecks
{
    public static object Run()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play");
        SalvageCrew.EditorTools.HarborHudStyle.Apply();
        int before=UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include).Length;
        var session=UnityEngine.Object.FindAnyObjectByType<HarborSession>();
        var fields=new[]{"panel","address","port","statusLabel","roleLabel","hostButton","clientButton","disconnectButton","resumeButton"};
        var so=new SerializedObject(session);
        var ids=fields.Select(f=>so.FindProperty(f).objectReferenceValue.GetEntityId()).ToArray();
        SalvageCrew.EditorTools.HarborHudStyle.Apply();so.Update();
        int after=UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include).Length;
        bool same=ids.SequenceEqual(fields.Select(f=>so.FindProperty(f).objectReferenceValue.GetEntityId()));
        var textures=new[]{"Cargo","Helm","Anchor"}.Select(name=>{
            var tex=new Texture2D(2,2);ImageConversion.LoadImage(tex,System.IO.File.ReadAllBytes("Assets/_Game/Art/UI/"+name+".png"));
            var pixels=tex.GetPixels32();bool transparent=pixels.Any(p=>p.a==0),opaque=pixels.Any(p=>p.a==255);
            UnityEngine.Object.DestroyImmediate(tex);return new{name,transparent,opaque};
        }).ToArray();
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/NetworkFirstPersonPlayer.prefab");
        var hud=prefab.GetComponentInChildren<HarborHudPresentation>(true);
        var presentation=new SerializedObject(hud);
        bool wired=new[]{"prompt","promptGroup","controlsGroup","icon","reticle","cargoIcon","helmIcon","cursorHint","controlsLabel"}.All(f=>presentation.FindProperty(f).objectReferenceValue!=null);
        if(before!=after||!same||!wired||textures.Any(t=>!t.transparent||!t.opaque))throw new Exception("UI integrity failed");
        var result=new{before,after,idempotent=before==after,sessionReferencesPreserved=same,networkPresentationWired=wired,textures};
        System.IO.File.WriteAllText("Docs/Verification/UIEditorResults.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));
        return result;
    }
}
