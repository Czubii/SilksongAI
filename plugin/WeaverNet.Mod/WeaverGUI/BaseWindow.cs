using System;
using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Elements;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI
{
    public abstract class BaseWindow : IWindow
    {
        private readonly string _name;
        public string Name => _name;

        private readonly bool _showInToolbar;
        public bool ShowInToolbar => _showInToolbar;

        public bool IsOpen { get; set; }

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

        protected Rect WindowRect;

        public bool IsActive => Context.ActiveWindow == this;


        // Resize configuration
        protected virtual float ResizeHandleSize => 30f;

        public float MinWidth { get; set; } = 400f;
        public float MinHeight { get; set; } = 150f;


        // Resize state
        private bool _isResizing;
        private Vector2 _resizeStartMousePos;
        private Vector2 _resizeStartWindowSize;
        private int _resizeControlID;

        protected BaseWindow(
            string name,
            Rect initialRect,
            bool showInToolbar = true)
        {
            _name = name;
            _showInToolbar = showInToolbar;
            WindowRect = initialRect;

            WindowRect.width = Mathf.Max(WindowRect.width, MinWidth);
            WindowRect.height = Mathf.Max(WindowRect.height, MinHeight);
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
            if (!IsOpen)
                return;

            if (!CanEnable())
            {
                IsOpen = false;
                return;
            }

            WindowRect = GUILayout.Window(
                ID,
                WindowRect,
                DrawWindow,
                GUIContent.none,
                IsActive
                    ? WeaverNetStyles.ActiveWindow
                    : WeaverNetStyles.Window);
        }


        private void DrawWindow(int id)
        {
            if (Event.current.type == EventType.MouseDown)
            {
                Context.SetActiveWindow(this);
            }

            bool locked = Context.IsLockedByPopup(this);

            if (locked)
            {
                BlockInput();
            }

            DrawHeader();
            DrawContent();

            if (locked)
            {
                DrawLockedOverlay();
                return;
            }

            HandleResize();
            GUI.DragWindow();
        }
        private void BlockInput()
        {
            Event current = Event.current;

            if (current.type == EventType.MouseDown ||
                current.type == EventType.MouseUp ||
                current.type == EventType.MouseDrag ||
                current.type == EventType.MouseMove)
            {
                current.Use();
            }
        }
        private void DrawLockedOverlay()
        {
            Event current = Event.current;

            Rect rect = new Rect(
                0,
                0,
                WindowRect.width,
                WindowRect.height);

            // Block all mouse interaction inside this window
            if (current.type == EventType.MouseDown ||
                current.type == EventType.MouseUp ||
                current.type == EventType.MouseDrag ||
                current.type == EventType.MouseMove)
            {
                if (rect.Contains(current.mousePosition))
                    current.Use();
            }


            if (current.type != EventType.Repaint)
                return;


            Color previous = GUI.color;

            GUI.color = new Color(0, 0, 0, 0.25f);

            GUI.DrawTexture(
                rect,
                Texture2D.whiteTexture);

            GUI.color = previous;


            // Optional border indicator
            WeaverNetStyles.PopupLock.Draw(
                rect,
                GUIContent.none,
                false,
                false,
                false,
                false);
        }
        protected virtual void DrawHeader()
        {
            if (PluginGUI.TopBar(Name))
                IsOpen = false;
        }


        private void HandleResize()
        {
            Rect resizeRect = new Rect(
                WindowRect.width - ResizeHandleSize,
                WindowRect.height - ResizeHandleSize,
                ResizeHandleSize,
                ResizeHandleSize);


            _resizeControlID = GUIUtility.GetControlID(FocusType.Passive);

            Event current = Event.current;

            bool hovering =
                resizeRect.Contains(current.mousePosition)
                || _isResizing;


            DrawResizeHandle(hovering);


            switch (current.GetTypeForControl(_resizeControlID))
            {
                case EventType.MouseDown:

                    if (current.button == 0 &&
                        resizeRect.Contains(current.mousePosition))
                    {
                        _isResizing = true;

                        _resizeStartMousePos =
                            GUIUtility.GUIToScreenPoint(current.mousePosition);

                        _resizeStartWindowSize =
                            new Vector2(
                                WindowRect.width,
                                WindowRect.height);

                        GUIUtility.hotControl = _resizeControlID;

                        current.Use();
                    }

                    break;


                case EventType.MouseDrag:

                    if (_isResizing &&
                        GUIUtility.hotControl == _resizeControlID)
                    {
                        Vector2 mouse =
                            GUIUtility.GUIToScreenPoint(current.mousePosition);

                        Vector2 delta =
                            mouse - _resizeStartMousePos;

                        WindowRect.width =Mathf.Max(MinWidth,_resizeStartWindowSize.x + delta.x);
                        WindowRect.height =Mathf.Max(MinHeight, _resizeStartWindowSize.y + delta.y);
                        current.Use();
                    }

                    break;

                case EventType.MouseUp:

                    if (_isResizing &&
                        current.button == 0)
                    {
                        _isResizing = false;
                        GUIUtility.hotControl = 0;
                        current.Use();
                    }

                    break;
            }
        }


        private void DrawResizeHandle(bool hovered)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Rect drawRect = new Rect(
                WindowRect.width - ResizeHandleSize + 2,
                WindowRect.height - ResizeHandleSize + 12,
                ResizeHandleSize,
                ResizeHandleSize);

            (hovered? WeaverNetStyles.ResizeHandleHover : WeaverNetStyles.ResizeHandle).Draw(drawRect,"◢",false,false,false,false);
        }

        protected abstract void DrawContent();

        public abstract bool CanEnable();
    }
}