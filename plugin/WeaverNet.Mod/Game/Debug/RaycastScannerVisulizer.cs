using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using WeaverNet.Core.DataCollection;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Mod.DataCollection;

namespace WeaverNet.Mod.Game.Debug
{
    public class RaycastScannerVisualizer : MonoBehaviour
    {
        public Color hitColor = Color.green;
        public Color missColor = Color.red;
        public Color normalColor = Color.yellow;
        public float normalLength = 0.2f;

        private IRaycastScanner _scanner;
        private Material lineMaterial;

        public int Mask { get; private set; }
        public void Initialize(IRaycastScanner scanner)
        {
            Mask = (int)HitboxLayers.Terrain;
            _scanner = scanner;
            enabled = true;
        }

        public void SetMask(HitboxLayers mask)
        {
            Mask = (int)mask;
        }
        public void SetMask(int mask)
        {
            Mask = mask;
        }

        void Awake()
        {
            enabled = false;

            Shader shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                PluginLog.Error("Couldn't find Hidden/Internal-Colored shader.");
                return;
            }

            lineMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            lineMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
            lineMaterial.SetInt("_ZTest", (int)CompareFunction.Always);
        }

        void OnRenderObject()
        {
            if (_scanner == null)
                return;

            Camera cam = Camera.current;
            if (cam != Camera.main)
                return;

            if (!_scanner.TryScan(Mask, out var rays) || rays == null)
                return;

            lineMaterial.SetPass(0);

            GL.PushMatrix();
            GL.LoadProjectionMatrix(cam.projectionMatrix);
            GL.modelview = cam.worldToCameraMatrix;

            GL.Begin(GL.LINES);

            foreach (RayData ray in rays)
            {
                if (ray == null)
                    continue;

                DrawRay(ray);
            }

            GL.End();
            GL.PopMatrix();
        }

        private void DrawRay(RayData ray)
        {
            if (ray.Hit)
            {
                // Line from Origin to Hit Point
                GL.Color(hitColor);
                GL.Vertex(ray.Origin);
                GL.Vertex(ray.Point);

                // Normal vector indicator at hit surface
                GL.Color(normalColor);
                GL.Vertex(ray.Point);
                GL.Vertex(ray.Point + (ray.Normal * normalLength));
            }
            else
            {
                // Line extending full distance
                GL.Color(missColor);
                GL.Vertex(ray.Origin);
                GL.Vertex(ray.Point);
            }
        }

        void OnDestroy()
        {
            if (lineMaterial != null)
                Destroy(lineMaterial);
        }
    }
}