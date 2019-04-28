using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoopRollingText : MonoBehaviour
{
    public Text rollingText;
    public RectTransform rollingRect;
    public int threshold = 10;
    public float rollingSpeed = 50;

    private bool isRolling = false;
    private RectTransform textRect;
    // Use this for initialization
    IEnumerator Start()
    {
        if (!rollingText || !rollingRect)
            yield break;

        yield return 0;
        textRect = rollingText.GetComponent<RectTransform>();
        float textWidth = textRect.rect.width;
        float rollingRectWidth = rollingRect.rect.width;
        if (textWidth > rollingRectWidth + threshold)
        {
            isRolling = true;
            textRect.anchoredPosition = new Vector2((textWidth + threshold) / 2, textRect.anchoredPosition.y);
            var tweener = textRect.DOAnchorPosX(textRect.anchoredPosition.x - textWidth + rollingRect.rect.width, (textWidth - rollingRect.rect.width) / rollingSpeed);
            tweener.SetLoops(-1, LoopType.Yoyo);
            tweener.SetEase(Ease.InOutSine);
        }
    }
}
