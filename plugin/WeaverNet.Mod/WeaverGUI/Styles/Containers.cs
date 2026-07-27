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
        private static GUIStyle _popupLock;
        public static GUIStyle PopupLock => Lazy(ref _popupLock, () =>
        {
            var style = new GUIStyle(GUI.skin.box);

            style.normal.background = RoundedBorderedTex(
                1,
                6,
                Theme.BorderAccent,
                Theme.BackgroundTranslucent);

            style.border = new RectOffset(6, 6, 6, 6);
            style.padding = new RectOffset(0, 0, 0, 0);

            return style;
        });

        private static GUIStyle _window = null;
        public static GUIStyle Window => Lazy(ref _window, () =>
        {
            var style = new GUIStyle(GUI.skin.box);

            Texture2D tex = RoundedBorderedTex(
                Theme.Border,
                Theme.Background);

            style.normal.background = tex;
            style.hover.background = tex;
            style.active.background = tex;
            style.focused.background = tex;


            style.border = new RectOffset(7,7,7,7);
            style.padding = new RectOffset(10, 10, 10, 10);
            style.margin = new RectOffset(0, 0, 0, 0);

            return style;
        });
        private static GUIStyle _activeWindow = null;
        public static GUIStyle ActiveWindow => Lazy(ref _activeWindow, () =>
        {
            var style = new GUIStyle(GUI.skin.box);

            Texture2D tex = RoundedBorderedTex(
                Theme.BorderAccent,
                Theme.Background);

            style.normal.background = tex;
            style.hover.background = tex;
            style.active.background = tex;
            style.focused.background = tex;

            //style.border = new RectOffset(1, 1, 1, 1);
            style.border = new RectOffset(7, 7, 7, 7);
            style.padding = new RectOffset(10, 10, 10, 10);
            style.margin = new RectOffset(0, 0, 0, 0);

            return style;
        });
        private static GUIStyle _card = null;
        public static GUIStyle Card => Lazy(ref _card, () =>
        {
            var style = new GUIStyle(GUI.skin.box);

            Texture2D tex = BorderedTex(
                Theme.Border,
                Theme.Background);

            style.normal.background = tex;

            style.border = new RectOffset(1, 1, 1, 1);

            style.padding = new RectOffset(10, 10, 8, 8);
            style.margin = new RectOffset(4, 4, 4, 4);
            return style;
        });

        private static GUIStyle _scrollView = null;
        public static GUIStyle ScrollView => Lazy(ref _scrollView, () =>
        {
            var _scrollView = new GUIStyle(GUI.skin.scrollView);

            Texture2D bg = Tex(Theme.Surface);

            _scrollView.normal.background = bg;

            _scrollView.padding = new RectOffset(0, 0, 0, 0);
            _scrollView.margin = new RectOffset(0, 0, 0, 0);
            _scrollView.border = new RectOffset(0, 0, 0, 0);
            _scrollView.overflow = new RectOffset(0, 0, 0, 0);

            return _scrollView;
        });

        private static GUIStyle _topBar = null;
        public static GUIStyle TopBar => Lazy(ref _topBar, () =>
        {
            var style = new GUIStyle
            {
                //padding = new RectOffset(5, 5, 5, 5)
            };

            return style;
        });
    }


}
