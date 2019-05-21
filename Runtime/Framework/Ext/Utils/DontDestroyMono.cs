using Capstones.UnityFramework;
using UnityEngine;
/// <summary>
/// 海鸣不骑猪 
/// 该脚本可以让一个物体 一直放在不销毁队列中
/// </summary>
public class DontDestroyMono : MonoBehaviour
{
    private void Awake()
    {
        GameObject.DontDestroyOnLoad(gameObject);
    }
}
