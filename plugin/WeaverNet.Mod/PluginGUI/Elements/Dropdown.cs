using BepInEx;
using System.Collections.Generic;
using UnityEngine;
using WeaverNet.Mod.PluginGUI.Styles;

namespace WeaverNet.Mod.PluginGUI.Elements
{
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
    public static partial class PluginGUIElements
    {
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
    }
}
