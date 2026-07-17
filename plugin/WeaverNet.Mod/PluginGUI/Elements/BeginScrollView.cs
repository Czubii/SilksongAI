using UnityEngine;
using WeaverNet.Mod.PluginGUI.Styles;
using Vector2 = UnityEngine.Vector2;

namespace WeaverNet.Mod.PluginGUI.Elements
{
    public static partial class PluginGUIElements
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

    }
}
