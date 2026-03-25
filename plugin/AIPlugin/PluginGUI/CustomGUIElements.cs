using AIPlugin.Networking;
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

        public class ArtifactSelectionDropdowns
        {
            public class ArtifactOption
            {
                public string BossName;
                public string ArtifactName;

                public ArtifactOption(string boss, string artifact)
                {
                    BossName = boss;
                    ArtifactName = artifact;
                }
            }

            private List<(string label, string value)> _bossDropdownElements = 
                new List<(string label, string value)>();

            private List<(string label, ArtifactOption)> _artifactDropdownElements = 
                new List<(string label, ArtifactOption)>();

            private DropdownState<string> _bossDropdownState = 
                new DropdownState<string>();

            private DropdownState<ArtifactOption> _artifactDropdownState = 
                new DropdownState<ArtifactOption>();

            private Dictionary<string, List<string>> _artifacts = 
                new Dictionary<string, List<string>>();

            public (string BossName, string ArtifactName) SelectedOption => 
                (_artifactDropdownState?.SelectedOption?.BossName ?? null,
                _artifactDropdownState?.SelectedOption?.ArtifactName ?? null);

            public bool SelectionValid()
            {
                return SelectedOption.BossName != null && SelectedOption.ArtifactName != null;
            }
            public void Draw()
            {
                GUILayout.BeginVertical();
                _bossDropdownState = 
                    Dropdown(_bossDropdownState, _bossDropdownElements, "Boss");

                if (_bossDropdownState.SelectionChanged)
                {
                    BuildArtifactDropdownElements();
                }

                _artifactDropdownState = 
                    Dropdown(_artifactDropdownState, _artifactDropdownElements, "Artifact");

                GUILayout.EndVertical();
            }
            public void UpdateElements(Dictionary<string, List<string>> artifacts)
            {
                _artifacts = artifacts;
                BuildBossDropdownElements();
                BuildArtifactDropdownElements();
            }
            private void BuildBossDropdownElements()
            {
                if (_artifacts.Count == 0)
                {
                    _bossDropdownElements = new List<(string label, string value)>();
                    return;
                }

                _bossDropdownElements = new List<(string label, string value)>() { ("Any", "") };

                foreach (var pair in _artifacts)
                {
                    var internalBossName = pair.Key;

                    string displayName =
                        BossReferenceDatabase.All
                            .First(x => x.InternalName == internalBossName)
                            .DisplayName;

                    if (displayName == null)
                    {
                        AIPlugin.Log.LogError($"Unknown boss: {pair.Key}");
                        continue;
                    }

                    _bossDropdownElements.Add((displayName, internalBossName));
                }

            }
            private void BuildArtifactDropdownElements()
            {
                if (_artifacts.Count == 0)
                {
                    _artifactDropdownElements = new List<(string label, ArtifactOption)>();
                    return;
                }

                if (_bossDropdownState.SelectedOption == "") //Any boss
                {
                    _artifactDropdownElements = new List<(string label, ArtifactOption)>();
                    foreach (var pair in _artifacts)
                    {
                        var internalBossName = pair.Key;
                        var artifactNames = pair.Value;

                        string displayName = BossReferenceDatabase.All
                                                    .First(x => x.InternalName == internalBossName)
                                                    .DisplayName;

                        if (displayName == null)
                        {
                            AIPlugin.Log.LogError($"Unknown boss: {pair.Key}");
                            continue;
                        }

                        foreach (var artifactName in artifactNames)
                        {
                            _artifactDropdownElements.Add(($"{displayName}:    {artifactName}",
                                new ArtifactOption(internalBossName, artifactName)));
                        }
                    }
                }
                else
                {
                    var bossName = _bossDropdownState.SelectedOption;
                    var anyArtifacts = _artifacts.TryGetValue(bossName, out var artifactNames);

                    if (anyArtifacts && artifactNames.Count > 0)
                    {
                        _artifactDropdownElements = artifactNames.Select(a => (a, new ArtifactOption(bossName, a))).ToList();
                    }
                    else
                    {
                        _artifactDropdownElements = new List<(string label, ArtifactOption)>(); // Empty
                    }
                }
            }
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

