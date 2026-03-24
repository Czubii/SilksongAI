using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AIPlugin.PluginGUI
{
    public class LinePlot
    {
        private Texture2D _plotTexture;
        private int _width;
        private int _height;
        private int _seriesPadding = 5;
        private int _seriesWidth;
        private int _seriesHeight;
        private int _legendEntryHeigth = 20;

        public string Title { get; private set; }

        private Color _bgColor = new Color(0.06f, 0.06f, 0.06f, 1f);

        // Each series has a list of values and a color
        private List<(List<float> values, Color color)> _series = new List<(List<float>, Color)>();

        private List<(Texture2D tex, string name)> _legend = new List<(Texture2D tex, string name)> ();

        public LinePlot(int width = 300, int height = 150, string title = null)
        {
            Title = title;
            _width = width;
            _height = height;
            _seriesWidth = _width - 2 * _seriesPadding;
            _seriesHeight = _height - 2 * _seriesPadding;

            _plotTexture = new Texture2D(_width, _height);
            ClearTexture();
        }
        public void SetSeries(int index, List<float> values, Color color, string name = null)
        {
            while (_series.Count <= index)
                _series.Add((new List<float>(), Color.green));
            while (_legend.Count <= index)
                _legend.Add((new Texture2D(1,1), ""));

            _series[index] = (new List<float>(values), color);

            Texture2D tex = new Texture2D(1,1);
            tex.SetPixel(0, 0, color);
            tex.Apply();

            if (name != null)
                _legend[index] = (tex, name);
            else
                _legend[index] = (tex, $"series {index}");
        }
        public void Update()
        {
            ClearTexture();

            float min = float.MaxValue;
            float max = float.MinValue;
            foreach (var (values, color) in _series)
            {
                min = Math.Min(min, values.Min());
                max = Math.Max(max, values.Max());
            }

            foreach (var (values, color) in _series)
            {
                if (values.Count < 2) continue;
                float range = Mathf.Max(1e-5f, max - min);

                for (int i = 1; i < values.Count; i++)
                {
                    float t0 = (float)(i - 1) / (values.Count - 1);
                    float t1 = (float)i / (values.Count - 1);

                    Vector2 p0 = new Vector2(t0 * _seriesWidth + _seriesPadding, ((values[i - 1] - min) / range) * _seriesHeight + _seriesPadding);
                    Vector2 p1 = new Vector2(t1 * _seriesWidth + _seriesPadding, ((values[i] - min) / range) * _seriesHeight + _seriesPadding);

                    DrawLineOnTexture(_plotTexture, p0, p1, color);
                }
            }

            _plotTexture.Apply();
        }
        public void Draw(bool drawLegend = true, params GUILayoutOption[] options)
        {
            GUILayout.Space(10);

            // Draw title
            if (Title != null)
            {
                var labelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                GUILayout.Label(Title, labelStyle);
            }

            // Draw the plot
            if (_plotTexture == null) return;

            Rect plotRect = GUILayoutUtility.GetRect(_width, _height, options);
            GUI.DrawTexture(plotRect, _plotTexture);

            // Draw legend below the plot
            if (drawLegend && _legend.Count > 0)
            {
                GUILayout.Space(6);

                // Legend container
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace(); // center legend horizontally

                foreach (var (tex, name) in _legend)
                {
 
                    GUILayout.BeginHorizontal(GUILayout.Height(30));

                    // Color box
                    GUILayout.Box(GUIContent.none, new GUIStyle
                    {
                        normal = { background = tex },
                        margin = new RectOffset(2, 2, 9, 9),
                        padding = new RectOffset(0, 0, 0, 0)
                    }, GUILayout.Width(12), GUILayout.Height(12));

                    GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                    labelStyle.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label(name, labelStyle);

                    GUILayout.EndHorizontal();
                    GUILayout.Space(15); 
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
        }

        public void Clear()
        {
            _series.Clear();
            ClearTexture();
        }
        private void ClearTexture()
        {
            Color[] fill = Enumerable.Repeat(_bgColor, _width * _height).ToArray();
            _plotTexture.SetPixels(fill);
            _plotTexture.Apply();
        }
        private void DrawLineOnTexture(Texture2D tex, Vector2 p0, Vector2 p1, Color color)
        {
            bool steep = Mathf.Abs(p1.y - p0.y) > Mathf.Abs(p1.x - p0.x);

            if (steep)
            {
                // swap x and y
                (p0.x, p0.y) = (p0.y, p0.x);
                (p1.x, p1.y) = (p1.y, p1.x);
            }

            if (p0.x > p1.x)
            {
                // swap points to always draw left to right
                (p0, p1) = (p1, p0);
            }

            float dx = p1.x - p0.x;
            float dy = p1.y - p0.y;
            float gradient = dx == 0 ? 1 : dy / dx;

            // handle first endpoint
            int xEnd = Mathf.RoundToInt(p0.x);
            float yEnd = p0.y + gradient * (xEnd - p0.x);
            float xGap = 1f - (p0.x + 0.5f - Mathf.Floor(p0.x + 0.5f));
            int xPixel1 = xEnd;
            int yPixel1 = Mathf.FloorToInt(yEnd);

            if (steep)
            {
                BlendPixel(tex, yPixel1, xPixel1, color, (1 - (yEnd - yPixel1)) * xGap);
                BlendPixel(tex, yPixel1 + 1, xPixel1, color, (yEnd - yPixel1) * xGap);
            }
            else
            {
                BlendPixel(tex, xPixel1, yPixel1, color, (1 - (yEnd - yPixel1)) * xGap);
                BlendPixel(tex, xPixel1, yPixel1 + 1, color, (yEnd - yPixel1) * xGap);
            }

            float intery = yEnd + gradient;

            // handle second endpoint
            xEnd = Mathf.RoundToInt(p1.x);
            yEnd = p1.y + gradient * (xEnd - p1.x);
            xGap = p1.x + 0.5f - Mathf.Floor(p1.x + 0.5f);
            int xPixel2 = xEnd;
            int yPixel2 = Mathf.FloorToInt(yEnd);

            if (steep)
            {
                BlendPixel(tex, yPixel2, xPixel2, color, (1 - (yEnd - yPixel2)) * xGap);
                BlendPixel(tex, yPixel2 + 1, xPixel2, color, (yEnd - yPixel2) * xGap);
            }
            else
            {
                BlendPixel(tex, xPixel2, yPixel2, color, (1 - (yEnd - yPixel2)) * xGap);
                BlendPixel(tex, xPixel2, yPixel2 + 1, color, (yEnd - yPixel2) * xGap);
            }

            // main loop
            if (steep)
            {
                for (int x = xPixel1 + 1; x < xPixel2; x++)
                {
                    int y = Mathf.FloorToInt(intery);
                    BlendPixel(tex, y, x, color, 1 - (intery - y));
                    BlendPixel(tex, y + 1, x, color, intery - y);
                    intery += gradient;
                }
            }
            else
            {
                for (int x = xPixel1 + 1; x < xPixel2; x++)
                {
                    int y = Mathf.FloorToInt(intery);
                    BlendPixel(tex, x, y, color, 1 - (intery - y));
                    BlendPixel(tex, x, y + 1, color, intery - y);
                    intery += gradient;
                }
            }
        }

        private void BlendPixel(Texture2D tex, int x, int y, Color col, float alpha)
        {
            if (x < 0 || x >= tex.width || y < 0 || y >= tex.height) return;

            Color bg = tex.GetPixel(x, y);
            Color blended = Color.Lerp(bg, col, Mathf.Clamp01(alpha));
            tex.SetPixel(x, y, blended);
        }
    }
}
