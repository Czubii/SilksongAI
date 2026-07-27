using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private readonly Rect _parentRect;
        private readonly Rect _windowRect;
        protected BasePopup(PopupLayer layer, Rect parentRect, Vector2 size)
        {
            _popupLayer = layer;
            _parentRect = parentRect;

            Vector2 position = parentRect.center - size/2;
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
            if (!IsAlive)
                return;

            GUILayout.Window(
                ID,
                _windowRect,
                DrawWindow,
                GUIContent.none,
                WeaverNetStyles.ActiveWindow);
        }
        protected abstract void DrawWindow(int id);

        public void BringToFront()
        {
            GUI.BringWindowToFront(ID);
        }
    }
}
