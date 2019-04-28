using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
namespace UIExt
{
    public class Polygon : MaskableGraphic
    {
        public float radius;
        public float maxValue;
        public float[] abilityValues;
        public Texture texture;
        //public new Material material;

        public Vector3[] GetVerticies()
        {
            float deltaDegree = 2 * Mathf.PI / abilityValues.Length;
            float rate = radius / maxValue;

            Vector3[] verticies = new Vector3[abilityValues.Length];
            for (int i = 0; i < abilityValues.Length; ++i)
            {
                verticies[i] = new Vector3(rate * abilityValues[i] * Mathf.Cos(deltaDegree * i), rate * abilityValues[i] * Mathf.Sin(deltaDegree * i), 0);
            }
            return verticies;
        }

#if UNITY_5 || UNITY_5_3_OR_NEWER
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            List<UIVertex> output = new List<UIVertex>();
            ApplyPolygon(output);
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
            ApplyPolygon(vbo);
        }
#endif

        public void ApplyPolygon(List<UIVertex> vbo)
        {
            if (abilityValues == null || abilityValues.Length < 2)
            {
                //if(GLog.IsLogErrorEnabled) GLog.LogError("Count Error!");
                return;
            }
            float deltaDegree = 2 * Mathf.PI / abilityValues.Length;
            float rate = radius / maxValue;

            float curDegree = 0.0f;

            UIVertex vert = UIVertex.simpleVert;
            vert.color = color;
            UIVertex center = UIVertex.simpleVert;
            center.position = Vector2.zero;
            center.uv0 = new Vector2(0.5f, 0.5f);
            center.color = color;

            UIVertex vt0 = center;
            UIVertex vt1 = vert;
            UIVertex vt2 = vert;
            UIVertex vt3 = vert;

            for (int i = 0; i < abilityValues.Length; i += 2)
            {
                vt1.position = new Vector2(rate * abilityValues[i] * Mathf.Cos(curDegree), rate * abilityValues[i] * Mathf.Sin(curDegree));
                if (texture != null)
                    vt1.uv0 = new Vector2(vt1.position.x / texture.width + 0.5f, vt1.position.y / texture.height + 0.5f);

                vt2.position = new Vector2(rate * abilityValues[(i + 1) % abilityValues.Length] * Mathf.Cos(curDegree + deltaDegree), rate * abilityValues[(i + 1) % abilityValues.Length] * Mathf.Sin(curDegree + deltaDegree));
                if (texture != null)
                    vt2.uv0 = new Vector2(vt2.position.x / texture.width + 0.5f, vt2.position.y / texture.height + 0.5f);

                bool lastTriangleOverflow = false;
                if (i + 2 > abilityValues.Length)
                {
                    lastTriangleOverflow = true;
                    vt3 = center;
                }
                else
                {
                    vt3.position = new Vector2(rate * abilityValues[(i + 2) % abilityValues.Length] * Mathf.Cos(curDegree + deltaDegree * 2), rate * abilityValues[(i + 2) % abilityValues.Length] * Mathf.Sin(curDegree + deltaDegree * 2));
                    if (texture != null)
                        vt3.uv0 = new Vector2(vt3.position.x / texture.width + 0.5f, vt3.position.y / texture.height + 0.5f);
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

                curDegree += deltaDegree * 2;
            }

        }

    }
}