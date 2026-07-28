using BepInEx;
using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI.Elements
{
    public class DropdownState<T>
        where T : class
    {
        public int SelectedIdx = -1;
        public T SelectedOption;
        public bool SelectionChanged;
        public bool Expanded;

        public Rect ButtonRect;
        public Rect ScreenButtonRect;
    }
    public static partial class PluginGUI
    {
        public static DropdownState<T> Dropdown<T>(
            IGUIContext context,
            DropdownState<T> state,
            IReadOnlyList<T> elements,
            Func<T, string> optionLabelSelector)
            where T : class
        {
            var oldSelection = state.SelectedIdx;
            state.SelectionChanged = false;

            var oldEnabled = GUI.enabled;

            if (elements.Count == 0)
            {
                state.SelectedIdx = -1;
                state.SelectedOption = null;
                state.Expanded = false;
            }
            else
            {
                state.SelectedIdx = Mathf.Clamp(
                    state.SelectedIdx,
                    0,
                    elements.Count - 1);
            }

            string buttonText;

            if (elements.Count > 0)
            {
                buttonText = optionLabelSelector(elements[state.SelectedIdx]);
            }
            else
            {
                buttonText = "No options available";
                GUI.enabled = false;
            }

            Rect buttonRect = GUILayoutUtility.GetRect(
                GUIContent.none,
                WeaverNetStyles.DropdownButton,
                GUILayout.Height(30),
                GUILayout.Width(180));

            state.ButtonRect = buttonRect;


            if (GUI.Button(
                buttonRect,
                GUIContent.none,
                WeaverNetStyles.DropdownButton))
            {
                state.Expanded = !state.Expanded;

                if (state.Expanded && elements.Count > 0)
                {
                    Vector2 screenPos = GUIUtility.GUIToScreenPoint(
                        buttonRect.position);

                    state.ScreenButtonRect = new Rect(
                        screenPos,
                        buttonRect.size);

                    context.ShowOverlay(
                        new DropdownOverlay<T>(
                            state,
                            elements,
                            optionLabelSelector));
                }
            }


            // Draw text
            Rect textRect = new Rect(
                buttonRect.x + 8,
                buttonRect.y,
                buttonRect.width - 28,
                buttonRect.height);

            GUI.Label(
                textRect,
                buttonText,
                WeaverNetStyles.DropdownButtonText);


            // Draw arrow
            Rect arrowRect = new Rect(
                buttonRect.xMax - 24,
                buttonRect.y,
                20,
                buttonRect.height);

            GUI.Label(
                arrowRect,
                state.Expanded ? "▲" : "▼",
                WeaverNetStyles.DropdownArrow);


            GUI.enabled = oldEnabled;


            if (elements.Count > 0)
                state.SelectedOption = elements[state.SelectedIdx];
            else
                state.SelectedOption = null;


            if (state.SelectedIdx != oldSelection)
                state.SelectionChanged = true;


            return state;
        }


    }
}
