using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WeaverNet.Core.PluginGUI
{
    public static class PluginGUIElements
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

        public class DropdownState<T>
            where T : class
        {
            public int SelectedIdx = -1;
            public T SelectedOption;
            public bool SelectionChanged = false;
            public bool Expanded = false;

            public void Reset()
            {
                SelectedIdx = -1;
                SelectedOption = null;
                Expanded = false;
                SelectionChanged = true;
            }
        }
        public static DropdownState<T> Dropdown<T>(DropdownState<T> state, List<(string label, T value)> elements, string label = "")
            where T : class
        {
            var oldSelection = state.SelectedIdx;
            state.SelectionChanged = false;

            var oldEnabled = GUI.enabled;

            state.SelectedIdx = elements.Count <= 0 ? 0 : Mathf.Clamp(state.SelectedIdx, 0, elements.Count - 1);

            if ((state.Expanded && !GUI.enabled) || elements.Count == 0)
                state.Expanded = false;

            if (!state.Expanded)
            {
                if (elements.Count > 0)
                {
                    string buttonText = elements[state.SelectedIdx].label;

                    if (!label.IsNullOrWhiteSpace())
                    {
                        buttonText = $"{label}: {buttonText}";
                    }

                    // Button showing current selection
                    if (GUILayout.Button(buttonText, PluginGUIStyles.Button))
                    {
                        state.Expanded = !state.Expanded;
                    }
                }
                else
                {
                    string buttonText = "No options available";

                    if (!label.IsNullOrWhiteSpace())
                    {
                        buttonText = $"{label}: {buttonText}";
                    }

                    GUI.enabled = false;
                    GUILayout.Button(buttonText, PluginGUIStyles.Button);
                }
            }
            else
            {
                GUILayout.BeginVertical("box");

                for (int i = 0; i < elements.Count; i++)
                {
                    // Draw a highlight box for the current selection
                    if (i == state.SelectedIdx)
                    {
                        if (GUILayout.Button(elements[i].label, PluginGUIStyles.GreenButton))
                        {
                            state.SelectedIdx = i;
                            state.Expanded = false;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(elements[i].label, PluginGUIStyles.Button))
                        {
                            state.SelectedIdx = i;
                            state.Expanded = false;
                        }
                    }
                }

                GUILayout.EndVertical();
            }

            GUI.enabled = oldEnabled;

            if (elements.Count > 0)
            {
                state.SelectedOption = elements[state.SelectedIdx].value;
            }
            else
            {
                state.SelectedOption = null;
            }

            if (state.SelectedIdx != oldSelection) state.SelectionChanged = true;
            return state;
        }
        public static bool LabeledToggle(bool value, string text, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(text, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            value = GUILayout.Toggle(value, "", PluginGUIStyles.Toggle, options);
            GUILayout.EndHorizontal();
            return value;
        }
        public static int IntegerField(int value, params GUILayoutOption[] options)
        {
            string text = GUILayout.TextField(value.ToString(), PluginGUIStyles.TextField, options);

            if (int.TryParse(text, out int parsed))
                return parsed;

            return value; // keep previous value if invalid
        }
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
