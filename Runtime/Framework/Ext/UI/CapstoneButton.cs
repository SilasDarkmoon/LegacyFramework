using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [XLua.LuaCallCSharp]
    [AddComponentMenu("CapstoneUI/CapstoneButton", 30)]
    public class CapstoneButton : Button
    {
        private static float s_LastTriggerTime;

        protected CapstoneButton()
        {
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            float nowTime = Time.realtimeSinceStartup;

            if (nowTime - CapstoneButton.s_LastTriggerTime < 0.3f)
            {
                return;
            }

            CapstoneButton.s_LastTriggerTime = nowTime;
            base.OnPointerClick(eventData);
        }
    }
}