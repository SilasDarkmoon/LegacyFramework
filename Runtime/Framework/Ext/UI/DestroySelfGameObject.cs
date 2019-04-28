using UnityEngine;
using UnityEngine.UI;

namespace UIExt
{
    [ExecuteInEditMode]
    public class DestroySelfGameObject : MonoBehaviour
    { // the component will destroy self-gameObject (or gameObject stored in ToBeDestroyed) on start.
        public Object ToBeDestroyed;

        void Start()
        {
            GameObject target = null;
            if (ToBeDestroyed)
            {
                if (ToBeDestroyed is GameObject)
                {
                    target = ToBeDestroyed as GameObject;
                }
                else if (ToBeDestroyed is Component)
                {
                    target = ((Component)ToBeDestroyed).gameObject;
                }
            }
            else
            {
                target = gameObject;
            }
            if (target)
            {
#if UNITY_EDITOR
                DestroyImmediate(target);
#else
                Destroy(target);
#endif
            }
        }
    }
}