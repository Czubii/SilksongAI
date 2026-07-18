using WeaverNet.Mod.WeaverGUI.Styles;
using UnityEngine;

namespace WeaverNet.Mod.WeaverGUI.Elements
{
    public static partial class PluginGUI
    {
        public static void ProgressBar(float progress, string label = null, params GUILayoutOption[] options)
        {
            progress = Mathf.Clamp01(progress);

            Rect rect = GUILayoutUtility.GetRect(1, 25, options);

            GUI.Box(rect, GUIContent.none, PluginGUIStyles.ProgressBarBackground);

            Rect fill = new Rect(rect.x, rect.y, rect.width * progress, rect.height);
            GUI.Box(fill, GUIContent.none, PluginGUIStyles.ProgressBarFill);

            if (!string.IsNullOrEmpty(label))
            {
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.alignment = TextAnchor.MiddleCenter;
                labelStyle.fontStyle = FontStyle.Bold;

                GUI.Label(rect, label, labelStyle);
            }
        }
    }
}
