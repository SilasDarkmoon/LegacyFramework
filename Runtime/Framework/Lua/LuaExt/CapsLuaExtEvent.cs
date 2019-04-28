using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using Capstones.Dynamic;
using Capstones.LuaLib;
using Capstones.LuaWrap;

using lua = Capstones.LuaLib.LuaCoreLib;
using lual = Capstones.LuaLib.LuaAuxLib;
using luae = Capstones.LuaLib.LuaLibEx;

namespace Capstones.LuaExt
{
    public static class LuaEvent
    {
        #region NativeEventPlugin
#if (UNITY_WP8 || UNITY_METRO) && !UNITY_EDITOR
        // Each Handler
        public delegate void CEventHandler(string cate);
        // Reg & Unreg
        public delegate int Del_RegHandler(string cate, CEventHandler handler);
        public delegate void Del_UnregHandler(string cate, int refid);
        // Call Other
        public delegate int Del_NewCallToken(); // token 0 means params and token 1 means returns.
        public delegate void Del_TrigEvent(string cate, int token);
        // Get Value
        public delegate bool Del_GetValBool();
        public delegate double Del_GetValNum();
        public delegate IntPtr Del_GetValPtr();
        public delegate string Del_GetValStr();
        public delegate int Del_GetValStrTo(IntPtr pstr);
        // Set Value
        public delegate void Del_SetValBool(bool val);
        public delegate void Del_SetValNum(double num);
        public delegate void Del_SetValPtr(IntPtr ptr);
        public delegate void Del_SetValStr(string str);
        public delegate void Del_UnsetVal();
        // Get Params
        public delegate int Del_GetParamCount(int token);
        public delegate void Del_SetParamCount(int token, int cnt);
        public delegate void Del_GetParam(int token, int index);
        public delegate void Del_SetParam(int token, int index);
        // Global Lua Val
        public delegate IntPtr Del_GetLuaState();
        public delegate void Del_GetGlobal(string name);
        public delegate void Del_SetGlobal(string name);

        public delegate void del_luaevent_init
            (Del_RegHandler func_RegHandler
            , Del_UnregHandler func_UnregHandler
            , Del_NewCallToken func_NewCallToken
            , Del_TrigEvent func_TrigEvent
            , Del_GetValBool func_GetValBool
            , Del_GetValNum func_GetValNum
            , Del_GetValPtr func_GetValPtr
            , Del_GetValStr func_GetValStr
            , Del_SetValBool func_SetValBool
            , Del_SetValNum func_SetValNum
            , Del_SetValPtr func_SetValPtr
            , Del_SetValStr func_SetValStr
            , Del_UnsetVal func_UnsetVal
            , Del_GetParamCount func_GetParamCount
            , Del_SetParamCount func_SetParamCount
            , Del_GetParam func_GetParam
            , Del_SetParam func_SetParam
            , Del_GetLuaState func_GetLuaState
            , Del_GetGlobal func_GetGlobal
            , Del_SetGlobal func_SetGlobal
            );
        public static del_luaevent_init func_luaevent_init;
        // Init Func to push the global funcs from C# to C -- This is the whole capstone-plugin's entrance.
        // In C plugin code, after receiving these funcs, we can do further init in this call, i.e. load the distribute-plugin
        public static void luaevent_init
            (Del_RegHandler func_RegHandler
            , Del_UnregHandler func_UnregHandler
            , Del_NewCallToken func_NewCallToken
            , Del_TrigEvent func_TrigEvent
            , Del_GetValBool func_GetValBool
            , Del_GetValNum func_GetValNum
            , Del_GetValPtr func_GetValPtr
            , Del_GetValStr func_GetValStr
            , Del_GetValStrTo func_GetValStrTo
            , Del_SetValBool func_SetValBool
            , Del_SetValNum func_SetValNum
            , Del_SetValPtr func_SetValPtr
            , Del_SetValStr func_SetValStr
            , Del_UnsetVal func_UnsetVal
            , Del_GetParamCount func_GetParamCount
            , Del_SetParamCount func_SetParamCount
            , Del_GetParam func_GetParam
            , Del_SetParam func_SetParam
            , Del_GetLuaState func_GetLuaState
            , Del_GetGlobal func_GetGlobal
            , Del_SetGlobal func_SetGlobal
            )
        {
            if (func_luaevent_init != null)
            {
                func_luaevent_init
                    (func_RegHandler
                    , func_UnregHandler
                    , func_NewCallToken
                    , func_TrigEvent
                    , func_GetValBool
                    , func_GetValNum
                    , func_GetValPtr
                    , func_GetValStr
                    , func_SetValBool
                    , func_SetValNum
                    , func_SetValPtr
                    , func_SetValStr
                    , func_UnsetVal
                    , func_GetParamCount
                    , func_SetParamCount
                    , func_GetParam
                    , func_SetParam
                    , func_GetLuaState
                    , func_GetGlobal
                    , func_SetGlobal
                    );
            }
        }
#elif UNITY_IPHONE && !UNITY_EDITOR
        // Each Handler
        public delegate void CEventHandler(string cate);
        // Reg & Unreg
        public delegate int Del_RegHandler(string cate, CEventHandler handler);
        public delegate void Del_UnregHandler(string cate, int refid);
        // Call Other
        public delegate int Del_NewCallToken(); // token 0 means params and token 1 means returns.
        public delegate void Del_TrigEvent(string cate, int token);
        // Get Value
        public delegate bool Del_GetValBool();
        public delegate double Del_GetValNum();
        public delegate IntPtr Del_GetValPtr();
        public delegate string Del_GetValStr();
        public delegate int Del_GetValStrTo(IntPtr pstr);
        // Set Value
        public delegate void Del_SetValBool(bool val);
        public delegate void Del_SetValNum(double num);
        public delegate void Del_SetValPtr(IntPtr ptr);
        public delegate void Del_SetValStr(string str);
        public delegate void Del_UnsetVal();
        // Get Params
        public delegate int Del_GetParamCount(int token);
        public delegate void Del_SetParamCount(int token, int cnt);
        public delegate void Del_GetParam(int token, int index);
        public delegate void Del_SetParam(int token, int index);
        // Global Lua Val
        public delegate IntPtr Del_GetLuaState();
        public delegate void Del_GetGlobal(string name);
        public delegate void Del_SetGlobal(string name);

        // Wrapper for AOT
        public delegate int Del_RegHandlerRaw(IntPtr czcate, IntPtr pfunc);
        public delegate void Del_UnregHandlerRaw(IntPtr czcate, int refid);
        public delegate void Del_TrigEventRaw(IntPtr czcate, int token);
        public delegate void Del_SetValStrRaw(IntPtr czstr);
        public delegate void Del_GetGlobalRaw(IntPtr czname);
        public delegate void Del_SetGlobalRaw(IntPtr czname);

        internal static readonly Del_RegHandlerRaw Func_RegHandlerRaw = new Del_RegHandlerRaw(RegHandlerRaw);
        internal static readonly Del_UnregHandlerRaw Func_UnregHandlerRaw = new Del_UnregHandlerRaw(UnregHandlerRaw);
        internal static readonly Del_TrigEventRaw Func_TrigEventRaw = new Del_TrigEventRaw(TrigEventRaw);
        internal static readonly Del_SetValStrRaw Func_SetValStrRaw = new Del_SetValStrRaw(SetValStrRaw);
        internal static readonly Del_GetGlobalRaw Func_GetGlobalRaw = new Del_GetGlobalRaw(GetGlobalRaw);
        internal static readonly Del_SetGlobalRaw Func_SetGlobalRaw = new Del_SetGlobalRaw(SetGlobalRaw);

        [AOT.MonoPInvokeCallback(typeof(Del_RegHandlerRaw))]
        public static int RegHandlerRaw(IntPtr czcate, IntPtr pfunc)
        {
            return RegHandler(Marshal.PtrToStringAnsi(czcate), (cate) => luaevent_callhandler(cate, pfunc));
        }
        [AOT.MonoPInvokeCallback(typeof(Del_UnregHandlerRaw))]
        public static void UnregHandlerRaw(IntPtr czcate, int refid)
        {
            UnregHandler(Marshal.PtrToStringAnsi(czcate), refid);
        }
        [AOT.MonoPInvokeCallback(typeof(Del_TrigEventRaw))]
        public static void TrigEventRaw(IntPtr czcate, int token)
        {
            TrigEvent(Marshal.PtrToStringAnsi(czcate), token);
        }
        [AOT.MonoPInvokeCallback(typeof(Del_SetValStrRaw))]
        public static void SetValStrRaw(IntPtr czstr)
        {
            if (czstr == IntPtr.Zero)
            {
                SetValStr(null);
            }
            else
            {
                List<byte> bytes = new List<byte>();
                try
                {
                    int off = -1;
                    while (true)
                    {
                        byte b = Marshal.ReadByte(czstr, ++off);
                        if (b == 0)
                        {
                            break;
                        }
                        else
                        {
                            bytes.Add(b);
                        }
                    }
                }
                catch { }
                SetValStr(System.Text.Encoding.UTF8.GetString(bytes.ToArray()));
            }
        }
        [AOT.MonoPInvokeCallback(typeof(Del_GetGlobalRaw))]
        public static void GetGlobalRaw(IntPtr czname)
        {
            GetGlobal(Marshal.PtrToStringAnsi(czname));
        }
        [AOT.MonoPInvokeCallback(typeof(Del_SetGlobalRaw))]
        public static void SetGlobalRaw(IntPtr czname)
        {
            SetGlobal(Marshal.PtrToStringAnsi(czname));
        }

#if DUMMY_NATIVE_EVENTS
        public static void luaevent_callhandler(string cate, IntPtr pfunc) { }
        public static void luaevent_init(
            Del_RegHandlerRaw func_RegHandler
            , Del_UnregHandlerRaw func_UnregHandler
            , Del_NewCallToken func_NewCallToken
            , Del_TrigEventRaw func_TrigEvent
            , Del_GetValBool func_GetValBool
            , Del_GetValNum func_GetValNum
            , Del_GetValPtr func_GetValPtr
            , Del_GetValStrTo func_GetValStrTo
            , Del_SetValBool func_SetValBool
            , Del_SetValNum func_SetValNum
            , Del_SetValPtr func_SetValPtr
            , Del_SetValStrRaw func_SetValStr
            , Del_UnsetVal func_UnsetVal
            , Del_GetParamCount func_GetParamCount
            , Del_SetParamCount func_SetParamCount
            , Del_GetParam func_GetParam
            , Del_SetParam func_SetParam
            , Del_GetLuaState func_GetLuaState
            , Del_GetGlobalRaw func_GetGlobal
            , Del_SetGlobalRaw func_SetGlobal
            ) { }
#else
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaevent_callhandler(string cate, IntPtr pfunc);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        // Init Func to push the global funcs from C# to C -- This is the whole capstone-plugin's entrance.
        // In C plugin code, after receiving these funcs, we can do further init in this call, i.e. load the distribute-plugin
        public static extern void luaevent_init(
            Del_RegHandlerRaw func_RegHandler
            , Del_UnregHandlerRaw func_UnregHandler
            , Del_NewCallToken func_NewCallToken
            , Del_TrigEventRaw func_TrigEvent
            , Del_GetValBool func_GetValBool
            , Del_GetValNum func_GetValNum
            , Del_GetValPtr func_GetValPtr
            , Del_GetValStrTo func_GetValStrTo
            , Del_SetValBool func_SetValBool
            , Del_SetValNum func_SetValNum
            , Del_SetValPtr func_SetValPtr
            , Del_SetValStrRaw func_SetValStr
            , Del_UnsetVal func_UnsetVal
            , Del_GetParamCount func_GetParamCount
            , Del_SetParamCount func_SetParamCount
            , Del_GetParam func_GetParam
            , Del_SetParam func_SetParam
            , Del_GetLuaState func_GetLuaState
            , Del_GetGlobalRaw func_GetGlobal
            , Del_SetGlobalRaw func_SetGlobal
            );
#endif

        public static void luaevent_init(
            Del_RegHandler func_RegHandler
            , Del_UnregHandler func_UnregHandler
            , Del_NewCallToken func_NewCallToken
            , Del_TrigEvent func_TrigEvent
            , Del_GetValBool func_GetValBool
            , Del_GetValNum func_GetValNum
            , Del_GetValPtr func_GetValPtr
            , Del_GetValStr func_GetValStr
            , Del_GetValStrTo func_GetValStrTo
            , Del_SetValBool func_SetValBool
            , Del_SetValNum func_SetValNum
            , Del_SetValPtr func_SetValPtr
            , Del_SetValStr func_SetValStr
            , Del_UnsetVal func_UnsetVal
            , Del_GetParamCount func_GetParamCount
            , Del_SetParamCount func_SetParamCount
            , Del_GetParam func_GetParam
            , Del_SetParam func_SetParam
            , Del_GetLuaState func_GetLuaState
            , Del_GetGlobal func_GetGlobal
            , Del_SetGlobal func_SetGlobal
            )
        {
            try
            {
                luaevent_init(
                    Func_RegHandlerRaw
                    , Func_UnregHandlerRaw
                    , func_NewCallToken
                    , Func_TrigEventRaw
                    , func_GetValBool
                    , func_GetValNum
                    , func_GetValPtr
                    , func_GetValStrTo
                    , func_SetValBool
                    , func_SetValNum
                    , func_SetValPtr
                    , Func_SetValStrRaw
                    , func_UnsetVal
                    , func_GetParamCount
                    , func_SetParamCount
                    , func_GetParam
                    , func_SetParam
                    , func_GetLuaState
                    , Func_GetGlobalRaw
                    , Func_SetGlobalRaw
                    );
            }
            catch (Exception e)
            {
                if(GLog.IsLogErrorEnabled) GLog.LogException(e);
            }
        }
#elif UNITY_ANDROID && !UNITY_EDITOR
        // Each Handler
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CEventHandler(string cate);
        // Reg & Unreg
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Del_RegHandler(string cate, CEventHandler handler);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_UnregHandler(string cate, int refid);
        // Call Other
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Del_NewCallToken(); // token 0 means params and token 1 means returns.
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_TrigEvent(string cate, int token);
        // Get Value
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool Del_GetValBool();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double Del_GetValNum();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr Del_GetValPtr();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate string Del_GetValStr();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Del_GetValStrTo(IntPtr pstr);
        // Set Value
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_SetValBool(bool val);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_SetValNum(double num);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_SetValPtr(IntPtr ptr);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_SetValStr(string str);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_UnsetVal();
        // Get Params
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Del_GetParamCount(int token);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_SetParamCount(int token, int cnt);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_GetParam(int token, int index);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_SetParam(int token, int index);
        // Global Lua Val
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr Del_GetLuaState();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_GetGlobal(string name);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_SetGlobal(string name);

        // Functions for Java Plugin
        internal class JavaEventHandler : IDisposable
        {
            internal IntPtr _Runnable;

            public void Call(string cate)
            {
                if (_Runnable != IntPtr.Zero)
                {
                    luaevent_calljava(cate, _Runnable);
                }
            }

            public void Dispose()
            {
                if (_Runnable != IntPtr.Zero)
                {
                    luaevent_releasejava(_Runnable);
                    _Runnable = IntPtr.Zero;
                }
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Del_RegJavaHandler(string cate, IntPtr runnable);

        internal static readonly Del_RegJavaHandler Func_RegJavaHandler = new Del_RegJavaHandler(RegJavaHandler);
        internal static readonly lua.CFunction Func_HandleJavaEvent = new lua.CFunction(HandleJavaEvent);

        [AOT.MonoPInvokeCallback(typeof(Del_RegJavaHandler))]
        public static int RegJavaHandler(string cate, IntPtr runnable)
        {
            var l = ContextLuaState;
            if (l != IntPtr.Zero)
            {
                using (var lr = new LuaStateRecover(l))
                {
                    l.PrepareLuaEventReg();
                    l.getfield(lua.LUA_REGISTRYINDEX, "___levt");
                    if (l.istable(-1))
                    {
                        l.getfield(-1, cate);
                        if (!l.istable(-1))
                        {
                            l.pop(1);
                            l.newtable();
                            l.pushvalue(-1);
                            l.setfield(-3, cate);
                        }
                        var cnt = l.getn(-1);
                        l.pushnumber(cnt + 1);
                        l.PushDisposableObj(new JavaEventHandler() { _Runnable = runnable });
                        l.pushcclosure(Func_HandleJavaEvent, 1);
                        l.settable(-3);
                        return cnt + 1;
                    }
                }
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int HandleJavaEvent(IntPtr l)
        {
            var top = l.gettop();
            var real = l.GetRawObj(lua.upvalueindex(1)) as JavaEventHandler;
            if (real != null)
            {
                var context = new LuaEventManager() { _L = l };
                PushLuaContext(context);
                var pars = new List<object>(top);
                context._P.Add(pars);
                context._P.Add(new List<object>());
                for (int i = 2; i <= top; ++i)
                {
                    pars.Add(l.GetLua(i));
                }

                //using (var lr = new LuaStateRecover(l))
                {
                    string key = l.tostring(1);
                    real.Call(key);
                }

                RemoveLuaContext(context);
                if (context._P.Count > 1)
                {
                    var rvs = context._P[1];
                    foreach (var rv in rvs)
                    {
                        l.PushLua(rv);
                    }
                }
            }
            return l.gettop() - top;
        }

#if DUMMY_NATIVE_EVENTS
        public static void luaevent_calljava(string cate, IntPtr pRunnable) { }
        public static void luaevent_releasejava(IntPtr pObj) { }
        public static void luaevent_init(
            string libPath
            , Del_RegHandler func_RegHandler
            , Del_RegJavaHandler func_RegJavaHandler
            , Del_UnregHandler func_UnregHandler
            , Del_NewCallToken func_NewCallToken
            , Del_TrigEvent func_TrigEvent
            , Del_GetValBool func_GetValBool
            , Del_GetValNum func_GetValNum
            , Del_GetValPtr func_GetValPtr
            , Del_GetValStrTo func_GetValStrTo
            , Del_SetValBool func_SetValBool
            , Del_SetValNum func_SetValNum
            , Del_SetValPtr func_SetValPtr
            , Del_SetValStr func_SetValStr
            , Del_UnsetVal func_UnsetVal
            , Del_GetParamCount func_GetParamCount
            , Del_SetParamCount func_SetParamCount
            , Del_GetParam func_GetParam
            , Del_SetParam func_SetParam
            , Del_GetLuaState func_GetLuaState
            , Del_GetGlobal func_GetGlobal
            , Del_SetGlobal func_SetGlobal
            ) { }
#else
        [DllImport("EventPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaevent_calljava(string cate, IntPtr pRunnable);

        [DllImport("EventPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaevent_releasejava(IntPtr pObj);

        [DllImport("EventPlugin", CallingConvention = CallingConvention.Cdecl)]
        // Init Func to push the global funcs from C# to C -- This is the whole capstone-plugin's entrance.
        // In C plugin code, after receiving these funcs, we can do further init in this call, i.e. load the distribute-plugin
        public static extern void luaevent_init(
            string libPath
            , Del_RegHandler func_RegHandler
            , Del_RegJavaHandler func_RegJavaHandler
            , Del_UnregHandler func_UnregHandler
            , Del_NewCallToken func_NewCallToken
            , Del_TrigEvent func_TrigEvent
            , Del_GetValBool func_GetValBool
            , Del_GetValNum func_GetValNum
            , Del_GetValPtr func_GetValPtr
            , Del_GetValStrTo func_GetValStrTo
            , Del_SetValBool func_SetValBool
            , Del_SetValNum func_SetValNum
            , Del_SetValPtr func_SetValPtr
            , Del_SetValStr func_SetValStr
            , Del_UnsetVal func_UnsetVal
            , Del_GetParamCount func_GetParamCount
            , Del_SetParamCount func_SetParamCount
            , Del_GetParam func_GetParam
            , Del_SetParam func_SetParam
            , Del_GetLuaState func_GetLuaState
            , Del_GetGlobal func_GetGlobal
            , Del_SetGlobal func_SetGlobal
            );
#endif

        public static void luaevent_init(
            Del_RegHandler func_RegHandler
            , Del_UnregHandler func_UnregHandler
            , Del_NewCallToken func_NewCallToken
            , Del_TrigEvent func_TrigEvent
            , Del_GetValBool func_GetValBool
            , Del_GetValNum func_GetValNum
            , Del_GetValPtr func_GetValPtr
            , Del_GetValStr func_GetValStr
            , Del_GetValStrTo func_GetValStrTo
            , Del_SetValBool func_SetValBool
            , Del_SetValNum func_SetValNum
            , Del_SetValPtr func_SetValPtr
            , Del_SetValStr func_SetValStr
            , Del_UnsetVal func_UnsetVal
            , Del_GetParamCount func_GetParamCount
            , Del_SetParamCount func_SetParamCount
            , Del_GetParam func_GetParam
            , Del_SetParam func_SetParam
            , Del_GetLuaState func_GetLuaState
            , Del_GetGlobal func_GetGlobal
            , Del_SetGlobal func_SetGlobal
            )
        {
            try
            {
                string appName = "";
                try
                {
                    appName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(UnityEngine.Application.persistentDataPath));
                }
                catch { }
                if (string.IsNullOrEmpty(appName))
                {
                    try
                    {
                        appName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(UnityEngine.Application.temporaryCachePath));
                    }
                    catch { }
                }
                if (string.IsNullOrEmpty(appName))
                {
                    try
                    {
                        appName = System.IO.Path.GetFileNameWithoutExtension(UnityEngine.Application.dataPath);
                        if (System.Text.RegularExpressions.Regex.IsMatch(appName, @"-\d+\z"))
                        {
                            appName = appName.Substring(0, appName.LastIndexOf('-'));
                        }
                    }
                    catch { }
                }
                if (string.IsNullOrEmpty(appName))
                {
                    appName = "";
                }
                var libPath = "/data/data/" + appName + "/lib/";
                luaevent_init(
                    libPath
                    , func_RegHandler
                    , Func_RegJavaHandler
                    , func_UnregHandler
                    , func_NewCallToken
                    , func_TrigEvent
                    , func_GetValBool
                    , func_GetValNum
                    , func_GetValPtr
                    , func_GetValStrTo
                    , func_SetValBool
                    , func_SetValNum
                    , func_SetValPtr
                    , func_SetValStr
                    , func_UnsetVal
                    , func_GetParamCount
                    , func_SetParamCount
                    , func_GetParam
                    , func_SetParam
                    , func_GetLuaState
                    , func_GetGlobal
                    , func_SetGlobal
                    );
            }
            catch (Exception e)
            {
                if(GLog.IsLogErrorEnabled) GLog.LogException(e);
            }
        }
#else
        // Each Handler
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CEventHandler(string cate);
        // Reg & Unreg
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Del_RegHandler(string cate, CEventHandler handler);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_UnregHandler(string cate, int refid);
        // Call Other
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Del_NewCallToken(); // token 0 means params and token 1 means returns.
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_TrigEvent(string cate, int token);
        // Get Value
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool Del_GetValBool();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double Del_GetValNum();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr Del_GetValPtr();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate string Del_GetValStr();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Del_GetValStrTo(IntPtr pstr);
        // Set Value
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_SetValBool(bool val);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_SetValNum(double num);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_SetValPtr(IntPtr ptr);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_SetValStr(string str);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_UnsetVal();
        // Get Params
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Del_GetParamCount(int token);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_SetParamCount(int token, int cnt);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_GetParam(int token, int index);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_SetParam(int token, int index);
        // Global Lua Val
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr Del_GetLuaState();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_GetGlobal(string name);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Del_SetGlobal(string name);

        // Init Func to push the global funcs from C# to C -- This is the whole capstone-plugin's entrance.
        // In C plugin code, after receiving these funcs, we can do further init in this call, i.e. load the distribute-plugin
        public static void luaevent_init(
            Del_RegHandler func_RegHandler
            , Del_UnregHandler func_UnregHandler
            , Del_NewCallToken func_NewCallToken
            , Del_TrigEvent func_TrigEvent
            , Del_GetValBool func_GetValBool
            , Del_GetValNum func_GetValNum
            , Del_GetValPtr func_GetValPtr
            , Del_GetValStr func_GetValStr
            , Del_GetValStrTo func_GetValStrTo
            , Del_SetValBool func_SetValBool
            , Del_SetValNum func_SetValNum
            , Del_SetValPtr func_SetValPtr
            , Del_SetValStr func_SetValStr
            , Del_UnsetVal func_UnsetVal
            , Del_GetParamCount func_GetParamCount
            , Del_SetParamCount func_SetParamCount
            , Del_GetParam func_GetParam
            , Del_SetParam func_SetParam
            , Del_GetLuaState func_GetLuaState
            , Del_GetGlobal func_GetGlobal
            , Del_SetGlobal func_SetGlobal
            ) { }
#endif
        #endregion

        internal static readonly Del_RegHandler Func_RegHandler = new Del_RegHandler(RegHandler);
        internal static readonly Del_UnregHandler Func_UnregHandler = new Del_UnregHandler(UnregHandler);
        internal static readonly Del_NewCallToken Func_NewCallToken = new Del_NewCallToken(NewCallToken);
        internal static readonly Del_TrigEvent Func_TrigEvent = new Del_TrigEvent(TrigEvent);
        internal static readonly Del_GetValBool Func_GetValBool = new Del_GetValBool(GetValBool);
        internal static readonly Del_GetValNum Func_GetValNum = new Del_GetValNum(GetValNum);
        internal static readonly Del_GetValPtr Func_GetValPtr = new Del_GetValPtr(GetValPtr);
        internal static readonly Del_GetValStr Func_GetValStr = new Del_GetValStr(GetValStr);
        internal static readonly Del_GetValStrTo Func_GetValStrTo = new Del_GetValStrTo(GetValStrTo);
        internal static readonly Del_SetValBool Func_SetValBool = new Del_SetValBool(SetValBool);
        internal static readonly Del_SetValNum Func_SetValNum = new Del_SetValNum(SetValNum);
        internal static readonly Del_SetValPtr Func_SetValPtr = new Del_SetValPtr(SetValPtr);
        internal static readonly Del_SetValStr Func_SetValStr = new Del_SetValStr(SetValStr);
        internal static readonly Del_UnsetVal Func_UnsetVal = new Del_UnsetVal(UnsetVal);
        internal static readonly Del_GetParamCount Func_GetParamCount = new Del_GetParamCount(GetParamCount);
        internal static readonly Del_SetParamCount Func_SetParamCount = new Del_SetParamCount(SetParamCount);
        internal static readonly Del_GetParam Func_GetParam = new Del_GetParam(GetParam);
        internal static readonly Del_SetParam Func_SetParam = new Del_SetParam(SetParam);
        internal static readonly Del_GetLuaState Func_GetLuaState = new Del_GetLuaState(GetLuaState);
        internal static readonly Del_GetGlobal Func_GetGlobal = new Del_GetGlobal(GetGlobal);
        internal static readonly Del_SetGlobal Func_SetGlobal = new Del_SetGlobal(SetGlobal);

        [AOT.MonoPInvokeCallback(typeof(Del_RegHandler))]
        public static int RegHandler(string cate, CEventHandler handler)
        {
            var l = ContextLuaState;
            if (l != IntPtr.Zero)
            {
                using (var lr = new LuaStateRecover(l))
                {
                    l.PrepareLuaEventReg();
                    l.getfield(lua.LUA_REGISTRYINDEX, "___levt");
                    if (l.istable(-1))
                    {
                        l.getfield(-1, cate);
                        if (!l.istable(-1))
                        {
                            l.pop(1);
                            l.newtable();
                            l.pushvalue(-1);
                            l.setfield(-3, cate);
                        }
                        var cnt = l.getn(-1);
                        l.pushnumber(cnt + 1);
                        l.PushRawObj(handler);
                        l.pushcclosure(Func_HandleNativeEvent, 1);
                        l.settable(-3);
                        return cnt + 1;
                    }
                }
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(Del_UnregHandler))]
        public static void UnregHandler(string cate, int refid)
        {
            var l = ContextLuaState;
            if (l != IntPtr.Zero)
            {
                using (var lr = new LuaStateRecover(l))
                {
                    l.getfield(lua.LUA_REGISTRYINDEX, "___levt");
                    if (l.istable(-1))
                    {
                        if (refid <= 0)
                        {
                            l.pushnil();
                            l.setfield(-2, cate);
                        }
                        else
                        {
                            l.getfield(-1, cate);
                            if (l.istable(-1))
                            {
                                var cnt = l.getn(-1);
                                l.pushnumber(refid);
                                l.pushnil();
                                l.settable(-3);
                                for (int i = refid; i <= cnt; ++i)
                                {
                                    l.pushnumber(i);
                                    l.pushnumber(i + 1);
                                    l.gettable(-3);
                                    l.settable(-3);
                                }
                            }
                        }
                    }
                }
            }
        }

        [AOT.MonoPInvokeCallback(typeof(Del_NewCallToken))]
        public static int NewCallToken()
        {
            var token = 0;
            var context = LuaEventContext;
            if (context != null)
            {
                if (context._IsRoot)
                {
                    token = 0;
                }
                else
                {
                    token = context._P.Count;
                    context._P.Add(new List<object>());
                }
            }
            return token;
        }

        [AOT.MonoPInvokeCallback(typeof(Del_TrigEvent))]
        public static void TrigEvent(string cate, int token)
        {
            if (CurThreadId == LastThreadId)
            {
                TrigEventRaw(LuaEventContext, cate, token);
            }
            else
            {
                //TrigEventRaw(LuaEventContext, "___Immediate_" + (cate != null ? cate : ""), token);
                var context = LuaEventContext;
                _RootLuaEventContext = null;
                var callInfo = new DelayedEventCallInfo() { Context = context, Cate = cate, Token = token };
                lock (delayedEvents)
                {
                    delayedEvents.AddLast(callInfo);
                }
            }
        }

        [AOT.MonoPInvokeCallback(typeof(Del_GetValBool))]
        public static bool GetValBool()
        {
            var val = ContextExchangeObj;
            if (val != null)
            {
                var cval = val.ConvertType(typeof(bool));
                return cval == null ? false : (bool)cval;
            }
            return false;
        }

        [AOT.MonoPInvokeCallback(typeof(Del_GetValNum))]
        public static double GetValNum()
        {
            var val = ContextExchangeObj;
            if (val != null)
            {
                var cval = val.ConvertType(typeof(double));
                return cval == null ? 0 : (double)cval;
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(Del_GetValPtr))]
        public static IntPtr GetValPtr()
        {
            var val = ContextExchangeObj;
            if (val != null)
            {
                var cval = val.ConvertType(typeof(IntPtr));
                return cval == null ? IntPtr.Zero : (IntPtr)cval;
            }
            return IntPtr.Zero;
        }

        [AOT.MonoPInvokeCallback(typeof(Del_GetValStr))]
        public static string GetValStr()
        {
            var val = ContextExchangeObj;
            if (val != null)
            {
                var cval = val.ConvertType(typeof(string));
                return cval == null ? "" : (string)cval;
            }
            return "";
        }

        [AOT.MonoPInvokeCallback(typeof(Del_GetValStrTo))]
        public static int GetValStrTo(IntPtr pstr)
        {
#if (UNITY_WP8 || UNITY_METRO) && !UNITY_EDITOR
            var enc = System.Text.Encoding.UTF8;
#elif UNITY_IPHONE && !UNITY_EDITOR
            var enc = System.Text.Encoding.UTF8;
#else
            var enc = System.Text.Encoding.Default;
#endif
            var str = GetValStr();
            if (pstr == IntPtr.Zero)
            {
                return enc.GetMaxByteCount(str.Length) + 1;
            }
            else
            {
                var bytes = new byte[enc.GetMaxByteCount(str.Length + 1)];
                var enclen = enc.GetBytes(str, 0, str.Length, bytes, 0);
                Marshal.Copy(bytes, 0, pstr, enclen);
                Marshal.WriteByte(pstr, enclen, 0);
                return bytes.Length + 1;
            }
        }

        [AOT.MonoPInvokeCallback(typeof(Del_SetValBool))]
        public static void SetValBool(bool val)
        {
            ContextExchangeObj = val;
        }

        [AOT.MonoPInvokeCallback(typeof(Del_SetValNum))]
        public static void SetValNum(double num)
        {
            ContextExchangeObj = num;
        }

        [AOT.MonoPInvokeCallback(typeof(Del_SetValPtr))]
        public static void SetValPtr(IntPtr ptr)
        {
            ContextExchangeObj = ptr;
        }

        [AOT.MonoPInvokeCallback(typeof(Del_SetValStr))]
        public static void SetValStr(string str)
        {
            ContextExchangeObj = str;
        }

        [AOT.MonoPInvokeCallback(typeof(Del_UnsetVal))]
        public static void UnsetVal()
        {
            ContextExchangeObj = null;
        }

        [AOT.MonoPInvokeCallback(typeof(Del_GetParamCount))]
        public static int GetParamCount(int token)
        {
            int cnt = 0;
            var context = LuaEventContext;
            if (context != null)
            {
                if (token >= 0 && token < context._P.Count)
                {
                    var p = context._P[token];
                    cnt = p.Count;
                }
            }
            return cnt;
        }

        [AOT.MonoPInvokeCallback(typeof(Del_SetParamCount))]
        public static void SetParamCount(int token, int cnt)
        {
            var context = LuaEventContext;
            if (context != null)
            {
                if (token >= 0 && token < context._P.Count)
                {
                    var p = context._P[token];
                    if (cnt > p.Count)
                    {
                        for (int i = p.Count; i < cnt; ++i)
                        {
                            p.Add(null);
                        }
                    }
                    else if (cnt < p.Count)
                    {
                        p.RemoveRange(cnt, p.Count - cnt);
                    }
                }
            }
        }

        [AOT.MonoPInvokeCallback(typeof(Del_GetParam))]
        public static void GetParam(int token, int index)
        {
            ContextExchangeObj = null;
            var context = LuaEventContext;
            if (context != null)
            {
                if (token >= 0 && token < context._P.Count)
                {
                    var p = context._P[token];
                    if (index >= 0 && index < p.Count)
                    {
                        ContextExchangeObj = p[index];
                    }
                }
            }
        }

        [AOT.MonoPInvokeCallback(typeof(Del_SetParam))]
        public static void SetParam(int token, int index)
        {
            var context = LuaEventContext;
            if (context != null)
            {
                if (token >= 0 && token < context._P.Count)
                {
                    var p = context._P[token];
                    if (index >= 0)
                    {
                        if (index >= p.Count)
                        {
                            SetParamCount(token, index + 1);
                        }
                        p[index] = ContextExchangeObj;
                    }
                }
            }
        }

        [AOT.MonoPInvokeCallback(typeof(Del_GetLuaState))]
        public static IntPtr GetLuaState()
        {
            return ContextLuaState;
        }

        [AOT.MonoPInvokeCallback(typeof(Del_GetGlobal))]
        public static void GetGlobal(string name)
        {
            ContextExchangeObj = null;
            var l = ContextLuaState;
            if (l != IntPtr.Zero)
            {
                ContextExchangeObj = l.GetHierarchical(lua.LUA_GLOBALSINDEX, name);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(Del_SetGlobal))]
        public static void SetGlobal(string name)
        {
            var l = ContextLuaState;
            if (l != IntPtr.Zero)
            {
                l.SetHierarchical(lua.LUA_GLOBALSINDEX, name, ContextExchangeObj);
            }
        }

        [ThreadStatic] private static int CurThreadId;
        private static int LastThreadId = 0;
        public static void Init(IntPtr L)
        {
            ++LastThreadId;
            CurThreadId = LastThreadId;

            L.newtable(); // luaevt
            L.pushvalue(-1); // luaevt luaevt
            L.SetGlobal("luaevt"); // luaevt
            L.pushcfunction(Func_TrigLuaEvent); // luaevt func
            L.SetField(-2, "trig"); // luaevt
            L.pushcfunction(Func_RegLuaEventHandler); // luaevt func
            L.SetField(-2, "reg"); // luaevt
            L.pushcfunction(Func_UnregLuaEventHandler); // luaevt func
            L.SetField(-2, "unreg"); // luaevt
            L.pushcfunction(Func_ResetLuaEventReg); // luaevt func
            L.SetField(-2, "reset"); // luaevt
            L.pushcfunction(Func_LuaDelayedEvents); // luaevt func
            L.SetField(-2, "delayed"); // luaevt

            var context = RootLuaEventContext;
            context._L = L;
            PushLuaContext(context);
            LuaEventManager.RegGlobalEventManager(context);

            InitNativeEventPlugin();
        }

        internal static void InitNativeEventPlugin()
        {
            luaevent_init(
                Func_RegHandler
                , Func_UnregHandler
                , Func_NewCallToken
                , Func_TrigEvent
                , Func_GetValBool
                , Func_GetValNum
                , Func_GetValPtr
                , Func_GetValStr
                , Func_GetValStrTo
                , Func_SetValBool
                , Func_SetValNum
                , Func_SetValPtr
                , Func_SetValStr
                , Func_UnsetVal
                , Func_GetParamCount
                , Func_SetParamCount
                , Func_GetParam
                , Func_SetParam
                , Func_GetLuaState
                , Func_GetGlobal
                , Func_SetGlobal
                );
        }

        internal static readonly lua.CFunction Func_TrigLuaEvent = new lua.CFunction(TrigLuaEvent);
        internal static readonly lua.CFunction Func_HandleNativeEvent = new lua.CFunction(HandleNativeEvent);
        internal static readonly lua.CFunction Func_RegLuaEventHandler = new lua.CFunction(RegLuaEventHandler);
        internal static readonly lua.CFunction Func_UnregLuaEventHandler = new lua.CFunction(UnregLuaEventHandler);
        internal static readonly lua.CFunction Func_ResetLuaEventReg = new lua.CFunction(ResetLuaEventReg);
        internal static readonly lua.CFunction Func_LuaDelayedEvents = new lua.CFunction(LuaDelayedEvents);

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int TrigLuaEvent(IntPtr l)
        {
            if (l != IntPtr.Zero)
            {
                var top = l.gettop();
                //var cnt = Math.Max(top - 1, 0);
                var pars = new List<object>(top);
                for (int i = 1; i <= top; ++i)
                {
                    pars.Add(l.GetLua(i));
                }
                var args = pars.ToArray();
                object[] rvs = null;

                using (var lr = new LuaStateRecover(l))
                {
                    l.getfield(lua.LUA_REGISTRYINDEX, "___levt");
                    if (l.istable(-1))
                    {
                        l.pushvalue(1);
                        l.gettable(-2);
                        if (l.istable(-1))
                        {
                            var hcnt = l.getn(-1);
                            for (int i = 1; i <= hcnt; ++i)
                            {
                                l.pushnumber(i);
                                l.gettable(-2);
                                if (l.isfunction(-1))
                                {
                                    rvs = new LuaOnStackFunc(l, -1).Call(args);
                                }
                                else
                                {
                                    rvs = null;
                                }
                                l.pop(1);
                            }
                        }
                    }
                }

                if (rvs != null)
                {
                    foreach (var rv in rvs)
                    {
                        l.PushLua(rv);
                    }
                }
                return l.gettop() - top;
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int HandleNativeEvent(IntPtr l)
        {
            var top = l.gettop();
            var real = l.GetRawObj(lua.upvalueindex(1)) as CEventHandler;
            if (real != null)
            {
                var context = new LuaEventManager() { _L = l };
                PushLuaContext(context);
                var pars = new List<object>(top);
                context._P.Add(pars);
                context._P.Add(new List<object>());
                for (int i = 2; i <= top; ++i)
                {
                    pars.Add(l.GetLua(i));
                }

                //using (var lr = new LuaStateRecover(l))
                {
                    string key = l.tostring(1);
                    real(key);
                }

                RemoveLuaContext(context);
                if (context._P.Count > 1)
                {
                    var rvs = context._P[1];
                    foreach (var rv in rvs)
                    {
                        l.PushLua(rv);
                    }
                }
            }
            return l.gettop() - top;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int RegLuaEventHandler(IntPtr l)
        {
            if (l != IntPtr.Zero)
            {
                var refid = 0;
                using (var lr = new LuaStateRecover(l))
                {
                    l.PrepareLuaEventReg();
                    l.getfield(lua.LUA_REGISTRYINDEX, "___levt");
                    if (l.istable(-1))
                    {
                        l.pushvalue(1);
                        l.gettable(-2);
                        if (!l.istable(-1))
                        {
                            l.pop(1);
                            l.newtable();
                            l.pushvalue(1);
                            l.pushvalue(-2);
                            l.settable(-4);
                        }
                        var cnt = l.getn(-1);
                        l.pushnumber(cnt + 1);
                        l.pushvalue(2);
                        l.settable(-3);
                        refid = cnt + 1;
                    }
                }
                if (refid != 0)
                {
                    l.pushnumber(refid);
                    return 1;
                }
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int UnregLuaEventHandler(IntPtr l)
        {
            if (l != IntPtr.Zero)
            {
                using (var lr = new LuaStateRecover(l))
                {
                    l.getfield(lua.LUA_REGISTRYINDEX, "___levt");
                    if (l.istable(-1))
                    {
                        if (lr.Top < 2 || l.isnoneornil(2))
                        {
                            l.pushvalue(1);
                            l.pushnil();
                            l.settable(-3);
                        }
                        else
                        {
                            l.pushvalue(1);
                            l.gettable(-2);
                            if (l.istable(-1))
                            {
                                l.pushvalue(2);
                                l.pushnil();
                                l.settable(-3);
                            }
                        }
                    }
                }
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ResetLuaEventReg(IntPtr l)
        {
            if (l != IntPtr.Zero)
            {
                using (var lr = new LuaStateRecover(l))
                {
                    l.newtable();
                    l.SetField(lua.LUA_REGISTRYINDEX, "___levt");
                }
            }
            lock (delayedEvents)
            {
                delayedEvents.Clear();
            }
            InitNativeEventPlugin();
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaDelayedEvents(IntPtr l)
        {
            if (l != IntPtr.Zero)
            {
                var top = l.gettop();
                DelayedTrigEvents();
                return l.gettop() - top;
            }
            return 0;
        }

        public static void PrepareLuaEventReg(this IntPtr l)
        {
            if (l != IntPtr.Zero)
            {
                using (var lr = new LuaStateRecover(l))
                {
                    l.getfield(lua.LUA_REGISTRYINDEX, "___levt");
                    if (!l.istable(-1))
                    {
                        l.pop(1);
                        l.newtable();
                        l.SetField(lua.LUA_REGISTRYINDEX, "___levt");
                    }
                }
            }
        }

        public static object[] TrigClrEvent(string cate, params object[] args)
        {
            var token = NewCallToken();
            int argcnt = args == null ? 0 : args.Length;
            SetParamCount(token, argcnt);
            for (int i = 0; i < argcnt; ++i)
            {
                ContextExchangeObj = args[i];
                SetParam(token, i);
            }
            TrigEvent(cate, token);
            int rvcnt = GetParamCount(token);
            var rv = new object[rvcnt];
            for (int i = 0; i < rvcnt; ++i)
            {
                GetParam(token, i);
                rv[i] = ContextExchangeObj;
            }
            return rv;
        }
        public static T TrigClrEvent<T>(string cate, params object[] args)
        {
            var rv = TrigClrEvent(cate, args);
            if (rv == null || rv.Length < 1)
            {
                return default(T);
            }
            return rv[0].ConvertType<T>();
        }

        [ThreadStatic] private static LuaEventManager _RootLuaEventContext;
        internal static LuaEventManager RootLuaEventContext
        {
            get
            {
                if (_RootLuaEventContext == null)
                {
                    var context = new LuaEventManager();
                    context._P.Add(new List<object>());
                    context._P.Add(new List<object>());
                    context._IsRoot = true;
                    _RootLuaEventContext = context;

                    if (LuaContextStack.Count > 0)
                    {
                        context._L = LuaContextStack.First.Value._L;
                    }
                }
                return _RootLuaEventContext;
            }
        }
        internal static LinkedList<LuaEventManager> LuaContextStack = new LinkedList<LuaEventManager>();
        internal static Dictionary<LuaEventManager, LinkedListNode<LuaEventManager>> LuaContextIndex = new Dictionary<LuaEventManager, LinkedListNode<LuaEventManager>>();
        internal static LuaEventManager LuaEventContext
        {
            get
            {
                if (CurThreadId == LastThreadId)
                {
                    if (LuaContextStack.Count > 0)
                    {
                        return LuaContextStack.Last.Value;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return RootLuaEventContext;
                }
            }
        }
        public static IntPtr ContextLuaState
        {
            get
            {
                var context = LuaEventContext;
                if (context != null)
                {
                    return context._L;
                }
                else
                {
                    return IntPtr.Zero;
                }
            }
        }
        public static object ContextExchangeObj
        {
            get
            {
                var context = LuaEventContext;
                if (context != null)
                {
                    return context._O;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                var context = LuaEventContext;
                if (context != null)
                {
                    context._O = value;
                }
            }
        }
        internal static void PushLuaContext(LuaEventManager context)
        {
            if (context != null)
            {
                LuaContextIndex[context] = LuaContextStack.AddLast(context);
            }
        }
        internal static void PopLuaContext()
        {
            var context = LuaEventContext;
            if (context != null)
            {
                LuaContextIndex.Remove(context);
                LuaContextStack.RemoveLast();
            }
        }
        internal static void RemoveLuaContext(LuaEventManager context)
        {
            if (context != null)
            {
                LinkedListNode<LuaEventManager> node = null;
                LuaContextIndex.TryGetValue(context, out node);
                if (node != null)
                {
                    LuaContextIndex.Remove(context);
                    LuaContextStack.Remove(node);
                }
            }
        }
        internal static void TrigEventRaw(LuaEventManager context, string cate, int token)
        {
            if (context != null)
            {
                var l = context._L;
                if (l != IntPtr.Zero)
                {
                    using (var lr = new LuaStateRecover(l))
                    {
                        object[] args = null;
                        if (context != null)
                        {
                            if (token >= 0 && token < context._P.Count)
                            {
                                var p = context._P[token];
                                if (p != null)
                                {
                                    args = new object[p.Count + 1];
                                    args[0] = cate;
                                    p.CopyTo(args, 1);
                                }
                            }
                        }
                        if (args == null)
                        {
                            args = new object[] { cate };
                        }

                        l.pushcfunction(Func_TrigLuaEvent);
                        var rvs = new LuaOnStackFunc(l, -1).Call(args);

                        if (context != null)
                        {
                            if (token >= 0 && token < context._P.Count)
                            {
                                List<object> p = null;
                                if (rvs != null && rvs.Length > 0)
                                {
                                    p = new List<object>(rvs.Length);
                                    foreach (var rv in rvs)
                                    {
                                        p.Add(rv);
                                    }
                                }
                                else
                                {
                                    p = new List<object>();
                                }
                                context._P[token] = p;
                            }
                        }
                    }
                }
            }
        }
        internal static LinkedList<DelayedEventCallInfo> delayedEvents = new LinkedList<DelayedEventCallInfo>();
        internal static void DelayedTrigEvents()
        {
            while (delayedEvents.Count > 0)
            {
                DelayedEventCallInfo callInfo = null;
                lock (delayedEvents)
                {
                    if (delayedEvents.Count > 0)
                    {
                        callInfo = delayedEvents.First.Value;
                        delayedEvents.RemoveFirst();
                    }
                }
                if (callInfo != null)
                {
                    TrigEventRaw(callInfo.Context, callInfo.Cate, callInfo.Token);
                }
            }
        }
    }

    internal class LuaEventManager
    {
        internal IntPtr _L;
        internal object _O;
        internal List<List<object>> _P = new List<List<object>>();
        internal bool _IsRoot = false;

        internal static readonly lua.CFunction FuncLuaGlobalEventManagerGc = new lua.CFunction(LuaGlobalEventManagerMetaGc);

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        internal static int LuaGlobalEventManagerMetaGc(IntPtr l)
        {
            using (var lr = new LuaStateRecover(l))
            {
                var oldtop = l.gettop();
                if (oldtop < 1)
                    return 0;

                if (l.isuserdata(1) && !l.islightuserdata(1))
                {
                    try
                    {
                        IntPtr pud = l.touserdata(1);
                        IntPtr hval = Marshal.ReadIntPtr(pud);
                        GCHandle handle = (GCHandle)hval;
                        LuaEventManager context = handle.Target as LuaEventManager;
                        handle.Free();
                        if (context != null)
                        {
                            LuaEvent.RemoveLuaContext(context);
                        }
                    }
                    catch (Exception e)
                    {
                        if(GLog.IsLogErrorEnabled) GLog.LogException(e);
                    }
                }
            }
            return 0;
        }

        internal static void RegGlobalEventManager(LuaEventManager context)
        {
            if (context != null && context._L != IntPtr.Zero)
            {
                var l = context._L;
                using (var lr = new LuaStateRecover(l))
                {
                    l.newtable(); // meta
                    l.pushcfunction(FuncLuaGlobalEventManagerGc); // meta, func
                    l.SetField(-2, "__gc"); // meta
                    IntPtr pud = l.newuserdata(new IntPtr(Marshal.SizeOf(typeof(IntPtr)))); // meta, ud
                    var handle = GCHandle.Alloc(context);
                    Marshal.WriteIntPtr(pud, (IntPtr)handle);
                    l.insert(-2); // ud meta
                    l.setmetatable(-2); // ud
                    l.SetField(lua.LUA_REGISTRYINDEX, "___levtgm");
                }
            }
        }
    }

    internal class DelayedEventCallInfo
    {
        public LuaEventManager Context { get; set; }
        public string Cate { get; set; }
        public int Token { get; set; }
    }
}