using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SalvageCrew.EditorTools
{
    // Updates existing controls in place; callbacks, object references and network roots survive.
    public static class HarborHudStyle
    {
        private static readonly Color Ink = new Color(.035f, .085f, .11f, .96f);
        private static readonly Color Paper = new Color(.97f, .92f, .80f);
        private static readonly Color Muted = new Color(.62f, .77f, .78f);
        private static readonly Color Teal = new Color(.10f, .57f, .57f);
        private static readonly Color Gold = new Color(.94f, .68f, .29f);
        private const string Art = "Assets/_Game/Art/UI/";
        private static TMP_FontAsset font;

        [MenuItem("SalvageCrew/UI/Apply Nautical HUD")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before styling UI.");
            var offline = GameObject.Find("HarborPrototypeGenerated/HarborHUD");
            var connection = GameObject.Find("HarborNetworkSession/ConnectionHUD");
            if (offline == null || connection == null) throw new InvalidOperationException("Open existing HarborPrototype first.");
            font = offline.GetComponentInChildren<TextMeshProUGUI>(true).font;
            AssetDatabase.Refresh();
            foreach (var name in new[] { "Cargo", "Helm", "Anchor" })
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(Art + name + ".png");
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 512;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            StyleHud(offline.transform);
            StyleConnection(connection.transform);
            const string path = "Assets/_Game/Prefabs/NetworkFirstPersonPlayer.prefab";
            var prefab = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var canvas = prefab.GetComponentInChildren<Canvas>(true);
                if (canvas == null) throw new InvalidOperationException("Missing network HUD.");
                StyleHud(canvas.transform);
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
            EditorSceneManager.MarkSceneDirty(offline.gameObject.scene);
            EditorSceneManager.SaveScene(offline.gameObject.scene);
            AssetDatabase.SaveAssets();
        }

        static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var existing = parent.Find(name);
            var t = existing != null ? (RectTransform)existing : (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            if (existing == null) t.SetParent(parent, false);
            t.anchorMin = t.anchorMax = anchor; t.pivot = new Vector2(.5f, .5f);
            t.anchoredPosition = pos; t.sizeDelta = size; t.localScale = Vector3.one;
            return t;
        }
        static T Need<T>(Component c) where T : Component
        {
            var existing = c.GetComponent<T>();
            return existing != null ? existing : c.gameObject.AddComponent<T>();
        }
        static UnityEngine.UI.Image Box(Transform p, string n, Vector2 a, Vector2 xy, Vector2 wh, Color color)
        {
            var im = Need<UnityEngine.UI.Image>(Rect(p,n,a,xy,wh));
            im.color = color; im.raycastTarget = false; return im;
        }
        static TextMeshProUGUI Text(Transform p, string n, Vector2 a, Vector2 xy, Vector2 wh, string value, float size, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
        {
            var text = Need<TextMeshProUGUI>(Rect(p,n,a,xy,wh));
            text.font = font; text.text = value; text.fontSize = size; text.enableAutoSizing = false;
            text.color = color; text.alignment = alignment; text.raycastTarget = false;
            text.fontStyle = FontStyles.Normal; text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow; text.margin = Vector4.zero;
            return text;
        }
        static UnityEngine.UI.Image Icon(Transform p,string n,Vector2 a,Vector2 xy,float size,string asset)
        {
            var im=Box(p,n,a,xy,new Vector2(size,size),Color.white);
            im.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+asset+".png"); im.preserveAspect=true;return im;
        }
        static CanvasGroup Group(Component c)
        {
            var g=Need<CanvasGroup>(c);g.blocksRaycasts=false;g.interactable=false;g.alpha=1;return g;
        }
        static void Ref(SerializedObject so,string name,UnityEngine.Object value) => so.FindProperty(name).objectReferenceValue=value;
        static void StyleHud(Transform canvas)
        {
            Vector2 center=new Vector2(.5f,.5f), bottom=new Vector2(.5f,0), left=new Vector2(0,0);
            var controls=Box(canvas,"ControlsPanel",left,new Vector2(237,69),new Vector2(418,82),Ink);
            Box(controls.transform,"Accent",center,new Vector2(-206,0),new Vector2(3,82),Teal);
            Text(controls.transform,"Controls",center,Vector2.zero,new Vector2(378,62),
                "<b><color=#F0AD4A>WASD</color></b>  Yürü    <b>SHIFT</b>  Koş    <b>SPACE</b>  Zıpla\n<b><color=#F0AD4A>E</color></b>  Etkileşim    <b>R</b>  Kurtar    <b>TAB</b>  Oturum",16,Paper);
            var card=Box(canvas,"InteractionCard",bottom,new Vector2(0,191),new Vector2(600,100),Ink);
            card.transform.SetSiblingIndex(0);
            Box(card.transform,"Accent",center,new Vector2(-298,0),new Vector2(4,100),Gold);
            var icon=Icon(card.transform,"ContextIcon",center,new Vector2(-249,0),76,"Cargo");
            var prompt=Text(canvas,"InteractionPrompt",bottom,new Vector2(40,191),new Vector2(462,80),"",22,Paper);
            prompt.lineSpacing=9;
            var reticle=Need<UnityEngine.UI.Image>(Rect(canvas,"Crosshair",center,Vector2.zero,new Vector2(4,4)));
            reticle.color=Paper; reticle.raycastTarget=false;
            var outline=Need<UnityEngine.UI.Outline>(reticle);outline.effectColor=Ink;outline.effectDistance=new Vector2(1,-1);
            var hint=Text(canvas,"CursorHint",new Vector2(.5f,1),new Vector2(0,-65),new Vector2(620,38),"İmleç serbest  ·  Devam etmek için oyun alanına tıkla",18,Paper,TextAlignmentOptions.Center);
            var shadow=Need<UnityEngine.UI.Shadow>(hint);shadow.effectColor=Ink;shadow.effectDistance=new Vector2(1,-2);
            var presentation=Need<HarborHudPresentation>(canvas);
            var so=new SerializedObject(presentation);
            Ref(so,"prompt",prompt);Ref(so,"promptGroup",Group(card));Ref(so,"controlsGroup",Group(controls));
            Ref(so,"icon",icon);Ref(so,"reticle",reticle);Ref(so,"cursorHint",hint);
            Ref(so,"controlsLabel",controls.transform.Find("Controls").GetComponent<TextMeshProUGUI>());
            Ref(so,"cargoIcon",AssetDatabase.LoadAssetAtPath<Sprite>(Art+"Cargo.png"));
            Ref(so,"helmIcon",AssetDatabase.LoadAssetAtPath<Sprite>(Art+"Helm.png"));so.ApplyModifiedPropertiesWithoutUndo();
            var carry=canvas.GetComponentInChildren<CarryHud>(true);
            var network=canvas.GetComponentInChildren<NetworkCarryHud>(true);
            var owner=new SerializedObject(carry != null ? (UnityEngine.Object)carry : network);
            Ref(owner,"presentation",presentation);owner.ApplyModifiedPropertiesWithoutUndo();
        }

        static void StyleConnection(Transform canvas)
        {
            var center=new Vector2(.5f,.5f);
            var brand=Box(canvas,"Brand",new Vector2(0,1),new Vector2(159,-59),new Vector2(262,62),Ink);
            Icon(brand.transform,"Anchor",center,new Vector2(-99,0),48,"Anchor");
            Text(brand.transform,"Name",center,new Vector2(29,0),new Vector2(192,40),"<b>SALVAGECREW</b>\n<size=11><color=#A9C6C8>HURDA • DENİZ • MÜRETTEBAT</color></size>",20,Paper);
            var role=Text(canvas,"Role",new Vector2(1,1),new Vector2(-185,-59),new Vector2(272,49),"SOLO",20,Paper,TextAlignmentOptions.Center);
            var badge=Box(canvas,"RoleBadge",new Vector2(1,1),new Vector2(-185,-59),new Vector2(314,62),Ink);
            badge.transform.SetSiblingIndex(0);
            Box(badge.transform,"Accent",center,new Vector2(155,0),new Vector2(3,62),Teal);
            var panel=Box(canvas,"ConnectionPanel",center,Vector2.zero,new Vector2(760,650),Ink);
            panel.raycastTarget=true;
            var p=panel.transform;
            Box(p,"TopRule",center,new Vector2(0,323),new Vector2(756,4),Gold);
            Icon(p,"AnchorEmblem",center,new Vector2(-279,239),114,"Anchor");
            Text(p,"Title",center,new Vector2(48,251),new Vector2(498,58),"<b>SALVAGECREW</b>",43,Paper);
            Text(p,"Subtitle",center,new Vector2(49,204),new Vector2(496,44),"Mürettebatını topla. Birlikte denize açıl.",20,Muted);
            Box(p,"Divider",center,new Vector2(0,166),new Vector2(650,1),new Color(.3f,.46f,.47f,.6f));
            Text(p,"EndpointLabel",center,new Vector2(-96,139),new Vector2(460,25),"BAĞLANTI ADRESİ",13,Muted);
            Text(p,"PortLabel",center,new Vector2(252,139),new Vector2(138,25),"PORT",13,Muted);
            Field(p,"Address",new Vector2(-86,98),new Vector2(480,52));
            Field(p,"Port",new Vector2(257,98),new Vector2(138,52));
            Button(p,"Host",new Vector2(-168,15),new Vector2(316,66),"HOST BAŞLAT",Teal,Paper);
            Button(p,"Client",new Vector2(168,15),new Vector2(316,66),"CLIENT BAĞLAN",new Color(.19f,.29f,.32f),Paper);
            Text(p,"LocalHelp",center,new Vector2(0,-37),new Vector2(652,26),"Aynı bilgisayarda: Editor → Host   ·   Standalone → Client",15,Muted,TextAlignmentOptions.Center);
            var status=Box(p,"StatusWell",center,new Vector2(0,-108),new Vector2(652,94),new Color(.015f,.045f,.065f,.9f));
            status.transform.SetSiblingIndex(0);
            Text(p,"Status",center,new Vector2(0,-108),new Vector2(614,76),"Tek oyuncu hazır.",18,Paper);
            Button(p,"Disconnect",new Vector2(190,-182),new Vector2(272,38),"Bağlantıyı kes",new Color(.22f,.14f,.14f),new Color(1,.72f,.64f));
            Text(p,"BackgroundNote",center,new Vector2(-156,-182),new Vector2(340,42),"Panel açıkken simülasyon sürer.",14,Muted);
            Button(p,"Resume",new Vector2(0,-248),new Vector2(652,64),"OYUNA DEVAM ET",Gold,Ink);
            Text(p,"Footer",center,new Vector2(0,-300),new Vector2(652,24),"TAB  Paneli aç / kapat     ·     ESC  İmleci serbest bırak",14,Muted,TextAlignmentOptions.Center);
        }
        static void Field(Transform p,string name,Vector2 pos,Vector2 size)
        {
            var t=Rect(p,name,new Vector2(.5f,.5f),pos,size);
            var im=t.GetComponent<UnityEngine.UI.Image>();im.color=new Color(.12f,.21f,.24f);im.raycastTarget=true;
            var input=t.GetComponent<TMP_InputField>();input.textComponent.fontSize=24;input.textComponent.color=Paper;
            input.caretColor=Gold;input.customCaretColor=true;input.selectionColor=new Color(.1f,.55f,.58f,.6f);
        }
        static void Button(Transform p,string name,Vector2 pos,Vector2 size,string caption,Color bg,Color fg)
        {
            var t=Rect(p,name,new Vector2(.5f,.5f),pos,size);
            var button=t.GetComponent<UnityEngine.UI.Button>();
            var image=t.GetComponent<UnityEngine.UI.Image>();image.color=bg;image.raycastTarget=true;
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.15f,1.15f,1.15f);
            colors.pressedColor=new Color(.7f,.82f,.83f);colors.selectedColor=new Color(1.1f,1.1f,1.1f);colors.disabledColor=new Color(.48f,.48f,.48f,.65f);colors.fadeDuration=.1f;button.colors=colors;
            Text(t,"Label",new Vector2(.5f,.5f),Vector2.zero,size-new Vector2(20,6),"<b>"+caption+"</b>",name=="Disconnect"?16:21,fg,TextAlignmentOptions.Center);
        }
    }
}
