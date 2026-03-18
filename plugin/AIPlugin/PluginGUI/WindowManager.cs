using BepInEx.Configuration;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using InControl;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AIPlugin.PluginGUI
{

    public abstract class BaseScreenLabel : MonoBehaviour
    {
        public abstract bool EnabledInConfig();
    }
    public abstract class BaseWindow
    {
        public string Name { get; }
        public bool Enabled { get; set; }

        private Rect _windowRect;

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
                _windowRect = GUILayout.Window(ID, _windowRect, DrawBase, GUIContent.none, Styles.Window);
            }
        }
        public void DrawBase(int ID)
        {
            if (CustomGUI.TopBar(Name)) Enabled = false;
            DrawContent();
            GUI.DragWindow();
        }
        public abstract void DrawContent();
        public abstract bool CanEnable();
    }

    public class PluginWindowManager : MonoBehaviour
    {
        private List<BaseWindow> _windows = new List<BaseWindow>();
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
        public void Register(BaseWindow window)
        {
            if (window == null)
                throw new ArgumentNullException("Widnow parameter cannot be null");

            if (!_windows.Contains(window))
                _windows.Add(window);

            UpdateRect();
        }

        private void UpdateRect()
        {
            _windowRect.x = (Screen.width - _windowRect.width) / 2;
        }
        void Update()
        {
            UpdateRect();
            foreach (var label in _labels)
            {
                if(label.EnabledInConfig() != label.enabled)
                    label.enabled = !label.enabled;
            }

            if (_enableKey != null && _enableKey.Value.IsDown())
            {
                _drawingEabled = !_drawingEabled;
                CursorManager.ForceFisible = _drawingEabled;
            }
        }
        void OnGUI()
        {
            if (!_drawingEabled) return;

            _windowRect = GUILayout.Window(0, _windowRect, Draw, GUIContent.none, Styles.Window);
            for(int i = 0; i<_windows.Count; i++)
            {
                _windows[i].MakeWindow(i+1);
            }
        }
        void Draw(int windowID)
        {
            GUILayout.BeginHorizontal();
            foreach (var window in _windows)
            {
                GUI.enabled = window.CanEnable();
                window.Enabled = GUILayout.Toggle(window.Enabled, window.Name, Styles.ToggleButton, GUILayout.Height(40), GUILayout.Width(_buttonWidth));
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
    }
}
