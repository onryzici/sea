using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SalvageCrew.Editor
{
    public static class MultiplayerPrototypeSetup
    {
        [MenuItem("SalvageCrew/Setup Multiplayer Prototype")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != HarborPrototypeSetup.ScenePath)
                throw new System.InvalidOperationException("Open HarborPrototype and stop Play first.");
            var harbor = GameObject.Find("HarborPrototypeGenerated").transform;
            var offlinePlayer = harbor.Find("FirstPersonPlayer").gameObject;
            var offlineHud = harbor.Find("HarborHUD").gameObject;
            var prefabs = new[] { "LightBox", "MetalCrate", "ScrapEngine" }.Select(BuildScrap).ToArray();
            var player = BuildPlayer(offlineHud);
            const string listPath = "Assets/_Game/Settings/NetworkPrefabs.asset";
            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(listPath);
            if (list == null) { list = ScriptableObject.CreateInstance<NetworkPrefabsList>(); AssetDatabase.CreateAsset(list, listPath); }
            foreach (var prefab in prefabs.Append(player))
                if (!list.Contains(prefab)) list.Add(new NetworkPrefab { Prefab = prefab });
            EditorUtility.SetDirty(list);

            var root = GameObject.Find("HarborNetworkSession");
            if (root == null) root = new GameObject("HarborNetworkSession");
            var transport = root.GetComponent<UnityTransport>() ?? root.AddComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", 7777, "0.0.0.0");
            transport.ConnectTimeoutMS = 1000; transport.MaxConnectAttempts = 5; transport.DisconnectTimeoutMS = 5000;
            var manager = root.GetComponent<NetworkManager>() ?? root.AddComponent<NetworkManager>();
            manager.NetworkConfig.NetworkTransport = transport;
            manager.NetworkConfig.PlayerPrefab = player;
            manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Clear();
            manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(list);
            manager.NetworkConfig.EnableSceneManagement = false;
            manager.NetworkConfig.ConnectionApproval = true; manager.NetworkConfig.TickRate = 30;
            var session = root.GetComponent<HarborSession>() ?? root.AddComponent<HarborSession>();
            var settings = new SerializedObject(session);
            Ref(settings, "offlinePlayer", offlinePlayer); Ref(settings, "offlineHud", offlineHud);
            Ref(settings, "offlineScraps", harbor.Find("ScrapTestItems"));
            var array = settings.FindProperty("scrapPrefabs"); array.arraySize = prefabs.Length;
            for (int i = 0; i < prefabs.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];
            BuildPanel(root.transform, settings);
            settings.ApplyModifiedPropertiesWithoutUndo();
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var events = new GameObject("HarborEventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
                events.transform.SetParent(root.transform, false);
            }
            PlayerSettings.runInBackground = true;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes(); AssetDatabase.SaveAssets();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(BoatPrototypeSetup.BoatPath) != null) BoatPrototypeSetup.Apply();
        }
        private static GameObject BuildScrap(string name)
        {
            string path = "Assets/_Game/Prefabs/Network" + name + ".prefab";
            var saved = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (saved != null) return saved;
            var source = PrefabUtility.LoadPrefabContents("Assets/_Game/Prefabs/" + name + ".prefab");
            try
            {
                source.name = "Network" + name;
                source.AddComponent<NetworkObject>();
                var transform = source.AddComponent<NetworkTransform>();
                transform.Interpolate = true; transform.UseUnreliableDeltas = true;
                transform.SyncScaleX = transform.SyncScaleY = transform.SyncScaleZ = false;
                source.AddComponent<NetworkRigidbody>(); source.AddComponent<NetworkScrap>();
                return PrefabUtility.SaveAsPrefabAsset(source, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(source); }
        }
        private static GameObject BuildPlayer(GameObject hud)
        {
            const string path = "Assets/_Game/Prefabs/NetworkFirstPersonPlayer.prefab";
            var saved = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (saved != null) return saved;
            var player = PrefabUtility.LoadPrefabContents(HarborPrototypeSetup.PrefabPath);
            try
            {
                player.name = "NetworkFirstPersonPlayer"; player.AddComponent<NetworkObject>();
                var net = player.AddComponent<NetworkCrewPlayer>();
                var visuals = new GameObject("RemoteCrewVisual"); visuals.transform.SetParent(player.transform, false);
                var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule); capsule.name = "CrewCapsule";
                capsule.transform.SetParent(visuals.transform, false); capsule.transform.localPosition = Vector3.up * .9f;
                capsule.transform.localScale = new Vector3(.6f, .9f, .6f); Object.DestroyImmediate(capsule.GetComponent<Collider>());
                var marker = new GameObject("LookDirection"); marker.transform.SetParent(visuals.transform, false);
                marker.transform.localPosition = Vector3.up * 1.5f;
                var arrow = GameObject.CreatePrimitive(PrimitiveType.Cube); arrow.name = "DirectionMark";
                arrow.transform.SetParent(marker.transform, false); arrow.transform.localPosition = Vector3.forward * .4f;
                arrow.transform.localScale = new Vector3(.08f, .08f, .5f); Object.DestroyImmediate(arrow.GetComponent<Collider>());
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit")); material.color = new Color(.3f, .65f, .8f);
                const string matPath = "Assets/_Game/Materials/CrewPrototype.mat";
                var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (existing == null) AssetDatabase.CreateAsset(material, matPath);
                else { Object.DestroyImmediate(material); material = existing; }
                capsule.GetComponent<Renderer>().sharedMaterial = arrow.GetComponent<Renderer>().sharedMaterial = material;
                var canvas = Object.Instantiate(hud); canvas.name = "LocalCrewHUD"; canvas.transform.SetParent(player.transform, false);
                Object.DestroyImmediate(canvas.GetComponentInChildren<CarryHud>());
                var presenter = canvas.AddComponent<NetworkCarryHud>();
                var hs = new SerializedObject(presenter); Ref(hs, "player", net);
                Ref(hs, "label", canvas.transform.Find("InteractionPrompt").GetComponent<TextMeshProUGUI>());
                hs.ApplyModifiedPropertiesWithoutUndo();
                var ps = new SerializedObject(net);
                Ref(ps, "cameraPivot", player.transform.Find("CameraPivot"));
                Ref(ps, "playerCamera", player.GetComponentInChildren<Camera>());
                Ref(ps, "remoteVisual", visuals); Ref(ps, "localHud", canvas); Ref(ps, "lookMarker", marker.transform);
                ps.ApplyModifiedPropertiesWithoutUndo();
                // Start inactive: remote prefab activation must not lock/release the local cursor.
                player.GetComponent<LocalPlayerInput>().enabled = false;
                player.GetComponent<FirstPersonMotor>().enabled = false;
                player.GetComponent<PhysicsCarry>().enabled = false;
                player.GetComponent<LocalCarryInteraction>().enabled = false;
                player.GetComponentInChildren<Camera>().enabled = false;
                player.GetComponentInChildren<AudioListener>().enabled = false;
                return PrefabUtility.SaveAsPrefabAsset(player, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }
        }
        private static void BuildPanel(Transform root, SerializedObject settings)
        {
            var canvas = root.Find("ConnectionHUD");
            if (canvas == null)
            {
                canvas = new GameObject("ConnectionHUD", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster)).transform;
                canvas.SetParent(root, false);
                canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay; canvas.GetComponent<Canvas>().sortingOrder = 100;
                var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
                var role = Text(canvas, "Role", "SOLO  |  Tab / Esc: Bağlantı paneli", new Vector2(-25, -25), new Vector2(650, 45));
                var rr = role.rectTransform; rr.anchorMin = rr.anchorMax = rr.pivot = new Vector2(1, 1); role.alignment = TextAlignmentOptions.TopRight;
                var panel = new GameObject("ConnectionPanel", typeof(RectTransform), typeof(UnityEngine.UI.Image)).transform;
                panel.SetParent(canvas, false); var rect = (RectTransform)panel;
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.sizeDelta = new Vector2(640, 455);
                panel.GetComponent<UnityEngine.UI.Image>().color = new Color(.025f, .08f, .1f, .97f);
                Text(panel, "Title", "SALVAGECREW — LOCAL CO-OP", new Vector2(0, 180), new Vector2(570, 50)).fontSize = 28;
                Text(panel, "EndpointLabel", "Adres                                 Port", new Vector2(0, 120), new Vector2(570, 35));
                Input(panel, "Address", "127.0.0.1", new Vector2(-95, 75), new Vector2(360, 50));
                Input(panel, "Port", "7777", new Vector2(210, 75), new Vector2(120, 50));
                Button(panel, "Host", "Host başlat", new Vector2(-155, 0));
                Button(panel, "Client", "Client bağlan", new Vector2(155, 0));
                Button(panel, "Disconnect", "Bağlantıyı kes", new Vector2(-155, -65));
                Button(panel, "Resume", "Oyuna / Solo devam", new Vector2(155, -65));
                Text(panel, "Status", "Tek oyuncu hazır.", new Vector2(0, -155), new Vector2(570, 100)).fontSize = 21;
            }
            var cp = canvas.Find("ConnectionPanel");
            Ref(settings, "panel", cp.gameObject); Ref(settings, "address", cp.Find("Address").GetComponent<TMP_InputField>());
            Ref(settings, "port", cp.Find("Port").GetComponent<TMP_InputField>());
            Ref(settings, "statusLabel", cp.Find("Status").GetComponent<TextMeshProUGUI>());
            Ref(settings, "roleLabel", canvas.Find("Role").GetComponent<TextMeshProUGUI>());
            Ref(settings, "hostButton", cp.Find("Host").GetComponent<UnityEngine.UI.Button>());
            Ref(settings, "clientButton", cp.Find("Client").GetComponent<UnityEngine.UI.Button>());
            Ref(settings, "disconnectButton", cp.Find("Disconnect").GetComponent<UnityEngine.UI.Button>());
            Ref(settings, "resumeButton", cp.Find("Resume").GetComponent<UnityEngine.UI.Button>());
        }
        private static TextMeshProUGUI Text(Transform parent, string name, string value, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); go.transform.SetParent(parent, false);
            var text = go.GetComponent<TextMeshProUGUI>(); text.font = TMP_Settings.defaultFontAsset; text.fontSize = 24;
            text.text = value; text.color = Color.white; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            text.rectTransform.anchoredPosition = position; text.rectTransform.sizeDelta = size; return text;
        }
        private static void Input(Transform parent, string name, string value, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(TMP_InputField));
            go.transform.SetParent(parent, false); var rect = (RectTransform)go.transform;
            rect.anchoredPosition = position; rect.sizeDelta = size;
            go.GetComponent<UnityEngine.UI.Image>().color = new Color(.14f, .2f, .23f);
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(UnityEngine.UI.RectMask2D)).transform;
            viewport.SetParent(go.transform, false); var vr = (RectTransform)viewport;
            vr.anchorMin = Vector2.zero; vr.anchorMax = Vector2.one; vr.offsetMin = new Vector2(12, 5); vr.offsetMax = new Vector2(-12, -5);
            var text = Text(viewport, "Text", "", Vector2.zero, Vector2.zero);
            text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero; text.alignment = TextAlignmentOptions.MidlineLeft;
            var input = go.GetComponent<TMP_InputField>(); input.textViewport = vr; input.textComponent = text; input.text = value;
            input.targetGraphic = go.GetComponent<UnityEngine.UI.Image>();
        }
        private static void Button(Transform parent, string name, string value, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
            go.transform.SetParent(parent, false); var rect = (RectTransform)go.transform;
            rect.anchoredPosition = position; rect.sizeDelta = new Vector2(285, 50);
            go.GetComponent<UnityEngine.UI.Image>().color = new Color(.14f, .35f, .39f);
            go.GetComponent<UnityEngine.UI.Button>().targetGraphic = go.GetComponent<UnityEngine.UI.Image>();
            Text(go.transform, "Label", value, Vector2.zero, rect.sizeDelta).fontSize = 23;
        }
        private static void Ref(SerializedObject settings, string name, Object target)
        { settings.FindProperty(name).objectReferenceValue = target; }
    }
}
