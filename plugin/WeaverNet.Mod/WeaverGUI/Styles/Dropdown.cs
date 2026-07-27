using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WeaverNet.Mod.WeaverGUI.Styles
{
    public static partial class WeaverNetStyles
    {
        private static GUIStyle _dropdown = null;

        public static GUIStyle DropdownButton => Lazy(ref _dropdown, () =>
        {
            var style = new GUIStyle(GUI.skin.textField);

            Texture2D normalTex = RoundedBorderedTex(Theme.Border, Theme.Background);
            Texture2D hoverTex = RoundedBorderedTex(Theme.Border, Theme.Surface);
            Texture2D activeTex = RoundedBorderedTex(Theme.BorderAccent, Theme.Surface);

            style.normal.background = normalTex;
            style.hover.background = hoverTex;
            style.active.background = activeTex;
            style.focused.background = activeTex;

            style.onNormal.background = normalTex;
            style.onHover.background = hoverTex;
            style.onActive.background = activeTex;
            style.onFocused.background = activeTex;

            style.border = new RectOffset(7,7,7,7);

            // Increased size
            style.fontSize = 13;
            style.padding = new RectOffset(8, 8, 6, 6);
            style.margin = new RectOffset(2, 2, 3, 3);

            style.alignment = TextAnchor.MiddleLeft;

            return style;
        });

        private static GUIStyle _dropdownArrow;
        public static GUIStyle DropdownArrow => Lazy(ref _dropdownArrow, () =>
        {
            var style = new GUIStyle(GUI.skin.label);

            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 12;
            style.normal.textColor = Theme.Text;

            return style;
        });

        private static GUIStyle _dropdownButtonText;
        public static GUIStyle DropdownButtonText => Lazy(ref _dropdownButtonText, () =>
        {
            var style = new GUIStyle(GUI.skin.label);

            style.alignment = TextAnchor.MiddleLeft;
            style.fontSize = 14;
            style.normal.textColor = Theme.Text;
            style.clipping = TextClipping.Clip;

            return style;
        });

        private static GUIStyle _dropdownOverlayBacground;
        public static GUIStyle DropdownOverlayBacground => Lazy(ref _dropdownOverlayBacground, () =>
        {
            var style = new GUIStyle(GUI.skin.box);

            Texture2D tex = RoundedBorderedTex(
                Theme.Border,
                Theme.Border);

            style.normal.background = tex;
            style.hover.background = tex;
            style.active.background = tex;
            style.focused.background = tex;


            style.border = new RectOffset(7, 7, 7, 7);
            style.padding = new RectOffset(10, 10, 10, 10);
            style.margin = new RectOffset(0, 0, 0, 0);

            return style;
        });
    }
}
