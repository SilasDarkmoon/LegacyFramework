using System.Collections.Generic;

namespace UnityEngine.UI
{
    [XLua.LuaCallCSharp]
    [AddComponentMenu("CapstoneUI/Effects/CapstoneOutline", 15)]
    public class CapstoneOutline : CapstoneBaseShadowEffect
    {
        /// <summary>
        /// X=left, Y=bottom, Z=right, W=top.
        /// </summary>
        [SerializeField]
        private Vector4 m_EffectDistance = new Vector4(-1f, -1f, 1f, 1f);

        protected CapstoneOutline()
        {
        }

        //protected override void OnValidate()
        //{
        //    this.effectDistance = this.m_EffectDistance;
        //    base.OnValidate();
        //}

        /// <summary>
        ///   <para>How far is the shadow from the graphic.</para>
        /// </summary>
        public Vector4 effectDistance
        {
            get
            {
                return this.m_EffectDistance;
            }
            set
            {
                if ((double)value.x > kMaxEffectDistance)
                    value.x = kMaxEffectDistance;
                if ((double)value.x < -kMaxEffectDistance)
                    value.x = -kMaxEffectDistance;
                if ((double)value.y > kMaxEffectDistance)
                    value.y = kMaxEffectDistance;
                if ((double)value.y < -kMaxEffectDistance)
                    value.y = -kMaxEffectDistance;
                if ((double)value.z > kMaxEffectDistance)
                    value.z = kMaxEffectDistance;
                if ((double)value.z < -kMaxEffectDistance)
                    value.z = -kMaxEffectDistance;
                if ((double)value.w > kMaxEffectDistance)
                    value.w = kMaxEffectDistance;
                if ((double)value.w < -kMaxEffectDistance)
                    value.w = -kMaxEffectDistance;
                if (this.m_EffectDistance == value)
                    return;
                this.m_EffectDistance = value;
                if (!((Object)this.graphic != (Object)null))
                    return;
                this.graphic.SetVerticesDirty();
            }
        }

        public override void ModifyMesh(List<UIVertex> uiVertexList)
        {
            int num = uiVertexList.Count * 5;
            if (uiVertexList.Capacity < num)
                uiVertexList.Capacity = num;
            int start1 = 0;
            int count1 = uiVertexList.Count;
            this.ApplyShadowZeroAlloc(uiVertexList, (Color32)this.effectColor, start1, uiVertexList.Count, this.effectDistance.x, this.effectDistance.y);
            int start2 = count1;
            int count2 = uiVertexList.Count;
            this.ApplyShadowZeroAlloc(uiVertexList, (Color32)this.effectColor, start2, uiVertexList.Count, this.effectDistance.z, this.effectDistance.y);
            int start3 = count2;
            int count3 = uiVertexList.Count;
            this.ApplyShadowZeroAlloc(uiVertexList, (Color32)this.effectColor, start3, uiVertexList.Count, this.effectDistance.x, this.effectDistance.w);
            int start4 = count3;
            this.ApplyShadowZeroAlloc(uiVertexList, (Color32)this.effectColor, start4, uiVertexList.Count, this.effectDistance.z, this.effectDistance.w);
        }
    }
}