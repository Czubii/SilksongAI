using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI.Elements
{
    public static partial class PluginGUI
    {
        public static bool Button(string text, params GUILayoutOption[] options)
        {
            return GUILayout.Button(text, WeaverNetStyles.Button, options);
        }
        public static bool Toggle(bool value, params GUILayoutOption[] options)
        {
            GUILayoutOption[] toggleOptions = new GUILayoutOption[options.Length + 2];

            for (int i = 0; i < options.Length; i++)
                toggleOptions[i] = options[i];

            toggleOptions[options.Length] = GUILayout.Width(64);
            toggleOptions[options.Length + 1] = GUILayout.Height(28);

            Rect rect = GUILayoutUtility.GetRect(
                64,
                28,
                toggleOptions);

            Event current = Event.current;

            if (current.type == EventType.MouseDown &&
                rect.Contains(current.mousePosition))
            {
                value = !value;
                GUI.changed = true;
                current.Use();
            }

            if (current.type == EventType.Repaint)
            {
                (value
                    ? WeaverNetStyles.ToggleBackgroundActive
                    : WeaverNetStyles.ToggleBackground)
                    .Draw(
                        rect,
                        GUIContent.none,
                        false,
                        false,
                        false,
                        false);

                float notchSize = rect.height - 8f;

                Rect notchRect = new Rect(
                    value
                        ? rect.xMax - notchSize - 4f
                        : rect.x + 4f,
                    rect.y + 4f,
                    notchSize,
                    notchSize);

                WeaverNetStyles.ToggleNotch.Draw(
                    notchRect,
                    GUIContent.none,
                    false,
                    false,
                    false,
                    false);
            }

            return value;
        }

    }
}
