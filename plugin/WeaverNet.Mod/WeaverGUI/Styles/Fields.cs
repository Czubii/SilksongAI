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
        private static GUIStyle _textField = null;

        public static GUIStyle TextField => Lazy(ref _textField, () =>
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

            style.border = new RectOffset(6,6,6,6);

            // Increased size
            style.fontSize = 13;
            style.padding = new RectOffset(8, 8, 6, 6);
            style.margin = new RectOffset(2, 2, 3, 3);

            style.alignment = TextAnchor.MiddleLeft;

            return style;
        });
    }
}
