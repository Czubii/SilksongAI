using Microsoft.SqlServer.Server;
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
                RoundedBorderedTex(Theme.Border, normal),
                RoundedBorderedTex(Theme.Border, hover),
                RoundedBorderedTex(Theme.Border, active));

            style.fontSize = 13;
            style.border = new RectOffset(7, 7, 7, 7);
            style.padding = new RectOffset(8, 8, 6, 6);
            style.margin = margin ?? new RectOffset(2, 2, 3, 3);
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


        private static GUIStyle _foldableButton = null;
        public static GUIStyle FoldableButton => Lazy(ref _foldableButton, () =>
        {
            // Derive from label instead of textField to remove button/input behavior defaults
            var style = new GUIStyle();

            SetBackgrounds(
                style,
                RoundedBorderedTex(Color.clear, Color.clear),
                RoundedBorderedTex(Theme.Border, Theme.ControlHover),
                RoundedBorderedTex(Theme.Border, Theme.ControlPressed));

            // Layout & Spacing
            style.alignment = TextAnchor.MiddleLeft;
            style.padding = new RectOffset(4, 4, 4, 4);
            style.margin = new RectOffset(0, 0, 2, 4);
            style.border = new RectOffset(7, 7, 7, 7);

            return style;
        });
    }
}
