using UnityEngine;
using System.Collections;
public class RenderTextureBase : MonoBehaviour
{
    public Material m_BaseMaterial;

    void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
    {
        Graphics.Blit(sourceTexture, destTexture, m_BaseMaterial);
    }
}