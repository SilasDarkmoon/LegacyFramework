using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/ColorAdjustEffect")]
[RequireComponent(typeof(Camera))]

public class ColorAdjustEffect : ImageEffectBase
{
    [Range(0.0f, 3.0f)]
    public float brightness = 1.0f;
    [Range(0.0f, 3.0f)]
    public float contrast = 1.0f;
    [Range(0.0f, 3.0f)]
    public float saturation = 1.0f;


    override protected void Start()
    {
        base.Start();
    }

    override protected void OnDisable()
    {
        base.OnDisable();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        material.SetFloat("_Brightness", brightness);
        material.SetFloat("_Saturation", saturation);
        material.SetFloat("_Contrast", contrast);

        Graphics.Blit(source, destination, material);
    }
}
