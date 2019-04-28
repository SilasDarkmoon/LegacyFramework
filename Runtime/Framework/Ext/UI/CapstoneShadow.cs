using System.Collections.Generic;

namespace UnityEngine.UI
{
    [XLua.LuaCallCSharp]
    [AddComponentMenu("CapstoneUI/Effects/CapstoneShadow", 14)]
    public class CapstoneShadow : CapstoneBaseShadowEffect
    {
        [SerializeField]
        private Vector2 m_EffectDistance = new Vector2(1f, -1f);

        protected CapstoneShadow()
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
        public Vector2 effectDistance
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
            this.ApplyShadowZeroAlloc(uiVertexList, (Color32)this.effectColor, 0, uiVertexList.Count, this.effectDistance.x, this.effectDistance.y);
        }
    }
}