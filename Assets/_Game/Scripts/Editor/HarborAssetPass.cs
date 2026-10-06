using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object=UnityEngine.Object;

namespace SalvageCrew.Editor
{
    // Author-created asset placement only. Physics/NGO components remain on their existing roots.
    public static class HarborAssetPass
    {
        const string Art="Assets/_Game/Art/";
        const string Kay=Art+"ThirdParty/KayKit/";
        const string Ken=Art+"ThirdParty/Kenney/";
        static Material kay,pirate,boatMat;
        static Transform Root(Transform parent)
        {
            var t=parent.Find("ArtVisuals");if(t==null){t=new GameObject("ArtVisuals").transform;t.SetParent(parent,false);}
            foreach(Transform c in t.Cast<Transform>().ToArray())Object.DestroyImmediate(c.gameObject);
            t.localPosition=Vector3.zero;t.localRotation=Quaternion.identity;t.localScale=Vector3.one;return t;
        }
        static Material Mat(string name,string texture,Color tint,float smooth=.25f)
        {
            string path=Art+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.shader=Shader.Find("Universal Render Pipeline/Lit");m.color=tint;m.SetTexture("_BaseMap",string.IsNullOrEmpty(texture)?null:AssetDatabase.LoadAssetAtPath<Texture2D>(texture));m.SetFloat("_Smoothness",smooth);EditorUtility.SetDirty(m);return m;
        }
        static Bounds Bounds(GameObject g)
        {var rs=g.GetComponentsInChildren<Renderer>(true);var b=rs[0].bounds;foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);return b;}
        static GameObject Model(Transform parent,string path,Vector3 center,Vector3 size,Material material,float yaw=0)
        {
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(asset==null)throw new InvalidOperationException("Missing asset: "+path);
            var wrapper=new GameObject(System.IO.Path.GetFileNameWithoutExtension(path));
            var g=Object.Instantiate(asset);g.transform.SetParent(wrapper.transform,false);
            foreach(var c in g.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var c in g.GetComponentsInChildren<MonoBehaviour>(true))Object.DestroyImmediate(c);
            foreach(var a in g.GetComponentsInChildren<Animator>(true))Object.DestroyImmediate(a);
            wrapper.transform.rotation=Quaternion.Euler(0,yaw,0);var b=Bounds(wrapper);
            // Fit on world axes, retaining the FBX's coordinate-system conversion transform.
            var fit=new GameObject("AssetFit").transform;wrapper.transform.SetParent(fit,false);
            float Fit(float target,float extent)=>extent>1e-6f?target/extent:1;
            fit.localScale=new Vector3(Fit(size.x,b.size.x),Fit(size.y,b.size.y),Fit(size.z,b.size.z));
            b=Bounds(wrapper);fit.position=center-b.center;fit.SetParent(parent,false);
            if(material!=null)foreach(var r in g.GetComponentsInChildren<Renderer>(true))r.sharedMaterials=Enumerable.Repeat(material,r.sharedMaterials.Length).ToArray();
            return fit.gameObject;
        }
        static void Boat(Transform t,Vector3 offset)
        {
            var root=Root(t);root.localPosition=offset;
            foreach(var r in t.GetComponentsInChildren<MeshRenderer>(true))if(!r.transform.IsChildOf(root))r.enabled=false;
            var ivory=Mat("CabinInteriorIvory",null,new Color(.78f,.74f,.63f),.16f);
            var floor=Mat("PaintedDock",Art+"ThirdParty/Loafbrr/DockTrim_Diffuse.png",new Color(1,.91f,.78f),.16f);
            var model=Model(root,Art+"ThirdParty/UserBoat/TurquoiseHarborTug.fbx",new Vector3(0,2.25f,0),new Vector3(4.15f,4.98f,10.0f),null,-90);
            foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>m!=null&&m.name.Contains("CabinInteriorIvory")?ivory:m!=null&&m.name.Contains("CabinInteriorTimber")?floor:boatMat).ToArray();
            CabinCollision(t);
            var helm=t.Find("Helm");
            if(helm==null){helm=new GameObject("Helm").transform;helm.SetParent(t,false);helm.localPosition=new Vector3(0,2.3f,1.02f);helm.gameObject.AddComponent<BoxCollider>().size=new Vector3(.7f,.7f,.35f);}
            helm.localPosition=new Vector3(0,2.3f,2.94f);
            var controls=Mat("HelmControls",Ken+"Factory/Models/FBX format/Textures/colormap.png",new Color(1,.85f,.5f),.25f);
            if(t.Find("ShipWheelVisual")!=null) SalvageCrew.EditorTools.WreckExpeditionSetup.InstallWheel(t);
            else Model(root,Ken+"Factory/Models/FBX format/pipe-large-valve.fbx",helm.localPosition,new Vector3(.55f,.55f,.25f),controls);
        }
        static void CabinCollision(Transform boat)
        {
            // Replace the old solid cabin block with a compound walk-through shell.
            foreach(string name in new[]{"Cabin","CabinRoof"}){
                var old=boat.Find(name);if(old!=null)foreach(var c in old.GetComponents<Collider>())c.enabled=false;
            }
            var root=boat.Find("CabinInteriorPhysics");
            if(root==null){root=new GameObject("CabinInteriorPhysics").transform;root.SetParent(boat,false);}
            foreach(Transform child in root.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
            void Box(string name,Vector3 center,Vector3 size){var g=new GameObject(name);g.transform.SetParent(root,false);g.transform.localPosition=center;g.AddComponent<BoxCollider>().size=size;}
            Box("Port wall",new Vector3(-.785f,2.53f,2.28f),new Vector3(.13f,2.06f,1.9f));
            Box("Starboard wall",new Vector3(.745f,2.53f,2.28f),new Vector3(.13f,2.06f,1.9f));
            Box("Front wall",new Vector3(-.02f,2.53f,3.23f),new Vector3(1.66f,2.06f,.14f));
            Box("Aft port jamb",new Vector3(-.655f,2.53f,1.35f),new Vector3(.25f,2.06f,.10f));
            Box("Aft starboard jamb",new Vector3(.635f,2.53f,1.35f),new Vector3(.21f,2.06f,.10f));
            Box("Door header",new Vector3(0,3.535f,1.35f),new Vector3(1.06f,.11f,.10f));
            Box("Ceiling",new Vector3(-.02f,3.59f,2.28f),new Vector3(1.66f,.12f,1.9f));
        }
        static void CameraTextures(GameObject g)
        {foreach(var c in g.GetComponentsInChildren<Camera>(true)){c.farClipPlane=3000;var d=c.GetUniversalAdditionalCameraData();d.requiresDepthTexture=true;d.requiresColorTexture=true;EditorUtility.SetDirty(d);}}
        static void FloatHull(GameObject g,bool offline)
        {
            var body=g.GetComponent<Rigidbody>();if(body==null)body=g.AddComponent<Rigidbody>();
            body.mass=2500;body.useGravity=true;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;body.centerOfMass=new Vector3(0,.5f,0);
            var floating=g.GetComponent<CalmWaterBuoyancy>();if(floating==null)floating=g.AddComponent<CalmWaterBuoyancy>();
            var so=new SerializedObject(floating);so.FindProperty("mooredOffline").boolValue=offline;so.ApplyModifiedPropertiesWithoutUndo();
            if(offline){var position=g.transform.position;position.y=floating.RestHeight;g.transform.position=position;}
        }
        static void PastelVolume()
        {
            const string path="Assets/_Game/Settings/HarborArtProfile.asset";
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if(profile==null){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);}
            T Effect<T>() where T:VolumeComponent {if(profile.TryGet<T>(out var e))return e;var added=profile.Add<T>();AssetDatabase.AddObjectToAsset(added,profile);return added;}
            var tone=Effect<Tonemapping>();tone.mode.Override(TonemappingMode.Neutral);
            var color=Effect<ColorAdjustments>();color.postExposure.Override(.15f);color.contrast.Override(3);color.saturation.Override(3);
            var bloom=Effect<Bloom>();bloom.intensity.Override(.10f);bloom.threshold.Override(1.1f);
            foreach(var c in profile.components)EditorUtility.SetDirty(c);EditorUtility.SetDirty(profile);
            var volume=Object.FindAnyObjectByType<Volume>();volume.sharedProfile=profile;EditorUtility.SetDirty(volume);
        }
        static Mesh Sea()
        {
            const int n=513;var v=new Vector3[n*n];var tris=new int[(n-1)*(n-1)*6];
            // 75cm samples across the near swell, then an exponentially spaced horizon skirt.
            float Coordinate(int i){float a=i-(n-1)*.5f,d=Mathf.Abs(a),u=Mathf.Max(0,d-192);return Mathf.Sign(a)*(d*.75f+2808*Mathf.Pow(u/64,3));}
            for(int z=0;z<n;z++)for(int x=0;x<n;x++){
                int i=z*n+x;v[i]=new Vector3(Coordinate(x),0,Coordinate(z));if(x==n-1||z==n-1)continue;
                int q=(z*(n-1)+x)*6;tris[q]=i;tris[q+1]=i+n;tris[q+2]=i+1;tris[q+3]=i+1;tris[q+4]=i+n;tris[q+5]=i+n+1;
            }
            var mesh=new Mesh{name="SeaSurfaceGrid",indexFormat=IndexFormat.UInt32,vertices=v,triangles=tris};mesh.RecalculateNormals();mesh.RecalculateBounds();
            const string path=Art+"SeaSurfaceGrid.asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(old!=null){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(old);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        [MenuItem("SalvageCrew/Apply Downloaded Stylized Assets")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.name!="HarborPrototype")throw new InvalidOperationException("Open HarborPrototype");
            kay=Mat("KayKitHarbor",Kay+"hexagons_medieval.png",Color.white);
            pirate=Mat("PiratePalette",Ken+"Pirate/Models/FBX format/Textures/colormap.png",Color.white);
            // Preserve the manufacturer's UV palette rather than flattening whole props to one colour.
            Mat("CrateOchre",Ken+"Factory/Models/FBX format/Textures/colormap.png",Color.white,.25f);
            Mat("EngineMetal",Ken+"Factory/Models/FBX format/Textures/colormap.png",new Color(.8f,.85f,.86f),.30f);
            Mat("Rust",Ken+"Factory/Models/FBX format/Textures/colormap.png",new Color(.85f,.63f,.44f),.15f);
            boatMat=Mat("UserTug",Art+"ThirdParty/UserBoat/Image_0.png",Color.white,.28f);
            boatMat.SetTexture("_EmissionMap",boatMat.mainTexture);boatMat.SetColor("_EmissionColor",new Color(.045f,.045f,.045f));boatMat.EnableKeyword("_EMISSION");
            var ni=(TextureImporter)AssetImporter.GetAtPath(Art+"ThirdParty/UserBoat/Image_2.png");ni.textureType=TextureImporterType.NormalMap;ni.SaveAndReimport();
            boatMat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"ThirdParty/UserBoat/Image_2.png"));boatMat.SetFloat("_BumpScale",.5f);boatMat.EnableKeyword("_NORMALMAP");
            var generated=scene.GetRootGameObjects().First(g=>g.name=="HarborPrototypeGenerated").transform;
            var offline=generated.Find("StaticBoat_10m_x_4m");
            if(offline.GetComponent<Rigidbody>()==null){var shift=new Vector3(7,0,10)-offline.position;offline.position+=shift;foreach(Transform child in offline)child.position-=shift;}
            FloatHull(offline.gameObject,true);Boat(offline,Vector3.zero);
            var driver=offline.GetComponent<OfflineBoatController>();if(driver==null)driver=offline.gameObject.AddComponent<OfflineBoatController>();
            var ds=new SerializedObject(driver);ds.FindProperty("helm").objectReferenceValue=offline.Find("Helm");ds.ApplyModifiedPropertiesWithoutUndo();
            foreach(string name in new[]{"NetworkBoat","NetworkFirstPersonPlayer","FirstPersonPlayer"}){
                string path="Assets/_Game/Prefabs/"+name+".prefab";var p=PrefabUtility.LoadPrefabContents(path);
                try{
                    if(name=="NetworkBoat"){FloatHull(p,false);Boat(p.transform,Vector3.zero);}
                    else{
                        CameraTextures(p);if(p.GetComponent<DeckPassenger>()==null)p.AddComponent<DeckPassenger>();
                        var remote=p.transform.Find("RemoteCrewVisual");
                        if(remote!=null){
                            foreach(var r in remote.GetComponentsInChildren<Renderer>(true))r.enabled=false;
                            var art=Root(remote);var mat=Mat("CrewAsset",Art+"ThirdParty/KayKitCrew/rogue_texture.png",Color.white);
                            Model(art,Art+"ThirdParty/KayKitCrew/Rogue.fbx",new Vector3(0,.88f,0),new Vector3(.68f,1.76f,.50f),mat);
                        }
                    }
                    PrefabUtility.SaveAsPrefabAsset(p,path);
                }finally{PrefabUtility.UnloadPrefabContents(p);}
            }
            CameraTextures(generated.gameObject);
            var local=generated.GetComponentInChildren<FirstPersonMotor>(true);if(local.GetComponent<DeckPassenger>()==null)local.gameObject.AddComponent<DeckPassenger>();
            var harbor=generated.Find("Harbor");var root=Root(harbor);
            foreach(var r in harbor.GetComponentsInChildren<MeshRenderer>(true))if(!r.transform.IsChildOf(root))r.enabled=false;
            string dock=Art+"ThirdParty/Loafbrr/Floor_Dock.fbx";
            var timber=Mat("PaintedDock",Art+"ThirdParty/Loafbrr/DockTrim_Diffuse.png",new Color(1,.91f,.78f),.16f);
            for(int i=0;i<6;i++)Model(root,dock,new Vector3(0,1.06f,-1+i*2),new Vector3(5,.3f,2),timber);
            for(int i=0;i<3;i++)Model(root,Art+"ThirdParty/Loafbrr/Floor_Support.fbx",new Vector3(0,-.2f,i*4),new Vector3(4.8f,2.4f,2),timber);
            // The ramp visual belongs to the existing ramp root, so departure still hides it.
            var ramp=harbor.Find("BoardingRamp");ramp.position=new Vector3(3.75f,.965f,7.5f);ramp.rotation=Quaternion.Euler(0,0,-6.05f);
            foreach(string guide in new[]{"RampGuideFront","RampGuideRear"}){var t=harbor.Find(guide);var pos=t.position;pos.y=1.395f;t.position=pos;t.rotation=ramp.rotation;}
            var rampArt=Root(ramp);rampArt.localScale=new Vector3(1/ramp.localScale.x,1/ramp.localScale.y,1/ramp.localScale.z);
            Model(rampArt,dock,new Vector3(0,0,0),new Vector3(3.3f,.18f,1.8f),timber);
            var beams=Mat("DockBeams",Art+"ThirdParty/Loafbrr/WoodBeams_Diffuse.png",new Color(.9f,.83f,.71f),.12f);
            beams.SetFloat("_Cull",2);beams.SetFloat("_AlphaClip",1);beams.SetFloat("_Cutoff",.4f);beams.EnableKeyword("_ALPHATEST_ON");beams.renderQueue=2450;
            for(int side=-1;side<=1;side+=2){
                // Imported single-sided fence gets an opposite-facing copy, not a black backface.
                foreach(float facing in new[]{0f,180f})Model(rampArt,Art+"ThirdParty/Loafbrr/Fence_Plane.fbx",new Vector3(0,.50f,side*.93f),new Vector3(3.2f,.85f,.09f),beams,facing);
                foreach(float x in new[]{-1.55f,1.55f})Model(rampArt,Art+"ThirdParty/Loafbrr/Fence_Post.fbx",new Vector3(x,.50f,side*.93f),new Vector3(.12f,.95f,.12f),beams);
            }
            foreach(float x in new[]{-2.1f,2.1f})foreach(float z in new[]{0f,4f,9.3f})
                Model(root,Art+"ThirdParty/Loafbrr/Dock_Bollard_1.fbx",new Vector3(x,1.52f,z),new Vector3(.4f,.6f,.4f),timber);
            // Imported shore and architecture, placed outside the existing traversal corridors.
            for(int i=0;i<7;i++)Model(root,dock,new Vector3(-8+i*2.66f,.96f,-4),new Vector3(2.67f,.5f,4),timber);
            // Local shore replaced below with the same imported, irregular beach mesh as the islands.
            Model(root,Kay+"building_tavern_red.fbx",new Vector3(-8,3.55f,-9),new Vector3(6,4.7f,5),kay);
            var rockMats=new Material[3];for(int r=0;r<3;r++){
                string rp=Art+"ThirdParty/Rubberduck/rock"+(r+1);
                rockMats[r]=Mat("PaintedRock"+(r+1),rp+".png",new Color(.91f,.91f,.87f),.12f);
                var normal=(TextureImporter)AssetImporter.GetAtPath(rp+"_n.png");normal.textureType=TextureImporterType.NormalMap;normal.SaveAndReimport();
                rockMats[r].SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(rp+"_n.png"));rockMats[r].SetFloat("_BumpScale",.4f);rockMats[r].EnableKeyword("_NORMALMAP");
            }
            // Irregular separated islands, not a repeating wall of equally sized cliffs.
            var sand=Mat("WarmBeach",Art+"ThirdParty/Rubberduck/rock1.png",new Color(1.18f,1.12f,.89f),.08f);
            Model(root,Art+"ThirdParty/Rubberduck/rock1.obj",new Vector3(0,-1.6f,-15),new Vector3(40,5.6f,28),sand,22);
            var islands=new[]{new Vector3(-40,0,66),new Vector3(48,0,68),new Vector3(115,0,45)};
            const string nature=Art+"ThirdParty/Quaternius/";
            var leaves=Mat("PalmLeaves",nature+"PalmTree_Leaves.png",new Color(.65f,.85f,.58f),.1f);
            leaves.SetFloat("_AlphaClip",1);leaves.SetFloat("_Cutoff",.35f);leaves.SetFloat("_Cull",0);leaves.EnableKeyword("_ALPHATEST_ON");leaves.renderQueue=2450;
            var trunk=Mat("PalmTrunk",nature+"PalmTree_Trunk.png",Color.white,.12f);
            var bush=Mat("CoastalBush",nature+"Bush_Leaves.png",new Color(.58f,.78f,.43f),.08f);
            bush.SetFloat("_AlphaClip",1);bush.SetFloat("_Cutoff",.35f);bush.SetFloat("_Cull",0);bush.EnableKeyword("_ALPHATEST_ON");bush.renderQueue=2450;
            foreach(string tex in new[]{"PalmTree_Leaves.png","PalmTree_Trunk.png","Bush_Leaves.png"}){var ti=(TextureImporter)AssetImporter.GetAtPath(nature+tex);ti.maxTextureSize=1024;ti.SaveAndReimport();}
            for(int i=0;i<islands.Length;i++){
                var c=islands[i];float scale=i==2?1.6f:1;
                Model(root,Art+"ThirdParty/Rubberduck/rock1.obj",c+new Vector3(0,-2.1f,0),new Vector3(54,6.5f,40)*scale,sand,i*57);
                for(int k=0;k<9;k++){
                    // One high point, descending shoulders; avoid a row of equal monoliths.
                    float x=-15+k*3.7f,z=6+Mathf.Sin(k*2.1f)*3;
                    float height=3.5f+9*Mathf.Exp(-Mathf.Pow((k-2.5f)/1.8f,2));
                    Model(root,Art+"ThirdParty/Rubberduck/rock"+(k%3+1)+".obj",c+new Vector3(x,height*.36f,z),new Vector3(7+(k%2)*3,height,8)*scale,rockMats[k%3],i*73+k*51);
                }
                // Broken shoreline: authored rocks overlap the broad beach edge at varying scales.
                for(int j=0;j<13;j++){
                    float a=j*2.399f;float radius=18+(j%3)*1.3f;
                    Model(root,Art+"ThirdParty/Rubberduck/rock"+(j%3+1)+".obj",c+new Vector3(Mathf.Cos(a)*radius,.25f,Mathf.Sin(a)*radius*.69f),new Vector3(2.8f+j%3,1.8f+j%4*.5f,3.4f+j%2),rockMats[j%3],j*83);
                }
                Model(root,Kay+"building_home_A_red.fbx",c+new Vector3(-12,3.1f,-7),new Vector3(4,4.8f,4),kay);
                for(int j=0;j<25;j++){
                    float angle=j*2.399f;float radius=7+(j%4)*2.7f;
                    var spot=c+new Vector3(Mathf.Cos(angle)*radius,.7f,Mathf.Sin(angle)*radius*.65f);
                    var palm=Model(root,nature+"PalmTree_"+(j%2+1)+".fbx",spot+Vector3.up*(3.8f+j%3*.4f),new Vector3(7.4f,7.6f+j%3*.8f,7.4f),null,j*83);
                    foreach(var renderer in palm.GetComponentsInChildren<Renderer>())renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>m.name.Contains("Leaves")?leaves:trunk).ToArray();
                    Model(root,nature+"Bush.fbx",spot+new Vector3(2,.5f,1),new Vector3(5,2.5f,4),bush,j*63);
                }
            }
            for(int i=0;i<5;i++)Model(root,Ken+"Pirate/Models/FBX format/barrel.fbx",new Vector3(-2.8f-i*.7f,1.7f,-4.5f),new Vector3(.6f,1,.6f),pirate);
            // Distant islands are now reachable by the drivable boat: use their low-poly authored meshes as static collision.
            foreach(var mesh in root.GetComponentsInChildren<MeshFilter>())if(AssetDatabase.GetAssetPath(mesh.sharedMesh).Contains("Rubberduck")&&mesh.GetComponent<Renderer>().bounds.center.z>25)
                mesh.gameObject.AddComponent<MeshCollider>().sharedMesh=mesh.sharedMesh;
            var wr=harbor.Find("RefinedWater");if(wr==null){wr=new GameObject("RefinedWater").transform;wr.SetParent(harbor,false);}foreach(Transform c in wr.Cast<Transform>().ToArray())Object.DestroyImmediate(c.gameObject);
            var water=AssetDatabase.LoadAssetAtPath<Material>(Art+"CalmHarborWater.mat");water.SetColor("_DeepColor",new Color(.012f,.32f,.39f));water.SetColor("_ShallowColor",new Color(.04f,.55f,.56f));water.SetFloat("_WaveHeight",1.2f);water.SetFloat("_Speed",.65f);EditorUtility.SetDirty(water);
            const string ripplePath=Art+"ThirdParty/MatrixRexWater/Normal1.png";
            var rippleImport=(TextureImporter)AssetImporter.GetAtPath(ripplePath);rippleImport.textureType=TextureImporterType.NormalMap;rippleImport.wrapMode=TextureWrapMode.Repeat;rippleImport.anisoLevel=8;rippleImport.mipmapEnabled=true;rippleImport.SaveAndReimport();
            water.SetTexture("_RippleNormal",AssetDatabase.LoadAssetAtPath<Texture2D>(ripplePath));water.SetFloat("_NormalStrength",.65f);
            var clock=wr.GetComponent<HarborWater>();if(clock==null)clock=wr.gameObject.AddComponent<HarborWater>();var ws=new SerializedObject(clock);ws.FindProperty("waterMaterial").objectReferenceValue=water;ws.ApplyModifiedPropertiesWithoutUndo();
            var sea=new GameObject("Ocean surface",typeof(MeshFilter),typeof(MeshRenderer));sea.transform.SetParent(wr,false);sea.transform.localPosition=new Vector3(0,-.05f,5);sea.GetComponent<MeshFilter>().sharedMesh=Sea();sea.GetComponent<Renderer>().sharedMaterial=water;
            const string skyPath=Art+"ThirdParty/AllSkyFree/EpicBlueSunset.png";
            var skyImport=(TextureImporter)AssetImporter.GetAtPath(skyPath);skyImport.maxTextureSize=8192;
            skyImport.textureCompression=TextureImporterCompression.CompressedHQ;skyImport.mipmapEnabled=true;skyImport.filterMode=FilterMode.Trilinear;skyImport.npotScale=TextureImporterNPOTScale.None;skyImport.SaveAndReimport();
            var panorama=AssetDatabase.LoadAssetAtPath<Texture2D>(skyPath);
            var sky=Mat("MediterraneanSky",skyPath,Color.white);sky.shader=Shader.Find("SalvageCrew/Painterly Panorama");sky.SetTexture("_MainTex",panorama);sky.SetFloat("_Rotation",110);sky.SetColor("_CloudShade",new Color(.34f,.49f,.62f));RenderSettings.skybox=sky;
            water.SetTexture("_Environment",panorama);
            var sun=generated.Find("Daylight").GetComponent<Light>();sun.transform.rotation=Quaternion.Euler(34,65,0);sun.color=new Color(1,.94f,.83f);sun.intensity=2.0f;sun.shadowStrength=1;
            var pipeline=(UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;pipeline.shadowDistance=180;EditorUtility.SetDirty(pipeline);
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.34f,.48f,.61f);RenderSettings.ambientEquatorColor=new Color(.23f,.31f,.34f);RenderSettings.ambientGroundColor=new Color(.16f,.15f,.13f);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=180;RenderSettings.fogEndDistance=1200;RenderSettings.fogColor=new Color(.47f,.70f,.78f);
            PastelVolume();DynamicGI.UpdateEnvironment();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
    }
}
