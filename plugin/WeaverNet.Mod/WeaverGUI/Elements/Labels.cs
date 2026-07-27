using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI.Elements
{
    public static partial class PluginGUI
    {
        public static void Label(string text) => GUILayout.Label(text, WeaverNetStyles.StandardLabel);
        public static void Label(string text, GUIStyle style) => GUILayout.Label(text, style);
    }
}
