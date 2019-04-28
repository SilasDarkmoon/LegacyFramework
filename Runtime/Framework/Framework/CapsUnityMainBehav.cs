//#define LUA_USE_SYSTEM_ENCODING_ON_EDITOR_WIN
//#define ALWAYS_LIMIT_FPS

using UnityEngine;
using System;
using System.Collections;
using Capstones.Dynamic;
using XLuaExt;
using Capstones.UnityFramework;

public class CapsUnityMainBehav : MonoBehaviour
{
    public UnityEngine.UI.Text textLoading
    {
        get
        {
            if (dataDict != null)
            {
                object val;
                if (dataDict.TryGetValue("loadingText1", out val))
                {
                    if (val != null)
                    {
                        return val as UnityEngine.UI.Text;
                    }
                }
            }
            return null;
        }
    }
    public UnityEngine.UI.Text textDesc
    {
        get
        {
            if (dataDict != null)
            {
                object val;
                if (dataDict.TryGetValue("loadingText2", out val))
                {
                    if (val != null)
                    {
                        return val as UnityEngine.UI.Text;
                    }
                }
            }
            return null;
        }
    }
    public UnityEngine.UI.Slider sliderLoading
    {
        get
        {
            if (dataDict != null)
            {
                object val;
                if (dataDict.TryGetValue("loadingSlider", out val))
                {
                    if (val != null)
                    {
                        return val as UnityEngine.UI.Slider;
                    }
                }
            }
            return null;
        }
    }
    public UIExt.DynamicLoadPrefab dynRoot;
    public UIExt.DynamicLoadPrefab splashs;
    private static bool firstShowSplash = true;
    private System.Collections.Generic.Dictionary<string, object> dataDict;

    private string GetDataStr(string key)
    {
        if (dataDict != null)
        {
            object val;
            if (dataDict.TryGetValue(key, out val))
            {
                if (val != null)
                {
                    return val.ToString();
                }
            }
        }
        return "";
    }

    void Awake()
    {
        ProfilerUtl.Init();
        Capstones.PlatExt.PlatDependant.Init();
        if (GLog.IsLogInfoEnabled) GLog.LogInfo("Awake run 1");
        if (dynRoot != null)
        {
            if (GLog.IsLogInfoEnabled) GLog.LogInfo("Awake run 2");
            dynRoot.ApplySource();
            if (GLog.IsLogInfoEnabled) GLog.LogInfo("Awake run 3");
            var go = dynRoot._Child;
            if (GLog.IsLogInfoEnabled) GLog.LogInfo("Awake run 4");
            if (go != null)
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("Awake run 5");
                var datacomp = go.GetComponent<LuaBehaviour>();
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("Awake run 6");
                if (datacomp != null)
                {
                    if (GLog.IsLogInfoEnabled) GLog.LogInfo("Awake run 7");
                    dataDict = datacomp.ExpandExVal();
                }
            }
        }
        if (GLog.IsLogInfoEnabled) GLog.LogInfo("Awake run 8");
        textDesc.text = GetDataStr("DescStr");
    }

    void Start()
    {
#if !UNITY_EDITOR && TENCENT_RELEASE
        TssSDKInstance.Init();
        if (GLog.IsLogInfoEnabled) GLog.LogInfo("Start run 1");
#endif
        Time.timeScale = 1;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#if !UNITY_EDITOR && !UNITY_STANDALONE || ALWAYS_LIMIT_FPS
        Application.targetFrameRate = 30;
#endif
        if (firstShowSplash)
        {
            if (GLog.IsLogInfoEnabled) GLog.LogInfo("Start run 2");
            StartCoroutine(ShowSplashs());
            firstShowSplash = false;
            if (GLog.IsLogInfoEnabled) GLog.LogInfo("Start run 3");
        }
        else
        {
            StartCoroutine(PreStartCheck());
            if (GLog.IsLogInfoEnabled) GLog.LogInfo("Start run 4");
        }
    }

    IEnumerator ShowSplashs()
    {
        if (GLog.IsLogInfoEnabled) GLog.LogInfo("ShowSplashs run 0");
        if (splashs != null)
        {
            if (GLog.IsLogInfoEnabled) GLog.LogInfo("ShowSplashs run 1");
            splashs.ApplySource();
            if (GLog.IsLogInfoEnabled) GLog.LogInfo("ShowSplashs run 2");
            var go = splashs._Child;
            if (GLog.IsLogInfoEnabled) GLog.LogInfo("ShowSplashs run 3");
            if (go != null && go.GetComponent<SplashManager>() != null)
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("ShowSplashs run 4");
                var splashManager = go.GetComponent<SplashManager>();
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("ShowSplashs run 5");
                yield return new WaitForSecondsRealtime(Mathf.Clamp(splashManager.countShowTime, 2, 4));
            }
        }
        if (GLog.IsLogInfoEnabled) GLog.LogInfo("ShowSplashs run 6");
        StartCoroutine(PreStartCheck());
        if (GLog.IsLogInfoEnabled) GLog.LogInfo("ShowSplashs run 7");
    }

    IEnumerator PreStartCheck()
    {
        if (GLog.IsLogInfoEnabled) GLog.LogInfo("PreStartCheck run 0");
        string ready = LuaEvent.TrigClrEvent<string>("SDK_READY_TO_START");
        if (GLog.IsLogInfoEnabled) GLog.LogInfo("PreStartCheck run 1");
        if (!string.IsNullOrEmpty(ready))
        {
            if (GLog.IsLogInfoEnabled) GLog.LogInfo("PreStartCheck run 2");
            while (ready != "ready")
            {
                yield return null;
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("PreStartCheck run 3");
                ready = LuaEvent.TrigClrEvent<string>("SDK_READY_TO_START");
            }
        }

#if EFUN_SDK_EN || EFUN_SDK_TW
        // obb
        ResManager.Init();
#endif

        if (GLog.IsLogInfoEnabled) GLog.LogInfo("PreStartCheck run 4");
        var workpending = ResManager.MovePendingUpdate(LoadingReport);
        if (workpending != null)
        {
            while (workpending.MoveNext())
            {
                yield return workpending.Current;
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("PreStartCheck run 5");
            }
        }

#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
        ResManager.RecordCacheVersion("editor", int.MaxValue);
#else
        IEnumerator work = null;
        try
        {
            SetLoadingPhaseAmount(3);
            SetLoadingPhase(0);
            work = ResManager.DecompressScriptBundle("default", LoadingReport);
        }
        catch (Exception e)
        {
            if(GLog.IsLogErrorEnabled) GLog.LogException(e);
            LoadingReportError();
            yield break;
        }
        if (work != null)
        {
            while (true)
            {
                try
                {
                    if (!work.MoveNext())
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    if(GLog.IsLogErrorEnabled) GLog.LogException(e);
                    LoadingReportError();
                    yield break;
                }
                yield return work.Current;
            }
        }
        foreach (var flag in ResManager.GetDistributeFlags())
        {
            try
            {
                //SetLoadingPhase(loadingPhase + 1);
                work = ResManager.DecompressScriptBundle("distribute/" + flag, LoadingReport);
            }
            catch (Exception e)
            {
                if(GLog.IsLogErrorEnabled) GLog.LogException(e);
                LoadingReportError();
                yield break;
            }
            if (work != null)
            {
                while (true)
                {
                    try
                    {
                        if (!work.MoveNext())
                        {
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        if(GLog.IsLogErrorEnabled) GLog.LogException(e);
                        LoadingReportError();
                        yield break;
                    }
                    yield return work.Current;
                }
            }
        }
        try
        {
            SetLoadingPhase(loadingPhase + 1);
            work = ResManager.UpdateResourceBundleLocalAll(LoadingReport);
        }
        catch (Exception e)
        {
            if(GLog.IsLogErrorEnabled) GLog.LogException(e);
            LoadingReportError();
            yield break;
        }
        if (work != null)
        {
            while (true)
            {
                try
                {
                    if (!work.MoveNext())
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    if(GLog.IsLogErrorEnabled) GLog.LogException(e);
                    LoadingReportError();
                    yield break;
                }
                yield return work.Current;
            }
        }
        try
        {
            SetLoadingPhase(loadingPhase + 1);
            work = ResManager.SplitResIndexAsyncAll(LoadingReport);
        }
        catch (Exception e)
        {
            if(GLog.IsLogErrorEnabled) GLog.LogException(e);
            LoadingReportError();
            yield break;
        }
        if (work != null)
        {
            while (true)
            {
                try
                {
                    if (!work.MoveNext())
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    if(GLog.IsLogErrorEnabled) GLog.LogException(e);
                    LoadingReportError();
                    yield break;
                }
                yield return work.Current;
            }
        }
        yield return new WaitForEndOfFrame();
        
        if (ResManager.TryGetAssetDesc("Assets/CapstonesRes/Common/Fonts/CapstonesPlaceHolder.otf") != null)
        {
            ResManager.MarkPermanent("Assets/CapstonesRes/Common/Fonts/DistributeFontInfo.fi.txt");
            ResManager.MarkPermanent("Assets/CapstonesRes/Common/Fonts/CapstonesPlaceHolder.otf");
            ResManager.LoadRes("Assets/CapstonesRes/Common/Fonts/DistributeFontInfo.fi.txt");
            ResManager.LoadRes("Assets/CapstonesRes/Common/Fonts/CapstonesPlaceHolder.otf");
        }
#endif

        LoadingReport("WaitForBiReport", null);
        yield return new WaitForEndOfFrame();

        try
        {
            //ResManager.UnloadAllRes();
            GC.Collect();
            //GC.WaitForPendingFinalizers();
        }
        catch (Exception e)
        {
            if(GLog.IsLogErrorEnabled) GLog.LogException(e);
            LoadingReportError();
            yield break;
        }

        //LuaBehaviour.luaEnv.DoString("require 'main'");
        UnityEngine.SceneManagement.SceneManager.LoadScene("DemoEntry");

        yield break;
    }

    private int loadingPhase = 0;
    private int loadingPhaseAmount = 1;
    private float partProgress = 0;
    private float partAmount = 1;
    void SetLoadingPhaseAmount(int cnt)
    {
        loadingPhaseAmount = cnt;
        if (loadingPhaseAmount <= 0)
        {
            loadingPhaseAmount = 1;
        }
        if (sliderLoading != null)
        {
            sliderLoading.maxValue = loadingPhaseAmount;
        }
    }
    void SetLoadingPhase(int phase)
    {
        loadingPhase = phase;
        if (sliderLoading != null)
        {
            sliderLoading.value = loadingPhase;
            partProgress = 0;
        }
    }
    void SetPartAmount(float cnt)
    {
        partAmount = cnt;
        partProgress = 0;
        if (partAmount <= 0)
        {
            partAmount = 1;
        }
        if (sliderLoading != null)
        {
            sliderLoading.value = loadingPhase + partProgress / partAmount;
        }
    }
    void SetPartProgress(float progress)
    {
        partProgress = progress;
        if (sliderLoading != null)
        {
            sliderLoading.value = loadingPhase + partProgress / partAmount;
        }
    }
    void LoadingReport(string key, object val)
    {
        if (key == "Error")
        {
            if (textLoading != null)
            {
                textLoading.gameObject.FastSetActive(true);
                textDesc.gameObject.FastSetActive(true);
                textLoading.text = GetDataStr("ErrorStr");
            }
        }
        else if (key == "FirstLoad")
        {
            if (textLoading != null)
            {
                textLoading.gameObject.FastSetActive(true);
                textDesc.gameObject.FastSetActive(true);
                textLoading.text = GetDataStr("FirstLoadStr");
            }
            if (sliderLoading != null)
            {
                sliderLoading.gameObject.FastSetActive(true);
            }
        }
        else if (key == "PendingUpdate")
        {
            if (textLoading != null)
            {
                textLoading.gameObject.FastSetActive(true);
                textDesc.gameObject.FastSetActive(true);
                textLoading.text = GetDataStr("PendingUpdateStr");
            }
            if (sliderLoading != null)
            {
                sliderLoading.gameObject.FastSetActive(true);
            }
        }
        else if (key == "WaitForBiReport")
        {
            if (textLoading != null)
            {
                textLoading.gameObject.FastSetActive(true);
                textDesc.gameObject.FastSetActive(false);
                textLoading.text = GetDataStr("WaitForBiReportStr");
            }
        }
        else if (key == "Count")
        {
            SetPartAmount(val.ConvertType<int>());
            if (textLoading != null)
            {
                textLoading.text = GetDataStr("ProgressStr") + " 0/" + val + ", " + GetDataStr("PhaseStr") + " " + (loadingPhase + 1) + "/" + loadingPhaseAmount + ".";
            }
        }
        else if (key == "Progress")
        {
            SetPartProgress(partProgress + 1);
            if (textLoading != null)
            {
                textLoading.text = GetDataStr("ProgressStr") + " " + partProgress + "/" + partAmount + ", " + GetDataStr("PhaseStr") + " " + (loadingPhase + 1) + "/" + loadingPhaseAmount + ".";
            }
        }
        else if (key == "Percent")
        {
            SetPartAmount(100);
            SetPartProgress(val.ConvertType<float>() * 100);
            if (textLoading != null)
            {
                textLoading.text = GetDataStr("ProgressStr") + (int)partProgress + "%, " + GetDataStr("PhaseStr") + " " + (loadingPhase + 1) + "/" + loadingPhaseAmount + ".";
            }
        }
        else if (key == "Run")
        {
            SetPartAmount(100);
            SetPartProgress(val.ConvertType<float>() * 100);
            if (textLoading != null)
            {
                textLoading.text = GetDataStr("WaitForBiReportStr");
            }
        }
    }
    void LoadingReportError()
    {
        LoadingReport("Error", null);
        ResManager.ResetCacheVersion();
    }
}
