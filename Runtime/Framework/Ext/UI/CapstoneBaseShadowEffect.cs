using System.Collections.Generic;

namespace UnityEngine.UI
{
    public abstract class CapstoneBaseShadowEffect : BaseMeshEffect
    {
        [SerializeField]
        private Color m_EffectColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        [SerializeField]
        private bool m_UseGraphicAlpha = false;
        protected const float kMaxEffectDistance = 600f;

        protected CapstoneBaseShadowEffect()
        {
        }

        /// <summary>
        ///   <para>Color for the effect.</para>
        /// </summary>
        public Color effectColor
        {
            get
            {
                return this.m_EffectColor;
            }
            set
            {
                this.m_EffectColor = value;
                if (!((Object)this.graphic != (Object)null))
                    return;
                this.graphic.SetVerticesDirty();
            }
        }

        /// <summary>
        ///   <para>Should the shadow inherit the alpha from the graphic?</para>
        /// </summary>
        public bool useGraphicAlpha
        {
            get
            {
                return this.m_UseGraphicAlpha;
            }
            set
            {
                this.m_UseGraphicAlpha = value;
                if (!((Object)this.graphic != (Object)null))
                    return;
                this.graphic.SetVerticesDirty();
            }
        }

        protected void ApplyShadowZeroAlloc(List<UIVertex> verts, Color32 color, int start, int end, float x, float y)
        {
            int num = verts.Count + end - start;

            if (verts.Capacity < num)
                verts.Capacity = num;

            for (int index = start; index < end; ++index)
            {
                UIVertex vert = verts[index];
                verts.Add(vert);
                Vector3 position = vert.position;
                position.x += x;
                position.y += y;
                vert.position = position;

                if (this.useGraphicAlpha)
                {
                    Color32 color32 = color;
                    color32.a = (byte)((int)color32.a * (int)verts[index].color.a / (int)byte.MaxValue);
                    vert.color = color32;
                }
                else
                {
                    vert.color = color;
                }

                verts[index] = vert;
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!this.IsActive())
                return;
            List<UIVertex> uiVertexList = CapstoneListPool<UIVertex>.Get();
            vh.GetUIVertexStream(uiVertexList);
            ModifyMesh(uiVertexList);
            vh.Clear();
            vh.AddUIVertexTriangleStream(uiVertexList);
            CapstoneListPool<UIVertex>.Release(uiVertexList);
        }

        public abstract void ModifyMesh(List<UIVertex> verts);
    }
}