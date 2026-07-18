using WeaverNet.Mod.WeaverGUI.Styles;
using UnityEngine;

namespace WeaverNet.Mod.WeaverGUI.Elements
{
    public static partial class PluginGUI
    {
        public static bool LabeledToggle(bool value, string text, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(text, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            value = GUILayout.Toggle(value, "", PluginGUIStyles.Toggle, options);
            GUILayout.EndHorizontal();
            return value;
        }
    }
}
