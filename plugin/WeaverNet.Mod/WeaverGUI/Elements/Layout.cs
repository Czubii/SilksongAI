using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Styles;
using Vector2 = UnityEngine.Vector2;

namespace WeaverNet.Mod.WeaverGUI.Elements
{
    public static partial class PluginGUI
    {
        public static Vector2 BeginScrollView(Vector2 scroll)
        {
            scroll = GUILayout.BeginScrollView(scroll, 
                PluginGUIStyles.ScrollView, 
                PluginGUIStyles.VerticalScrollbar, 
                GUILayout.ExpandHeight(true));

            GUI.skin.verticalScrollbarThumb = PluginGUIStyles.VerticalScrollbarThumb;
            return scroll;
        }
        public static void EndScrollView()
        {
            GUILayout.EndScrollView();
        }
        public static void BeginHorizontal() => GUILayout.BeginHorizontal();
        public static void EndHorizontal() => GUILayout.EndHorizontal();
    }
}
