using System.Collections.Generic;
using UnityEngine;

namespace WeaverNet.Mod.PluginGUI
{
    public abstract class BaseWindow
    {
        public string Name { get; }
        public bool Enabled { get; set; }

        private Rect _windowRect;

        private List<string> _errors = new List<string>();
        private Vector2 _errorLogScroll = new Vector2();

        public BaseWindow(string name, Rect windowRect)
        {
            _windowRect = windowRect;
            Name = name;
            Enabled = false;
        }

        public void MakeWindow(int ID)
        {
            if (Enabled)
            {
                if (!CanEnable())
                {
                    Enabled = false;
                    return;
                }
                _windowRect = GUILayout.Window(ID, _windowRect, DrawBase, GUIContent.none, PluginGUIStyles.Window);
            }
        }
        public void DrawBase(int ID)
        {
            if (PluginGUIElements.TopBar(Name)) Enabled = false;
            if (_errors.Count > 0) DrawError();
            else DrawContent();
            GUI.DragWindow();
        }
        public void NotifyError(string error)
        {
            _errors.Add(error);
        }
        private void DrawError()
        {
            _errorLogScroll = GUILayout.BeginScrollView(_errorLogScroll, PluginGUIStyles.ScrollView, PluginGUIStyles.VerticalScrollbar, GUILayout.ExpandHeight(true));
            GUI.skin.verticalScrollbarThumb = PluginGUIStyles.VerticalScrollbarThumb;

            GUILayout.Label("Exception Has Occured Somhere: ");
            GUILayout.Label(_errors[0]);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Okay", PluginGUIStyles.Button))
            {
                _errors.RemoveAt(0);
            }
            GUILayout.EndScrollView();
        }
        public abstract void DrawContent();
        public abstract bool CanEnable();
    }
}
