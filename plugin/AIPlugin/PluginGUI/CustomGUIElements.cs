using AIPlugin.Networking;
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
        public class DropdownState
        {
            public int SelectedIdx = 0;
            public bool Expanded = false;
            public Vector2 Scroll;
        }
        public static DropdownState Dropdown(DropdownState state, List<string> options)
        {
            var oldEnabled = GUI.enabled;

            state.SelectedIdx = options.Count <= 0 ? 0 : Mathf.Clamp(state.SelectedIdx, 0, options.Count - 1);

            if ((state.Expanded && !GUI.enabled) || options.Count == 0)
                state.Expanded = false;

            if (!state.Expanded)
            {
                if (options.Count > 0)
                {
                    // Button showing current selection
                    if (GUILayout.Button(options[state.SelectedIdx]))
                    {
                        state.Expanded = !state.Expanded;
                    }
                }
                else
                {
                    GUI.enabled = false;
                    GUILayout.Button("No options available");
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
                        var rect = GUILayoutUtility.GetRect(new GUIContent(options[i]), GUI.skin.button);
                        GUI.Box(rect, "", GUI.skin.box); // TODO MAKE THIS MORE VISIBLE Draw an empty box behind the button
                        if (GUI.Button(rect, options[i]))
                        {
                            state.SelectedIdx = i;
                            state.Expanded = false;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(options[i]))
                        {
                            state.SelectedIdx = i;
                            state.Expanded = false;
                        }
                    }
                }

                GUILayout.EndVertical();
            }

            GUI.enabled = oldEnabled;

            return state;
        }

        public static bool LabelToggle(bool value, string text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(text, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            value = GUILayout.Toggle(value, "");
            GUILayout.EndHorizontal();
            return value;
        }
        private static string RemoveNonNumberChar(string input)
        {
            return new string(input.Where(c => { return char.IsDigit(c); }).ToArray());
        }

        public static int IntegerField(int value, GUILayoutOption[] options) //TODO add min and max
        {
            string val_str = "";
            if (value != 0)
                val_str = value.ToString();

            val_str = GUILayout.TextField(val_str, options);
            if (val_str.Length == 0) return 0;

            bool negative = val_str[0] == '-';
            val_str = RemoveNonNumberChar(val_str);

            if (val_str.Length > 0)
                value = int.Parse(val_str, System.Globalization.NumberStyles.Integer);
            else value = 0;

            if (negative) value = -value;//TODO fix this if needed ever

            //value = Math.Min(max, Math.Max(min, value)); TODO

            return value;
        }

        public static Payloads.FunctionParam ServerFunctionParamField(Payloads.FunctionParam param, GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(param.VariableName, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();

            switch (param.Type)
            {
                case "int":
                    param.Value = GUILayout.TextField(param.Value, options);
                    param.Value = RemoveNonNumberChar(param.Value);
                    break;

                default:
                    param.Value = GUILayout.TextField(param.Value, options);
                    break;
            }

            GUILayout.EndHorizontal();
            return param;
        }
    }
}

