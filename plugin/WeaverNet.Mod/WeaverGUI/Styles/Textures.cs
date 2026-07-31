using System.Collections.Generic;
using UnityEngine;

namespace WeaverNet.Mod.WeaverGUI.Styles
{
    public static partial class WeaverNetStyles
    {
        private static readonly Dictionary<Color, Texture2D> _texCache = new Dictionary<Color, Texture2D>();
        private static readonly Dictionary<(int size, int border, Color borderColor, Color fillColor), Texture2D> _borderedTexCache
            = new Dictionary<(int, int, Color, Color), Texture2D>();
        private static readonly Dictionary<(int Border, int Radius, Color BorderColor, Color FillColor),Texture2D> _roundedBorderedTexCache
            = new Dictionary<(int, int, Color, Color),Texture2D>();

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
        private static Texture2D MakeRoundedBorderedTex(
            int border,
            int radius,
            Color borderColor,
            Color fillColor)
        {
            int size = radius * 2 + 1 + border * 2;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];

            Rect outer = new Rect(0, 0, size, size);
            Rect inner = new Rect(
                border,
                border,
                size - border * 2,
                size - border * 2);

            int innerRadius = Mathf.Max(0, radius - border);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;

                    // 1. Calculate continuous distance-based coverage (0.0 to 1.0)
                    float outerAlpha = GetRoundedRectAlpha(px, py, outer, radius);
                    float innerAlpha = GetRoundedRectAlpha(px, py, inner, innerRadius);

                    // 2. Blend inner fill over border color
                    Color color = Color.Lerp(borderColor, fillColor, innerAlpha);

                    // 3. Apply outer edge anti-aliasing transparency
                    color.a *= outerAlpha;

                    pixels[y * size + x] = color;
                }
            }

            tex.SetPixels(pixels);
            tex.wrapMode = TextureWrapMode.Clamp;

            // Switch to Bilinear filtering so GPU smoothly renders subpixel edges
            tex.filterMode = FilterMode.Bilinear;

            tex.Apply(false, true);

            return tex;
        }

        /// <summary>
        /// Returns continuous coverage alpha [0..1] for a point near a rounded rectangle boundary.
        /// Uses a Signed Distance Field (SDF) smoothed over 1 pixel unit.
        /// </summary>
        private static float GetRoundedRectAlpha(float x, float y, Rect rect, float radius)
        {
            radius = Mathf.Min(radius, rect.width / 2f, rect.height / 2f);

            // Compute distance vector from the inner corner anchor
            float cx = Mathf.Clamp(x, rect.xMin + radius, rect.xMax - radius);
            float cy = Mathf.Clamp(y, rect.yMin + radius, rect.yMax - radius);

            float dx = x - cx;
            float dy = y - cy;

            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            // Signed distance: positive outside, negative inside
            float sd = dist - radius;

            // Smooth over a 1.0 pixel band (from -0.5 to +0.5 distance from boundary)
            return Mathf.Clamp01(0.5f - sd);
        }

        private static bool IsInsideRoundedRect(
            float x,
            float y,
            Rect rect,
            float radius)
        {
            // Ensure radius doesn't exceed half the width/height
            radius = Mathf.Min(radius, rect.width / 2f, rect.height / 2f);

            // If point is outside the bounding box entirely, early exit
            if (x < rect.xMin || x > rect.xMax || y < rect.yMin || y > rect.yMax)
                return false;

            // Clamp the point to the inner region (excluding corner radii zones)
            float cx = Mathf.Clamp(x, rect.xMin + radius, rect.xMax - radius);
            float cy = Mathf.Clamp(y, rect.yMin + radius, rect.yMax - radius);

            float dx = x - cx;
            float dy = y - cy;

            // Check if the distance to the nearest inner core point is within radius
            return (dx * dx + dy * dy) <= (radius * radius);
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
        private static Texture2D RoundedBorderedTex(
            int border,
            int radius,
            Color borderColor,
            Color fillColor)
        {
            var key = (border, radius, borderColor, fillColor);

            if (_roundedBorderedTexCache.TryGetValue(key, out var tex))
                return tex;

            tex = MakeRoundedBorderedTex(
                border,
                radius,
                borderColor,
                fillColor);

            _roundedBorderedTexCache.Add(key, tex);

            return tex;
        }
        private static Texture2D RoundedBorderedTex(
            Color borderColor,
            Color fillColor)
        {
            return RoundedBorderedTex(
                border: 1,
                radius: 6,
                borderColor,
                fillColor);
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


        private static Texture2D _testNineSliceTexture;
    }
}
