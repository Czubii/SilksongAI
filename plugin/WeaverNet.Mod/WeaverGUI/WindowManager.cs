using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI
{

    public abstract class BaseScreenLabel : MonoBehaviour
    {
        protected int _lineOffsetY = 20;
        public abstract bool EnabledInConfig();
    }

    public class PluginWindowManager : MonoBehaviour
    {
        private Dictionary<BaseWindow, bool> _windows = new Dictionary<BaseWindow, bool>();
        private List<BaseScreenLabel> _labels = new List<BaseScreenLabel>();

        private int _buttonWidth = 150;
        private Rect _windowRect = new Rect(100, 0, 0, 0);

        private ConfigFile _config;
        private ConfigEntry<KeyboardShortcut> _enableKey;
        private bool _drawingEabled = false;

        public void Initialize(ConfigFile config)
        {
            _config = config;
            _enableKey = _config.Bind("Key Binds", "Show/Hide Main Plugin Menu", new KeyboardShortcut(KeyCode.F2));
        }

        public void Register(BaseScreenLabel screenLabel)
        {
            if (screenLabel == null)
                throw new ArgumentNullException("screenLabel parameter cannot be null");

            if (!_labels.Contains(screenLabel))
                _labels.Add(screenLabel);
        }
        public void Register(BaseWindow window, bool addButton)
        {
            if (window == null)
                throw new ArgumentNullException("Widnow parameter cannot be null");

            if (!_windows.Keys.Contains(window))
                _windows.Add(window, addButton);

            UpdateRect();
        }

        private void UpdateRect()
        {
            _windowRect.x = (Screen.width - _windowRect.width) / 2;
        }
        void Update()
        {
            foreach (var label in _labels)
            {
                if(label.EnabledInConfig() != label.enabled)
                    label.enabled = !label.enabled;
            }

            if (_enableKey != null && _enableKey.Value.IsDown())
            {
                _drawingEabled = !_drawingEabled;
                CursorPatcher.ForceFisible = _drawingEabled;
            }
        }
        void OnGUI()
        {
            if (!_drawingEabled) return;
            UpdateRect();

            _windowRect = GUILayout.Window(0, _windowRect, Draw, GUIContent.none, PluginGUIStyles.Window);
            for(int i = 0; i<_windows.Count; i++)
            {
                _windows.Keys.ToArray()[i].MakeWindow(i+1);
            }
        }
        void Draw(int windowID)
        {
            GUILayout.BeginHorizontal();
            foreach (var kvp in _windows)
            {
                if(!kvp.Value) continue;

                GUI.enabled = kvp.Key.CanEnable();
                kvp.Key.Enabled = 
                    GUILayout.Toggle(kvp.Key.Enabled, kvp.Key.Name, 
                    PluginGUIStyles.ToggleButton,
                    GUILayout.Height(40), 
                    GUILayout.Width(_buttonWidth));
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
    }
}
