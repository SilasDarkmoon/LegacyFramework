using System;
using Capstones.Dynamic;
using Capstones.LuaLib;
using Capstones.LuaWrap;
using Capstones.PlatExt;

using lua = Capstones.LuaLib.LuaCoreLib;

namespace Capstones.LuaExt
{
    public static class LuaFramework
    {
        internal static string _OldPath = null;
        internal static int _LoaderIndex = 0;

        private static string _cached_Application_platform = null;
        private static string _cached_Application_streamingAssetsPath = null;
        private static string _cached_Application_temporaryCachePath = null;
        private static string _cached_Application_dataPath = null;
        private static string _cached_UpdatePath = null;
        private static string _cached_Capid = null;

        public static string AppPlatform
        {
            get
            {
                if (_cached_Application_platform == null)
                {
                    _cached_Application_platform = UnityEngine.Application.platform.ToString();
                }
                return _cached_Application_platform;
            }
        }
        public static string AppStreamingAssetsPath
        {
            get
            {
                if (_cached_Application_streamingAssetsPath == null)
                {
                    _cached_Application_streamingAssetsPath = UnityEngine.Application.streamingAssetsPath;
                }
                return _cached_Application_streamingAssetsPath;
            }
        }
        public static string AppTemporaryCachePath
        {
            get
            {
                if (_cached_Application_temporaryCachePath == null)
                {
                    _cached_Application_temporaryCachePath = UnityEngine.Application.temporaryCachePath;
                }
                return _cached_Application_temporaryCachePath;
            }
        }
        public static string AppDataPath
        {
            get
            {
                if (_cached_Application_dataPath == null)
                {
                    _cached_Application_dataPath = UnityEngine.Application.dataPath;
                }
                return _cached_Application_dataPath;
            }
        }
        public static string AppUpdatePath
        {
            get
            {
                if (_cached_UpdatePath == null)
                {
                    _cached_UpdatePath = Capstones.UnityFramework.ResManager.UpdatePath;
                }
                return _cached_UpdatePath;
            }
        }

        private static int NextThreadId = 1;
        [ThreadStatic] private static int CurrentThreadId;
        private static int MainThreadId = 0;

        public static void Init(IntPtr L)
        {
            if (CurrentThreadId <= 0)
            {
                CurrentThreadId = NextThreadId++;
                if (MainThreadId <= 0)
                {
                    MainThreadId = CurrentThreadId;
                }
            }

            if (L != IntPtr.Zero)
            {
                L.atpanic(ClrDelPanic);

                ClrFuncReset(L);

                L.pushcfunction(ClrDelPrint); // func
                L.SetGlobal("print"); // (empty)
                L.pushcfunction(ClrDelPrintWarning); // func
                L.SetGlobal("printw"); // (empty)
                L.pushcfunction(ClrDelPrintError); // func
                L.SetGlobal("printe"); // (empty)
            }
        }

        public static readonly lua.CFunction ClrDelCoroutine = new lua.CFunction(ClrFuncCoroutine);
        public static readonly lua.CFunction ClrDelBehavCoroutine = new lua.CFunction(ClrFuncBehavCoroutine);
        public static readonly lua.CFunction ClrDelPrint = new lua.CFunction(ClrFuncPrint);
        public static readonly lua.CFunction ClrDelPrintWarning = new lua.CFunction(ClrFuncPrintWarning);
        public static readonly lua.CFunction ClrDelPrintError = new lua.CFunction(ClrFuncPrintError);
        public static readonly lua.CFunction ClrDelPanic = new lua.CFunction(ClrFuncPanic);
        public static readonly lua.CFunction ClrDelReset = new lua.CFunction(ClrFuncReset);
        public static readonly lua.CFunction ClrDelApkLoader = new lua.CFunction(ClrFuncApkLoader);
        public static readonly lua.CFunction ClrDelEncryptPostData = new lua.CFunction(ClrFuncEncryptPostData);
        public static readonly lua.CFunction ClrDelResponse = new lua.CFunction(ClrFuncParseResponse);
        public static readonly lua.CFunction ClrDelGetCapID = new lua.CFunction(ClrFuncGetCapID);
        public static readonly lua.CFunction ClrDelSplitStr = new lua.CFunction(ClrFuncSplitStr);
        public static readonly lua.CFunction ClrDelGetLangValueOfUserDataType = new lua.CFunction(ClrFuncGetLangValueOfUserDataType);
        public static readonly lua.CFunction ClrDelGetLangValueOfStringType = new lua.CFunction(ClrFuncGetLangValueOfStringType);
        public static readonly lua.CFunction ClrDelErrorHandler = new lua.CFunction(ClrFuncErrorHandler);

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncCoroutine(IntPtr l)
        {
            var oldtop = l.gettop();

            if (l.isfunction(1))
            {
                var lfunc = new LuaFunc(l, 1);
                var co = UnityFramework.UnityLua.StartLuaCoroutine(lfunc);
                l.settop(oldtop);
                l.PushLua(co);
            }

            return l.gettop() - oldtop;
        }
        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncBehavCoroutine(IntPtr l)
        {
            var oldtop = l.gettop();

            if (l.isuserdata(1) && !l.islightuserdata(1) && l.isfunction(2))
            {
                var go = l.GetLua(1).UnwrapDynamic() as UnityEngine.MonoBehaviour;
                var lfunc = new LuaFunc(l, 2);
                var co = UnityFramework.UnityLua.StartLuaCoroutineForBehav(go, lfunc);
                l.settop(oldtop);
                l.PushLua(co);
            }

            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncPrint(IntPtr l)
        {
            using (var lr = new LuaStateRecover(l))
            {
                var obj = l.GetLua(1).UnwrapDynamic();
                if (GLog.IsLogInfoEnabled) GLog.LogInfo(obj);
            }
            return 0;
        }
        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncPrintWarning(IntPtr l)
        {
            using (var lr = new LuaStateRecover(l))
            {
                var obj = l.GetLua(1).UnwrapDynamic();
                if (GLog.IsLogWarningEnabled) GLog.LogWarning(obj);
            }
            return 0;
        }
        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncPrintError(IntPtr l)
        {
            using (var lr = new LuaStateRecover(l))
            {
                var obj = l.GetLua(1).UnwrapDynamic();
                if (GLog.IsLogErrorEnabled) GLog.LogError(obj);
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncPanic(IntPtr l)
        {
            if (GLog.IsLogErrorEnabled) GLog.LogError(l.tostring(-1));
            return 0;
        }

        private static string GetLuaPackagePath()
        {
#if UNITY_EDITOR
            return AppDataPath + "/CapstonesScripts/spt/?.lua;" + AppUpdatePath + "/spt/?.lua;";
#else
            if (AppStreamingAssetsPath.Contains("://"))
                return AppUpdatePath + "/spt/?.lua;";
            else
                return AppUpdatePath + "/spt/?.lua;" + AppStreamingAssetsPath + "/spt/?.lua;";
#endif
        }

        private static string GetLuaDistributePath(string flag)
        {
#if UNITY_EDITOR
            return AppDataPath + "/CapstonesScripts/distribute/" + flag + "/?.lua;" + AppUpdatePath + "/distribute/" + flag + "/?.lua;";
#else
            if (AppStreamingAssetsPath.Contains("://"))
                return AppUpdatePath + "/distribute/" + flag + "/?.lua;";
            else
                return AppUpdatePath + "/spt/distribute/" + flag + "/?.lua;" + AppStreamingAssetsPath + "/spt/distribute/" + flag + "/?.lua;";
#endif
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncReset(IntPtr l)
        {
            // old package.path
            if (_OldPath == null)
            {
                _OldPath = "";
                using (var lr = new LuaStateRecover(l))
                {
                    // package.path
                    l.GetGlobal("package"); // package
                    if (l.istable(-1))
                    {
                        l.GetField(-1, "path"); // package path
                        if (l.isstring(-1))
                        {
                            _OldPath = l.tostring(-1);
                        }
                        l.pop(1); // package
                        l.GetField(-1, "loaders"); // package loaders
                        if (l.istable(-1))
                        {
                            _LoaderIndex = l.getn(-1) + 1;
                        }
                    }
                }
            }
            else
            {
                if (MainThreadId == CurrentThreadId)
                {
                    Capstones.UnityFramework.ResManager.UnloadAllRes();
                    Capstones.UnityFramework.ResManager.ReloadDistributeFlags();
                }
            }

            // package.path
            l.GetGlobal("package"); // package
            if (l.istable(-1))
            {
                var packagepath = GetLuaPackagePath();
                foreach (var flag in Capstones.UnityFramework.ResManager.GetDistributeFlags())
                {
                    packagepath = GetLuaDistributePath(flag) + packagepath;
                }
                packagepath += _OldPath;
                l.PushString(packagepath); // package path
#if UNITY_ANDROID && !UNITY_EDITOR
                if (UnityFramework.ResManager.LoadAssetsFromApk)
                {
                    l.GetField(-2, "loaders"); // package path loaders
                    if (l.istable(-1))
                    {
                        l.pushnumber(_LoaderIndex); // package path loaders n
                        l.pushcfunction(ClrDelApkLoader); // package path loaders n func
                        l.settable(-3); // package path loaders
                    }
                    l.pop(1); // package path
                }
#endif
                l.SetField(-2, "path"); // package
            }
            l.pop(1);

            // res version
            l.pushnil();
            l.SetGlobal("___resver");

            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncApkLoader(IntPtr l)
        {
            var oldtop = l.gettop();
            if (UnityFramework.ResManager.LoadAssetsFromApk)
            {
                string mname = l.GetString(1);
                if (!string.IsNullOrEmpty(mname))
                {
                    mname = mname.Replace('.', '/');
                    int retryTimes = 10;
                    for (int i = 0; i < retryTimes; ++i)
                    {
                        System.Exception error = null;
                        do
                        {
                            Unity.IO.Compression.ZipArchive za = UnityFramework.ResManager.AndroidApkZipArchive;
                            if (za == null)
                            {
                                error = new Exception("Apk Archive Cannot be read.");
                                break;
                            }
                            try
                            {
                                bool done = false;
                                var dflags = Capstones.UnityFramework.ResManager.GetDistributeFlags();
                                for (int j = dflags.Length - 1; j >= 0; --j)
                                {
                                    var flag = dflags[j];
                                    var entryname = "assets/spt/distribute/" + flag + "/" + mname + ".lua";
                                    var entry = za.GetEntry(entryname);
                                    if (entry != null)
                                    {
                                        var pathd = AppUpdatePath + "/spt/" + entryname.Substring("assets/spt/".Length);
                                        using (var srcstream = entry.Open())
                                        {
                                            using (var dststream = PlatExt.PlatDependant.OpenWrite(pathd))
                                            {
                                                srcstream.CopyTo(dststream);
                                            }
                                        }

                                        l.loadfile(pathd);
                                        done = true;
                                        break;
                                    }
                                }
                                if (!done)
                                {
                                    var entryname = "assets/spt/" + mname + ".lua";
                                    var entry = za.GetEntry(entryname);
                                    if (entry != null)
                                    {
                                        var pathd = AppUpdatePath + "/spt/" + entryname.Substring("assets/spt/".Length);
                                        using (var srcstream = entry.Open())
                                        {
                                            using (var dststream = PlatExt.PlatDependant.OpenWrite(pathd))
                                            {
                                                srcstream.CopyTo(dststream);
                                            }
                                        }

                                        l.loadfile(pathd);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                error = e;
                                break;
                            }
                        } while (false);
                        if (error != null)
                        {
                            if (i == retryTimes - 1)
                            {
                                if(GLog.IsLogErrorEnabled) GLog.LogException(error);
                                throw error;
                            }
                            else
                            {
                                if(GLog.IsLogErrorEnabled) GLog.LogException(error + "\nNeed Retry " + i);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncEncryptPostData(IntPtr l)
        {
            var oldtop = l.gettop();
            if (oldtop >= 1 && l.isstring(1))
            {
                var data = System.Text.Encoding.UTF8.GetBytes(l.tostring(1));
                string token = null;
                ulong seq = 0;
                if (oldtop >= 2 && !l.isnoneornil(2))
                {
                    token = l.tostring(2);
                }
                if (oldtop >= 3 && !l.isnoneornil(3))
                {
                    seq = (ulong)l.tonumber(3);
                }
                var encrypted = PlatExt.PlatDependant.EncryptPostData(data, token, seq);
                if (encrypted != null)
                {
                    l.PushUserDataOfType(encrypted, typeof(byte[]));
                }
            }
            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncParseResponse(IntPtr l)
        {
            var oldtop = l.gettop();
            var reqobj = l.GetLua(1).UnwrapDynamic();
            var www = reqobj as UnityEngine.WWW;
            var httpreq = reqobj as Capstones.UnityFramework.HttpRequest;

            string token = null;
            ulong seq = 0;
            if (oldtop >= 2 && !l.isnoneornil(2))
            {
                token = l.tostring(2);
            }
            if (oldtop >= 3 && !l.isnoneornil(3))
            {
                seq = (ulong)l.tonumber(3);
            }

            string resp = "";
            if (www != null)
            {
                resp = ParseWWWResponse(www, token, seq);
            }
            else if (httpreq != null)
            {
                resp = httpreq.ParseResponse(token, seq);
            }
            l.pushstring(resp);
            return 1;
        }

        public static string ParseWWWResponse(this UnityEngine.WWW www)
        {
            return ParseWWWResponse(www, null, 0);
        }
        public static string ParseWWWResponse(this UnityEngine.WWW www, string token, ulong seq)
        {
            if (www == null)
            {
                return "Invalid request obj.";
            }
            else
            {
                if (!www.isDone)
                {
                    return "Request undone.";
                }
                else
                {
                    if (!string.IsNullOrEmpty(www.error))
                    {
                        return www.error;
                    }
                    else
                    {
                        string enc = "";
                        bool encrypted = false;
                        string txt = "";
                        if (www.responseHeaders != null)
                        {
                            foreach (var kvp in www.responseHeaders)
                            {
                                var lkey = kvp.Key.ToLower();
                                if (lkey == "content-encoding")
                                {
                                    enc = kvp.Value.ToLower();
                                }
                                else if (lkey == "encrypted")
                                {
                                    var val = kvp.Value;
                                    if (val != null) val = val.ToLower();
                                    encrypted = !string.IsNullOrEmpty(val) && val != "n" && val != "0" && val != "f" && val != "no" && val != "false";
                                }
                            }
                        }

                        bool zipHandledBySystem = false;
#if !UNITY_EDITOR && UNITY_IPHONE
                        zipHandledBySystem = true;
#endif
                        if (enc != "gzip" || zipHandledBySystem)
                        {
                            try
                            {
                                txt = www.text;
                                if (encrypted)
                                {
                                    var data = Convert.FromBase64String(txt);
                                    var decrypted = PlatExt.PlatDependant.DecryptPostData(data, token, seq);
                                    if (decrypted != null)
                                    {
#if NETFX_CORE
                                        txt = System.Text.Encoding.UTF8.GetString(decrypted, 0, decrypted.Length);
#else
                                        txt = System.Text.Encoding.UTF8.GetString(decrypted);
#endif
                                    }
                                }
                            }
                            catch { }
                        }
                        else
                        {
                            try
                            {
                                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(www.bytes, false))
                                {
                                    using (Unity.IO.Compression.GZipStream gs = new Unity.IO.Compression.GZipStream(ms, Unity.IO.Compression.CompressionMode.Decompress))
                                    {
                                        using (var sr = new System.IO.StreamReader(gs))
                                        {
                                            txt = sr.ReadToEnd();
                                        }
                                        if (encrypted)
                                        {
                                            var data = Convert.FromBase64String(txt);
                                            var decrypted = PlatExt.PlatDependant.DecryptPostData(data, token, seq);
                                            if (decrypted != null)
                                            {
#if NETFX_CORE
                                                txt = System.Text.Encoding.UTF8.GetString(decrypted, 0, decrypted.Length);
#else
                                                txt = System.Text.Encoding.UTF8.GetString(decrypted);
#endif
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                        return txt;
                    }
                }
            }
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncGetCapID(IntPtr l)
        {
            var capID = GetCapID();
            l.PushString(capID);
            return 1;
        }
        private static void SplitStr(IntPtr l, string str)
        {
            l.newtable();
            for (int i = 0; i < str.Length; ++i)
            {
                l.pushnumber(i + 1);
                System.Text.StringBuilder pstr = new System.Text.StringBuilder();
                var ch = str[i];
                pstr.Append(ch);
                if (ch >= 0xD800 && ch <= 0xDFFF)
                {
                    if (++i < str.Length)
                    {
                        pstr.Append(str[i]);
                    }
                }
                l.PushString(pstr.ToString());
                l.settable(-3);
            }
        }
        private static void SplitStr(IntPtr l, System.Text.StringBuilder str)
        {
            l.newtable();
            for (int i = 0; i < str.Length; ++i)
            {
                l.pushnumber(i + 1);
                System.Text.StringBuilder pstr = new System.Text.StringBuilder();
                var ch = str[i];
                pstr.Append(ch);
                if (ch >= 0xD800 && ch <= 0xDFFF)
                {
                    if (++i < str.Length)
                    {
                        pstr.Append(str[i]);
                    }
                }
                l.PushString(pstr.ToString());
                l.settable(-3);
            }
        }
        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncSplitStr(IntPtr l)
        {
            if (l.isstring(1))
            {
                SplitStr(l, l.GetString(1));
            }
            else if (l.isuserdata(1))
            {
                var inputStr = l.GetLua(1);
                if (inputStr is string)
                {
                    SplitStr(l, inputStr.ToString());
                }
                else if (inputStr is System.Text.StringBuilder)
                {
                    SplitStr(l, (System.Text.StringBuilder)inputStr);
                }
            }
            return 1;
        }
        public static string GetCapID()
        {
            if (_cached_Capid == null)
            {
                _cached_Capid = Capstones.UnityEngineEx.IsolatedPrefs.IsolatedID;
            }
            return _cached_Capid;
        }
        public static string ReloadCapID()
        {
            _cached_Capid = null;
            return GetCapID();
        }

        private static int ClrFuncGetLangValue(IntPtr l, bool isStringType)
        {
            var oldtop = l.gettop();
            if (l.istable(1))
            {
                l.pushnumber(1);
                l.gettable(1);
                string key = l.tostring(-1);
                l.pop(1);
                var len = l.getn(1);
                string[] args = new string[len - 1];
                for (int i = 2; i <= len; i++)
                {
                    l.pushnumber(i);
                    l.gettable(1);
                    args[i - 2] = l.tostring(-1);
                    l.pop(1);
                }
                string val = Capstones.UnityFramework.LanguageConverter.GetLangValue(key, args);

                if (isStringType)
                {
                    l.pushstring(val);
                }
                else
                {
                    l.PushUserData(val);
                }
            }
            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncGetLangValueOfUserDataType(IntPtr l)
        {
            return ClrFuncGetLangValue(l, false);
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncGetLangValueOfStringType(IntPtr l)
        {
            return ClrFuncGetLangValue(l, true);
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncErrorHandler(IntPtr l)
        {
            var oldtop = l.gettop();
            l.GetGlobal("dump");
            l.insert(1);
            l.pcall(oldtop, 1, 0);
            return 1;
        }
    }
}