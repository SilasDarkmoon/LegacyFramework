using UnityEngine;
using System.Collections;

public class DummyEffect : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination);
    }
#endif
}
