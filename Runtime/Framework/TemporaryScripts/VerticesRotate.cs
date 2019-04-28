using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIExt
{
#if UNITY_5 || UNITY_5_3_OR_NEWER
    public class VerticesRotate : BaseMeshEffect
#else
    public class VerticesRotate : MonoBehaviour, IVertexModifier
#endif
    {
        [SerializeField]
        private float m_Progress;
        public float progress
        {
            get { return m_Progress; }
            set
            {
                m_Progress = value;
                if (m_Progress < 0.5)
                {
                    if (image.overrideSprite == null || !image.overrideSprite.name.Equals(image1.overrideSprite.name))
                    {
                        image.overrideSprite = image1.overrideSprite;
                    }
                }
                else
                {
                    if (image.overrideSprite == null || !image.overrideSprite.name.Equals(image2.overrideSprite.name))
                    {
                        image.overrideSprite = image2.overrideSprite;
                    }
                }
                SetVerticesDirty();
            }
        }
        [SerializeField]
        private float m_WiderParam;
        public float widerParam { get { return m_WiderParam; } set { m_WiderParam = value; SetVerticesDirty(); } }
        [SerializeField]
        private float m_PivotHeight;
        public float pivotHeight { get { return m_PivotHeight; } set { m_PivotHeight = value; SetVerticesDirty(); } }
        private Image m_Image;
        private Image image
        {
            get
            {
                if (m_Image == null) m_Image = gameObject.AddComponent<Image>();
                return m_Image;
            }
        }
        public Image image1;
        public Image image2;
        private RectTransform _rt;
        private RectTransform rt
        {
            get
            {
                if (_rt == null) _rt = GetComponent<RectTransform>();
                return _rt;
            }
        }
        private void SetVerticesDirty()
        {
            if (image != null) image.SetVerticesDirty();
        }

#if UNITY_5 || UNITY_5_3_OR_NEWER
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            List<UIVertex> output = new List<UIVertex>();
            vh.GetUIVertexStream(output);
            ApplyVerticesRotate(output);
            vh.Clear();
            vh.AddUIVertexTriangleStream(output);
            output.Clear();
        }
#endif

        public void ModifyVertices(List<UIVertex> verts)
        {
            ApplyVerticesRotate(verts);
        }

        private UIVertex[] tmpVert = new UIVertex[4];
        private void ApplyVerticesRotate(List<UIVertex> verts)
        {
            if (image != null)
            {
                var overrideSprite = image.overrideSprite;
                if (overrideSprite)
                {
                    var outter = UnityEngine.Sprites.DataUtility.GetOuterUV(overrideSprite);
                    if (m_Progress < 0.5)
                    {
                        tmpVert[0] = verts[0];
                        tmpVert[0].position = new Vector2(-rt.rect.width / 2, rt.rect.height * m_PivotHeight - rt.rect.height / 2);
                        tmpVert[0].uv0 = new Vector2(tmpVert[0].uv0.x, outter.y * (1 - m_PivotHeight) + outter.w * m_PivotHeight);
                        tmpVert[0].uv1 = new Vector2(0, m_PivotHeight);

                        tmpVert[1] = verts[1];
                        tmpVert[1].position = new Vector2(-rt.rect.width / 2 - m_WiderParam * m_Progress * rt.rect.width, rt.rect.height / 2 - m_Progress * rt.rect.height * (1 - m_PivotHeight) * 2);
                        tmpVert[1].uv1 = new Vector2(0, 1);

                        tmpVert[2] = verts[2];
                        tmpVert[2].position = new Vector2(rt.rect.width / 2 + m_WiderParam * m_Progress * rt.rect.width, rt.rect.height / 2 - m_Progress * rt.rect.height * (1 - m_PivotHeight) * 2);
                        tmpVert[2].uv1 = new Vector2(1, 1);
#if UNITY_5 || UNITY_5_3_OR_NEWER
                        tmpVert[3] = verts[4];
#else
                        tmpVert[3] = verts[3];
#endif
                        tmpVert[3].position = new Vector2(rt.rect.width / 2, rt.rect.height * m_PivotHeight - rt.rect.height / 2);
                        tmpVert[3].uv0 = new Vector2(tmpVert[3].uv0.x, outter.y * (1 - m_PivotHeight) + outter.w * m_PivotHeight);
                        tmpVert[3].uv1 = new Vector2(1, m_PivotHeight);
                    }
                    else
                    {
                        tmpVert[0] = verts[0];
                        tmpVert[0].position = new Vector2(-rt.rect.width / 2 - m_WiderParam * (1 - m_Progress) * rt.rect.width, 2 * m_PivotHeight * rt.rect.height * (1 - m_Progress) - rt.rect.height / 2);
                        tmpVert[0].uv1 = new Vector2(0, 0);

                        tmpVert[1] = verts[1];
                        tmpVert[1].position = new Vector2(-rt.rect.width / 2, rt.rect.height * m_PivotHeight - rt.rect.height / 2);
                        tmpVert[1].uv0 = new Vector2(tmpVert[1].uv0.x, outter.y * (1 - m_PivotHeight) + outter.w * m_PivotHeight);
                        tmpVert[1].uv1 = new Vector2(0, m_PivotHeight);

                        tmpVert[2] = verts[2];
                        tmpVert[2].position = new Vector2(rt.rect.width / 2, rt.rect.height * m_PivotHeight - rt.rect.height / 2);
                        tmpVert[2].uv0 = new Vector2(tmpVert[2].uv0.x, outter.y * (1 - m_PivotHeight) + outter.w * m_PivotHeight);
                        tmpVert[2].uv1 = new Vector2(1, m_PivotHeight);
#if UNITY_5 || UNITY_5_3_OR_NEWER
                        tmpVert[3] = verts[4];
#else
                        tmpVert[3] = verts[3];
#endif
                        tmpVert[3].position = new Vector2(rt.rect.width / 2 + m_WiderParam * (1 - m_Progress) * rt.rect.width, 2 * m_PivotHeight * rt.rect.height * (1 - m_Progress) - rt.rect.height / 2);
                        tmpVert[3].uv1 = new Vector2(1, 0);
                    }

                    verts[0] = tmpVert[0];
                    verts[1] = tmpVert[1];
                    verts[2] = tmpVert[2];
#if UNITY_5 || UNITY_5_3_OR_NEWER
                    verts[3] = tmpVert[2];
                    verts[4] = tmpVert[3];
                    verts[5] = tmpVert[0];
#else
                    verts[3] = tmpVert[3];
#endif
                }
            }
        }
    }
}
