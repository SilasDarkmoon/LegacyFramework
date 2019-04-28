using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashManager : MonoBehaviour
{
    [System.Serializable]
    public class Splash
    {
        public GameObject splash;
        public float showTime;
    }

    [SerializeField]
    public Splash[] splashList = new Splash[] { };
    [HideInInspector]
    public float countShowTime;

    void Awake()
    {
        countShowTime = 0;
        Splash splash;
        for (int i = 0; i < splashList.Length; i++)
        {
            splash = splashList[i];
            if (splash == null || splash.splash == null) continue;
            countShowTime += splash.showTime;
        }
        if (GLog.IsLogInfoEnabled) GLog.LogInfo("SplashManager Awake ===> countShowTime :" + countShowTime);
    }

    void Start()
    {
        StartCoroutine(ShowSplash());
    }

    IEnumerator ShowSplash()
    {
        if (splashList == null || splashList.Length == 0) yield break;
        for (int i = 0; i < splashList.Length; i++)
        {
            var splash = splashList[i];
            if (splash != null && splash.splash != null && splash.showTime > 0)
            {
                splash.splash.FastSetActive(true);
                yield return new WaitForSecondsRealtime(splash.showTime);
                //splash.splash.FastSetActive(false);
            }
        }
    }
}
