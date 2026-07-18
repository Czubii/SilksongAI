using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI.Elements
{
    public static partial class PluginGUI
    {
        public static bool Button(string text, params GUILayoutOption[] options)
        {
            return GUILayout.Button(text, PluginGUIStyles.Button, options);
        }

    }
}
