using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SalvageCrew.Editor
{
    // Visual-only pass. Gameplay roots, collider geometry, GUIDs and serialized references are retained.
    public static class HarborArtSetup
    {
        const string Base = "Assets/_Game/Art/";
        static Material teal, cream, wood, yellow, black, rust, glass, rope;
        static Transform Root(Transform parent)
        {
            var t=parent.Find("ArtVisuals");
            if(t==null) { t=new GameObject("ArtVisuals").transform; t.SetParent(parent,false); }
            // Only this tool's named visual subtree is replaceable.
            foreach(Transform c in t.Cast<Transform>().ToArray()) Object.DestroyImmediate(c.gameObject);
            return t;
        }
        static Material Mat(string name, Color color, float smooth=.15f)
        {
            string path=Base+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null) { m=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m,path); }
            m.color=color; m.SetFloat("_Smoothness",smooth); EditorUtility.SetDirty(m); return m;
        }
        static GameObject Shape(Transform p,string name,PrimitiveType type,Vector3 pos,Vector3 scale,Material m)
        {
            var g=GameObject.CreatePrimitive(type); g.name=name; Object.DestroyImmediate(g.GetComponent<Collider>());
            g.transform.SetParent(p,false); g.transform.localPosition=pos; g.transform.localScale=scale;
            g.GetComponent<Renderer>().sharedMaterial=m; return g;
        }
        static Material Palette(string pack)
        {
            var m=Mat(pack+"Palette",Color.white);
            var source=AssetDatabase.LoadAssetAtPath<Texture2D>(Base+"ThirdParty/Kenney/"+pack+"/Models/FBX format/Textures/colormap.png");
            m.mainTexture=source;
            if(pack=="Suburban")
            {
                string path=Base+"MediterraneanPalette.asset"; var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if(texture==null)
                {
                    var rt=RenderTexture.GetTemporary(source.width,source.height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
                    var previous=RenderTexture.active; Graphics.Blit(source,rt);RenderTexture.active=rt;
                    texture=new Texture2D(source.width,source.height,TextureFormat.RGBA32,false); texture.ReadPixels(new Rect(0,0,source.width,source.height),0,0);texture.Apply();
                    RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);
                    var pixels=texture.GetPixels();for(int i=0;i<pixels.Length;i++)if(pixels[i].g>pixels[i].r*1.25f && pixels[i].g>pixels[i].b*1.1f) {float light=pixels[i].g;pixels[i]=new Color(light*1.05f,light*.53f,light*.31f,pixels[i].a);}
                    texture.SetPixels(pixels);texture.Apply();texture.name="Warm plaster and terracotta palette";texture.filterMode=FilterMode.Point;AssetDatabase.CreateAsset(texture,path);
                }
                m.mainTexture=texture;
            }
            return m;
        }
        static GameObject Model(Transform p,string pack,string name,Vector3 center,Vector3 size,Material overrideMat=null)
        {
            string path=Base+"ThirdParty/Kenney/"+pack+"/Models/FBX format/"+name+".fbx";
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(asset==null) throw new InvalidOperationException("Missing model "+path);
            var g=Object.Instantiate(asset); g.name=name; g.transform.SetParent(p,false);
            foreach(var c in g.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
            foreach(var c in g.GetComponentsInChildren<MonoBehaviour>(true)) Object.DestroyImmediate(c);
            var renderers=g.GetComponentsInChildren<Renderer>();
            var bounds=renderers[0].bounds; foreach(var r in renderers) bounds.Encapsulate(r.bounds);
            // Temporarily detach for bounds fitting; all caller roots use unit scale.
            g.transform.SetParent(null,true); g.transform.position=Vector3.zero; g.transform.rotation=Quaternion.identity; g.transform.localScale=Vector3.one;
            bounds=renderers[0].bounds; foreach(var r in renderers) bounds.Encapsulate(r.bounds);
            Vector3 ratio=new Vector3(size.x/bounds.size.x,size.y/bounds.size.y,size.z/bounds.size.z);
            g.transform.localScale=ratio; g.transform.position=center-Vector3.Scale(bounds.center,ratio);
            g.transform.SetParent(p,false);
            var mat=overrideMat??Palette(pack);
            foreach(var r in renderers) r.sharedMaterials=Enumerable.Repeat(mat,r.sharedMaterials.Length).ToArray();
            return g;
        }
        static Mesh RingMesh()
        {
            const string path=Base+"Ring.asset"; var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path); if(mesh!=null)return mesh;
            const int n=24,k=8; var v=new Vector3[n*k]; var normals=new Vector3[v.Length]; var tris=new int[n*k*6];
            for(int i=0;i<n;i++) for(int j=0;j<k;j++)
            { float a=i*Mathf.PI*2/n,b=j*Mathf.PI*2/k; int index=i*k+j; normals[index]=new Vector3(Mathf.Cos(a)*Mathf.Cos(b),Mathf.Sin(b),Mathf.Sin(a)*Mathf.Cos(b)); v[index]=new Vector3(Mathf.Cos(a)*(.7f+.22f*Mathf.Cos(b)),.22f*Mathf.Sin(b),Mathf.Sin(a)*(.7f+.22f*Mathf.Cos(b))); int q=index*6, next=((i+1)%n)*k+j, up=i*k+(j+1)%k, diagonal=((i+1)%n)*k+(j+1)%k; tris[q]=index;tris[q+1]=up;tris[q+2]=next;tris[q+3]=next;tris[q+4]=up;tris[q+5]=diagonal; }
            mesh=new Mesh {name="Stylized visual ring",vertices=v,normals=normals,triangles=tris}; mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        static GameObject Ring(Transform p,string name,Vector3 pos,float size,Material m,Vector3 rotation)
        { var g=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=Vector3.one*size;g.transform.localEulerAngles=rotation;g.GetComponent<MeshFilter>().sharedMesh=RingMesh();g.GetComponent<Renderer>().sharedMaterial=m;return g; }
        static void Boat(Transform boat,Vector3 offset)
        {
            var art=Root(boat); art.localPosition=offset;
            foreach(var r in boat.GetComponentsInChildren<MeshRenderer>())
            {
                if(r.transform.IsChildOf(art))continue;
                r.sharedMaterial=r.name.Contains("Window")?glass:r.name.Contains("Hull")?teal:r.name.Contains("Deck")?wood:r.name.Contains("Rail")?teal:cream;
            }
            var oldHull=boat.Find("Hull"); if(oldHull!=null)oldHull.GetComponent<Renderer>().enabled=false;
            string hullPath=Base+"WorkboatHull.asset";var hullMesh=AssetDatabase.LoadAssetAtPath<Mesh>(hullPath);
            if(hullMesh==null)
            {
                var outline=new[]{new Vector2(-1.6f,-5.1f),new Vector2(1.6f,-5.1f),new Vector2(2.06f,-4.7f),new Vector2(2.06f,4.6f),new Vector2(1.35f,5.3f),new Vector2(0,5.6f),new Vector2(-1.35f,5.3f),new Vector2(-2.06f,4.6f),new Vector2(-2.06f,-4.7f)};
                var vertices=new Vector3[outline.Length*4];var triangles=new int[outline.Length*6];
                for(int i=0;i<outline.Length;i++) {var a=outline[i];var b=outline[(i+1)%outline.Length];int v=i*4,q=i*6;vertices[v]=new Vector3(a.x,1.38f,a.y);vertices[v+1]=new Vector3(b.x,1.38f,b.y);vertices[v+2]=new Vector3(b.x*.78f,.04f,b.y*.96f);vertices[v+3]=new Vector3(a.x*.78f,.04f,a.y*.96f);triangles[q]=v;triangles[q+1]=v+1;triangles[q+2]=v+2;triangles[q+3]=v;triangles[q+4]=v+2;triangles[q+5]=v+3;}
                hullMesh=new Mesh {name="Working boat visual hull",vertices=vertices,triangles=triangles};hullMesh.RecalculateNormals();hullMesh.RecalculateBounds();AssetDatabase.CreateAsset(hullMesh,hullPath);
            }
            var hull=new GameObject("Tapered workboat hull",typeof(MeshFilter),typeof(MeshRenderer));hull.transform.SetParent(art,false);hull.GetComponent<MeshFilter>().sharedMesh=hullMesh;hull.GetComponent<Renderer>().sharedMaterial=teal;
            // Existing 10 x 4 m deck and compound colliders remain untouched. A tapered bow is visual only.
            for(int i=0;i<20;i++) Shape(art,"Deck board "+i,PrimitiveType.Cube,new Vector3(0,1.51f,-4.75f+i*.5f),new Vector3(3.97f,.018f,.475f),i%3==0?Mat("TimberLight",new Color(.48f,.30f,.16f)):wood);
            for(int side=-1;side<=1;side+=2)
            {
                for(int i=0;i<3;i++) Ring(art,"Tire fender",new Vector3(side*2.07f,.98f,-3.5f+i*2.8f),.42f,black,new Vector3(0,0,90));
                Shape(art,"Gunwale trim",PrimitiveType.Cube,new Vector3(side*1.94f,1.55f,0),new Vector3(.1f,.12f,9.85f),cream);
                for(int i=0;i<8;i++) Shape(art,"Paint wear",PrimitiveType.Cube,new Vector3(side*2.004f,.8f+(i%3)*.16f,-4.1f+i*1.05f),new Vector3(.008f,.055f,.16f+(i%2)*.1f),rust);
            }
            Shape(art,"Cabin fascia",PrimitiveType.Cube,new Vector3(0,3.73f,2.4f),new Vector3(2.38f,.1f,2.6f),teal);
            Shape(art,"Exhaust",PrimitiveType.Cylinder,new Vector3(.7f,4.0f,3.0f),new Vector3(.16f,.3f,.16f),black);
            Shape(art,"Navigation mast",PrimitiveType.Cylinder,new Vector3(0,4.35f,2.6f),new Vector3(.055f,.6f,.055f),yellow);
            Shape(art,"Mast crossbar",PrimitiveType.Cube,new Vector3(0,4.62f,2.6f),new Vector3(1.1f,.055f,.055f),yellow);
            Ring(art,"Lifebuoy",new Vector3(1.125f,2.65f,2.0f),.42f,Mat("SafetyOrange",new Color(.94f,.28f,.065f)),new Vector3(0,0,90));
            for(int i=0;i<3;i++) Ring(art,"Rope coil",new Vector3(-1.4f,1.55f+i*.025f,3.7f),.3f-i*.025f,rope,Vector3.zero);
            // Two bow-side cheeks soften the blockout silhouette without obstructing the walking surface.
            var cheek=Shape(art,"Bow cheek port",PrimitiveType.Cube,new Vector3(-1.55f,.85f,4.75f),new Vector3(.7f,.65f,.55f),teal);cheek.transform.localEulerAngles=new Vector3(0,-20,0);
            cheek=Shape(art,"Bow cheek starboard",PrimitiveType.Cube,new Vector3(1.55f,.85f,4.75f),new Vector3(.7f,.65f,.55f),teal);cheek.transform.localEulerAngles=new Vector3(0,20,0);
            var helm=boat.Find("Helm"); if(helm!=null) { helm.GetComponent<Renderer>().sharedMaterial=yellow; Ring(art,"Wheel",new Vector3(0,2.38f,1.12f),.25f,black,new Vector3(90,0,0)); }
        }
        static void Scrap(Transform item,string name)
        {
            var existingArt=item.Find("ArtVisuals");
            foreach(var r in item.GetComponentsInChildren<MeshRenderer>()) if(existingArt==null || !r.transform.IsChildOf(existingArt))r.enabled=false;
            var art=Root(item); art.localPosition=Vector3.zero;
            if(name.Contains("Light")) Model(art,"Pirate","crate",new Vector3(0,.02f,0),new Vector3(.6f,.55f,.6f));
            else if(name.Contains("Metal")) Model(art,"Factory","box-large",Vector3.zero,new Vector3(.85f,.7f,.65f),Mat("CrateOchre",new Color(.66f,.46f,.16f)));
            else { Model(art,"Factory","machine",Vector3.zero,new Vector3(.85f,.58f,.58f),Mat("EngineMetal",new Color(.26f,.32f,.31f)));Model(art,"Factory","pipe-large-valve",new Vector3(.15f,.34f,0),new Vector3(.35f,.22f,.3f),rust); }
        }
        static void Hud(Transform root)
        {
            foreach(var label in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if(label.name=="Controls") { label.text="WASD Yürü  ·  Shift Koş  ·  Space Zıpla\nE Tut / Bırak / Dümen  ·  R Kurtar\nEsc İmleç  ·  Tıkla Devam  ·  Tab Bağlantı"; label.fontSize=17; label.enableAutoSizing=false; }
                if(label.name=="Role") { label.fontSize=18; label.rectTransform.sizeDelta=new Vector2(520,32); }
            }
            foreach(var t in root.GetComponentsInChildren<RectTransform>(true)) if(t.name=="ControlsPanel")
            { t.anchorMin=t.anchorMax=t.pivot=Vector2.zero;t.anchoredPosition=new Vector2(20,20);t.sizeDelta=new Vector2(455,94);var img=t.GetComponent<Image>();if(img!=null)img.color=new Color(.025f,.09f,.1f,.76f); }
        }
        [MenuItem("SalvageCrew/Apply Harbor Art Pass")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play before applying art.");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene(); if(scene.name!="HarborPrototype")throw new InvalidOperationException("Open HarborPrototype first.");
            teal=Mat("WeatheredTeal",new Color(.075f,.44f,.43f));cream=Mat("WarmIvory",new Color(.86f,.83f,.68f));wood=Mat("DeckTimber",new Color(.36f,.22f,.105f));yellow=Mat("WorkYellow",new Color(.93f,.62f,.10f));black=Mat("Rubber",new Color(.045f,.055f,.055f));rust=Mat("Rust",new Color(.43f,.22f,.10f));glass=Mat("CabinGlass",new Color(.13f,.38f,.44f),.65f);rope=Mat("Rope",new Color(.67f,.54f,.33f));
            var generated=scene.GetRootGameObjects().First(g=>g.name=="HarborPrototypeGenerated").transform;
            Boat(generated.Find("StaticBoat_10m_x_4m"),new Vector3(7,0,10));
            string[] prefabs={"NetworkBoat","LightBox","MetalCrate","ScrapEngine","NetworkLightBox","NetworkMetalCrate","NetworkScrapEngine","NetworkFirstPersonPlayer"};
            foreach(string name in prefabs) { string path="Assets/_Game/Prefabs/"+name+".prefab";var p=PrefabUtility.LoadPrefabContents(path);try {if(name=="NetworkBoat")Boat(p.transform,Vector3.zero);else if(name.Contains("FirstPerson"))Hud(p.transform);else Scrap(p.transform,name);PrefabUtility.SaveAsPrefabAsset(p,path);}finally{PrefabUtility.UnloadPrefabContents(p);} }
            foreach(Transform t in generated.Find("ScrapTestItems")) Scrap(t,t.name);
            var harbor=generated.Find("Harbor"); var environment=Root(harbor); environment.localPosition=Vector3.zero;
            harbor.Find("Pier").GetComponent<Renderer>().sharedMaterial=wood;
            harbor.Find("Shore").GetComponent<Renderer>().sharedMaterial=Mat("CoastalSand",new Color(.71f,.62f,.43f));
            harbor.Find("Quay").GetComponent<Renderer>().sharedMaterial=Mat("QuayLimestone",new Color(.69f,.68f,.57f));
            for(int i=0;i<6;i++) Model(environment,"Pirate","structure-platform-dock-small",new Vector3(0,.72f,-.4f+i*1.75f),new Vector3(4.8f,.95f,1.72f),wood);
            for(int i=0;i<7;i++) { float x=i<4?18+i*5:-18-(i-4)*6;float z=23+(i%3)*7;Model(environment,"Pirate",i%2==0?"rocks-a":"rocks-b",new Vector3(x,1.8f,z),new Vector3(9,4.5f,8),Mat("CoastalRock",new Color(.61f,.60f,.49f))); }
            for(int i=0;i<5;i++) Model(environment,"Suburban",i%2==0?"building-type-a":"building-type-c",new Vector3(-13+i*6,3,-20-(i%2)*4),new Vector3(4.5f,5.5f,5));
            for(int i=0;i<4;i++) Model(environment,"Suburban",i%2==0?"building-type-c":"building-type-a",new Vector3(19+i*5,5.6f,31+(i%2)*3),new Vector3(4,4.5f,4));
            Shape(environment,"Coastal island base",PrimitiveType.Sphere,new Vector3(26,-.35f,32),new Vector3(28,8,22),Mat("CoastalRock",new Color(.61f,.60f,.49f)));
            Shape(environment,"Village terrace",PrimitiveType.Cube,new Vector3(26,3.18f,32.5f),new Vector3(20,.34f,8),cream);
            Model(environment,"Watercraft","boat-fishing-small",new Vector3(-13,.6f,17),new Vector3(2.5f,2.5f,6.5f));
            Model(environment,"Watercraft","buoy",new Vector3(17,.5f,10),new Vector3(.8f,2,.8f));
            Model(environment,"Pirate","barrel",new Vector3(-6,1.1f,-5),new Vector3(.7f,1,.7f));
            Shape(environment,"Lighthouse",PrimitiveType.Cylinder,new Vector3(30,7,31),new Vector3(2,4,2),cream);
            Shape(environment,"Beacon roof",PrimitiveType.Cylinder,new Vector3(30,11.2f,31),new Vector3(2.5f,.2f,2.5f),rust);
            Shape(environment,"Beacon glass",PrimitiveType.Cylinder,new Vector3(30,10.6f,31),new Vector3(1.7f,.4f,1.7f),yellow);
            var shader=Shader.Find("SalvageCrew/Calm Harbor Water");if(shader==null)throw new InvalidOperationException("Water shader missing.");
            string waterPath=Base+"CalmHarborWater.mat";var water=AssetDatabase.LoadAssetAtPath<Material>(waterPath);if(water==null){water=new Material(shader);AssetDatabase.CreateAsset(water,waterPath);}harbor.Find("Sea_VisualOnly").GetComponent<Renderer>().sharedMaterial=water;
            var sun=generated.Find("Daylight").GetComponent<Light>();sun.color=new Color(1,.90f,.74f);sun.intensity=1.65f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(38,-35,0);
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.53f,.68f,.76f);RenderSettings.ambientEquatorColor=new Color(.48f,.50f,.48f);RenderSettings.ambientGroundColor=new Color(.27f,.24f,.17f);RenderSettings.fog=false;
            Hud(generated);Hud(scene.GetRootGameObjects().First(g=>g.name=="HarborNetworkSession").transform);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
    }
}
