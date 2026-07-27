using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI
{
    public class GUIManager : MonoBehaviour, IGUIContext
    {
        private readonly List<IWindow> _windows = new List<IWindow>();
        private readonly List<IGUIOverlay> _overlays = new List<IGUIOverlay>();

        private int _buttonWidth = 150;

        private Rect _toolbarRect = new Rect(100, 0, 0, 0);

        private ConfigFile _config;
        private ConfigEntry<KeyboardShortcut> _enableKey;

        private bool _drawingEnabled;
        public bool IsGUIVisible => _drawingEnabled;

        private IWindow _activeWindow;
        public IWindow ActiveWindow => _activeWindow;

        private int _lastId = 0;
        public void Initialize(ConfigFile config)
        {
            _config = config;

            _enableKey = _config.Bind(
                "Key Binds",
                "Show/Hide Main Plugin Menu",
                new KeyboardShortcut(KeyCode.F2));
        }

        public void Register(IWindow window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            if (_windows.Contains(window))
                return;

            window.Initialize(this);

            _windows.Add(window);
        }
        public void HideGUI()
        {
            _drawingEnabled = false;
            CursorPatches.ForceFisible = false;
        }
        public void ShowGUI()
        {
            _drawingEnabled = true;
            CursorPatches.ForceFisible = true;
        }
        private void Update()
        {
            if (_enableKey != null && _enableKey.Value.IsDown())
            {
                if (_drawingEnabled)
                    HideGUI();
                else
                    ShowGUI();
            }
        }
        private void OnGUI()
        {
            if (!_drawingEnabled)
                return;
            DrawWindows();
            DrawOverlays();
        }
        private void DrawWindows()
        {
            _toolbarRect = GUILayout.Window(
                0,
                _toolbarRect,
                DrawToolbar,
                GUIContent.none,
                WeaverNetStyles.Window);

            foreach (var window in _windows)
            {
                window.Render();
            }
        }
        private void DrawToolbar(int id)
        {
            GUILayout.BeginHorizontal();

            foreach (var window in _windows)
            {
                if (!window.ShowInToolbar)
                    continue;

                GUI.enabled = window.CanEnable();

                window.Enabled =
                    GUILayout.Toggle(
                        window.Enabled,
                        window.Name,
                        WeaverNetStyles.ToggleButton,
                        GUILayout.Height(40),
                        GUILayout.Width(_buttonWidth));
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }
        public int AllocateID()
        {
            _lastId += 1;
            return _lastId;
        }

        public void SetActiveWindow(IWindow window)
        {
            _activeWindow = window;
        }
        public void ShowOverlay(IGUIOverlay overlay)
        {
            if (overlay == null)
                throw new ArgumentNullException(nameof(overlay));

            if (!_overlays.Contains(overlay))
                _overlays.Add(overlay);
        }
        private void DrawOverlays()
        {
            GUI.depth = -1000;

            for (int i = _overlays.Count - 1; i >= 0; i--)
            {
                var overlay = _overlays[i];

                if (!overlay.IsAlive)
                {
                    _overlays.RemoveAt(i);
                    continue;
                }

                overlay.Draw();
            }

            GUI.depth = 0;
        }
    }
}