using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(Text))]
#if UNITY_5 || UNITY_5_3_OR_NEWER
    public class CurveText : BaseMeshEffect
    {
        public static int elementStep = 6;
#else
    public class CurveText : BaseVertexEffect
    {
        public static int elementStep = 4;
#endif
        public AnimationCurve curveShape = AnimationCurve.Linear(0, 0, 1, 1);
        public bool isMatchSlope = true;
        public float sampleInterval = 0.01f;

        private Text m_text;
        public Text text
        {
            get
            {
                if (m_text == null)
                {
                    m_text = this.GetComponent<Text>();
                }
                return m_text;
            }
        }

        private RectTransform m_rectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (m_rectTransform == null)
                {
                    m_rectTransform = this.GetComponent<RectTransform>();
                }
                return m_rectTransform;
            }
        }


#if UNITY_5 || UNITY_5_3_OR_NEWER
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            List<UIVertex> output = new List<UIVertex>();
            vh.GetUIVertexStream(output);
            ApplyCurveText(output);
            vh.Clear();
            vh.AddUIVertexTriangleStream(output);
            output.Clear();
        }
#else
        public override void ModifyVertices(List<UIVertex> vh)
        {
            int count = vh.Count;
            if (!IsActive() || count == 0)
            {
                return;
            }

            ApplyCurveText(vh);
        }
#endif

        public void ApplyCurveText(List<UIVertex> verts)
        {
            if (verts.Count <= 0) return;

            Vector3 baseLine = Vector3.zero;
            {
                UIVertex vt0 = verts[0];
                UIVertex vt1 = verts[1];
                UIVertex vt2 = verts[2];
#if UNITY_5 || UNITY_5_3_OR_NEWER
                UIVertex vt3 = verts[4];
#else
                UIVertex vt3 = verts[3];
#endif
                baseLine = (vt3.position + vt2.position) / 2;
            }

            int count = verts.Count;
            for (int i = 0; i < count; )
            {
                UIVertex vt0 = verts[i + 0];
                UIVertex vt1 = verts[i + 1];
                UIVertex vt2 = verts[i + 2];
#if UNITY_5 || UNITY_5_3_OR_NEWER
                UIVertex vt3 = verts[i + 4];
#else
                UIVertex vt3 = verts[i + 3];
#endif

                // 一个文字所在矩形的中心点
                Vector3 centerPos = (vt0.position + vt2.position) / 2;
                float yOffset = centerPos.y - baseLine.y;

                float evalutePosY = this.curveShape.Evaluate(centerPos.x / this.rectTransform.rect.width) * this.rectTransform.rect.height;
                evalutePosY += yOffset;

                if (this.isMatchSlope)
                {
                    Vector3 newCenterPos = new Vector3(centerPos.x, evalutePosY, centerPos.z);
                    float preSampleX = (centerPos.x - this.sampleInterval);
                    float preSampleY = this.curveShape.Evaluate(preSampleX / this.rectTransform.rect.width) * this.rectTransform.rect.height;
                    float nextSampleX = (centerPos.x + this.sampleInterval);
                    float nextSampleY = this.curveShape.Evaluate(nextSampleX / this.rectTransform.rect.width) * this.rectTransform.rect.height;
                    Vector3 slopeVector = new Vector3(nextSampleX - preSampleX, nextSampleY - preSampleY).normalized;
                    Vector3 originVector = (vt1.position - vt0.position).normalized;

                    Vector3 angleCross = Vector3.Cross(originVector, slopeVector);
                    float angle = Vector3.Angle(originVector, slopeVector);
                    angle = angleCross.z > 0 ? angle : -angle;

                    // 计算匹配当前点切线方向需要三个矩阵（平移到原点->旋转->平移到新的位置）
                    Matrix4x4 m1 = Matrix4x4.identity;
                    m1.SetTRS(-centerPos, Quaternion.identity, Vector3.one);
                    Matrix4x4 m2 = Matrix4x4.identity;
                    m2.SetTRS(Vector3.zero, Quaternion.Euler(0, 0, angle), Vector3.one);
                    Matrix4x4 m3 = Matrix4x4.identity;
                    m3.SetTRS(newCenterPos, Quaternion.identity, Vector3.one);

                    // !!! 倒序组合变换矩阵
                    Matrix4x4 finalMatrix = m3 * m2 * m1;
                    vt0.position = finalMatrix.MultiplyPoint3x4(vt0.position);
                    vt1.position = finalMatrix.MultiplyPoint3x4(vt1.position);
                    vt2.position = finalMatrix.MultiplyPoint3x4(vt2.position);
                    vt3.position = finalMatrix.MultiplyPoint3x4(vt3.position);
                }
                else
                {
                    float yDiff = evalutePosY - centerPos.y;

                    vt0.position.y += yDiff;
                    vt1.position.y += yDiff;
                    vt2.position.y += yDiff;
                    vt3.position.y += yDiff;
                }

                verts[i + 0] = vt0;
                verts[i + 1] = vt1;
                verts[i + 2] = vt2;
#if UNITY_5 || UNITY_5_3_OR_NEWER
                verts[i + 3] = vt2;
                verts[i + 4] = vt3;
                verts[i + 5] = vt0;
#else
                verts[i + 3] = vt3;
#endif

                i += elementStep;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // 设置Text组件以及RectTransform
            this.text.alignment = TextAnchor.MiddleCenter;
            this.rectTransform.pivot = new Vector2(0, 0);
        }
    }
}
