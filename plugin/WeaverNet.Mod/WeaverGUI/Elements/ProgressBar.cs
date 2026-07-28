using WeaverNet.Mod.WeaverGUI.Styles;
using UnityEngine;
using System;

namespace WeaverNet.Mod.WeaverGUI.Elements
{
    public static partial class PluginGUI
    {
        public static void ProgressBar(float progress, string label = null, params GUILayoutOption[] options)
        {
            const int minFillWidth = 14;
            progress = Mathf.Clamp01(progress);

            Rect rect = GUILayoutUtility.GetRect(1, 25, options);

            GUI.Box(rect, GUIContent.none, WeaverNetStyles.ProgressBarBackground);

            Rect fill = new Rect(rect.x, rect.y, Math.Max(rect.width * progress, minFillWidth), rect.height);
            GUI.Box(fill, GUIContent.none, WeaverNetStyles.ProgressBarFill);

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
