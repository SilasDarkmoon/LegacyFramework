using UnityEngine;
using System.Collections;
/// <summary>
/// 海鸣不骑猪 2018.4.23 修改
/// 主要修改内容是控制后期处理只抓取一帧的图像数据
/// 并关闭摄像机的相关渲染层级 减少dc 增加cpu性能
/// </summary>
//设置在编辑模式下也执行该脚本
//[ExecuteInEditMode]
[XLua.LuaCallCSharp]
public class RapidBlurEffect : MonoBehaviour
{
    //-------------------变量声明部分-------------------
    /// <summary>
    /// 静态帧的图片
    /// </summary>
    private Texture2D _screenTex = null;
    #region Variables
    //控制要执行后期的camer
    private Camera _currCamera;
    //获取摄像机默认配置 以在后期停止后重新设置回去
    private int _currCullingMaskConfig;
    //指定Shader名称
    private string ShaderName = "Custom/ImageEffect/RapidBlurEffect";
    //着色器和材质实例
    public Shader CurShader;
    private Material CurMaterial;
    //降采样次数
    private const int DownSampleMix = 1;
    [Range(1, 8), Tooltip("[降采样次数]向下采样的次数。此值越大,则采样间隔越大,需要处理的像素点越少,运行速度越快。")]
    public int DownSampleNum = 8;
    //模糊扩散度
    [Range(0.0f, 20.0f), Tooltip("[模糊扩散度]进行高斯模糊时，相邻像素点的间隔。此值越大相邻像素间隔越远，图像越模糊。但过大的值会导致失真。")]
    public float BlurSpreadSize = 3.0f;
    //迭代次数
    private const int BlurIterationsMax = 3;
    [Range(0, 3), Tooltip("[迭代次数]此值越大,则模糊操作的迭代次数越多，模糊效果越好，但消耗越大。")]
    public int BlurIterations = 3;
    public Color color = new Color(0, 0, 0, 0.8f);
    //背景刷新频率，0-不刷新
    public int BgFreshRate = 0;
    private int BgFreshTick;
    #endregion
    //-------------------------材质的get&set----------------------------
#region MaterialGetAndSet
    Material material
    {
        get
        {
            if (CurMaterial == null)
            {
                CurMaterial = new Material(CurShader);
                CurMaterial.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.NotEditable;
            }
            return CurMaterial;
        }
    }
#endregion

#region Functions
    void OnDisable()
    {
        if (_screenTex != null) Destroy(_screenTex);
        _screenTex = null;
        if (_currCamera != null) _currCamera.cullingMask = _currCullingMaskConfig;
    }

    void OnDestroy()
    {
        OnDisable();
        _currCamera = null;
    }

    void OnEnable()
    {
        //判断当前设备是否支持屏幕特效
        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }
        //找到当前的Shader文件
        if (CurShader == null) CurShader = Shader.Find(ShaderName);
    }

    void Start()
    {
        _currCamera = GetComponent<Camera>();
        if (_currCamera != null) _currCullingMaskConfig = _currCamera.cullingMask;
    }

    private void RefreshScreenTexture(RenderTexture sourceTexture, RenderTexture destTexture)
    {
        //【0】参数准备
        //根据向下采样的次数确定宽度系数。用于控制降采样后相邻像素的间隔
        if (DownSampleNum < DownSampleMix) DownSampleNum = DownSampleMix;
        float widthMod = 1.0f / (1.0f * (1 << DownSampleNum));
        //Shader的降采样参数赋值
        material.SetFloat("_DownSampleValue", BlurSpreadSize * widthMod);
        // 叠加的颜色
        material.SetColor("_Color", this.color);
        //设置渲染模式：双线性
        sourceTexture.filterMode = FilterMode.Bilinear;
        //通过右移，准备长、宽参数值
        int renderWidth = sourceTexture.width >> DownSampleNum;
        int renderHeight = sourceTexture.height >> DownSampleNum;
        // 【1】处理Shader的通道0，用于降采样 ||Pass 0,for down sample
        //准备一个缓存renderBuffer，用于准备存放最终数据
        RenderTexture renderBuffer = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, sourceTexture.format);
        renderBuffer.name = "Rapid Blur Render Texture 1";
        //设置渲染模式：双线性
        renderBuffer.filterMode = FilterMode.Bilinear;
        //拷贝sourceTexture中的渲染数据到renderBuffer,并仅绘制指定的pass0的纹理数据
        Graphics.Blit(sourceTexture, renderBuffer, material, 0);
        //【2】根据BlurIterations（迭代次数），来进行指定次数的迭代操作

        if (BlurIterations > BlurIterationsMax) BlurIterations = BlurIterationsMax;
        for (int i = 0; i < BlurIterations; i++)
        {
            //【2.1】Shader参数赋值
            //迭代偏移量参数
            float iterationOffs = (i * 1.0f);
            //Shader的降采样参数赋值
            material.SetFloat("_DownSampleValue", BlurSpreadSize * widthMod + iterationOffs);
            // 【2.2】处理Shader的通道1，垂直方向模糊处理 || Pass1,for vertical blur
            // 定义一个临时渲染的缓存tempBuffer
            RenderTexture tempBuffer = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, sourceTexture.format);
            tempBuffer.name = "Rapid Blur Render Texture 2";
            // 拷贝renderBuffer中的渲染数据到tempBuffer,并仅绘制指定的pass1的纹理数据
            Graphics.Blit(renderBuffer, tempBuffer, material, 1);
            //  清空renderBuffer
            RenderTexture.ReleaseTemporary(renderBuffer);
            // 将tempBuffer赋给renderBuffer，此时renderBuffer里面pass0和pass1的数据已经准备好
            renderBuffer = tempBuffer;

            // 【2.3】处理Shader的通道2，竖直方向模糊处理 || Pass2,for horizontal blur
            // 获取临时渲染纹理
            tempBuffer = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, sourceTexture.format);
            tempBuffer.name = "Rapid Blur Render Texture 3";
            // 拷贝renderBuffer中的渲染数据到tempBuffer,并仅绘制指定的pass2的纹理数据
            Graphics.Blit(renderBuffer, tempBuffer, CurMaterial, 2);

            //【2.4】得到pass0、pass1和pass2的数据都已经准备好的renderBuffer
            // 再次清空renderBuffer
            RenderTexture.ReleaseTemporary(renderBuffer);
            // 再次将tempBuffer赋给renderBuffer，此时renderBuffer里面pass0、pass1和pass2的数据都已经准备好
            renderBuffer = tempBuffer;
        }
        //这里保存为Texture2D 防止部分机型 RenderTexture 丢失
        RenderTexture.active = renderBuffer;
        if (_screenTex == null)
        {
            _screenTex = new Texture2D(renderWidth, renderHeight, TextureFormat.RGB24, false);
            _screenTex.wrapMode = TextureWrapMode.Clamp;
        }
        _screenTex.ReadPixels(new Rect(0, 0, renderWidth, renderHeight), 0, 0);
        _screenTex.Apply(false, false);
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderBuffer);
        ///Graphics Blit
        Graphics.Blit(_screenTex, destTexture);
        if (BgFreshRate == 0)
        {
            if (_currCamera != null) _currCamera.cullingMask = 0;
        }
    }
    //-------------------------------------【OnRenderImage()函数】------------------------------------  
    // 说明：此函数在当完成所有渲染图片后被调用，用来渲染图片后期效果
    //--------------------------------------------------------------------------------------------------------
    void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
    {
        //着色器实例不为空，就进行参数设置
        if (CurShader != null && _screenTex == null)
        {
            RefreshScreenTexture(sourceTexture, destTexture);
        }
        //着色器实例为空，直接拷贝屏幕上的效果。此情况下是没有实现屏幕特效的
        else
        {
            if (_screenTex != null)
            {
                Graphics.Blit(_screenTex, destTexture);

                if (BgFreshRate != 0)
                {
                    BgFreshTick++;
                    if (BgFreshTick >= BgFreshRate)
                    {
                        BgFreshTick = 0;
                        RefreshScreenTexture(sourceTexture, destTexture);
                    }
                }
                return;
            }
            //直接拷贝源纹理到目标渲染纹理
            Graphics.Blit(sourceTexture, destTexture);
        }
    }
#endregion
}