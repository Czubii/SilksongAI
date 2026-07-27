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
        private static GUIStyle _viewTitleLabel = null;
        public static GUIStyle ViewTitleLabel => Lazy(ref _viewTitleLabel, () =>
        {
            var style = new GUIStyle(GUI.skin.label);

            style.fontStyle = FontStyle.Bold;
            style.fontSize = 15;
            style.alignment = TextAnchor.MiddleLeft;
            style.margin = new RectOffset(2, 2, 2, 2);
            style.normal.textColor = Theme.TextMuted;
            return style;
        });

        private static GUIStyle _windowTitleLabel = null;
        public static GUIStyle WindowTitleLabel => Lazy(ref _windowTitleLabel, () =>
        {
            var style = new GUIStyle(GUI.skin.label);

            style.fontStyle = FontStyle.Bold;
            style.fontSize = 20;
            style.normal.textColor = Theme.Text;
            style.margin = new RectOffset(0, 0, 0, 0);
            style.padding = new RectOffset(0, 0, 0, 0);
            style.wordWrap = false;
            style.clipping = TextClipping.Clip;

            return style;
        });

        private static GUIStyle _elementLabelTitle = null;
        public static GUIStyle ElementLabelTitle => Lazy(ref _elementLabelTitle, () =>
        {
            var style = new GUIStyle(GUI.skin.label);

            style.fontStyle = FontStyle.Bold;
            style.fontSize = 16;
            style.alignment = TextAnchor.MiddleLeft;
            style.margin = new RectOffset(0,0,0,0);
            style.normal.textColor = Theme.Text;
            return style;
        });

        private static GUIStyle _elementLabelDescription = null;
        public static GUIStyle ElementLabelDescription => Lazy(ref _elementLabelDescription, () =>
        {
            var style = new GUIStyle(GUI.skin.label);

            style.fontStyle = FontStyle.Normal;
            style.fontSize = 13;
            style.alignment = TextAnchor.MiddleLeft;
            style.margin = new RectOffset(0, 0, 0, 0);
            style.normal.textColor = Theme.TextMuted;
            return style;
        });
    }
}
