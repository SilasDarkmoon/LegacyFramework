using UnityEngine;
using UnityEngine.Events;

[XLua.LuaCallCSharp]
public class AuthorizationManager
{
    private static event AuthorizationManagerBridge.Callback AuthorizationRequestEvent;
    [AOT.MonoPInvokeCallback(typeof(AuthorizationManagerBridge.Callback))]
    private static void OnCallBack(int i)
    {
        AuthorizationRequestEvent(i);
    }
    public enum AuthorizationStatus
    {
        AuthorizationStatusNotDetermined = 0,   // 用户从未进行过授权等处理，首次访问相应内容会提示用户进行授权
        AuthorizationStatusAuthorized = 1,      // 已授权
        AuthorizationStatusDenied = 2,          // 拒绝
        AuthorizationStatusRestricted = 3,      // 应用没有相关权限，且当前用户无法改变这个权限，比如:家长控制
        AuthorizationStatusNotSupport = 4,      // 硬件等不支持
    }
    
    public enum AuthorizationType
    {
        LocationServices = 1,
        Photos = 2,
        Microphone = 3,
        Camera = 4,
        MotionAndFitness = 5,
    }

    public static int GetAuthorizationStatus(AuthorizationType type)
    {
        int status = AuthorizationManagerBridge.CheckAuthorization((int)type);
        return status;
    }

    public static void RequestAuthorization(AuthorizationType type, UnityAction<int> callback)
    {
        AuthorizationRequestEvent = new AuthorizationManagerBridge.Callback(callback);
        AuthorizationManagerBridge.RequestAuthorization((int)type, OnCallBack);
    }

    public static void GuideSettingAuthorization(AuthorizationType type)
    {
        AuthorizationManagerBridge.GuideSettingAuthorization((int)type);
    }
}
