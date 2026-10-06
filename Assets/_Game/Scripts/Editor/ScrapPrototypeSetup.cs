using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SalvageCrew.Editor
{
    public static class ScrapPrototypeSetup
    {
        [MenuItem("SalvageCrew/Setup Scrap Carry Prototype")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play first.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != HarborPrototypeSetup.ScenePath)
                throw new System.InvalidOperationException("Open HarborPrototype first.");
            AddInput();
            var prefab = PrefabUtility.LoadPrefabContents(HarborPrototypeSetup.PrefabPath);
            try
            {
                WirePlayer(prefab);
                PrefabUtility.SaveAsPrefabAsset(prefab, HarborPrototypeSetup.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }

            var player = Object.FindAnyObjectByType<FirstPersonMotor>().gameObject;
            WirePlayer(player);
            var root = GameObject.Find("HarborPrototypeGenerated").transform;
            var scraps = root.Find("ScrapTestItems");
            if (scraps == null) { scraps = new GameObject("ScrapTestItems").transform; scraps.SetParent(root, false); }
            CreateItem(scraps, "LightBox", "Hafif kutu", 3, new Vector3(.55f, .55f, .55f),
                new Color(.58f, .37f, .18f), new Vector3(1.55f, 1.49f, 2), .95f, true);
            CreateItem(scraps, "MetalCrate", "Metal kasa", 12, new Vector3(.75f, .65f, .65f),
                new Color(.36f, .43f, .47f), new Vector3(1.55f, 1.54f, 4), .75f, true);
            CreateItem(scraps, "ScrapEngine", "Hurda motor", 35, new Vector3(.8f, .6f, .65f),
                new Color(.23f, .27f, .25f), new Vector3(1.55f, 1.64f, 6), .5f, false);
            WireHud(root.Find("HarborHUD"), player);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static void AddInput()
        {
            const string path = "Assets/_Game/Settings/PlayerControls.inputactions";
            var original = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            var asset = Object.Instantiate(original);
            try
            {
                var map = asset.FindActionMap("Player", true);
                if (map.FindAction("Interact") != null) return;
                map.AddAction("Interact", InputActionType.Button, "<Keyboard>/e");
                File.WriteAllText(path, asset.ToJson());
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }
            finally { Object.DestroyImmediate(asset); }
        }
        private static void WirePlayer(GameObject player)
        {
            var carry = player.GetComponent<PhysicsCarry>() ?? player.AddComponent<PhysicsCarry>();
            if (player.GetComponent<LocalCarryInteraction>() == null) player.AddComponent<LocalCarryInteraction>();
            var settings = new SerializedObject(carry);
            settings.FindProperty("view").objectReferenceValue = player.GetComponentInChildren<Camera>().transform;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void CreateItem(Transform parent, string assetName, string label, float mass,
            Vector3 size, Color color, Vector3 position, float movement, bool sprint)
        {
            string path = "Assets/_Game/Prefabs/" + assetName + ".prefab";
            var saved = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (saved == null)
            {
                var item = new GameObject(assetName);
                var body = item.AddComponent<Rigidbody>();
                body.mass = mass; body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.linearDamping = .15f; body.angularDamping = .8f;
                body.solverIterations = 10; body.solverVelocityIterations = 4;
                var scrap = item.AddComponent<ScrapItem>();
                var materialPath = "Assets/_Game/Materials/" + assetName + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", .25f);
                    AssetDatabase.CreateAsset(material, materialPath);
                }
                Shape(item.transform, "Body", Vector3.zero, size, material);
                if (mass == 35)
                {
                    Shape(item.transform, "CylinderHead", new Vector3(0, .38f, 0), new Vector3(.65f, .22f, .48f), material);
                    Shape(item.transform, "SideHousing", new Vector3(.42f, -.1f, 0), new Vector3(.2f, .3f, .4f), material);
                }
                else if (mass == 12)
                {
                    Shape(item.transform, "TopBrace", new Vector3(0, .335f, 0), new Vector3(.78f, .05f, .12f), material);
                }
                var so = new SerializedObject(scrap);
                so.FindProperty("displayName").stringValue = label;
                so.FindProperty("mass").floatValue = mass;
                so.FindProperty("springForce").floatValue = mass == 3 ? 180 : mass == 12 ? 240 : 300;
                so.FindProperty("dampingForce").floatValue = mass == 3 ? 38 : mass == 12 ? 85 : 160;
                so.FindProperty("maximumForce").floatValue = mass == 3 ? 180 : mass == 12 ? 400 : 650;
                so.FindProperty("maximumSpeed").floatValue = mass == 3 ? 5 : mass == 12 ? 4 : 2.5f;
                so.FindProperty("movementMultiplier").floatValue = movement;
                so.FindProperty("allowSprint").boolValue = sprint;
                so.ApplyModifiedPropertiesWithoutUndo();
                saved = PrefabUtility.SaveAsPrefabAsset(item, path);
                Object.DestroyImmediate(item);
            }
            // Existing instances and Inspector overrides survive repeat setup calls.
            if (parent.Find(assetName) != null) return;
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(saved);
            instance.name = assetName; instance.transform.SetParent(parent, false);
            instance.transform.position = position;
        }
        private static void Shape(Transform parent, string name, Vector3 position, Vector3 size, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name;
            go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = material;
        }
        private static void WireHud(Transform hud, GameObject player)
        {
            var child = hud.Find("InteractionPrompt");
            if (child == null)
            {
                child = new GameObject("InteractionPrompt", typeof(RectTransform), typeof(TextMeshProUGUI)).transform;
                child.SetParent(hud, false);
                var rect = (RectTransform)child;
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
                rect.anchoredPosition = new Vector2(0, -90); rect.sizeDelta = new Vector2(600, 85);
                var text = child.GetComponent<TextMeshProUGUI>();
                text.font = TMP_Settings.defaultFontAsset; text.fontSize = 26; text.color = Color.white;
                text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            }
            var presenter = child.GetComponent<CarryHud>() ?? child.gameObject.AddComponent<CarryHud>();
            var so = new SerializedObject(presenter);
            so.FindProperty("carry").objectReferenceValue = player.GetComponent<PhysicsCarry>();
            so.FindProperty("input").objectReferenceValue = player.GetComponent<LocalPlayerInput>();
            so.FindProperty("label").objectReferenceValue = child.GetComponent<TextMeshProUGUI>();
            so.ApplyModifiedPropertiesWithoutUndo();
            var controls = hud.Find("ControlsPanel/Controls").GetComponent<TextMeshProUGUI>();
            if (!controls.text.Contains("E  Hold")) controls.text += "\nE  Hold / Drop scrap";
            ((RectTransform)hud.Find("ControlsPanel")).sizeDelta = new Vector2(420, 170);
        }
    }
}
