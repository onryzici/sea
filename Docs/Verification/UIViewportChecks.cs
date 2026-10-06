using System;
using System.Reflection;
using UnityEditor;

// Only changes the Editor preview preset. Does not change 4K Player Settings.
public static class UIViewportChecks
{
    public static object Select(int width, int height)
    {
        var assembly=typeof(UnityEditor.Editor).Assembly;
        var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
        var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var sizes=singleton.GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null);
        var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{0});
        var gt=group.GetType();var get=gt.GetMethod("GetGameViewSize");
        int count=(int)gt.GetMethod("GetTotalCount").Invoke(group,null),index=-1;
        for(int i=0;i<count;i++)
        {
            var s=get.Invoke(group,new object[]{i});var t=s.GetType();
            if((int)t.GetProperty("width").GetValue(s)==width&&(int)t.GetProperty("height").GetValue(s)==height){index=i;break;}
        }
        if(index<0)
        {
            var t=assembly.GetType("UnityEditor.GameViewSize");var e=assembly.GetType("UnityEditor.GameViewSizeType");
            var s=Activator.CreateInstance(t,new object[]{Enum.Parse(e,"FixedResolution"),width,height,"HUD verification"});
            gt.GetMethod("AddCustomSize").Invoke(group,new[]{s});index=count;
        }
        var vt=assembly.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(vt);
        vt.GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).SetValue(view,index);
        view.Repaint();return new{width,height,index};
    }
}
