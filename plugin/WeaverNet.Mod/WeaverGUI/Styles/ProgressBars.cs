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

            Texture2D bgTex = RoundedBorderedTex(Theme.Border, Theme.Surface);

            style.normal.background = bgTex;

            style.fontSize = 13;
            style.padding = new RectOffset(8, 8, 6, 6);
            style.margin = new RectOffset(2, 2, 3, 3);
            style.border = new RectOffset(7,7,7,7);

            return style;
        });

        private static GUIStyle _progressBarFill = null;
        public static GUIStyle ProgressBarFill => Lazy(ref _progressBarFill, () =>
        {
            var style = new GUIStyle(GUI.skin.box);

            Texture2D bgTex = RoundedBorderedTex(Theme.PrimaryPressed, Theme.Primary);

            style.normal.background = bgTex;

            style.fontSize = 13;
            style.padding = new RectOffset(8, 8, 6, 6);
            style.margin = new RectOffset(2, 2, 3, 3);
            style.border = new RectOffset(7, 7, 7, 7);

            return style;
        });
    }
}
