using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
namespace UIExt
{
    [XLua.LuaCallCSharp]
    public class PolygonCustom : MaskableGraphic
    {
        public RectTransform originPoint;
        public RectTransform[] customVectex;

        public float[] abilityValues;
        public float maxValue;
        public Texture texture;
        public Vector2[] indexPositionArray;

        public bool isUseAA = true;
        private Texture2D borderTex;
        private UIVertex[] borderUIVertexs;
        private CanvasRenderer borderCanvasRenderer;

        protected override void Start()
        {
            if (Application.isPlaying && isUseAA)
            {
                borderTex = new Texture2D(1, 3, TextureFormat.RGBA32, false);
                borderTex.wrapMode = TextureWrapMode.Clamp;
                borderTex.SetPixel(0, 0, new Color(0, 0, 0, 0));
                borderTex.SetPixel(0, 1, new Color(1, 1, 1, 1));
                borderTex.SetPixel(0, 2, new Color(0, 0, 0, 0));
                borderTex.Apply();

                Material defaultMaterial = new Material(Shader.Find("UI/Default"));
                GameObject lineObj = new GameObject("borderLine");
                borderCanvasRenderer = lineObj.AddComponent<CanvasRenderer>();
                borderCanvasRenderer.SetMaterial(defaultMaterial, borderTex);
                RectTransform rectTrans = lineObj.AddComponent<RectTransform>();
                rectTrans.offsetMin = Vector2.zero;
                rectTrans.offsetMax = Vector2.zero;
                rectTrans.anchorMin = Vector2.one * 0.5f;
                rectTrans.anchorMax = Vector2.one * 0.5f;
                rectTrans.pivot = Vector2.zero;
                rectTrans.anchoredPosition = Vector2.zero;
                lineObj.transform.SetParent(transform, false);

                borderUIVertexs = new UIVertex[abilityValues.Length * 4];
                for (int idx = 0; idx < abilityValues.Length * 4; idx += 4)
                {
                    borderUIVertexs[idx].uv0 = new Vector2(0.0f, 1.0f);
                    borderUIVertexs[idx + 3].uv0 = new Vector2(0.0f, 0.0f);
                    borderUIVertexs[idx + 2].uv0 = new Vector2(1.0f, 0.0f);
                    borderUIVertexs[idx + 1].uv0 = new Vector2(1.0f, 1.0f);

                    borderUIVertexs[idx].color = (Color32)Color.white;
                    borderUIVertexs[idx + 3].color = (Color32)Color.white;
                    borderUIVertexs[idx + 2].color = (Color32)Color.white;
                    borderUIVertexs[idx + 1].color = (Color32)Color.white;
                }
            }
        }

#if UNITY_5 || UNITY_5_3_OR_NEWER
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            List<UIVertex> output = new List<UIVertex>();
            ApplyPolygonCustom(output);
            vh.Clear();
            vh.AddUIVertexTriangleStream(output);
            output.Clear();
        }
#else
        protected override void UpdateMaterial()
        {
            if (IsActive())
                canvasRenderer.SetMaterial(material, texture);
        }

        protected override void OnFillVBO(List<UIVertex> vbo)
        {
            ApplyPolygonCustom(vbo);
        }
#endif

        public void ApplyPolygonCustom(List<UIVertex> vbo)
        {
            if (customVectex == null || customVectex.Length < 2) return;
            if (abilityValues == null || abilityValues.Length < 2) return;
            if (customVectex.Length != abilityValues.Length) return;

            UIVertex vert = UIVertex.simpleVert;
            vert.color = (Color32)color;
            UIVertex center = UIVertex.simpleVert;
            center.position = Vector2.zero;
            center.uv0 = new Vector2(0.5f, 0.5f);
            center.color = (Color32)color;

            UIVertex vt0 = center;
            UIVertex vt1 = vert;
            UIVertex vt2 = vert;
            UIVertex vt3 = vert;

            if (indexPositionArray.Length != abilityValues.Length)
            {
                indexPositionArray = new Vector2[abilityValues.Length];
            }

            for (int i = 0; i < abilityValues.Length; i += 2)
            {
                int tmpIndex = i;

                Vector2 normal = customVectex[tmpIndex].anchoredPosition - originPoint.anchoredPosition;
                float tmpRadius = normal.magnitude;
                vert.position = originPoint.anchoredPosition + normal.normalized * abilityValues[tmpIndex] * tmpRadius / maxValue;
                vt1 = vert;
                indexPositionArray[tmpIndex] = vert.position;

                tmpIndex = (i + 1) % abilityValues.Length;
                normal = customVectex[tmpIndex].anchoredPosition - originPoint.anchoredPosition;
                tmpRadius = normal.magnitude;
                vert.position = originPoint.anchoredPosition + normal.normalized * abilityValues[tmpIndex] * tmpRadius / maxValue;
                vt2 = vert;
                indexPositionArray[tmpIndex] = vert.position;

                bool lastTriangleOverflow = false;
                if (i + 2 > abilityValues.Length)
                {
                    lastTriangleOverflow = true;
                    vt3 = center;
                }
                else
                {
                    tmpIndex = (i + 2) % abilityValues.Length;
                    normal = customVectex[tmpIndex].anchoredPosition - originPoint.anchoredPosition;
                    tmpRadius = normal.magnitude;
                    vert.position = originPoint.anchoredPosition + normal.normalized * abilityValues[tmpIndex] * tmpRadius / maxValue;
                    vt3 = vert;
                    indexPositionArray[tmpIndex] = vert.position;
                }

                vbo.Add(vt0);
                vbo.Add(vt1);
                vbo.Add(vt2);
#if UNITY_5 || UNITY_5_3_OR_NEWER
                if (!lastTriangleOverflow)
                {
                    vbo.Add(vt2);
                    vbo.Add(vt3);
                    vbo.Add(vt0);
                }
#else
                vbo.Add(vt3);
#endif
            }

            if (Application.isPlaying && isUseAA)
            {
                int idx = 0;
                for (int i = 0; i < indexPositionArray.Length; i++)
                {
                    var p1 = Vector3.zero;
                    var p2 = Vector3.zero;
                    var v1 = Vector3.zero;
                    var px = Vector3.zero;

                    p1.x = indexPositionArray[i].x;
                    p1.y = indexPositionArray[i].y;
                    p2.x = indexPositionArray[(i + 1) % indexPositionArray.Length].x;
                    p2.y = indexPositionArray[(i + 1) % indexPositionArray.Length].y;
                    px.x = p2.y - p1.y; px.y = p1.x - p2.x;
                    float normalizedDistance = (1.0f / (float)System.Math.Sqrt((px.x * px.x) + (px.y * px.y)));
                    px *= normalizedDistance * borderTex.height / 2;

                    borderUIVertexs[idx].position.x = p1.x - px.x;
                    borderUIVertexs[idx].position.y = p1.y - px.y;
                    borderUIVertexs[idx + 3].position.x = p1.x + px.x;
                    borderUIVertexs[idx + 3].position.y = p1.y + px.y;
                    borderUIVertexs[idx + 2].position.x = p2.x + px.x;
                    borderUIVertexs[idx + 2].position.y = p2.y + px.y;
                    borderUIVertexs[idx + 1].position.x = p2.x - px.x;
                    borderUIVertexs[idx + 1].position.y = p2.y - px.y;
                    idx += 4;
                }

                borderCanvasRenderer.SetColor(color);
#if UNITY_5 || UNITY_5_3_OR_NEWER
                Mesh mesh = new Mesh();
                List<Vector3> vertices = new List<Vector3>();
                List<int> triangles = new List<int>();
                List<Color32> colors = new List<Color32>();
                List<Vector2> uvs = new List<Vector2>();
                for (int i = 0; i < abilityValues.Length; i++)
                {
                    vt0 = borderUIVertexs[i * 4 + 0];
                    vt1 = borderUIVertexs[i * 4 + 1];
                    vt2 = borderUIVertexs[i * 4 + 2];
                    vt3 = borderUIVertexs[i * 4 + 3];
                    // position
                    vertices.Add(vt0.position);
                    vertices.Add(vt1.position);
                    vertices.Add(vt2.position);
                    vertices.Add(vt3.position);
                    // triangle
                    triangles.Add(i * 4 + 0);
                    triangles.Add(i * 4 + 1);
                    triangles.Add(i * 4 + 2);
                    triangles.Add(i * 4 + 2);
                    triangles.Add(i * 4 + 3);
                    triangles.Add(i * 4 + 0);
                    // colors
                    colors.Add(vt0.color);
                    colors.Add(vt1.color);
                    colors.Add(vt2.color);
                    colors.Add(vt3.color);
                    // uvs
                    uvs.Add(vt0.uv0);
                    uvs.Add(vt1.uv0);
                    uvs.Add(vt2.uv0);
                    uvs.Add(vt3.uv0);
                }

                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();
                mesh.RecalculateNormals();
                // 必须设置color32，设置color是无效的
                mesh.colors32 = colors.ToArray();
                mesh.uv = uvs.ToArray();
                borderCanvasRenderer.SetMesh(mesh);
#else
                borderCanvasRenderer.SetVertices(borderUIVertexs, abilityValues.Length * 4);
#endif
            }
        }

    }
}