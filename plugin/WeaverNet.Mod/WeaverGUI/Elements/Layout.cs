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
                WeaverNetStyles.ScrollView, 
                WeaverNetStyles.VerticalScrollbar,
                GUILayout.ExpandHeight(false));

            GUI.skin.verticalScrollbarThumb = WeaverNetStyles.VerticalScrollbarThumb;
            return scroll;
        }
        public static void EndScrollView()
        {
            GUILayout.EndScrollView();
        }
        public static void BeginHorizontal() => GUILayout.BeginHorizontal();
        public static void EndHorizontal() => GUILayout.EndHorizontal();
        public static void BeginVertical() => GUILayout.BeginVertical();
        public static void EndVertical() => GUILayout.EndVertical();
    }
}
