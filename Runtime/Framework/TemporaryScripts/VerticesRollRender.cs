using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
namespace UIExt
{
    public class VerticesRollRender : MaskableGraphic
    {
        [SerializeField]
        private float m_Progress;
        public float progress { get { return m_Progress; } set { m_Progress = value; SetAllDirty(); } }
        [SerializeField]
        private float m_WiderParam;
        public float widerParam { get { return m_WiderParam; } set { m_WiderParam = value; } }
        [SerializeField]
        private float m_PivotHeight;
        public float pivotHeight { get { return m_PivotHeight; } set { m_PivotHeight = value; } }
        public Image image1;
        public Image image2;

        private RectTransform rt;

        protected override void Awake()
        {
            rt = transform as RectTransform;
        }

        protected override void UpdateMaterial()
        {
            if (IsActive())
            {
                if (m_Progress < 0.5)
                {
                    canvasRenderer.SetMaterial(material, image1.mainTexture);
                }
                else
                {
                    canvasRenderer.SetMaterial(material, image2.mainTexture);
                }
            }
        }

        protected override void OnFillVBO(List<UIVertex> vbo)
        {
            if (m_Progress < 0.5)
            {
                UIVertex vert = UIVertex.simpleVert;
                vert.color = color;
                vert.position = new Vector2(-rt.rect.width / 2 - m_WiderParam * m_Progress * rt.rect.width, rt.rect.height / 2 - m_Progress * rt.rect.height * (1 - m_PivotHeight) * 2);
                vert.uv0 = new Vector2(0, 1);
                vbo.Add(vert);

                vert.color = color;
                vert.position = new Vector2(rt.rect.width / 2 + m_WiderParam * m_Progress * rt.rect.width, rt.rect.height / 2 - m_Progress * rt.rect.height * (1 - m_PivotHeight) * 2);
                vert.uv0 = new Vector2(1, 1);
                vbo.Add(vert);

                vert.color = color;
                vert.position = new Vector2(rt.rect.width / 2, rt.rect.height * m_PivotHeight - rt.rect.height / 2);
                vert.uv0 = new Vector2(1, m_PivotHeight);
                vbo.Add(vert);

                vert.color = color;
                vert.position = new Vector2(-rt.rect.width / 2, rt.rect.height * m_PivotHeight - rt.rect.height / 2);
                vert.uv0 = new Vector2(0, m_PivotHeight);
                vbo.Add(vert);
            }
            else
            {
                UIVertex vert = UIVertex.simpleVert;
                vert.color = color;
                vert.position = new Vector2(-rt.rect.width / 2, rt.rect.height * m_PivotHeight - rt.rect.height / 2);
                vert.uv0 = new Vector2(0, m_PivotHeight);
                vbo.Add(vert);

                vert.color = color;
                vert.position = new Vector2(rt.rect.width / 2, rt.rect.height * m_PivotHeight - rt.rect.height / 2);
                vert.uv0 = new Vector2(1, m_PivotHeight);
                vbo.Add(vert);

                vert.color = color;
                vert.position = new Vector2(rt.rect.width / 2 + m_WiderParam * (1 - m_Progress) * rt.rect.width, 2 * m_PivotHeight * rt.rect.height * (1 - progress) - rt.rect.height / 2);
                vert.uv0 = new Vector2(1, 0);
                vbo.Add(vert);

                vert.color = color;
                vert.position = new Vector2(-rt.rect.width / 2 - m_WiderParam * (1 - m_Progress) * rt.rect.width, 2 * m_PivotHeight * rt.rect.height * (1 - progress) - rt.rect.height / 2);
                vert.uv0 = new Vector2(0, 0);
                vbo.Add(vert);
            }
        }

    }
}