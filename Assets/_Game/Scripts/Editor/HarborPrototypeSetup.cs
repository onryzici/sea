using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SalvageCrew.Editor
{
    public static class HarborPrototypeSetup
    {
        public const string ScenePath = "Assets/_Game/Scenes/HarborPrototype.unity";
        public const string PrefabPath = "Assets/_Game/Prefabs/FirstPersonPlayer.prefab";
        private const string ActionsPath = "Assets/_Game/Settings/PlayerControls.inputactions";
        private const string RootName = "HarborPrototypeGenerated";

        [MenuItem("SalvageCrew/Build Harbor Prototype")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play mode first.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Regenerate only our owned subtree. Other objects in the scene survive.
            var oldRoot = scene.GetRootGameObjects().FirstOrDefault(g => g.name == RootName);
            if (oldRoot != null) Object.DestroyImmediate(oldRoot);
            var root = new GameObject(RootName).transform;
            var scenery = Child("Harbor", root);
            var boat = Child("StaticBoat_10m_x_4m", root);

            var stone = Material("QuayStone", new Color(.38f, .43f, .42f));
            var wood = Material("DockTimber", new Color(.48f, .32f, .19f));
            var hull = Material("HullTeal", new Color(.08f, .24f, .28f));
            var deck = Material("DeckCream", new Color(.74f, .70f, .53f));
            var metal = Material("RailSteel", new Color(.21f, .28f, .30f));
            var cabin = Material("CabinIvory", new Color(.85f, .84f, .72f));
            var glass = Material("WindowBlue", new Color(.12f, .26f, .32f));
            var sea = Material("SeaBlue", new Color(.09f, .38f, .49f));
            var sand = Material("ShoreSand", new Color(.66f, .61f, .45f));

            Box("Shore", scenery, new Vector3(0, -.4f, -12), new Vector3(32, 3.2f, 14), sand);
            Box("Quay", scenery, new Vector3(0, .6f, -4), new Vector3(18, 1.2f, 4), stone);
            Box("Pier", scenery, new Vector3(0, .95f, 4), new Vector3(5, .5f, 12), wood);
            for (int z = -1; z <= 9; z += 2)
            {
                Box("PierPlank", scenery, new Vector3(0, 1.205f, z), new Vector3(4.9f, .015f, .035f), metal, false);
                Box("PierPileLeft", scenery, new Vector3(-2.2f, .3f, z), new Vector3(.3f, 2f, .3f), wood);
                Box("PierPileRight", scenery, new Vector3(2.2f, .3f, z), new Vector3(.3f, 2f, .3f), wood);
            }
            Box("Sea_VisualOnly", scenery, new Vector3(0, -.08f, 5), new Vector3(180, .1f, 180), sea, false);
            Box("Hull", boat, new Vector3(7, .65f, 10), new Vector3(4, 1.2f, 10), hull);
            Box("Deck", boat, new Vector3(7, 1.35f, 10), new Vector3(4, .3f, 10), deck);
            Box("StarboardRail", boat, new Vector3(8.9f, 1.925f, 10), new Vector3(.16f, .85f, 10), metal);
            Box("SternRail", boat, new Vector3(7, 1.925f, 5.1f), new Vector3(4, .85f, .16f), metal);
            Box("BowRail", boat, new Vector3(7, 1.925f, 14.9f), new Vector3(4, .85f, .16f), metal);
            Box("PortRailFront", boat, new Vector3(5.1f, 1.925f, 11.75f), new Vector3(.16f, .85f, 6.5f), metal);
            Box("PortRailRear", boat, new Vector3(5.1f, 1.925f, 5.75f), new Vector3(.16f, .85f, 1.5f), metal);
            Box("Cabin", boat, new Vector3(7, 2.55f, 12.4f), new Vector3(2.2f, 2.1f, 2.4f), cabin);
            Box("CabinRoof", boat, new Vector3(7, 3.67f, 12.4f), new Vector3(2.5f, .14f, 2.7f), metal);
            Box("CabinFrontWindow", boat, new Vector3(7, 2.85f, 11.19f), new Vector3(1.7f, .7f, .025f), glass, false);
            Box("CabinSideWindow", boat, new Vector3(5.89f, 2.85f, 12.4f), new Vector3(.025f, .7f, 1.5f), glass, false);
            var ramp = Box("BoardingRamp", scenery, new Vector3(3.75f, 1.29f, 7.5f), new Vector3(3.3f, .18f, 1.8f), wood);
            ramp.transform.localRotation = Quaternion.Euler(0, 0, 5.2f);
            Box("RampGuideFront", scenery, new Vector3(3.75f, 1.72f, 8.45f), new Vector3(3.2f, .55f, .12f), metal).transform.rotation = ramp.transform.rotation;
            Box("RampGuideRear", scenery, new Vector3(3.75f, 1.72f, 6.55f), new Vector3(3.2f, .55f, .12f), metal).transform.rotation = ramp.transform.rotation;
            for (int i = 0; i < 3; i++)
                Box("HarborBollard", scenery, new Vector3(-1.8f, 1.45f, i * 3f), new Vector3(.28f, .5f, .28f), metal);
            Box("WarehouseMockup", scenery, new Vector3(-8, 3.2f, -9), new Vector3(6, 4f, 5), hull);

            var light = Child("Daylight", root).gameObject.AddComponent<Light>();
            light.type = LightType.Directional; light.intensity = 1.6f; light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(45, -35, 0);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.65f, .73f, .8f);
            var volume = Child("GlobalVolume", root).gameObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Settings/SampleSceneProfile.asset");

            var spawn = Child("PierSpawn", root);
            spawn.position = new Vector3(0, 1.25f, 4);
            spawn.rotation = Quaternion.Euler(0, 58, 0);
            var prefab = BuildPlayer(BuildActions());
            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            player.transform.SetParent(root);
            player.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
            var motor = new SerializedObject(player.GetComponent<FirstPersonMotor>());
            motor.FindProperty("spawnPoint").objectReferenceValue = spawn;
            motor.ApplyModifiedPropertiesWithoutUndo();
            BuildHud(root);
            EditorSceneManager.SaveScene(scene, ScenePath);
            ScrapPrototypeSetup.Apply();

            EditorSceneManager.SaveScene(scene, ScenePath);
            var others = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToArray();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) }.Concat(others).ToArray();
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = player;
        }

        private static InputActionAsset BuildActions()
        {
            if (File.Exists(ActionsPath)) return AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath);
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = asset.AddActionMap("Player");
            var move = map.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            map.AddAction("Look", InputActionType.Value, "<Mouse>/delta", expectedControlLayout: "Vector2");
            map.AddAction("Jump", InputActionType.Button, "<Keyboard>/space");
            map.AddAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
            map.AddAction("Reset", InputActionType.Button, "<Keyboard>/r");
            map.AddAction("ReleaseCursor", InputActionType.Button, "<Keyboard>/escape");
            map.AddAction("CaptureCursor", InputActionType.Button, "<Mouse>/leftButton");
            File.WriteAllText(ActionsPath, asset.ToJson());
            Object.DestroyImmediate(asset);
            AssetDatabase.ImportAsset(ActionsPath, ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath);
        }

        private static GameObject BuildPlayer(InputActionAsset actions)
        {
            var player = new GameObject("FirstPersonPlayer");
            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = .3f; cc.center = new Vector3(0, .9f, 0);
            cc.stepOffset = .3f; cc.slopeLimit = 45; cc.skinWidth = .03f; cc.minMoveDistance = 0;
            var input = player.AddComponent<LocalPlayerInput>();
            var motor = player.AddComponent<FirstPersonMotor>();
            var pivot = Child("CameraPivot", player.transform); pivot.localPosition = new Vector3(0, 1.65f, 0);
            var camera = Child("PlayerCamera", pivot).gameObject.AddComponent<Camera>();
            camera.tag = "MainCamera"; camera.nearClipPlane = .05f; camera.farClipPlane = 250; camera.fieldOfView = 75;
            camera.gameObject.AddComponent<AudioListener>();
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            var inputSettings = new SerializedObject(input);
            inputSettings.FindProperty("actions").objectReferenceValue = actions;
            inputSettings.ApplyModifiedPropertiesWithoutUndo();
            var settings = new SerializedObject(motor);
            settings.FindProperty("cameraPivot").objectReferenceValue = pivot;
            settings.ApplyModifiedPropertiesWithoutUndo();
            var prefab = PrefabUtility.SaveAsPrefabAsset(player, PrefabPath);
            Object.DestroyImmediate(player);
            return prefab;
        }

        private static void BuildHud(Transform root)
        {
            var go = new GameObject("HarborHUD", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
            go.transform.SetParent(root);
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            var cross = new GameObject("Crosshair", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            cross.transform.SetParent(go.transform, false);
            var cr = (RectTransform)cross.transform; cr.anchorMin = cr.anchorMax = new Vector2(.5f, .5f);
            cr.sizeDelta = new Vector2(5, 5);
            cross.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, .9f);
            cross.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            var panel = new GameObject("ControlsPanel", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            panel.transform.SetParent(go.transform, false);
            var pr = (RectTransform)panel.transform; pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(0, 1);
            pr.anchoredPosition = new Vector2(24, -24); pr.sizeDelta = new Vector2(420, 140);
            panel.GetComponent<UnityEngine.UI.Image>().color = new Color(.025f, .08f, .10f, .82f);
            panel.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            var label = new GameObject("Controls", typeof(RectTransform), typeof(TextMeshProUGUI));
            label.transform.SetParent(panel.transform, false);
            var lr = (RectTransform)label.transform; lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
            lr.offsetMin = new Vector2(18, 10); lr.offsetMax = new Vector2(-18, -10);
            var text = label.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset; text.fontSize = 22; text.color = Color.white; text.raycastTarget = false;
            text.text = "<b>SALVAGECREW / HARBOR</b>\nWASD  Move    Shift  Sprint    Space  Jump\nMouse  Look    R  Return to pier\nEsc  Release cursor    Click  Resume";
        }

        private static Transform Child(string name, Transform parent)
        { var go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform; }
        private static GameObject Box(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool solid = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name;
            go.transform.SetParent(parent, false); go.transform.position = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }
        private static Material Material(string name, Color color)
        {
            string path = "Assets/_Game/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", .2f);
            AssetDatabase.CreateAsset(material, path); return material;
        }
    }
}
