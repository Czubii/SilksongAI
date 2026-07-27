using WeaverNet.Mod.WeaverGUI.Styles;
using UnityEngine;

namespace WeaverNet.Mod.WeaverGUI.Elements
{
    public static partial class PluginGUI
    {
        public static bool TopBar(string text)
        {
            bool pressed = false;

            GUILayout.BeginHorizontal(WeaverNetStyles.TopBar);

            GUILayout.Label(text, WeaverNetStyles.WindowTitleLabel, GUILayout.ExpandWidth(true));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("✕", WeaverNetStyles.CancelButton, GUILayout.Width(22), GUILayout.Height(22)))
            {
                pressed = true;
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            return pressed;
        }
    }
}
