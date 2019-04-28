using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Blur/Radial Blur")]
[RequireComponent(typeof(Camera))]

public class RadialBlur : ImageEffectBase
{
    [Range(0.0f, 1.0f)]
    public float SampleDist = 0.17f;
    [Range(1.0f, 5.0f)]
    public float SampleStrength = 2.09f;


    override protected void Start()
    {
        base.Start();
    }

    override protected void OnDisable()
    {
        base.OnDisable();
    }

    // Called by camera to apply image effect
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (SampleDist != 0 && SampleStrength != 0)
        {
            int rtW = source.width / 2;
            int rtH = source.height / 2;

            material.SetFloat("_SampleDist", SampleDist);
            material.SetFloat("_SampleStrength", SampleStrength);

            RenderTexture rtTempA = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default);
            rtTempA.name = "RadialBlur Render Texture A";
            rtTempA.filterMode = FilterMode.Bilinear;

            Graphics.Blit(source, rtTempA);

            RenderTexture rtTempB = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default);
            rtTempB.name = "RadialBlur Render Texture B";
            rtTempB.filterMode = FilterMode.Bilinear;

            Graphics.Blit(rtTempA, rtTempB, material, 0);

            material.SetTexture("_BlurTex", rtTempB);
            Graphics.Blit(source, destination, material, 1);

            RenderTexture.ReleaseTemporary(rtTempA);
            RenderTexture.ReleaseTemporary(rtTempB);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}
