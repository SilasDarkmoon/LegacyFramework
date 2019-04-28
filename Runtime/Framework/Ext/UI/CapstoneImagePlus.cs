using System.Collections.Generic;

namespace UnityEngine.UI
{
    [XLua.LuaCallCSharp]
    [AddComponentMenu("CapstoneUI/CapstoneImagePlus", 12)]
    public class CapstoneImagePlus : CapstoneImage
    {
        private PolygonCollider2D regionCollider;

        protected CapstoneImagePlus()
        {
        }

        protected override void Awake()
        {
            base.Awake();
            regionCollider = GetComponent<PolygonCollider2D>();
        }

        public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (!regionCollider)
            {
                return base.IsRaycastLocationValid(sp, eventCamera);
            }
            return ContainsPoint(regionCollider.points, sp, eventCamera);
        }

        private bool ContainsPoint(Vector2[] polyPoints, Vector2 sp, Camera camera)
        {
            var j = polyPoints.Length - 1;
            var p = new Vector2();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, sp, camera, out p);
            var inside = false;
            for (int i = 0; i < polyPoints.Length; j = i++)
            {
                if (((polyPoints[i].y <= p.y && p.y < polyPoints[j].y) || (polyPoints[j].y <= p.y && p.y < polyPoints[i].y)) &&
                    (p.x < (polyPoints[j].x - polyPoints[i].x) * (p.y - polyPoints[i].y) / (polyPoints[j].y - polyPoints[i].y) + polyPoints[i].x))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

#if UNITY_5 || UNITY_5_3_OR_NEWER
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);

            List<UIVertex> vbo = new List<UIVertex>();
            vh.GetUIVertexStream(vbo);
            if (overrideSprite)
            {
                var outter = UnityEngine.Sprites.DataUtility.GetOuterUV(overrideSprite);
                float w = outter.z - outter.x;
                float h = outter.w - outter.y;
                if (w == 0) w = 1;
                if (h == 0) h = 1;

                for (int i = 0; i < vbo.Count; ++i)
                {
                    var vertex = vbo[i];
                    var uv = vertex.uv0;
                    var u = (uv.x - outter.x) / w;
                    var v = (uv.y - outter.y) / h;

                    vertex.uv1 = new Vector2(u, v);
                    vbo[i] = vertex;
                }
            }
            vh.Clear();
            vh.AddUIVertexTriangleStream(vbo);
            vbo.Clear();
        }
#else
        protected override void OnFillVBO(System.Collections.Generic.List<UIVertex> vbo)
        {
            base.OnFillVBO(vbo);
            if (overrideSprite)
            {
                var outter = UnityEngine.Sprites.DataUtility.GetOuterUV(overrideSprite);
                float w = outter.z - outter.x;
                float h = outter.w - outter.y;
                if (w == 0) w = 1;
                if (h == 0) h = 1;

                for (int i = 0; i < vbo.Count; ++i)
                {
                    var vertex = vbo[i];
                    var uv = vertex.uv0;
                    var u = (uv.x - outter.x) / w;
                    var v = (uv.y - outter.y) / h;

                    vertex.uv1 = new Vector2(u, v);
                    vbo[i] = vertex;
                }
            }
        }
#endif
    }
}
