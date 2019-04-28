using UnityEngine;
using System.Collections;

public class GravityMove : MonoBehaviour
{
    public RectTransform spriteRect; //移动对象Rect
    public Vector3 speed = new Vector3(1f, 1f, 1f); //移动速度  
    public Vector3 addSpeed = new Vector3(0.2f, 0.2f, 0f); //移动加速度 
    public Vector3 moveDistance = new Vector3(0f, 0f, 0f); //设置匹配屏幕  
    private float x, y, z; //贴图的坐标  
    private Vector3 startSpeed;
    private float standardWidth = 1334;
    // Use this for initialization
    void Start()
    {
        var aspectRatio = (spriteRect.rect.width - standardWidth) / 2;
        if (aspectRatio < 0) aspectRatio = 0; // 极端情况下会取不到rect正确属性
        moveDistance.x = moveDistance.x == 0 ? aspectRatio : moveDistance.x;
        moveDistance.z = moveDistance.z == 0 ? aspectRatio : moveDistance.z;

        startSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        x += Input.acceleration.x * startSpeed.x;
        y += -Input.acceleration.z * startSpeed.y;
        z += -Input.acceleration.y * startSpeed.z;
        startSpeed += addSpeed;
        //避免超出屏幕  
        if (x <= -moveDistance.x)
        {
            x = -moveDistance.x;
            startSpeed.x = speed.x;
        }
        else if (x >= moveDistance.x)
        {
            x = moveDistance.x;
            startSpeed.x = speed.x;
        }

        if (y <= -moveDistance.y)
        {
            y = -moveDistance.y;
            startSpeed.y = speed.y;
        }
        else if (y >= moveDistance.y)
        {
            y = moveDistance.y;
            startSpeed.y = speed.y;
        }

        if (z >= 0)
        {
            z = 0;
            startSpeed.z = speed.z;
        }
        else if (z <= -moveDistance.z)
        {
            z = -moveDistance.z;
            startSpeed.z = speed.z;
        }

        spriteRect.anchoredPosition3D = new Vector3(x, y, z);
    }
}
