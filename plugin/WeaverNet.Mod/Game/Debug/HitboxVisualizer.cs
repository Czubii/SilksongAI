using UnityEngine;
using UnityEngine.Rendering;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Mod.Game.Debug
{
    public class HitboxVisualizer : MonoBehaviour//TODO finish this 
    {
        public Color color = Color.green;

        private Material lineMaterial;

        void Awake()
        {
            enabled = false;
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                PluginLog.Error("Couldn't find Hidden/Internal-Colored shader.");
                enabled = false;
                return;
            }

            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;

            lineMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
            lineMaterial.SetInt("_ZTest", (int)CompareFunction.Always);
        }

        void OnRenderObject()
        {
            Camera cam = Camera.current;
            if (cam != Camera.main)
                return;

            Collider2D[] colliders = FindObjectsOfType<Collider2D>();

            lineMaterial.SetPass(0);

            GL.PushMatrix();

            GL.LoadProjectionMatrix(cam.projectionMatrix);
            GL.modelview = cam.worldToCameraMatrix;

            GL.Begin(GL.LINES);
            GL.Color(color);

            foreach (Collider2D col in colliders)
            {
                if (!col.enabled || !col.gameObject.activeInHierarchy)
                    continue;

                DrawBounds(col.bounds);
            }

            GL.End();
            GL.PopMatrix();
        }

        void DrawBounds(Bounds b)
        {
            float z = b.center.z;

            Vector3 bl = new Vector3(b.min.x, b.min.y, z);
            Vector3 br = new Vector3(b.max.x, b.min.y, z);
            Vector3 tr = new Vector3(b.max.x, b.max.y, z);
            Vector3 tl = new Vector3(b.min.x, b.max.y, z);

            GL.Vertex(bl); GL.Vertex(br);
            GL.Vertex(br); GL.Vertex(tr);
            GL.Vertex(tr); GL.Vertex(tl);
            GL.Vertex(tl); GL.Vertex(bl);
        }

        void OnDestroy()
        {
            if (lineMaterial != null)
                Destroy(lineMaterial);
        }
    }
}