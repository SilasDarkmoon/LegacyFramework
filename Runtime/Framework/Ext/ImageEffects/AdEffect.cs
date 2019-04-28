using UnityEngine;
using System.Collections;

public class AdEffect : MonoBehaviour
{
    // 动画单元数量
    public int cellAmount;
    // 广告总行数
    public int rowAmount;
    // 广告切换间隔
    public float adSwitchInterval;
    // 条纹移动速度
    [Range(0, 5)]
    public float stripeMoveSpeed;
    // 材质
    private Material material;
    // 广告索引
    private int adIndex = 0;
    // 剩余的广告切换时间
    private float lastAdSwitchTime;
    // 条纹移动时间
    private float stripeMoveTime;

    void Awake()
    {
        material = GetComponent<Renderer>().material;
        lastAdSwitchTime = adSwitchInterval;
        material.SetFloat("_RowAmount", rowAmount);
        material.SetFloat("_StripeMoveSpeed", stripeMoveSpeed);
    }

    void Update()
    {
        lastAdSwitchTime -= Time.deltaTime;
        stripeMoveTime += Time.deltaTime;
        if (lastAdSwitchTime <= 0)
        {
            lastAdSwitchTime = adSwitchInterval;
            adIndex += 1;
            if (adIndex >= cellAmount)
            {
                adIndex = 0;
            }
            float adUVX = 0;
            float adUVY = 0;
            if (adIndex < rowAmount)
            {
                adUVY = (float)adIndex / rowAmount;
            }
            else
            {
                adUVX = 1.0f;
                adUVY = (float)(adIndex - rowAmount) / rowAmount;
            }
            material.SetFloat("_AdUVX", adUVX);
            material.SetFloat("_AdUVY", adUVY);
            stripeMoveTime = 0;
        }
        material.SetFloat("_StripeMoveTime", stripeMoveTime);
    }

    void OnDisable()
    {
        DestroyImmediate(material);
    }
}
