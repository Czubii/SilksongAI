using WeaverNet.Mod.PluginGUI.Styles;
using UnityEngine;

namespace WeaverNet.Mod.PluginGUI.Elements
{
    public static partial class PluginGUIElements
    {
        public static int IntegerField(int value, params GUILayoutOption[] options)
        {
            string text = GUILayout.TextField(value.ToString(), PluginGUIStyles.TextField, options);

            if (int.TryParse(text, out int parsed))
                return parsed;

            return value; // keep previous value if invalid
        }
    }
}
