using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public static class AuthorizationManagerBridge
{
    public delegate void Callback(int status);

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal", EntryPoint = "RequestAuthorization")]
    public static extern void RequestAuthorization(int privacyType, Callback callback);

    [DllImport("__Internal", EntryPoint = "CheckAuthorization")]
    public static extern int CheckAuthorization(int privacyType);

    [DllImport("__Internal", EntryPoint = "GuideSettingAuthorization")]
    public static extern void GuideSettingAuthorization(int privacyType);
#elif UNITY_ANDROID && !UNITY_EDITOR
    static AndroidJavaClass AuthorizationTools = new AndroidJavaClass("com.capstones.luaext.AuthorizationTools");
    class CallbackProxy : AndroidJavaProxy
    {
        private Callback authorizationCallback;

        public CallbackProxy(Callback callback) : base("com.capstones.luaext.AuthorizationTools$AuthorizationRequestCallback")
        {
            authorizationCallback = callback;
        }

        public void AuthorizationRequestCallback(int status)
        {
            authorizationCallback(status);
        }
    }


    public static void RequestAuthorization(int privacyType, Callback callback)
    {
        AuthorizationTools.CallStatic("RequestAuthorization", privacyType, new CallbackProxy(callback));
    }
    
    public static int CheckAuthorization(int privacyType)
    {
        return AuthorizationTools.CallStatic<int>("CheckAuthorization", privacyType);
    }

    public static void GuideSettingAuthorization(int privacyType)
    {
        AuthorizationTools.CallStatic("GuideSettingAuthorization", privacyType);
    }
#else
    public static void RequestAuthorization(int privacyType, Callback callback)
    {
        callback(0);
    }

    public static int CheckAuthorization(int privacyType)
    {
        return 2;
    }

    public static void GuideSettingAuthorization(int privacyType)
    {
    }
#endif
}