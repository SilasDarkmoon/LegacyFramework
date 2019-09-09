using System.Collections;
using UnityEngine;

public class SafeAreaFullScreenRect : MonoBehaviour
{
    void Start()
    {
        //子对象的 awake start 会在父对象之前执行，因此需要在父对象SafeAreaRect的start之后执行
        StartCoroutine(DelayToLayout());
    }

    IEnumerator DelayToLayout()
    {
        yield return new WaitForEndOfFrame();

        var safeAreaRect = GetComponentInParent<SafeAreaRect>();
        if (safeAreaRect != null)
        {
            var safeRectTransform = safeAreaRect.GetComponent<RectTransform>();
            var rectTransform = GetComponent<RectTransform>();
            var xpercent = (1 - safeRectTransform.anchorMin.x * 2);
            if (xpercent != 0)
            {
                var x = (safeRectTransform.rect.width / xpercent - safeRectTransform.rect.width) / 2;
                rectTransform.offsetMin = new Vector2(-x - 1f, 0);
                rectTransform.offsetMax = new Vector2(x + 1f, 0);
            }
        }
    }
}
