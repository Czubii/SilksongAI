using WeaverNet.Mod.PluginGUI.Styles;
using UnityEngine;

namespace WeaverNet.Mod.PluginGUI.Elements
{
    public static partial class PluginGUIElements
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
