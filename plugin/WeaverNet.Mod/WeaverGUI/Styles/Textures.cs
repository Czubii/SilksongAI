using System.Collections.Generic;
using UnityEngine;

namespace WeaverNet.Mod.WeaverGUI.Styles
{
    public static partial class WeaverNetStyles
    {
        private static readonly Dictionary<Color, Texture2D> _texCache = new Dictionary<Color, Texture2D>();
        private static readonly Dictionary<(int size, int border, Color borderColor, Color fillColor), Texture2D> _borderedTexCache
            = new Dictionary<(int, int, Color, Color), Texture2D>();

        private static Texture2D MakeBorderedTex(int size, int border, Color borderColor, Color fillColor)
        {
            Texture2D tex = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder =
                        x < border ||
                        y < border ||
                        x >= size - border ||
                        y >= size - border;

                    pixels[y * size + x] = isBorder ? borderColor : fillColor;
                }
            }

            tex.SetPixels(pixels);

            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;

            tex.Apply(updateMipmaps: false, makeNoLongerReadable: true);

            return tex;
        }
        private static Texture2D BorderedTex(int size, int border, Color borderColor, Color fillColor)
        {
            var key = (size, border, borderColor, fillColor);

            if (_borderedTexCache.TryGetValue(key, out var tex))
                return tex;

            tex = MakeBorderedTex(size, border, borderColor, fillColor);

            _borderedTexCache.Add(key, tex);

            return tex;
        }

        private static Texture2D BorderedTex(Color borderColor, Color fillColor)
        {
            return BorderedTex(3, 1, borderColor, fillColor);
        }
        private static Texture2D MakeTex(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            return tex;

        }
        private static Texture2D Tex(Color color)
        {
            if (_texCache.TryGetValue(color, out var tex))
                return tex;

            tex = MakeTex(color);
            _texCache.Add(color, tex);

            return tex;
        }

        private static void SetBackgrounds(
            GUIStyle style,
            Texture2D normal,
            Texture2D hover,
            Texture2D active,
            bool applyOnStates = true)
        {
            style.normal.background = normal;
            style.hover.background = hover;
            style.active.background = active;

            if (applyOnStates)
            {
                style.onNormal.background = normal;
                style.onHover.background = hover;
                style.onActive.background = active;
            }
        }
    }
}
