using AIPlugin.Networking;
using AIPlugin.Networking.Requests;
using AIPlugin.PluginGUI.Windows;
using BepInEx;
using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using static AIPlugin.Networking.Requests.Requests.GetArtifacts.Response;
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
            labelStyle.fontSize = 20;
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

                    if(!label.IsNullOrWhiteSpace())
                    {
                        buttonText = $"{label}: {buttonText}";
                    }

                    // Button showing current selection
                    if (GUILayout.Button(buttonText, Styles.Button))
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
                    GUILayout.Button(buttonText, Styles.Button);
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

        public static void ArtifactSelectionCard(ArtifactSelection selection)
        {
            
            if(!selection?.AnySelected ?? true)
            {
                GUILayout.BeginHorizontal(Styles.CardOrangeHighlight);
                GUILayout.Label("No Artifact Selected. Make sure to choose one inside Artifact Settings window");
                GUILayout.EndHorizontal();
            }
            else
            {
                ArtifactCard(selection.Artifact);
            }
        }

        public static void ArtifactCard(Artifact artifact)
        {
            GUILayout.BeginVertical(Styles.CardGreenHighlight);

            GUILayout.Label(artifact.Name, Styles.HeaderLabel);
            GUILayout.Label($"Boss: {artifact.BossName} \n " +
                            $"Architecture: {artifact.ArchitectureName}");

            GUILayout.EndVertical();
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

        public static void ProgressBar(float progress, string label = null, params GUILayoutOption[] options)
        {
            progress = Mathf.Clamp01(progress);

            Rect rect = GUILayoutUtility.GetRect(1, 25, options);

            GUI.Box(rect, GUIContent.none, Styles.ProgressBarBackground);

            Rect fill = new Rect(rect.x, rect.y, rect.width * progress, rect.height);
            GUI.Box(fill, GUIContent.none, Styles.ProgressBarFill);

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

