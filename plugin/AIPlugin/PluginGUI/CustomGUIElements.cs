using AIPlugin.Networking;
using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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

        public class DropdownState
        {
            public int SelectedIdx = 0;
            public bool SelectionChanged = false;  
            public bool Expanded = false;
            public Vector2 Scroll;
        }
        public static DropdownState Dropdown(DropdownState state, List<string> options)
        {
            var oldSelection = state.SelectedIdx;
            state.SelectionChanged = false;

            var oldEnabled = GUI.enabled;

            state.SelectedIdx = options.Count <= 0 ? 0 : Mathf.Clamp(state.SelectedIdx, 0, options.Count - 1);

            if ((state.Expanded && !GUI.enabled) || options.Count == 0)
                state.Expanded = false;

            if (!state.Expanded)
            {
                if (options.Count > 0)
                {
                    // Button showing current selection
                    if (GUILayout.Button(options[state.SelectedIdx], Styles.Button))
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

                for (int i = 0; i < options.Count; i++)
                {
                    // Draw a highlight box for the current selection
                    if (i == state.SelectedIdx)
                    {
                        if (GUILayout.Button(options[i], Styles.GreenButton))
                        {
                            state.SelectedIdx = i;
                            state.Expanded = false;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(options[i], Styles.Button))
                        {
                            state.SelectedIdx = i;
                            state.Expanded = false;
                        }
                    }
                }

                GUILayout.EndVertical();
            }

            GUI.enabled = oldEnabled;

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

        public static ArchitectureConstructorParams ArchitectureConstructorParamField(ArchitectureConstructorParams param,
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

