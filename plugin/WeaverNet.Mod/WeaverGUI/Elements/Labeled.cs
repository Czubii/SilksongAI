using WeaverNet.Mod.WeaverGUI.Styles;
using UnityEngine;
using System;

namespace WeaverNet.Mod.WeaverGUI.Elements
{
    public static partial class PluginGUI
    {
        public static TResult Labeled<TResult>(
             string label,
             Func<TResult> elementHandler,
             string description = null)
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();

            
            GUILayout.BeginVertical();  // --- Left Side ---
            GUILayout.Label(
                label,
                WeaverNetStyles.ElementLabelTitle);
            if (description != null)
            {
                GUILayout.Label(
                    description,
                    WeaverNetStyles.ElementLabelDescription);
            }
            GUILayout.EndVertical();    // --- --------- ---

            GUILayout.FlexibleSpace();  // push the handler to the right side of the view

            TResult result = elementHandler();


            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            return result;
        }
    }
}
