using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SalvageCrew.Editor
{
    public static class UhdDisplaySetup
    {
        [MenuItem("SalvageCrew/Display/Apply 4K UHD Defaults")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play before configuring display defaults.");
            PlayerSettings.defaultScreenWidth=3840;
            PlayerSettings.defaultScreenHeight=2160;
            PlayerSettings.defaultIsNativeResolution=false;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.resizableWindow=true;
            PlayerSettings.macRetinaSupport=true;
            var pipeline=(UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            pipeline.renderScale=1;
            EditorUtility.SetDirty(pipeline);
            AssetDatabase.SaveAssets();
            SelectGameView();
        }

        [MenuItem("SalvageCrew/Display/Select 3840 x 2160 Game View")]
        public static void SelectGameView()
        {
            // Editor-only resolution preset; no runtime reflection or custom render target.
            var assembly=typeof(UnityEditor.Editor).Assembly;
            var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes=singleton.GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null);
            var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{0}); // Standalone
            var groupType=group.GetType();
            var get=groupType.GetMethod("GetGameViewSize");
            int count=(int)groupType.GetMethod("GetTotalCount").Invoke(group,null),index=-1;
            for(int i=0;i<count;i++)
            {
                var s=get.Invoke(group,new object[]{i});var type=s.GetType();
                if((int)type.GetProperty("width").GetValue(s)==3840&&(int)type.GetProperty("height").GetValue(s)==2160){index=i;break;}
            }
            if(index<0)
            {
                var sizeType=assembly.GetType("UnityEditor.GameViewSize");
                var enumType=assembly.GetType("UnityEditor.GameViewSizeType");
                var s=Activator.CreateInstance(sizeType,new object[]{Enum.Parse(enumType,"FixedResolution"),3840,2160,"SalvageCrew 4K UHD"});
                groupType.GetMethod("AddCustomSize").Invoke(group,new[]{s});index=count;
            }
            var viewType=assembly.GetType("UnityEditor.GameView");
            var view=EditorWindow.GetWindow(viewType);
            viewType.GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).SetValue(view,index);
            view.Repaint();
            Debug.Log("SalvageCrew: 3840 x 2160 Game view selected; URP renders at 100%. Window display may scale to fit the monitor.");
        }
    }
}
