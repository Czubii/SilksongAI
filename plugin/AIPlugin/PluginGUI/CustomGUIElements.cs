using AIPlugin.Networking;
using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static GamepadVibrationMixer.GamepadVibrationEmission;

namespace AIPlugin.PluginGUI
{
    public static class CustomGUI
    {
        public static bool TopBar(string text)
        {
            bool pressed = false;
            GUILayout.BeginHorizontal(GUI.skin.label);

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 15;
            labelStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label(text, labelStyle, GUILayout.ExpandWidth(true));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("✕", Styles.CloseButton, GUILayout.Width(20), GUILayout.Height(20)))
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
        }
        public static DropdownState<T> Dropdown<T>(DropdownState<T> state, List<(string label, T value)> elements)
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
                    // Button showing current selection
                    if (GUILayout.Button(elements[state.SelectedIdx].label, Styles.Button))
                    {
                        state.Expanded = !state.Expanded;
                    }
                }
                else
                {
                    GUI.enabled = false;
                    GUILayout.Button("No options available", Styles.Button);
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
                        if (GUILayout.Button(elements[i].label, Styles.GreenButton))
                        {
                            state.SelectedIdx = i;
                            state.Expanded = false;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(elements[i].label, Styles.Button))
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
            value = GUILayout.Toggle(value, "", Styles.Toggle, options);
            GUILayout.EndHorizontal();
            return value;
        }
        public static int IntegerField(int value, params GUILayoutOption[] options)
        {
            string text = GUILayout.TextField(value.ToString(), Styles.TextField, options);

            if (int.TryParse(text, out int parsed))
                return parsed;

            return value; // keep previous value if invalid
        }

        public static ServerParam ParamField(ServerParam param,
            params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(param.VariableName, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();

            switch (param.Type)
            {
                case "int":
                    if(int.TryParse(param.Value, out int parsed))
                        param.Value = IntegerField(parsed, options).ToString();
                    else
                        param.Value = IntegerField(0, options).ToString();
                    break;

                default:
                    param.Value = GUILayout.TextField(param.Value, Styles.TextField, options);
                    break;
            }

            GUILayout.EndHorizontal();
            return param;
        }
    }
}

