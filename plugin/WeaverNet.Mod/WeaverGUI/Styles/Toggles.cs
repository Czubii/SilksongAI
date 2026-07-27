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

        private static GUIStyle _toggleButton;
        public static GUIStyle ToggleButton => Lazy(ref _toggleButton, () =>
        {
            var style = new GUIStyle(GUI.skin.button);

            style.normal.background = Tex(Theme.Control);
            style.hover.background = Tex(Theme.ControlHover);
            style.active.background = Tex(Theme.ControlPressed);

            style.onNormal.background = Tex(Theme.Primary);
            style.onHover.background = Tex(Theme.PrimaryHover);
            style.onActive.background = Tex(Theme.PrimaryPressed);

            style.padding = new RectOffset(4, 4, 4, 4);
            style.margin = new RectOffset(4, 4, 4, 4);
            style.alignment = TextAnchor.MiddleCenter;

            return style;
        });

        private static GUIStyle _toggleBackground;

        public static GUIStyle ToggleBackground => Lazy(ref _toggleBackground, () =>
        {
            var style = new GUIStyle();

            style.normal.background = BorderedTex(
                Theme.Border,
                Theme.Background);

            style.border = new RectOffset(1, 1, 1, 1);

            return style;
        });


        private static GUIStyle _toggleBackgroundActive;

        public static GUIStyle ToggleBackgroundActive => Lazy(ref _toggleBackgroundActive, () =>
        {
            var style = new GUIStyle();

            style.normal.background = BorderedTex(
                Theme.BorderAccent,
                Theme.Primary);

            style.border = new RectOffset(1, 1, 1, 1);

            return style;
        });


        private static GUIStyle _toggleNotch;

        public static GUIStyle ToggleNotch => Lazy(ref _toggleNotch, () =>
        {
            var style = new GUIStyle();

            style.normal.background = Tex(Theme.Text);

            return style;
        });
    }
}
