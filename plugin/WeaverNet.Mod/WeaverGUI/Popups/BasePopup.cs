using System;
using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI.Popups
{
    public abstract class BasePopup : IPopup
    {
        private PopupLayer _popupLayer { get; }
        public PopupLayer Layer => _popupLayer;

        private bool _isOpen = true;
        public bool IsAlive => _isOpen;

        private int? _id;
        public int ID
        {
            get
            {
                if (_id == null)
                    throw new InvalidOperationException("Window has not yet been assigned an ID");

                return _id.Value;
            }
        }

        private IGUIContext _context;
        protected IGUIContext Context
        {
            get
            {
                if (_context == null)
                    throw new InvalidOperationException("Window has not been initialized");

                return _context;
            }
        }
        public abstract bool AlwaysVisible { get; }
        private Rect _windowRect;

        /// <summary>
        /// Creates the popup at the center of the parent rect
        /// </summary>
        protected BasePopup(PopupLayer layer, Rect parentRect, Vector2 size)
        {
            _popupLayer = layer;

            Vector2 position = parentRect.center - size/2;
            _windowRect = new Rect(position, size);
        }
        /// <summary>
        /// Creates the popup at given screen positon
        /// </summary>
        protected BasePopup(PopupLayer layer, Vector2 position, Vector2 size)
        {
            _popupLayer = layer;
            _windowRect = new Rect(position, size);
        }
        public void Close()
        {
            _isOpen = false;
        }

        public void Initialize(IGUIContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (_context != null)
                throw new InvalidOperationException(
                    "Window has already been initialized");

            _context = context;
            _id = context.AllocateID();
        }

        public void Render()
        {
            if (!IsAlive) return;
            if (!_context.IsGUIVisible && !AlwaysVisible) return;

            if (_context.IsGUIVisible)
            {
                
                _windowRect = GUILayout.Window(
                    ID,
                    _windowRect,
                    DrawWindow,
                    GUIContent.none,
                    WeaverNetStyles.ActiveWindow);

            }
            else
            {
                GUILayout.Window(
                    ID,
                    _windowRect,
                    DrawWindow,
                    GUIContent.none,
                    WeaverNetStyles.TransparentWindow);
            }
        }
        private void DrawWindow(int id)
        {
            DrawContent();
            GUI.DragWindow();
        }
        protected abstract void DrawContent();

        public void BringToFront()
        {
            GUI.BringWindowToFront(ID);
        }
    }
}
