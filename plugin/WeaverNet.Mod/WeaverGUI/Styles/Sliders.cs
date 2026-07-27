using UnityEngine;

namespace WeaverNet.Mod.WeaverGUI.Styles
{
    public static partial class WeaverNetStyles
    {
        private static GUIStyle _verticalScrollbar = null;
        public static GUIStyle VerticalScrollbar => Lazy(ref _verticalScrollbar, () =>
        {
            var style = new GUIStyle(GUI.skin.verticalScrollbar);

            Texture2D bg = Tex(Theme.SurfaceRaised);

            style.normal.background = bg;

            style.fixedWidth = 10;
            style.margin = new RectOffset(6, 0, 4, 4);

            return style;
        });

        private static GUIStyle _verticalScrollbarThumb = null;
        public static GUIStyle VerticalScrollbarThumb => Lazy(ref _verticalScrollbarThumb, () =>
        {
            var style = new GUIStyle(GUI.skin.verticalScrollbarThumb);

            Texture2D normal = Tex(Theme.Control);
            Texture2D hover = Tex(Theme.ControlHover);
            Texture2D active = Tex(Theme.ControlPressed);

            style.normal.background = normal;
            style.hover.background = hover;
            style.active.background = active;

            style.fixedWidth = 10;
            style.padding = new RectOffset(0, 0, 0, 0);
            style.margin = new RectOffset(0, 0, 2, 2);
            style.border = new RectOffset(0, 0, 0, 0);
            style.overflow = new RectOffset(0, 0, 0, 0);

            return style;
        });


        private static GUIStyle _sliderTrack = null;
        public static GUIStyle SliderTrack => Lazy(ref _sliderTrack, () =>
        {
            var style = new GUIStyle(GUI.skin.horizontalSlider);
            Texture2D trackTex = Tex(Theme.SurfaceRaised);

            style.normal.background = trackTex;
            style.hover.background = trackTex;
            style.active.background = trackTex;
            style.fixedHeight = 18;
            style.padding = new RectOffset(0, 0, 0, 0);

            return style;
        });

        private static GUIStyle _sliderThumb = null;
        public static GUIStyle SliderThumb => Lazy(ref _sliderThumb, () =>
        {
            var style = new GUIStyle(GUI.skin.horizontalSliderThumb);
            Texture2D normalTex = BorderedTex(18, 2,
                new Color(0.1f, 0.1f, 0.1f, 1f),  //TODO use theme
                new Color(0.3f, 0.55f, 0.35f, 1f));   //TODO use theme
            normalTex.filterMode = FilterMode.Point;
            normalTex.wrapMode = TextureWrapMode.Clamp;

            Texture2D hoverTex = BorderedTex(18, 2,
                new Color(0.1f, 0.1f, 0.1f, 1f),       //TODO use theme
                new Color(0.37f, 0.63f, 0.41f, 1f));  //TODO use theme

            hoverTex.filterMode = FilterMode.Point;
            hoverTex.wrapMode = TextureWrapMode.Clamp;

            style.normal.background = normalTex;
            style.hover.background = hoverTex;
            style.active.background = hoverTex;
            style.fixedWidth = 18;
            style.fixedHeight = 18;

            return style;
        });
    }
}
