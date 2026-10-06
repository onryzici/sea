using System;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SalvageCrew.EditorTools
{
    public static class WreckExpeditionSetup
    {
        const string Prefabs="Assets/_Game/Prefabs/";
        const string WheelArt="Assets/_Game/Art/ThirdParty/ShipWheel/";
        static T Need<T>(GameObject g) where T:Component {var c=g.GetComponent<T>();return c!=null?c:g.AddComponent<T>();}
        static Transform Child(Transform p,string name)
        {var t=p.Find(name);if(t==null){t=new GameObject(name).transform;t.SetParent(p,false);}return t;}
        static void Ref(SerializedObject s,string n,UnityEngine.Object v)=>s.FindProperty(n).objectReferenceValue=v;
        [MenuItem("SalvageCrew/Expedition/Install Wreck Search")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first.");
            var root=GameObject.Find("HarborPrototypeGenerated");
            var session=UnityEngine.Object.FindAnyObjectByType<HarborSession>();
            var offline=UnityEngine.Object.FindAnyObjectByType<OfflineBoatController>();
            if(root==null||session==null||offline==null)throw new InvalidOperationException("Open HarborPrototype.");
            AssetDatabase.Refresh();
            InstallWheel(offline.transform);
            var boat=PrefabUtility.LoadPrefabContents(Prefabs+"NetworkBoat.prefab");
            try{InstallWheel(boat.transform);PrefabUtility.SaveAsPrefabAsset(boat,Prefabs+"NetworkBoat.prefab");}
            finally{PrefabUtility.UnloadPrefabContents(boat);}
            var world=Child(root.transform,"WreckExpedition");
            var core=Need<WreckExpedition>(world.gameObject);var so=new SerializedObject(core);
            var sites=so.FindProperty("sites");sites.arraySize=2;
            for(int i=0;i<2;i++)
            {
                var site=Child(world,"WreckSite_"+i);
                site.position=i==0?new Vector3(20,0,42):new Vector3(-4,0,78);
                site.rotation=Quaternion.Euler(0,i==0?0:28,0);
                var model=site.Find("WreckVisual");
                if(model==null)
                {
                    model=((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/ThirdParty/Kenney/Pirate/Models/FBX format/ship-wreck.fbx"),site)).transform;
                    model.name="WreckVisual";
                }
                model.localPosition=new Vector3(0,-1.1f,0);model.localScale=Vector3.one*.85f;
                foreach(var r in model.GetComponentsInChildren<MeshRenderer>())
                {r.sharedMaterials=Enumerable.Repeat(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/PiratePalette.mat"),r.sharedMaterials.Length).ToArray();r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;r.receiveShadows=true;}
                foreach(var f in model.GetComponentsInChildren<MeshFilter>())
                {var c=Need<MeshCollider>(f.gameObject);c.sharedMesh=f.sharedMesh;c.convex=false;}
                var row=sites.GetArrayElementAtIndex(i);row.FindPropertyRelative("title").stringValue=i==0?"KIRIK DİREK ENKAZI":"SIĞLIK ENKAZI";
                row.FindPropertyRelative("location").objectReferenceValue=site;
                var spawns=row.FindPropertyRelative("cargoSpawns");spawns.arraySize=3;
                for(int j=0;j<3;j++)
                {var p=Child(site,"CargoSpawn_"+j);p.localPosition=new Vector3(1.1f,1.7f,-1.6f+j*1.6f);spawns.GetArrayElementAtIndex(j).objectReferenceValue=p;}
            }
            foreach(string field in new[]{"offlineCargo","networkCargo"})
            {
                var array=so.FindProperty(field);array.arraySize=3;int i=0;
                foreach(string name in new[]{"LightBox","MetalCrate","ScrapEngine"})array.GetArrayElementAtIndex(i++).objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+(field=="networkCargo"?"Network":"")+name+".prefab");
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            const string path=Prefabs+"NetworkWreckExpedition.prefab";
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(prefab==null)
            {var g=new GameObject("NetworkWreckExpedition",typeof(NetworkObject),typeof(NetworkWreckExpedition));prefab=PrefabUtility.SaveAsPrefabAsset(g,path);UnityEngine.Object.DestroyImmediate(g);}
            var list=AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>("Assets/_Game/Settings/NetworkPrefabs.asset");
            if(!list.Contains(prefab)){list.Add(new NetworkPrefab{Prefab=prefab});EditorUtility.SetDirty(list);}
            var sessionSo=new SerializedObject(session);Ref(sessionSo,"expeditionPrefab",prefab);sessionSo.ApplyModifiedPropertiesWithoutUndo();
            SetupHud(session.transform.Find("ConnectionHUD"));
            EditorSceneManager.MarkSceneDirty(root.scene);EditorSceneManager.SaveScene(root.scene);AssetDatabase.SaveAssets();
        }
        public static void InstallWheel(Transform boat)
        {
            var helm=boat.Find("Helm");if(helm==null)throw new InvalidOperationException("Missing existing helm collider.");
            foreach(var r in boat.GetComponentsInChildren<Renderer>(true))
                if(r.name.Contains("pipe-large-valve"))r.enabled=false;
            var wrapper=Child(boat,"ShipWheelVisual");
            wrapper.localPosition=new Vector3(0,1.52f,2.55f);wrapper.localRotation=Quaternion.Euler(0,180,0);wrapper.localScale=Vector3.one*.59f;
            var model=wrapper.Find("WheelModel");
            if(model==null){model=((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(WheelArt+"ShipsWheel.fbx"),wrapper)).transform;model.name="WheelModel";}
            var sourceModel=AssetDatabase.LoadAssetAtPath<GameObject>(WheelArt+"ShipsWheel.fbx");
            model.localPosition=sourceModel.transform.localPosition;model.localRotation=sourceModel.transform.localRotation;model.localScale=sourceModel.transform.localScale;
            foreach(var r in model.GetComponentsInChildren<Renderer>())
            {
                r.enabled=true;
                r.sharedMaterials=r.sharedMaterials.Select(m=>WheelMaterial(m.name)).ToArray();
                r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;r.receiveShadows=true;
            }
            var rotating=model.GetComponentsInChildren<Transform>().First(x=>x.name=="wheel");
            var visual=Need<HelmWheelVisual>(wrapper.gameObject);var so=new SerializedObject(visual);Ref(so,"wheel",rotating);so.ApplyModifiedPropertiesWithoutUndo();
            // Preserve interaction identity and compound physics; fit the existing interaction collider to the new wheel.
            helm.localPosition=new Vector3(0,2.15f,2.37f);
            var collider=helm.GetComponent<BoxCollider>();collider.size=new Vector3(1.05f,1.15f,.28f);
        }
        static Material WheelMaterial(string name)
        {
            name=name.Replace(" (Instance)","");var path=WheelArt+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            var source=AssetDatabase.LoadAllAssetsAtPath(WheelArt+"ShipsWheel.fbx").OfType<Material>().FirstOrDefault(x=>x.name==name);
            m.color=source!=null?source.color:Color.white;
            if(name=="deck"||name=="oak")m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(WheelArt+"WheelWood.png"));
            m.SetFloat("_Metallic",name=="brass"?.65f:name=="iron"?.5f:0);m.SetFloat("_Smoothness",name=="brass"?.4f:.22f);
            EditorUtility.SetDirty(m);return m;
        }
        static RectTransform Rect(Transform parent,string name,Vector2 pos,Vector2 size)
        {
            var t=parent.Find(name) as RectTransform;
            if(t==null){t=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;t.SetParent(parent,false);}
            t.anchorMin=t.anchorMax=new Vector2(0,1);t.pivot=new Vector2(0,1);t.anchoredPosition=pos;t.sizeDelta=size;return t;
        }
        static void SetupHud(Transform canvas)
        {
            var card=Rect(canvas,"ExpeditionCard",new Vector2(28,-108),new Vector2(450,126));
            var image=Need<UnityEngine.UI.Image>(card.gameObject);image.color=new Color(.035f,.085f,.11f,.91f);image.raycastTarget=false;
            var group=Need<CanvasGroup>(card.gameObject);group.blocksRaycasts=false;group.interactable=false;
            var font=canvas.GetComponentInChildren<TextMeshProUGUI>(true).font;
            TextMeshProUGUI Text(string name,Vector2 pos,Vector2 size,int fontSize)
            {var t=Need<TextMeshProUGUI>(Rect(card,name,pos,size).gameObject);t.font=font;t.fontSize=fontSize;t.color=new Color(.97f,.92f,.8f);t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;return t;}
            var heading=Text("Heading",new Vector2(18,-14),new Vector2(414,30),21);heading.text="ENKAZ SEFERİ";
            var detail=Text("Detail",new Vector2(18,-51),new Vector2(414,64),17);detail.text="F  Sonar taraması · Teknedeyken";
            var hud=Need<WreckSearchHud>(card.gameObject);var so=new SerializedObject(hud);Ref(so,"heading",heading);Ref(so,"detail",detail);Ref(so,"group",group);so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
