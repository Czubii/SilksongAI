using UnityEngine;

namespace WeaverNet.Mod.WeaverGUI.Styles
{
    public static partial class WeaverNetStyles
    {
        private static GUIStyle CreateButton(
            Color normal,
            Color hover,
            Color active,
            RectOffset margin = null)
        {
            var style = new GUIStyle(GUI.skin.button);

            SetBackgrounds(
                style,
                Tex(normal),
                Tex(hover),
                Tex(active));

            style.padding = new RectOffset(4, 4, 4, 4);
            style.margin = margin ?? new RectOffset(2, 2, 2, 2);
            style.alignment = TextAnchor.MiddleCenter;

            return style;
        }

        private static GUIStyle _button = null;
        public static GUIStyle Button => Lazy(ref _button, () =>
        {
            return CreateButton(
                Theme.Control,
                Theme.ControlHover,
                Theme.ControlPressed
                );
        });
        private static GUIStyle _cancelButton = null;
        public static GUIStyle CancelButton => Lazy(ref _cancelButton, () =>
        {
            return CreateButton(
                Theme.Control,
                Theme.DangerHover,
                Theme.DangerPressed
                );
        });
        private static GUIStyle _accentButton = null;
        public static GUIStyle AccentButton => Lazy(ref _accentButton, () =>
        {
            return CreateButton(
                Theme.Primary,
                Theme.PrimaryHover,
                Theme.PrimaryPressed
                );
        });
        private static GUIStyle _resizeHandle = null;
        private static GUIStyle _resizeHandleHover = null;

        public static GUIStyle ResizeHandle => Lazy(ref _resizeHandle, () =>
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerRight,
                fontSize = 30,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                contentOffset = Vector2.zero
            };

            style.normal.textColor = Theme.Control;

            return style;
        });
        public static GUIStyle ResizeHandleHover => Lazy(ref _resizeHandleHover, () =>
        {
            var style = new GUIStyle(ResizeHandle);

            style.normal.textColor = Theme.ControlHover;

            return style;
        });
    }
}
