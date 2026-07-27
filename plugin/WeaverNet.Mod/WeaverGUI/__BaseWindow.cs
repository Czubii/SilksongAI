using System.Collections.Generic;
using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Elements;
using WeaverNet.Mod.WeaverGUI.Styles;

public abstract class __BaseWindow
{
    public string Name { get; }
    public bool Enabled { get; set; }

    protected Rect _windowRect;

    public float MinWidth { get; set; } = 200f;
    public float MinHeight { get; set; } = 150f;

    private List<string> _errors = new List<string>();
    private Vector2 _errorLogScroll = new Vector2();

    private List<string> _notifications = new List<string>();
    private Vector2 _notificationLogScroll = new Vector2();

    private Texture2D _resizeCursorTexture;

    // State tracking for reliable resizing
    private bool _isResizing = false;
    private Vector2 _resizeStartMousePos;
    private Vector2 _resizeStartWindowSize;
    private int _resizeControlID;

    protected __BaseWindow(string name, Rect windowRect)
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
            _windowRect = GUILayout.Window(ID, _windowRect, DrawBase, GUIContent.none, WeaverNetStyles.Window);
        }
    }

    public void DrawBase(int ID)
    {
        if (PluginGUI.TopBar(Name)) Enabled = false;

        if (_errors.Count > 0) DrawError();
        else if(_notifications.Count > 0) DrawNotification();
        else DrawContent();

        // Handle resizing BEFORE DragWindow
        HandleResize();

        GUI.DragWindow();
    }

    private void HandleResize()
    {
        // Fetch your custom window padding dynamically
        RectOffset windowPadding = WeaverNetStyles.Window.padding;

        float handleSize = 20f;

        Rect resizeClickRect = new Rect(
                _windowRect.width - handleSize,
                _windowRect.height - handleSize,
                handleSize,
                handleSize
            );
        Rect resizeVisualRect = new Rect(
            _windowRect.width - handleSize + 1f,
            _windowRect.height - handleSize + 9f,
            handleSize,
            handleSize
        );
        _resizeControlID = GUIUtility.GetControlID(FocusType.Passive);
        Event currentEvent = Event.current;

        // Check if hovering over the clickable region or actively resizing
        bool isHovering = resizeClickRect.Contains(currentEvent.mousePosition) || _isResizing;

        // Handle Cursor Icon Change
        if (isHovering)
        {
            Cursor.SetCursor(_resizeCursorTexture, new Vector2(10, 10), CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        // 3. Render the symbol during the Repaint event to ignore layout constraints
        if (currentEvent.type == EventType.Repaint)
        {
            GUIStyle cornerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerRight,
                fontSize = 22, 
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                contentOffset = new Vector2(0, 0) // Reset to zero so it relies purely on the Visual Rect positioning
            };
            Color originalColor = GUI.color;
            if (isHovering)
            {
                GUI.color = new Color(0.3f, 0.3f, 0.3f, 1.0f);
            }
            else
            {
                GUI.color = new Color(0.6f, 0.6f, 0.6f, 1.0f);
            }

            cornerStyle.Draw(resizeVisualRect, "◢", false, false, false, false);
            GUI.color = originalColor;
        }

        // 4. Robust Resize Logic (Screen Space) using the Click Zone
        switch (currentEvent.GetTypeForControl(_resizeControlID))
        {
            case EventType.MouseDown:
                if (resizeClickRect.Contains(currentEvent.mousePosition) && currentEvent.button == 0)
                {
                    _isResizing = true;
                    _resizeStartMousePos = GUIUtility.GUIToScreenPoint(currentEvent.mousePosition);
                    _resizeStartWindowSize = new Vector2(_windowRect.width, _windowRect.height);

                    GUIUtility.hotControl = _resizeControlID;
                    currentEvent.Use();
                }
                break;

            case EventType.MouseUp:
                if (_isResizing && currentEvent.button == 0)
                {
                    _isResizing = false;
                    GUIUtility.hotControl = 0;
                    currentEvent.Use();
                }
                break;

            case EventType.MouseDrag:
                if (_isResizing && GUIUtility.hotControl == _resizeControlID)
                {
                    Vector2 currentMousePos = GUIUtility.GUIToScreenPoint(currentEvent.mousePosition);
                    Vector2 delta = currentMousePos - _resizeStartMousePos;

                    _windowRect.width = Mathf.Max(MinWidth, _resizeStartWindowSize.x + delta.x);
                    _windowRect.height = Mathf.Max(MinHeight, _resizeStartWindowSize.y + delta.y);

                    currentEvent.Use();
                }
                break;
        }
    }

    protected void NotifyError(string error)
    {
        _errors.Add(error);
    }
    protected void Notify(string message)
    {
        _notifications.Add(message);
    }
    protected virtual void DrawError()
    {
        _errorLogScroll = GUILayout.BeginScrollView(_errorLogScroll, WeaverNetStyles.ScrollView, WeaverNetStyles.VerticalScrollbar, GUILayout.ExpandHeight(true));
        GUI.skin.verticalScrollbarThumb = WeaverNetStyles.VerticalScrollbarThumb;

        GUILayout.Label("Exception Has Occured Somhere: ");
        GUILayout.Label(_errors[0]);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Okay", WeaverNetStyles.Button))
        {
            _errors.RemoveAt(0);
        }
        GUILayout.EndScrollView();
    }
    protected virtual void DrawNotification()
    {
        _notificationLogScroll = GUILayout.BeginScrollView(_notificationLogScroll, WeaverNetStyles.ScrollView, WeaverNetStyles.VerticalScrollbar, GUILayout.ExpandHeight(true));
        GUI.skin.verticalScrollbarThumb = WeaverNetStyles.VerticalScrollbarThumb;

        GUILayout.Label(_notifications[0]);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Okay", WeaverNetStyles.Button))
        {
            _notifications.RemoveAt(0);
        }
        GUILayout.EndScrollView();
    }

    public abstract void DrawContent();
    public abstract bool CanEnable();
}