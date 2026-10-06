using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SalvageCrew.EditorTools
{
    // Incremental dressing: owns only CrewHarborDetails, CrewCard and CrewMarker.
    public static class CrewHarborSetup
    {
        const string Art = "Assets/_Game/Art/";
        const string Pirate = Art + "ThirdParty/Kenney/Pirate/Models/FBX format/";
        const string Crew = Art + "ThirdParty/KayKitCrew/Rogue.fbx";
        static T Need<T>(GameObject g) where T : Component { var c=g.GetComponent<T>(); return c!=null?c:g.AddComponent<T>(); }
        static void Ref(SerializedObject s,string n,Object v) => s.FindProperty(n).objectReferenceValue=v;
        static Transform Child(Transform parent,string name)
        { var t=parent.Find(name);if(t==null){t=new GameObject(name).transform;t.SetParent(parent,false);}return t; }
        [MenuItem("SalvageCrew/Art/Install Crew Presence and Harbor Details")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.name!="HarborPrototype")throw new InvalidOperationException("Open HarborPrototype");
            AssetDatabase.Refresh();
            Dress(GameObject.Find("Harbor").transform);
            SetupHud(GameObject.Find("ConnectionHUD").transform);
            SetupCrew();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        static GameObject Prop(Transform parent,string name,string path,Vector3 basePoint,Vector3 size,Material material,float yaw=0,bool solid=true)
        {
            var root=Child(parent,name);root.SetPositionAndRotation(Vector3.zero,Quaternion.identity);root.localScale=Vector3.one;
            if(root.childCount==0)
            {
                var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(asset==null)throw new InvalidOperationException(path);
                var model=Object.Instantiate(asset,root);model.name="ImportedVisual";
                foreach(var c in model.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);
                foreach(var c in model.GetComponentsInChildren<MonoBehaviour>())Object.DestroyImmediate(c);
                foreach(var c in model.GetComponentsInChildren<Animator>())Object.DestroyImmediate(c);
            }
            var rs=root.GetComponentsInChildren<Renderer>();
            Bounds Bounds(){var b=rs[0].bounds;foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);return b;}
            var bounds=Bounds();root.localScale=new Vector3(size.x/bounds.size.x,size.y/bounds.size.y,size.z/bounds.size.z);
            bounds=Bounds();var offset=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            root.rotation=Quaternion.Euler(0,yaw,0);root.position=basePoint-root.rotation*offset;
            foreach(var r in rs)
            {r.sharedMaterials=Enumerable.Repeat(material,r.sharedMaterials.Length).ToArray();r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;}
            if(solid)foreach(var f in root.GetComponentsInChildren<MeshFilter>())Need<MeshCollider>(f.gameObject).sharedMesh=f.sharedMesh;
            return root.gameObject;
        }
        static void Dress(Transform harbor)
        {
            var root=Child(harbor,"CrewHarborDetails");
            var wood=AssetDatabase.LoadAssetAtPath<Material>(Art+"PaintedDock.mat");
            var palette=AssetDatabase.LoadAssetAtPath<Material>(Art+"PiratePalette.mat");
            var kay=AssetDatabase.LoadAssetAtPath<Material>(Art+"KayKitHarbor.mat");
            void P(string n,string f,float x,float y,float z,float w,float h,float d,float yaw=0,bool solid=true)
                =>Prop(root,n,Pirate+f+".fbx",new Vector3(x,y,z),new Vector3(w,h,d),palette,yaw,solid);
            // A walkable supply berth beyond the existing pier; the original boarding corridor is untouched.
            Prop(root,"SupplyBerth",Art+"ThirdParty/Loafbrr/Floor_Dock.fbx",new Vector3(-.5f,.9f,12.5f),new Vector3(6,.3f,5),wood);
            Prop(root,"SupplyBerthPiles",Art+"ThirdParty/Loafbrr/Floor_Support.fbx",new Vector3(-.5f,-1.4f,12.5f),new Vector3(5.7f,2.3f,4.5f),wood);
            P("SupplyShelter","structure",-.7f,1.2f,13.5f,4.7f,3.1f,2.8f);
            P("SupplyRoof","structure-roof",-.7f,4.3f,13.5f,5.1f,1.05f,3.3f);
            P("BerthFlag","flag-high",-3,1.2f,10.5f,.65f,3.7f,.7f,30,false);
            P("SupplyCrateA","crate",-1.9f,1.2f,13.25f,.85f,.85f,.85f,8);
            P("SupplyCrateB","crate",-1,1.2f,13.4f,.85f,.85f,.85f,-5);
            P("SupplyCrateStack","crate-bottles",-1.8f,2.05f,13.25f,.65f,.55f,.65f,12);
            P("SupplyBarrelA","barrel",.7f,1.2f,14,.65f,.95f,.65f);
            P("SupplyBarrelB","barrel",1.35f,1.2f,13.5f,.65f,.95f,.65f,21);
            P("SupplyChest","chest",-.1f,1.2f,14.1f,.95f,.65f,.6f);
            P("PierCargo","crate-bottles",-1.85f,1.21f,6.7f,.75f,.65f,.8f,12);
            P("PierBarrel","barrel",-1.85f,1.21f,5.75f,.65f,.95f,.65f);
            P("Tender","boat-row-small",-5.8f,-.3f,10.8f,1.6f,.8f,3.7f,-12);
            P("TenderPaddle","tool-paddle",-5.65f,.3f,10.7f,.16f,.12f,2.3f,15,false);
            // Shore-side working yard, not clutter in the player/cargo route.
            for(int i=0;i<4;i++)P("WarehouseCrate"+i,"crate",-5.5f+(i%2)*.94f,1.21f+(i/2)*.8f,-4.9f,.8f,.8f,.8f,i*7);
            for(int i=0;i<3;i++)P("QuayBarrel"+i,"barrel",5+i*.8f,1.21f,-4.7f,.68f,.98f,.68f,i*17);
            P("QuayProvisions","crate-bottles",7,1.21f,-3.5f,.82f,.65f,.8f,12);
            Prop(root,"HarborHouse",Art+"ThirdParty/KayKit/building_home_B_red.fbx",new Vector3(5.5f,1.2f,-11),new Vector3(5,5.4f,5),kay,180);
            Prop(root,"HarborWorkshop",Art+"ThirdParty/KayKit/building_home_A_red.fbx",new Vector3(11.5f,1.2f,-15.5f),new Vector3(5,4.3f,4),kay,155);
            P("YardShelter","structure",3.5f,1.2f,-6.5f,3.4f,2.4f,2.4f);
            P("YardRoof","structure-roof",3.5f,3.6f,-6.5f,3.8f,.8f,2.8f);
            P("YardTools","chest",3.5f,1.2f,-6.6f,1.2f,.75f,.8f);
            // Reuse the already licensed painted rock/palm materials (no flat colour stand-ins).
            // Imported palms have separate bark/leaf materials; clone an existing fitted visual intact instead.
            var palmSource=harbor.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name=="PalmTree_1"&&t.parent.name=="AssetFit");
            if(palmSource!=null)for(int i=0;i<3;i++)
            {
                string n="YardPalm"+i;var existing=root.Find(n);
                if(existing==null){existing=Object.Instantiate(palmSource.parent,root);existing.name=n;}
                existing.position=new Vector3(i==0?-11:i==1?8:12,.15f,i==0?-13:i==1?-16:-18);
                existing.rotation=Quaternion.Euler(0,i*74,0);
                // Use the source's fitted scale, but move its mesh bounds base to the ground.
                var rs=existing.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);
                existing.position+=new Vector3(existing.position.x-b.center.x,.15f-b.min.y,existing.position.z-b.center.z);
            }
            for(int i=0;i<7;i++)
                Prop(root,"ShoreRock"+i,Art+"ThirdParty/Rubberduck/rock"+(i%3+1)+".obj",new Vector3(-13+i*4.6f,.55f,-19.5f),new Vector3(2.5f+i%2,1.5f+i%3*.35f,2.4f),AssetDatabase.LoadAssetAtPath<Material>(Art+"PaintedRock"+(i%3+1)+".mat"),i*39);
            Label(root,"SupplySign","İKMAL İSKELESİ",new Vector3(-.7f,3.85f,11.95f),Quaternion.identity,5);
            Label(root,"QuaySign","SALVAGE CREW\nKURTARMA LİMANI",new Vector3(3.5f,3.1f,-5.2f),Quaternion.Euler(0,180,0),4);
        }
        static void Label(Transform root,string name,string text,Vector3 pos,Quaternion rot,float width)
        {
            var t=Child(root,name);t.SetPositionAndRotation(pos,rot);
            var label=Need<TextMeshPro>(t.gameObject);label.text=text;label.font=GameObject.Find("ConnectionHUD").GetComponentInChildren<TextMeshProUGUI>(true).font;
            label.fontSize=2.4f;label.alignment=TextAlignmentOptions.Center;label.color=new Color(1,.9f,.65f);label.rectTransform.sizeDelta=new Vector2(width,.6f);
        }
        static RectTransform Rect(Transform p,string name,Vector2 anchor,Vector2 pos,Vector2 size)
        {var t=p.Find(name) as RectTransform;if(t==null){t=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;t.SetParent(p,false);}t.anchorMin=t.anchorMax=t.pivot=anchor;t.anchoredPosition=pos;t.sizeDelta=size;return t;}
        static void SetupHud(Transform canvas)
        {
            var card=Rect(canvas,"CrewCard",new Vector2(1,1),new Vector2(-28,-108),new Vector2(314,198));
            var bg=Need<UnityEngine.UI.Image>(card.gameObject);bg.color=new Color(.035f,.085f,.11f,.91f);bg.raycastTarget=false;
            var group=Need<CanvasGroup>(card.gameObject);group.blocksRaycasts=false;
            var font=canvas.GetComponentInChildren<TextMeshProUGUI>(true).font;
            TextMeshProUGUI Text(Transform p,string n,Vector2 anchor,Vector2 pos,Vector2 size,float fs)
            {var t=Need<TextMeshProUGUI>(Rect(p,n,anchor,pos,size).gameObject);t.font=font;t.fontSize=fs;t.color=new Color(.97f,.92f,.8f);t.raycastTarget=false;return t;}
            var h=Text(card,"Heading",new Vector2(0,1),new Vector2(16,-12),new Vector2(282,26),19);
            var r=Text(card,"Roster",new Vector2(0,1),new Vector2(16,-46),new Vector2(282,144),18);
            var marker=Text(canvas,"CrewMarker",new Vector2(.5f,.5f),Vector2.zero,new Vector2(360,52),19);marker.alignment=TextAlignmentOptions.Center;
            var c=Need<CrewPresence>(card.gameObject);var so=new SerializedObject(c);
            Ref(so,"heading",h);Ref(so,"roster",r);Ref(so,"marker",marker);Ref(so,"markerRect",marker.rectTransform);Ref(so,"canvasRect",canvas);Ref(so,"group",group);so.ApplyModifiedPropertiesWithoutUndo();
        }
        static void SetupCrew()
        {
            const string controllerPath="Assets/_Game/Settings/CrewLocomotion.controller";
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if(controller==null)
            {
                controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);controller.AddParameter("Speed",AnimatorControllerParameterType.Float);
                var state=controller.layers[0].stateMachine.AddState("Locomotion");controller.layers[0].stateMachine.defaultState=state;
                var tree=new BlendTree{name="Idle Walk Run",blendParameter="Speed",useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(tree,controller);
                var clips=AssetDatabase.LoadAllAssetsAtPath(Crew).OfType<AnimationClip>().ToArray();
                foreach(var pair in new[]{("Idle",0f),("Walking_A",2.5f),("Running_A",5.5f)})
                {
                    var source=clips.First(c=>c.name==pair.Item1);var copy=Object.Instantiate(source);copy.name="Crew"+pair.Item1;
                    var settings=AnimationUtility.GetAnimationClipSettings(copy);settings.loopTime=true;settings.loopBlend=true;AnimationUtility.SetAnimationClipSettings(copy,settings);
                    AssetDatabase.AddObjectToAsset(copy,controller);tree.AddChild(copy,pair.Item2);
                }
                state.motion=tree;EditorUtility.SetDirty(controller);
            }
            string path="Assets/_Game/Prefabs/NetworkFirstPersonPlayer.prefab";var p=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var model=p.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Rogue(Clone)");
                var animator=Need<Animator>(model.gameObject);animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                // Source includes weapons as optional attachments. They are not salvage equipment.
                foreach(var t in model.GetComponentsInChildren<Transform>(true))if(new[]{"Knife_Offhand","1H_Crossbow","2H_Crossbow","Knife","Throwable"}.Contains(t.name))t.gameObject.SetActive(false);
                // The old axis-by-axis fit included weapon extents and squeezed the body/head.
                // Fit height uniformly using the visible unarmed character, preserving its proportions.
                var fit=model.parent.parent;fit.localScale=Vector3.one;fit.localPosition=Vector3.zero;
                var bodyRenderers=model.GetComponentsInChildren<Renderer>();
                var bounds=bodyRenderers[0].bounds;foreach(var r in bodyRenderers.Skip(1))bounds.Encapsulate(r.bounds);
                float scale=1.76f/bounds.size.y;
                fit.localScale=Vector3.one*scale;
                fit.localPosition=new Vector3(-bounds.center.x,-bounds.min.y,-bounds.center.z)*scale;
                var presentation=Need<RemoteCrewAnimation>(model.gameObject);var so=new SerializedObject(presentation);Ref(so,"player",p.GetComponent<NetworkCrewPlayer>());Ref(so,"animator",animator);so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(p,path);
            }
            finally{PrefabUtility.UnloadPrefabContents(p);}
        }
    }
}
