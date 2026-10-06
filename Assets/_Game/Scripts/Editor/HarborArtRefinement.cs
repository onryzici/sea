using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace SalvageCrew.Editor
{
    // Revision of the visual layer only. Never moves or replaces a gameplay/physics transform.
    public static class HarborArtRefinement
    {
        const string Art = "Assets/_Game/Art/";
        const string Source = Art + "ThirdParty/PolyHaven/";
        static Material paint, ivory, timber, dark, steel, glass, orange, roof, stone;

        static Material Material(string name, Color color, float smoothness = .25f)
        {
            string path = Art + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
            m.shader = Shader.Find("Universal Render Pipeline/Lit");m.color = color; m.SetFloat("_Smoothness", smoothness); EditorUtility.SetDirty(m); return m;
        }
        static Transform VisualRoot(Transform parent)
        {
            var root = parent.Find("ArtVisuals");
            if (root == null) { root = new GameObject("ArtVisuals").transform; root.SetParent(parent, false); }
            foreach (Transform child in root.Cast<Transform>().ToArray()) Object.DestroyImmediate(child.gameObject);
            root.localRotation = Quaternion.identity; root.localScale = Vector3.one;
            return root;
        }
        static Mesh SaveMesh(string name, Mesh mesh)
        {
            string path = Art + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            mesh.name = name;
            if (existing != null) { EditorUtility.CopySerialized(mesh, existing); Object.DestroyImmediate(mesh); EditorUtility.SetDirty(existing); return existing; }
            AssetDatabase.CreateAsset(mesh, path); return mesh;
        }
        static GameObject MeshObject(Transform root, string name, Mesh mesh, Material material, Vector3 position)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(root, false); go.transform.localPosition = position;
            go.GetComponent<MeshFilter>().sharedMesh = mesh; go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }
        static GameObject Box(Transform root, string name, Vector3 position, Vector3 size, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>()); go.transform.SetParent(root, false);
            go.transform.localPosition = position; go.transform.localScale = size; go.GetComponent<Renderer>().sharedMaterial = material; return go;
        }
        static GameObject Cylinder(Transform root, string name, Vector3 position, Vector3 scale, Material material, Vector3 rotation = default)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>()); go.transform.SetParent(root, false);
            go.transform.localPosition = position; go.transform.localScale = scale; go.transform.localEulerAngles = rotation;
            go.GetComponent<Renderer>().sharedMaterial = material; return go;
        }
        static void Ring(Transform root, string name, Vector3 position, float radius, Material material, Vector3 rotation)
        {
            var go = MeshObject(root, name, AssetDatabase.LoadAssetAtPath<Mesh>(Art + "Ring.asset"), material, position);
            go.transform.localScale = Vector3.one * radius; go.transform.localEulerAngles = rotation;
        }
        static void Line(Transform root, string name, Vector3[] points, float width, Material material)
        {
            var go = new GameObject(name, typeof(LineRenderer)); go.transform.SetParent(root, false);
            var line = go.GetComponent<LineRenderer>(); line.useWorldSpace = false; line.positionCount = points.Length; line.SetPositions(points);
            line.widthMultiplier = width; line.sharedMaterial = material; line.numCornerVertices = 3; line.numCapVertices = 3;
        }
        static void Tube(Transform root, string name, Vector3 start, Vector3 end, float diameter, Material material)
        {
            var go=Cylinder(root,name,(start+end)*.5f,new Vector3(diameter,Vector3.Distance(start,end)*.5f,diameter),material);
            go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,(end-start).normalized);
        }
        static Mesh Plane(float width, float length, int subdivisions, bool woodUV = false)
        {
            int n = subdivisions + 1; var vertices = new Vector3[n * n]; var uv = new Vector2[vertices.Length]; var triangles = new int[subdivisions * subdivisions * 6];
            for (int z = 0; z < n; z++) for (int x = 0; x < n; x++)
            {
                int i = z * n + x; vertices[i] = new Vector3((x / (float)subdivisions - .5f) * width, 0, (z / (float)subdivisions - .5f) * length);
                uv[i] = woodUV ? new Vector2(vertices[i].x / 2, vertices[i].z / 2) : new Vector2(x / (float)subdivisions, z / (float)subdivisions);
                if (x == subdivisions || z == subdivisions) continue;
                int t = (z * subdivisions + x) * 6; triangles[t] = i; triangles[t + 1] = i + n; triangles[t + 2] = i + 1;
                triangles[t + 3] = i + 1; triangles[t + 4] = i + n; triangles[t + 5] = i + n + 1;
            }
            var mesh = new Mesh { vertices = vertices, uv = uv, triangles = triangles }; mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds(); return mesh;
        }
        static Mesh Hull()
        {
            // Rounded stern and shouldered bow; low chines taper below the original deck footprint.
            var outline = new[] { new Vector2(-1.45f,-5.22f),new Vector2(1.45f,-5.22f),new Vector2(1.92f,-5.02f),new Vector2(2.07f,-4.62f),new Vector2(2.07f,3.9f),new Vector2(1.9f,4.7f),new Vector2(1.23f,5.42f),new Vector2(0,5.73f),new Vector2(-1.23f,5.42f),new Vector2(-1.9f,4.7f),new Vector2(-2.07f,3.9f),new Vector2(-2.07f,-4.62f),new Vector2(-1.92f,-5.02f) };
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            float[] heights = { -.25f, .35f, 1.02f, 1.53f }; float[] widths = { .68f, .83f, .98f, 1 };
            for (int band = 0; band < 3; band++) for (int i = 0; i < outline.Length; i++)
            {
                Vector2 a = outline[i], b = outline[(i + 1) % outline.Length]; int v = vertices.Count;
                vertices.Add(new Vector3(a.x * widths[band+1], heights[band+1], a.y)); vertices.Add(new Vector3(b.x * widths[band+1], heights[band+1], b.y));
                vertices.Add(new Vector3(b.x * widths[band], heights[band], b.y * .985f)); vertices.Add(new Vector3(a.x * widths[band], heights[band], a.y * .985f));
                triangles.AddRange(new[] { v, v+1, v+2, v, v+2, v+3 });
            }
            var mesh = new Mesh { vertices = vertices.ToArray() }; mesh.subMeshCount=2;
            mesh.SetTriangles(triangles.Take(outline.Length*6).ToArray(),0);
            mesh.SetTriangles(triangles.Skip(outline.Length*6).ToArray(),1);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
        static void Window(Transform root, Vector3 pos, Vector2 size, bool side = false)
        {
            var pane = Box(root, "Recessed blue glazing", pos, new Vector3(size.x, size.y, .028f), glass);
            if (side) pane.transform.localEulerAngles = new Vector3(0, 90, 0);
            Vector3 horizontal = side ? Vector3.forward : Vector3.right;
            for (int i = -1; i <= 1; i += 2)
            {
                var frame = Box(root, "Window jamb", pos + horizontal * (size.x / 2 + .04f) * i, new Vector3(.065f, size.y + .13f, .075f), steel);
                if (side) frame.transform.localEulerAngles = new Vector3(0, 90, 0);
                frame = Box(root, "Window sill", pos + Vector3.up * (size.y / 2 + .045f) * i, new Vector3(size.x + .14f, .07f, .075f), steel);
                if (side) frame.transform.localEulerAngles = new Vector3(0, 90, 0);
            }
            var glint = Box(root, "Glass sky reflection", pos + Vector3.up * .13f + (side ? Vector3.right : Vector3.back) * .019f, new Vector3(size.x * .82f, .075f, .01f), Material("SoftGlassReflection",new Color(.58f,.76f,.75f),.65f));
            if (side) glint.transform.localEulerAngles = new Vector3(0, 90, 0);
        }
        static void Boat(Transform boat, Vector3 offset)
        {
            var root = VisualRoot(boat); root.localPosition = offset;
            foreach (var r in boat.GetComponentsInChildren<MeshRenderer>()) if (!r.transform.IsChildOf(root)) r.enabled = false;
            var hull=MeshObject(root, "Shaped steel hull", SaveMesh("RefinedHull", Hull()), paint, Vector3.zero);
            hull.GetComponent<Renderer>().sharedMaterials=new[]{Material("AntifoulingRed",new Color(.43f,.12f,.085f),.18f),paint};
            MeshObject(root, "Timber deck", SaveMesh("RefinedDeck", Plane(3.95f,9.8f,1,true)), timber, new Vector3(0,1.519f,0));
            // Skin the existing collision rails in place, then add visible panel seams, fasteners and cap rails.
            foreach (string name in new[] { "StarboardRail", "SternRail", "BowRail", "PortRailFront", "PortRailRear" })
            {
                var old = boat.Find(name); Vector3 pos = old.localPosition - offset;
                Vector3 panelSize=old.localScale;panelSize.y=.50f;
                Box(root, "Steel bulwark " + name, pos-Vector3.up*.175f, panelSize, paint);
                Vector3 size = old.localScale; size.y = .065f;
                if (size.x < size.z) size.x = .18f; else size.z = .18f;
                Box(root, "Turquoise gunwale cap", pos + Vector3.up * .105f, size, paint);
                Vector3 axis=size.x<size.z?Vector3.forward:Vector3.right;
                float length=Mathf.Max(size.x,size.z);
                Tube(root,"Polished pipe handrail",pos+Vector3.up*.40f-axis*(length*.47f),pos+Vector3.up*.40f+axis*(length*.47f),.065f,steel);
                int count = Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(size.x,size.z) / 1.5f));
                for (int j = 0; j < count; j++)
                {
                    Vector3 p = pos + (size.x < size.z ? Vector3.forward : Vector3.right) * ((j + .5f)/count - .5f) * Mathf.Max(size.x,size.z);
                    Box(root, "Bulwark rib", p-Vector3.up*.175f, size.x < size.z ? new Vector3(.19f,.48f,.045f) : new Vector3(.045f,.48f,.19f), paint);
                    Tube(root,"Handrail stanchion",p+Vector3.up*.105f,p+Vector3.up*.40f,.060f,steel);
                }
            }
            for (int side=-1;side<=1;side+=2)
            {
                Box(root,"Rubbing strake",new Vector3(side*2.06f,.96f,-.3f),new Vector3(.11f,.09f,9.1f),dark);
                for(int i=0;i<4;i++)
                {
                    float z=-3.7f+i*2.1f;if(side<0 && z>-3.5f && z<-1.5f)continue;
                    Ring(root,"Rubber fender",new Vector3(side*2.16f,.90f,z),.38f,dark,new Vector3(0,0,90));
                    Line(root,"Fender lashing",new[]{new Vector3(side*1.9f,2.41f,z),new Vector3(side*2.14f,1.6f,z),new Vector3(side*2.16f,1.23f,z)},.023f,Material("NaturalRope",new Color(.63f,.51f,.31f)));
                }
                // Real fastener silhouettes along the waterline, below walkable space.
                for(int i=0;i<16;i++) Cylinder(root,"Hull rivet",new Vector3(side*2.053f,1.30f,-4.4f+i*.57f),new Vector3(.026f,.012f,.026f),steel,new Vector3(0,0,90));
            }
            Box(root,"Ivory wheelhouse",new Vector3(0,2.55f,2.4f),new Vector3(2.18f,2.1f,2.38f),ivory);
            Box(root,"Wheelhouse lower apron",new Vector3(0,1.72f,2.4f),new Vector3(2.22f,.42f,2.42f),paint);
            Box(root,"Overhanging roof",new Vector3(0,3.66f,2.4f),new Vector3(2.60f,.17f,2.79f),paint);
            Box(root,"Roof steel edge",new Vector3(0,3.72f,2.4f),new Vector3(2.66f,.07f,2.85f),paint);
            Window(root,new Vector3(-.51f,2.91f,1.195f),new Vector2(.80f,.78f));Window(root,new Vector3(.51f,2.91f,1.195f),new Vector2(.80f,.78f));
            Window(root,new Vector3(-1.105f,2.90f,2.7f),new Vector2(1.28f,.76f),true);
            Window(root,new Vector3(1.105f,2.90f,2.7f),new Vector2(1.28f,.76f),true);
            Box(root,"Side access door",new Vector3(-1.108f,2.21f,1.77f),new Vector3(.025f,1.36f,.55f),timber);
            Box(root,"Door handle",new Vector3(-1.14f,2.13f,1.82f),new Vector3(.035f,.035f,.12f),steel);
            for(int i=-1;i<=1;i++) Cylinder(root,"Roof ventilation",new Vector3(.65f,3.91f,2.50f+i*.2f),new Vector3(.10f,.13f,.10f),steel);
            Cylinder(root,"Exhaust pipe",new Vector3(.72f,4.05f,3.25f),new Vector3(.18f,.34f,.18f),dark);
            Cylinder(root,"Antenna mast",new Vector3(-.5f,4.36f,2.1f),new Vector3(.055f,.65f,.055f),steel);
            Box(root,"Antenna crossarm",new Vector3(-.5f,4.80f,2.1f),new Vector3(1.2f,.05f,.05f),steel);
            Cylinder(root,"Amber roof beacon",new Vector3(.1f,3.99f,2.15f),new Vector3(.17f,.15f,.17f),orange);
            Cylinder(root,"Radar pedestal",new Vector3(-.42f,3.95f,2.80f),new Vector3(.14f,.24f,.14f),paint);
            var dome=GameObject.CreatePrimitive(PrimitiveType.Sphere);dome.name="Ivory radar dome";Object.DestroyImmediate(dome.GetComponent<Collider>());dome.transform.SetParent(root,false);dome.transform.localPosition=new Vector3(-.42f,4.24f,2.80f);dome.transform.localScale=new Vector3(.47f,.42f,.47f);dome.GetComponent<Renderer>().sharedMaterial=ivory;
            Ring(root,"Orange lifebuoy",new Vector3(-1.15f,2.23f,3.05f),.39f,orange,new Vector3(0,0,90));
            for(int i=0;i<4;i++)
            {
                float a=i*Mathf.PI/2;Box(root,"Lifebuoy white band",new Vector3(-1.245f,2.23f+Mathf.Sin(a)*.27f,3.05f+Mathf.Cos(a)*.27f),new Vector3(.085f,.095f,.095f),ivory);
            }
            for(int i=0;i<4;i++) Ring(root,"Coiled rope",new Vector3(-1.47f,1.555f+i*.025f,3.75f),.32f-i*.022f,Material("NaturalRope",new Color(.63f,.51f,.31f)),Vector3.zero);
            for(int side=-1;side<=1;side+=2)
            {
                Cylinder(root,"Mooring cleat base",new Vector3(side*1.68f,1.62f,-4.45f),new Vector3(.18f,.09f,.18f),steel);
                Box(root,"Mooring cleat",new Vector3(side*1.68f,1.76f,-4.45f),new Vector3(.40f,.065f,.07f),steel);
            }
            Ring(root,"Helm wheel",new Vector3(0,2.32f,1.02f),.27f,dark,new Vector3(90,0,0));
            for(int i=0;i<3;i++){var spoke=Box(root,"Helm spoke",new Vector3(0,2.32f,1.01f),new Vector3(.025f,.34f,.035f),steel);spoke.transform.localEulerAngles=new Vector3(0,0,i*120);}
        }
        static void CameraTextures(GameObject root)
        {
            foreach(var camera in root.GetComponentsInChildren<Camera>(true))
            {var data=camera.GetUniversalAdditionalCameraData();data.requiresDepthTexture=true;data.requiresColorTexture=true;EditorUtility.SetDirty(data);}
        }
        static float LandHeight(float x,float z)
        {
            float shore=41+Mathf.Sin(x*.052f)*5;
            return -2+Mathf.SmoothStep(0,1,Mathf.Clamp01((z-shore)/8))*7 + Mathf.PerlinNoise(x*.026f,z*.04f)*Mathf.Clamp(z-shore-7,0,22)*.8f;
        }
        static void Village(Transform harbor)
        {
            var root=VisualRoot(harbor);root.localPosition=Vector3.zero;
            var mesh=Plane(130,70,48);var verts=mesh.vertices;
            for(int i=0;i<verts.Length;i++){verts[i].x+=32;verts[i].z+=70;verts[i].y=LandHeight(verts[i].x,verts[i].z);}
            mesh.vertices=verts;mesh.RecalculateNormals();mesh.RecalculateBounds();MeshObject(root,"Continuous coastal headland",SaveMesh("CoastalHeadland",mesh),stone,Vector3.zero);
            for(int i=0;i<11;i++)
            {
                float x=-8+i*7,z=52+(i%3)*5;float height=LandHeight(x,z),h=3.2f+(i%3)*.65f,w=3.8f+(i%2)*.6f;
                var house=new GameObject("Harbor plaster house "+i).transform;house.SetParent(root,false);house.localPosition=new Vector3(x,height,z);
                Box(house,"Plaster walls",new Vector3(0,h/2,0),new Vector3(w,h,4),Material(i%3==0?"WarmOchrePlaster":"WhiteLimePlaster",i%3==0?new Color(.73f,.55f,.29f):new Color(.89f,.86f,.75f)));
                var left=Box(house,"Terracotta roof left",new Vector3(-w/4,h+.5f,0),new Vector3(w*.58f,.18f,4.5f),roof);left.transform.localEulerAngles=new Vector3(0,0,23);
                var right=Box(house,"Terracotta roof right",new Vector3(w/4,h+.5f,0),new Vector3(w*.58f,.18f,4.5f),roof);right.transform.localEulerAngles=new Vector3(0,0,-23);
                Box(house,"Roof ridge",new Vector3(0,h+.95f,0),new Vector3(.20f,.15f,4.5f),roof);
                for(int floor=0;floor<2;floor++)for(int column=-1;column<=1;column+=2)
                {
                    float y=.9f+floor*1.5f; if(y>h-.25f)continue;
                    Box(house,"Window recess",new Vector3(column*w*.25f,y,-2.018f),new Vector3(.62f,.90f,.045f),dark);
                    Box(house,"Blue shutters",new Vector3(column*w*.25f-.40f,y,-2.06f),new Vector3(.18f,.92f,.065f),paint);
                    Box(house,"Stone window sill",new Vector3(column*w*.25f,y-.50f,-2.09f),new Vector3(.95f,.10f,.16f),ivory);
                }
                Box(house,"Timber entrance",new Vector3(0,.85f,-2.025f),new Vector3(.73f,1.7f,.06f),timber);
                Cylinder(house,"Chimney",new Vector3(w*.28f,h+1,-.8f),new Vector3(.42f,.60f,.42f),ivory);
            }
            // Narrow dark cypress silhouettes, not tropical palms or suburban lawn trees.
            for(int i=0;i<9;i++)
            {
                float x=-14+i*10,z=66+(i%2)*7,y=LandHeight(x,z);
                var tree=GameObject.CreatePrimitive(PrimitiveType.Sphere);tree.name="Cypress silhouette";Object.DestroyImmediate(tree.GetComponent<Collider>());tree.transform.SetParent(root,false);tree.transform.localPosition=new Vector3(x,y+3.3f,z);tree.transform.localScale=new Vector3(1.3f,6.8f,1.3f);tree.GetComponent<Renderer>().sharedMaterial=Material("CypressGreen",new Color(.13f,.22f,.085f));
            }
            float lighthouseY=LandHeight(78,58);
            Cylinder(root,"Lighthouse tower",new Vector3(78,lighthouseY+5,58),new Vector3(2.8f,5,2.8f),ivory);
            Cylinder(root,"Lighthouse gallery",new Vector3(78,lighthouseY+10,58),new Vector3(3.7f,.24f,3.7f),ivory);
            Cylinder(root,"Lighthouse lantern",new Vector3(78,lighthouseY+11,58),new Vector3(2.3f,.8f,2.3f),glass);
            Cylinder(root,"Lighthouse cap",new Vector3(78,lighthouseY+11.9f,58),new Vector3(3,.22f,3),roof);
            MeshObject(root,"Pier timber surface",SaveMesh("RefinedPier",Plane(4.94f,11.9f,1,true)),timber,new Vector3(0,1.215f,4));
            for(int i=0;i<6;i++) for(int side=-1;side<=1;side+=2)
            {
                Cylinder(root,"Pier timber post",new Vector3(side*2.28f,.65f,-.75f+i*2),new Vector3(.23f,.70f,.23f),timber);
                Box(root,"Post iron collar",new Vector3(side*2.28f,1.1f,-.75f+i*2),new Vector3(.26f,.045f,.26f),steel);
            }
        }
        [MenuItem("SalvageCrew/Refine Boat And Sea")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first.");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.name!="HarborPrototype")throw new InvalidOperationException("Open HarborPrototype.");
            var normalImporter=(TextureImporter)AssetImporter.GetAtPath(Source+"wooden_planks_nor_gl_1k.jpg");normalImporter.textureType=TextureImporterType.NormalMap;normalImporter.SaveAndReimport();
            timber=Material("RefinedTimber",new Color(.82f,.72f,.58f),.28f);timber.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"wooden_planks_diff_1k.jpg"));timber.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"wooden_planks_nor_gl_1k.jpg"));timber.SetFloat("_BumpScale",.55f);timber.EnableKeyword("_NORMALMAP");
            paint=Material("MarineTeal",new Color(.10f,.68f,.62f),.30f);paint.shader=Shader.Find("SalvageCrew/Weathered Marine Paint");paint.SetColor("_BaseColor",new Color(.10f,.68f,.62f));paint.SetColor("_RustColor",new Color(.38f,.19f,.08f));paint.SetFloat("_Wear",.20f);paint.SetFloat("_Metallic",.08f);
            ivory=Material("MarineIvory",new Color(.87f,.84f,.71f));dark=Material("FenderRubber",new Color(.035f,.042f,.039f),.12f);steel=Material("AgedSteel",new Color(.28f,.34f,.33f),.45f);steel.SetFloat("_Metallic",.65f);
            glass=Material("MarineGlass",new Color(.085f,.23f,.28f),.86f);orange=Material("SafetyCoral",new Color(.93f,.28f,.065f));roof=Material("Terracotta",new Color(.59f,.23f,.105f));stone=Material("HeadlandLimestone",new Color(.55f,.51f,.41f));
            var generated=scene.GetRootGameObjects().First(g=>g.name=="HarborPrototypeGenerated").transform;
            Boat(generated.Find("StaticBoat_10m_x_4m"),new Vector3(7,0,10));
            foreach(string name in new[]{"NetworkBoat","NetworkFirstPersonPlayer","FirstPersonPlayer"})
            {
                string path="Assets/_Game/Prefabs/"+name+".prefab";var prefab=PrefabUtility.LoadPrefabContents(path);
                try{if(name=="NetworkBoat")Boat(prefab.transform,Vector3.zero);else CameraTextures(prefab);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
            }
            CameraTextures(generated.gameObject);Village(generated.Find("Harbor"));
            var harbor=generated.Find("Harbor");harbor.Find("Sea_VisualOnly").GetComponent<Renderer>().enabled=false;
            harbor.Find("BoardingRamp").GetComponent<Renderer>().sharedMaterial=timber;
            harbor.Find("RampGuideFront").GetComponent<Renderer>().sharedMaterial=paint;harbor.Find("RampGuideRear").GetComponent<Renderer>().sharedMaterial=paint;
            var water=AssetDatabase.LoadAssetAtPath<Material>(Art+"CalmHarborWater.mat");water.SetColor("_DeepColor",new Color(.025f,.43f,.48f));water.SetColor("_ShallowColor",new Color(.085f,.64f,.57f));water.SetFloat("_WaveHeight",.065f);water.SetFloat("_Speed",.50f);water.SetTexture("_Environment",AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"kloofendal_48d_partly_cloudy_puresky_1k.hdr"));
            var waterRoot=harbor.Find("RefinedWater");if(waterRoot==null){waterRoot=new GameObject("RefinedWater").transform;waterRoot.SetParent(harbor,false);}foreach(Transform c in waterRoot.Cast<Transform>().ToArray())Object.DestroyImmediate(c.gameObject);
            MeshObject(waterRoot,"Tessellated sea surface",SaveMesh("SeaSurfaceGrid",Plane(600,600,160)),water,new Vector3(0,-.03f,5));
            var sky=Material("MediterraneanSky",Color.white);sky.shader=Shader.Find("Skybox/Panoramic");sky.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"kloofendal_48d_partly_cloudy_puresky_1k.hdr"));sky.SetFloat("_Exposure",1.10f);sky.SetFloat("_Rotation",115);RenderSettings.skybox=sky;
            var sun=generated.Find("Daylight").GetComponent<Light>();sun.transform.rotation=Quaternion.Euler(40,45,0);sun.color=new Color(1,.93f,.80f);sun.intensity=2.2f;sun.shadows=LightShadows.Soft;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.61f,.72f,.80f);RenderSettings.ambientEquatorColor=new Color(.51f,.55f,.53f);RenderSettings.ambientGroundColor=new Color(.29f,.26f,.19f);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=85;RenderSettings.fogEndDistance=250;RenderSettings.fogColor=new Color(.67f,.78f,.80f);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
    }
}
