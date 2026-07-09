using System.Collections.Generic;
using UnityEngine;

namespace WeaverNet.Mod.PluginGUI
{
    public static class PluginGUIStyles
    {
        public static Color UIGreen = new Color(0.3f, 0.55f, 0.35f, 1f); // new Color(0.25f, 0.35f, 0.55f, 1f)
        public static Color UIOrange = new Color(199/255f, 110/255f, 42/255f, 1f); 

        private static List<Texture2D> _textures = new List<Texture2D>();

        private static Texture2D _lightBackgroundTex = null;
        public static Texture2D LightBackgroundTex
        {
            get
            {
                if (_lightBackgroundTex == null)
                {
                    _lightBackgroundTex = MakeTex(new Color(0.15f, 0.15f, 0.15f, 1f));
                }
                return _lightBackgroundTex;
            }
        }

        private static Texture2D _darkBackgroundTex = null;
        public static Texture2D DarkBackgroundTex
        {
            get
            {
                if (_darkBackgroundTex == null)
                {
                    _darkBackgroundTex = MakeTex(new Color(0.06f, 0.06f, 0.06f, 1f));
                }
                return _darkBackgroundTex;
            }
        }

        private static Texture2D _orangeBackgroundTex = null;
        public static Texture2D OrangeBackgroundTex
        {
            get
            {
                if (_orangeBackgroundTex == null)
                {
                    _orangeBackgroundTex = MakeTex(UIOrange);
                }
                return _orangeBackgroundTex;
            }
        }

        private static GUIStyle _headerLabel = null;
        public static GUIStyle HeaderLabel
        {
            get
            {
                if (_headerLabel == null)
                {
                    _headerLabel = new GUIStyle(GUI.skin.label);

                    _headerLabel.fontStyle = FontStyle.Bold;
                    _headerLabel.fontSize = 15;
                }

                return _headerLabel;
            }
        }

        private static GUIStyle _card = null;
        public static GUIStyle Card
        {
            get
            {
                if (_card == null)
                {
                    _card = new GUIStyle(GUI.skin.box);

                    Texture2D normalTex = MakeTex(new Color(0.125f, 0.125f, 0.125f, 1f));

                    _textures.Add(normalTex);

                    _card.normal.background = normalTex;

                    _card.padding = new RectOffset(10, 10, 8, 8);
                    _card.margin = new RectOffset(4, 4, 4, 4);
                }

                return _card;
            }
        }

        private static GUIStyle _cardGreenHighlight = null;
        public static GUIStyle CardGreenHighlight
        {
            get
            {
                if (_cardGreenHighlight == null)
                {
                    _cardGreenHighlight = new GUIStyle(Card);

                    Texture2D selectedTex = MakeTex(UIGreen);

                    _textures.Add(selectedTex);

                    _cardGreenHighlight.normal.background = selectedTex;
                    _cardGreenHighlight.hover.background = selectedTex;
                    _cardGreenHighlight.active.background = selectedTex;
                }

                return _cardGreenHighlight;
            }
        }

        private static GUIStyle _cardOrangeHighlight = null;
        public static GUIStyle CardOrangeHighlight
        {
            get
            {
                if (_cardOrangeHighlight == null)
                {
                    _cardOrangeHighlight = new GUIStyle(Card);

                    Texture2D selectedTex = MakeTex(UIOrange);

                    _textures.Add(selectedTex);

                    _cardOrangeHighlight.normal.background = selectedTex;
                    _cardOrangeHighlight.hover.background = selectedTex;
                    _cardOrangeHighlight.active.background = selectedTex;
                }

                return _cardOrangeHighlight;
            }
        }

        private static GUIStyle _closeButton = null;
        public static GUIStyle CloseButton 
        {
            get
            {
                if (_closeButton == null)
                {
                    _closeButton = new GUIStyle(GUI.skin.button);

                    Texture2D normalTex = LightBackgroundTex;
                    Texture2D hoverTex = MakeTex(new Color(1f, 0f, 0f, 1f));
                    Texture2D activeTex = MakeTex(new Color(0.8f, 0f, 0f, 1f));

                    _textures.Add(normalTex);
                    _textures.Add(hoverTex);
                    _textures.Add(activeTex);

                    _closeButton.normal.background = normalTex;
                    _closeButton.hover.background = hoverTex;
                    _closeButton.active.background = activeTex;

                    _closeButton.padding = new RectOffset(0, 0, 0, 0);
                    _closeButton.margin = new RectOffset(2, 2, 2, 2);
                    _closeButton.alignment = TextAnchor.MiddleCenter;

                }

                return _closeButton;
            }
        }

        private static GUIStyle _button = null;
        public static GUIStyle Button
        {
            get
            {
                if (_button == null)
                {
                    _button = new GUIStyle(GUI.skin.button);

                    Texture2D normalTex = MakeTex(new Color(0.15f, 0.15f, 0.15f, 1f));
                    Texture2D hoverTex = MakeTex(new Color(0.2f, 0.2f, 0.2f, 1f));
                    Texture2D activeTex = MakeTex(new Color(0.3f, 0.3f, 0.3f, 1f));

                    _textures.Add(normalTex);
                    _textures.Add(hoverTex);
                    _textures.Add(activeTex);

                    _button.normal.background = normalTex;
                    _button.hover.background = hoverTex;
                    _button.active.background = activeTex;

                    _button.onNormal.background = normalTex;
                    _button.onHover.background = hoverTex;
                    _button.onActive.background = activeTex;

                    _button.padding = new RectOffset(4, 4, 4, 4);
                    _button.margin = new RectOffset(2, 2, 2, 2);
                    _button.alignment = TextAnchor.MiddleCenter;

                }

                return _button;
            }
        }

        private static GUIStyle _greenButton = null;
        public static GUIStyle GreenButton
        {
            get
            {
                if (_greenButton == null)
                {
                    _greenButton = new GUIStyle(GUI.skin.button);

                    Texture2D normalTex = MakeTex(new Color(0.3f, 0.55f, 0.35f, 1f));
                    Texture2D hoverTex = MakeTex(new Color(0.37f, 0.63f, 0.41f, 1f));
                    Texture2D activeTex = MakeTex(new Color(0.3f, 0.3f, 0.3f, 1f));

                    _textures.Add(normalTex);
                    _textures.Add(hoverTex);
                    _textures.Add(activeTex);

                    _greenButton.normal.background = normalTex;
                    _greenButton.hover.background = hoverTex;
                    _greenButton.active.background = activeTex;

                    _greenButton.onNormal.background = normalTex;
                    _greenButton.onHover.background = hoverTex;
                    _greenButton.onActive.background = activeTex;

                    _greenButton.padding = new RectOffset(4, 4, 4, 4);
                    _greenButton.margin = new RectOffset(4, 4, 4, 4);
                    _greenButton.alignment = TextAnchor.MiddleCenter;

                }

                return _greenButton;
            }
        }

        private static GUIStyle _toggleButton = null;
        public static GUIStyle ToggleButton
        {
            get
            {
                if (_toggleButton == null)
                {
                    _toggleButton = new GUIStyle(GUI.skin.button);

                    Texture2D normalTex = MakeTex(new Color(0.15f, 0.15f, 0.15f, 1f));
                    Texture2D hoverTex = MakeTex(new Color(0.2f, 0.2f, 0.2f, 1f));
                    Texture2D activeTex = MakeTex(new Color(0.3f, 0.3f, 0.3f, 1f));

                    Texture2D onNormalTex = MakeTex(new Color(0.3f, 0.55f, 0.35f, 1f));
                    Texture2D onHoverTex = MakeTex(new Color(0.37f, 0.63f, 0.41f, 1f));
                    Texture2D onActiveTex = MakeTex(new Color(0.3f, 0.3f, 0.3f, 1f));

                    _textures.Add(normalTex);
                    _textures.Add(hoverTex);
                    _textures.Add(activeTex);

                    _textures.Add(onNormalTex);
                    _textures.Add(onHoverTex);
                    _textures.Add(onActiveTex);

                    _toggleButton.normal.background = normalTex;
                    _toggleButton.hover.background = hoverTex;
                    _toggleButton.active.background = activeTex;

                    _toggleButton.onNormal.background = onNormalTex;
                    _toggleButton.onHover.background = onHoverTex;
                    _toggleButton.onActive.background = onActiveTex;

                    _toggleButton.padding = new RectOffset(4, 4, 4, 4);
                    _toggleButton.margin = new RectOffset(4, 4, 4, 4);
                    _toggleButton.alignment = TextAnchor.MiddleCenter;

                }

                return _toggleButton;
            }
        }

        private static GUIStyle _toggle = null;
        public static GUIStyle Toggle
        {
            get
            {
                if (_toggle == null)
                {
                    _toggle = new GUIStyle(GUI.skin.toggle);

                    Texture2D normalTex = MakeTex(new Color(0.06f, 0.06f, 0.06f, 1f));
                    Texture2D hoverTex = MakeTex(new Color(0.2f, 0.2f, 0.2f, 1f));
                    Texture2D activeTex = MakeTex(new Color(0.3f, 0.3f, 0.3f, 1f));

                    Texture2D onNormalTex = MakeBorderedTex(18, 3,
                        new Color(0.06f, 0.06f, 0.06f, 1f),   // border
                        new Color(0.3f, 0.55f, 0.35f, 1f));   // green fill
                    onNormalTex.filterMode = FilterMode.Point;
                    onNormalTex.wrapMode = TextureWrapMode.Clamp;

                    Texture2D onHoverTex = MakeBorderedTex(18, 2,
                        new Color(0.2f, 0.2f, 0.2f, 1f),       // border
                        new Color(0.37f, 0.63f, 0.41f, 1f));   // green fill
                    onHoverTex.filterMode = FilterMode.Point;
                    onHoverTex.wrapMode = TextureWrapMode.Clamp;

                    Texture2D onActiveTex = MakeTex(new Color(0.3f, 0.3f, 0.3f, 1f));

                    _textures.Add(normalTex);
                    _textures.Add(hoverTex);
                    _textures.Add(activeTex);

                    _textures.Add(onNormalTex);
                    _textures.Add(onHoverTex);
                    _textures.Add(onActiveTex);

                    _toggle.normal.background = normalTex;
                    _toggle.hover.background = hoverTex;
                    _toggle.active.background = activeTex;

                    _toggle.onNormal.background = onNormalTex;
                    _toggle.onHover.background = onHoverTex;
                    _toggle.onActive.background = onActiveTex;

                    _toggle.fixedWidth = 18;
                    _toggle.fixedHeight = 18;

                    _toggle.padding = new RectOffset(0, 0, 0, 0);
                    _toggle.margin = new RectOffset(2, 2, 2, 2);
                    _toggle.border = new RectOffset(0, 0, 0, 0);
                    _toggle.overflow = new RectOffset(0, 0, 0, 0);
                    _toggle.contentOffset = Vector2.zero;
                }

                return _toggle;
            }
        }

        private static GUIStyle _textField = null;
        public static GUIStyle TextField
        {
            get
            {
                if (_textField == null)
                {
                    _textField = new GUIStyle(GUI.skin.textField);

                    Texture2D normalTex = MakeTex(new Color(0.06f, 0.06f, 0.06f, 1f));
                    Texture2D hoverTex = MakeTex(new Color(0.2f, 0.2f, 0.2f, 1f));
                    Texture2D activeTex = MakeTex(new Color(0.3f, 0.3f, 0.3f, 1f));

                    _textures.Add(normalTex);
                    _textures.Add(hoverTex);
                    _textures.Add(activeTex);

                    _textField.normal.background = normalTex;
                    _textField.hover.background = hoverTex;
                    _textField.active.background = activeTex;
                    _textField.focused.background = activeTex;

                    _textField.onNormal.background = normalTex;
                    _textField.onHover.background = hoverTex;
                    _textField.onActive.background = activeTex;
                    _textField.onFocused.background = activeTex;

                    _textField.padding = new RectOffset(4, 4, 2, 2);
                    _textField.margin = new RectOffset(2, 2, 2, 2);
                    _textField.alignment = TextAnchor.MiddleLeft;

                }

                return _textField;
            }
        }

        private static GUIStyle _scrollView = null;
        public static GUIStyle ScrollView
        {
            get
            {
                if (_scrollView == null)
                {
                    _scrollView = new GUIStyle(GUI.skin.scrollView);

                    Texture2D bg = MakeTex(new Color(0.08f, 0.08f, 0.08f, 1f));
                    _textures.Add(bg);

                    _scrollView.normal.background = bg;

                    _scrollView.padding = new RectOffset(0, 0, 0, 0);
                    _scrollView.margin = new RectOffset(0, 0, 0, 0);
                    _scrollView.border = new RectOffset(0, 0, 0, 0);
                    _scrollView.overflow = new RectOffset(0, 0, 0, 0);
                }

                return _scrollView;
            }
        }

        private static GUIStyle _verticalScrollbar = null;
        public static GUIStyle VerticalScrollbar
        {
            get
            {
                if (_verticalScrollbar == null)
                {
                    _verticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);

                    Texture2D bg = MakeTex(new Color(0.12f, 0.12f, 0.12f, 1f));
                    _textures.Add(bg);

                    _verticalScrollbar.normal.background = bg;

                    _verticalScrollbar.fixedWidth = 10;
                    _verticalScrollbar.margin = new RectOffset(6, 0, 4, 4);
                }

                return _verticalScrollbar;
            }
        }

        private static GUIStyle _verticalScrollbarThumb = null;
        public static GUIStyle VerticalScrollbarThumb
        {
            get
            {
                if (_verticalScrollbarThumb == null)
                {
                    _verticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);

                    Texture2D normal = MakeTex(new Color(0.35f, 0.35f, 0.35f));
                    Texture2D hover = MakeTex(new Color(0.45f, 0.45f, 0.45f));
                    Texture2D active = MakeTex(new Color(0.55f, 0.55f, 0.55f));

                    _textures.Add(normal);
                    _textures.Add(hover);
                    _textures.Add(active);

                    _verticalScrollbarThumb.normal.background = normal;
                    _verticalScrollbarThumb.hover.background = hover;
                    _verticalScrollbarThumb.active.background = active;

                    _verticalScrollbarThumb.fixedWidth = 10;
                    _verticalScrollbarThumb.padding = new RectOffset(0, 0, 0, 0);
                    _verticalScrollbarThumb.margin = new RectOffset(0, 0, 2, 2);
                    _verticalScrollbarThumb.border = new RectOffset(0, 0, 0, 0);
                    _verticalScrollbarThumb.overflow = new RectOffset(0, 0, 0, 0);
                }

                return _verticalScrollbarThumb;
            }
        }

        private static GUIStyle _sliderTrack = null;
        public static GUIStyle SliderTrack
        {
            get
            {
                if (_sliderTrack == null)
                {
                    _sliderTrack = new GUIStyle(GUI.skin.horizontalSlider);
                    Texture2D trackTex = MakeTex(new Color(0.06f, 0.06f, 0.06f, 1f));
                    _textures.Add(trackTex);
                    _sliderTrack.normal.background = trackTex;
                    _sliderTrack.hover.background = trackTex;
                    _sliderTrack.active.background = trackTex;
                    _sliderTrack.fixedHeight = 18;
                    _sliderTrack.padding = new RectOffset(0, 0, 0, 0);
                }
                return _sliderTrack;
            }
        }

        private static GUIStyle _sliderThumb = null;
        public static GUIStyle SliderThumb
        {
            get
            {
                if (_sliderThumb == null)
                {
                    _sliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
                    Texture2D normalTex = MakeBorderedTex(18, 2,
                        new Color(0.1f, 0.1f, 0.1f, 1f),   // border
                        new Color(0.3f, 0.55f, 0.35f, 1f));   // green fill
                    normalTex.filterMode = FilterMode.Point;
                    normalTex.wrapMode = TextureWrapMode.Clamp;

                    Texture2D hoverTex = MakeBorderedTex(18, 2,
                        new Color(0.1f, 0.1f, 0.1f, 1f),       // border
                        new Color(0.37f, 0.63f, 0.41f, 1f));   // green fill
                    hoverTex.filterMode = FilterMode.Point;
                    hoverTex.wrapMode = TextureWrapMode.Clamp;

                    _textures.Add(normalTex);
                    _textures.Add(hoverTex);

                    _sliderThumb.normal.background = normalTex;
                    _sliderThumb.hover.background = hoverTex;
                    _sliderThumb.active.background = hoverTex;
                    _sliderThumb.fixedWidth = 18;
                    _sliderThumb.fixedHeight = 18;
                }
                return _sliderThumb;
            }
        }

        private static GUIStyle _window = null;
        public static GUIStyle Window
        {
            get
            {
                if (_window == null)
                {
                    _window = new GUIStyle(GUI.skin.box);

                    Texture2D tex = MakeTex(new Color(0.10f, 0.10f, 0.10f, 0.97f));
                    _textures.Add(tex);

                    _window.normal.background = tex;
                    _window.hover.background = tex;
                    _window.active.background = tex;
                    _window.focused.background = tex;

                    _window.padding = new RectOffset(6, 6, 6, 6);
                    _window.margin = new RectOffset(0, 0, 0, 0);
                }
                return _window;
            }
        }

        private static GUIStyle _progressBarBackground = null;
        public static GUIStyle ProgressBarBackground
        {
            get
            {
                if (_progressBarBackground == null)
                {
                    _progressBarBackground = new GUIStyle(GUI.skin.box);

                    Texture2D bgTex = MakeTex(new Color(0.06f, 0.06f, 0.06f, 1f));

                    _textures.Add(bgTex);

                    _progressBarBackground.normal.background = bgTex;

                    _progressBarBackground.padding = new RectOffset(2, 2, 2, 2);
                    _progressBarBackground.margin = new RectOffset(2, 2, 4, 4);
                    _progressBarBackground.border = new RectOffset(1, 1, 1, 1);
                }

                return _progressBarBackground;
            }
        }

        private static GUIStyle _progressBarFill = null;
        public static GUIStyle ProgressBarFill
        {
            get
            {
                if (_progressBarFill == null)
                {
                    _progressBarFill = new GUIStyle(GUI.skin.box);

                    Texture2D fillTex = MakeTex(new Color(0.3f, 0.55f, 0.35f, 1f));

                    _textures.Add(fillTex);

                    _progressBarFill.normal.background = fillTex;

                    _progressBarFill.margin = new RectOffset(0, 0, 0, 0);
                }

                return _progressBarFill;
            }
        }

        //private static GUIStyle _window = null; TEMPLATE  ---------------------------------------------------------
        //public static GUIStyle Window
        //{
        //    get
        //    {
        //        if (_window == null)
        //        {

        //        }
        //        return _window;
        //    }
        //}

        private static Texture2D MakeBorderedTex(int size, int border, Color borderColor, Color fillColor)
        {
            Texture2D tex = new Texture2D(size, size);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder =
                        x < border ||
                        y < border ||
                        x >= size - border ||
                        y >= size - border;

                    tex.SetPixel(x, y, isBorder ? borderColor : fillColor);
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D MakeTex(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;

        }
    }
}
