using System.Linq;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SalvageCrew.Editor
{
    // Additive and repeatable: never rebuild HarborPrototypeGenerated or existing boat visuals.
    public static class BoatPrototypeSetup
    {
        public const string BoatPath = "Assets/_Game/Prefabs/NetworkBoat.prefab";
        [MenuItem("SalvageCrew/Setup Moving Boat Prototype")]
        public static void Apply()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != HarborPrototypeSetup.ScenePath)
                throw new System.InvalidOperationException("Open HarborPrototype and stop Play first.");
            var harbor = GameObject.Find("HarborPrototypeGenerated").transform;
            var original = harbor.Find("StaticBoat_10m_x_4m").gameObject;
            var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/_Game/Settings/DeckGrip.physicMaterial");
            if (material == null)
            {
                material = new PhysicsMaterial("DeckGrip") { staticFriction = 1f, dynamicFriction = .85f,
                    frictionCombine = PhysicsMaterialCombine.Maximum, bounceCombine = PhysicsMaterialCombine.Minimum, bounciness = 0 };
                AssetDatabase.CreateAsset(material, "Assets/_Game/Settings/DeckGrip.physicMaterial");
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BoatPath);
            if (prefab == null)
            {
                var boat = Object.Instantiate(original); boat.name = "NetworkBoat";
                try
                {
                    foreach (Transform child in boat.transform) child.localPosition -= new Vector3(7, 0, 10);
                    boat.transform.position = Vector3.zero;
                    var body = boat.AddComponent<Rigidbody>(); body.mass = 2500; body.useGravity = true;
                    body.interpolation = RigidbodyInterpolation.Interpolate; body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                    body.centerOfMass = new Vector3(0, .8f, 0); body.solverIterations = 12; body.solverVelocityIterations = 4;
                    foreach (var collider in boat.GetComponentsInChildren<Collider>()) collider.sharedMaterial = material;
                    boat.AddComponent<NetworkObject>(); var controller = boat.AddComponent<NetworkBoat>();
                    var wheel = GameObject.CreatePrimitive(PrimitiveType.Cube); wheel.name = "Helm";
                    wheel.transform.SetParent(boat.transform, false); wheel.transform.localPosition = new Vector3(0, 2.3f, 1.02f);
                    wheel.transform.localScale = new Vector3(.55f, .35f, .15f);
                    wheel.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/RailSteel.mat");
                    wheel.GetComponent<Collider>().sharedMaterial = material;
                    var so = new SerializedObject(controller); so.FindProperty("helm").objectReferenceValue = wheel.transform; so.ApplyModifiedPropertiesWithoutUndo();
                    prefab = PrefabUtility.SaveAsPrefabAsset(boat, BoatPath);
                }
                finally { Object.DestroyImmediate(boat); }
            }
            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>("Assets/_Game/Settings/NetworkPrefabs.asset");
            if (!list.Contains(prefab)) { list.Add(new NetworkPrefab { Prefab = prefab }); EditorUtility.SetDirty(list); }
            const string playerPath = "Assets/_Game/Prefabs/NetworkFirstPersonPlayer.prefab";
            var player = PrefabUtility.LoadPrefabContents(playerPath);
            try
            {
                if (player.GetComponent<DeckPassenger>() == null) player.AddComponent<DeckPassenger>();
                foreach (var label in player.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
                    if (label.name == "Controls") label.text = label.text.Replace("Return to pier", "Return to boat");
                PrefabUtility.SaveAsPrefabAsset(player, playerPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }
            foreach (string name in new[] { "NetworkLightBox", "NetworkMetalCrate", "NetworkScrapEngine" })
            {
                string path = "Assets/_Game/Prefabs/" + name + ".prefab";
                var item = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (var collider in item.GetComponentsInChildren<Collider>())
                        if (collider.sharedMaterial == null) collider.sharedMaterial = material;
                    var body = item.GetComponent<Rigidbody>(); body.solverIterations = Mathf.Max(body.solverIterations, 12);
                    body.solverVelocityIterations = Mathf.Max(body.solverVelocityIterations, 4);
                    PrefabUtility.SaveAsPrefabAsset(item, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(item); }
            }
            var session = Object.FindAnyObjectByType<HarborSession>();
            var settings = new SerializedObject(session);
            settings.FindProperty("boatPrefab").objectReferenceValue = prefab;
            settings.FindProperty("offlineBoat").objectReferenceValue = original;
            var ramp = settings.FindProperty("boardingRamp");
            string[] names = { "BoardingRamp", "RampGuideFront", "RampGuideRear" }; ramp.arraySize = names.Length;
            for (int i = 0; i < names.Length; i++) ramp.GetArrayElementAtIndex(i).objectReferenceValue = harbor.Find("Harbor").Find(names[i]).gameObject;
            settings.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }
    }
}
