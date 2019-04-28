using UnityEngine.Profiling;
using System.Collections.Generic;
using UnityEngine;
using XLua;
/*
 * FileName:    ProfilerUtl
 * Author:      海鸣不骑猪
 * Description:性能优化相关工具
*/
[LuaCallCSharp]
public static class ProfilerUtl
{
    private static bool _isInit = false;
    /// <summary>
    /// 初始化
    /// </summary>
    [BlackList]
    public static void Init()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR || ALWAYS_SHOW_LOG || DEBUG
        if (_isInit) return;
        _isInit = true;
        _map = new Dictionary<string, float>();
#endif
    }

    private static float _time = 0;
    private static Dictionary<string, float> _map = null;

    public static void BeginSample(string flag)
    {
        if (!_isInit) return;
        if (!_map.ContainsKey(flag))
        {
            _map.Add(flag, Time.realtimeSinceStartup);
        }
        else
        {
            _map[flag] = Time.realtimeSinceStartup;
        }
        Profiler.BeginSample(flag);
    }

    public static void EndSample(string flag)
    {
        if (!_isInit) return;
        Profiler.EndSample();
        float time;
        if (_map.TryGetValue(flag, out time))
        {
            if (GLog.IsLogInfoEnabled) GLog.LogInfo(flag + " ====> " + (Time.realtimeSinceStartup - time).ToString("f4"));
        }
    }
}
