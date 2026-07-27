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
        private static GUIStyle _progressBarBackground = null;
        public static GUIStyle ProgressBarBackground => Lazy(ref _progressBarBackground, () =>
        {
            var style = new GUIStyle(GUI.skin.box);

            Texture2D bgTex = Tex(Theme.Control);

            style.normal.background = bgTex;

            style.padding = new RectOffset(2, 2, 2, 2);
            style.margin = new RectOffset(2, 2, 4, 4);
            style.border = new RectOffset(1, 1, 1, 1);

            return style;
        });

        private static GUIStyle _progressBarFill = null;
        public static GUIStyle ProgressBarFill => Lazy(ref _progressBarFill, () =>
        {
            var style = new GUIStyle(GUI.skin.box);

            Texture2D bgTex = Tex(Theme.Primary);

            style.normal.background = bgTex;

            style.padding = new RectOffset(2, 2, 2, 2);
            style.margin = new RectOffset(2, 2, 4, 4);
            style.border = new RectOffset(1, 1, 1, 1);

            return style;
        });
    }
}
