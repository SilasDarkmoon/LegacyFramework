using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using Capstones.UnityEngineEx;
/// <summary>
/// UI流光效果，支持裁剪
/// 可以在UGUI的Image或者Raw Image使用
/// </summary>
public class SetFlowTexMaterial : MonoBehaviour
{
    private float widthRate = 1;
    private float heightRate = 1;
    private float xOffsetRate = 0;
    private float yOffsetRate = 0;
    public Shader shader;
    public Color color = Color.yellow;
    public float power = 0.55f;
    public float speed = 5;
    public float largeWidth = 0.003f;
    public float littleWidth = 0.0003f;
    public float length = 0.1f;
    public float skewRadio = 0.2f;//倾斜
    //扩展参数
    public float widthRateScale = 1;
    public float heightRateScale = 1f;
    public float xOffsetRateScale = 1f;
    public float yOffsetRateScale = 1f;
    //编辑模式
    public bool EditMode = false;
    float endMoveTime = 0;
    private MaskableGraphic maskableGraphic;
    Image image;
    Material imageMat = null;
    void Awake()
    {
        maskableGraphic = GetComponent<MaskableGraphic>();
        if (maskableGraphic)
        {
            image = maskableGraphic as Image;
            if (image)
            {
                imageMat = new Material(ResManager.LoadRes("Assets/CapstonesRes/Common/Shaders/UI/UIFlowLight.shader") as Shader);
                widthRate = image.sprite.textureRect.width * 1.0f / image.sprite.texture.width;
                heightRate = image.sprite.textureRect.height * 1.0f / image.sprite.texture.height;
                xOffsetRate = (image.sprite.textureRect.xMin) * 1.0f / image.sprite.texture.width;
                yOffsetRate = (image.sprite.textureRect.yMin) * 1.0f / image.sprite.texture.height;
            }
        }
        image.material = imageMat;
    }

    void Start()
    {
        SetShader();
    }

#if UNITY_EDITOR
    void Update()
    {
        if(EditMode)
        {
            SetShader();
        }
    }
#endif

    void SetShader()
    {
        skewRadio = Mathf.Clamp(skewRadio, 0, 1);
        length = Mathf.Clamp(length, 0, 0.5f);
        imageMat.SetColor("_FlowlightColor", color);
        imageMat.SetFloat("_Power", power);
        imageMat.SetFloat("_MoveSpeed", speed);
        imageMat.SetFloat("_LargeWidth", largeWidth);
        imageMat.SetFloat("_LittleWidth", littleWidth);
        imageMat.SetFloat("_SkewRadio", skewRadio);
        imageMat.SetFloat("_Lengthlitandlar", length);
        imageMat.SetFloat("_WidthRate", widthRate * widthRateScale);
        imageMat.SetFloat("_HeightRate", heightRate * heightRateScale);
        imageMat.SetFloat("_XOffset", xOffsetRate * xOffsetRateScale);
        imageMat.SetFloat("_YOffset", yOffsetRate * yOffsetRateScale);
    }
}