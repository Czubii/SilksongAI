using WeaverNet.Mod.WeaverGUI.Styles;
using UnityEngine;

namespace WeaverNet.Mod.WeaverGUI.Elements
{
    public static partial class PluginGUI
    {
        public static bool TopBar(string text)
        {
            bool pressed = false;
            GUILayout.BeginHorizontal(GUI.skin.label);

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 20;
            labelStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label(text, labelStyle, GUILayout.ExpandWidth(true));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("✕", PluginGUIStyles.CloseButton, GUILayout.Width(20), GUILayout.Height(20)))
            {
                pressed = true;
            }

            GUILayout.EndHorizontal();

            return pressed;
        }
    }
}
