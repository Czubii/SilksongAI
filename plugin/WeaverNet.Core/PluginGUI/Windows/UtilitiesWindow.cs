using UnityEngine;

namespace WeaverNet.Core.PluginGUI.Windows
{
    public class UtilitiesWindow : BaseWindow
    {

        public UtilitiesWindow(string name) : base(name, new Rect(0, 0, 300, 0))
        {}
        public override bool CanEnable() => true;
        public override void DrawContent()
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Teleport", PluginGUIStyles.Button))
            {
                PluginLog.Info("Teleporting!");
            }

            GUILayout.EndVertical();
            GUI.enabled = true;
        }

    }
}
