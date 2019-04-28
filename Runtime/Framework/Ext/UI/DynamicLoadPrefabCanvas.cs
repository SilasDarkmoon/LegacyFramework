using UnityEngine;
using UnityEngine.UI;

namespace UIExt
{
    [ExecuteInEditMode]
    public class DynamicLoadPrefabCanvas : MonoBehaviour
    { // the dynamic-loaded canvas will not be shown correctly, so we add this to loaded canvas.
#if UNITY_EDITOR
        void Start()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas)
            {
                var e = canvas.enabled;
                canvas.enabled = false;
                Capstones.UnityFramework.EditorBridge.OnDelayedCallOnce += () =>
                {
                    if (canvas)
                    {
                        canvas.enabled = e;
                    }
                };
            }
            DestroyImmediate(this);
        }
#endif
    }
}