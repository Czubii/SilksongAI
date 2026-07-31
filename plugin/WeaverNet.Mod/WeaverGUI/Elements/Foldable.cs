using System;
using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI.Elements
{
    public class FoldableState
    {
        public Vector2 Scroll = new Vector2();
        public bool Expanded = false;
    }

    public static partial class PluginGUI
    {
        public static FoldableState Foldable(FoldableState state, string label, Action drawAction, float? maxHeight = null)
        {
            if (state == null) state = new FoldableState();

            // 1. Begin the main Card container for BOTH header and body
            GUILayout.BeginVertical(WeaverNetStyles.Card);

            // 2. Allocate rect spanning FULL width (ExpandWidth(true))
            Rect buttonRect = GUILayoutUtility.GetRect(
                GUIContent.none,
                WeaverNetStyles.FoldableButton,
                GUILayout.Height(30),
                GUILayout.ExpandWidth(true));

            if (GUI.Button(buttonRect, GUIContent.none, WeaverNetStyles.FoldableButton))
            {
                state.Expanded = !state.Expanded;
            }

            // Draw text over header button
            Rect textRect = new Rect(
                buttonRect.x + 8,
                buttonRect.y,
                buttonRect.width - 28,
                buttonRect.height);

            GUI.Label(textRect, label, WeaverNetStyles.ElementLabelTitle);

            // Draw arrow over header button
            Rect arrowRect = new Rect(
                buttonRect.xMax - 24,
                buttonRect.y,
                20,
                buttonRect.height);

            GUI.Label(arrowRect, state.Expanded ? "▲" : "▼", WeaverNetStyles.DropdownArrow);

            // 3. Draw content inside the same Card box if expanded
            if (state.Expanded)
            {
                PluginGUI.HorizontalLine();

                if (maxHeight.HasValue)
                {
                    // Restrict max height specifically for scrollable content
                    GUILayout.BeginVertical(GUILayout.MaxHeight(maxHeight.Value));
                    state.Scroll = PluginGUI.BeginScrollView(state.Scroll);

                    drawAction?.Invoke();

                    PluginGUI.EndScrollView();
                    GUILayout.EndVertical();
                }
                else
                {
                    drawAction?.Invoke();
                }
            }

            // Close the outer Card container
            GUILayout.EndVertical();

            return state;
        }
    }
}