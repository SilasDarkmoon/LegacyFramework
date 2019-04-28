namespace Capstones.PlatExt
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading;
    using UnityEngine;
    using System.Text.RegularExpressions;

    /// <summary>
    /// 海鸣不骑猪 2018.4.24 修改
    /// 主要修改 去除调用时后的 命名空间
    /// 优化日志输出及日志删除逻辑减少性能
    /// </summary>
    public static class PlatDependant
    {
        private static bool _isInit = false;
        /// <summary>
        /// 0 运行在非 AndroidSimulator
        /// 1 运行在 AndroidSimulator 
        /// -1 表示未进行初始化
        /// </summary>
        private static int _isRunAndroidSimulator = -1;
#if UNITY_IPHONE
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void Log(string a);
#endif
        [XLua.BlackList]
        private static void CallLogBack(string msg)
        {
#if UNITY_IPHONE
            if (string.IsNullOrEmpty(msg)) return;
            Log(msg);
#endif
        }
        /// <summary>
        /// init 操作  默认会去 Path.Combine(Application.persistentDataPath, "GlogCtrl.txt") 
        /// 找配置如果找不到 采用 编译配置 方便在出包后动态调整日志输出权限
        /// </summary>
        public static void Init()
        {
            if (_isInit) return;
            var logConfigUrl = Path.Combine(Application.persistentDataPath, "GlogCtrl.txt");
            ///如果是编辑器模式 则直接读取 Path.Combine(Application.streamingAssetsPath, "GlogCtrl.txt") 文件
            if (Application.isEditor) logConfigUrl = Path.Combine(Application.streamingAssetsPath, "GlogCtrl.txt");
            if (File.Exists(logConfigUrl))
            {
                try
                {
                    using (var sr = OpenReadText(logConfigUrl))
                    {
                        string item;
                        string[] attr = null;
                        while (!sr.EndOfStream)
                        {
                            item = sr.ReadLine();
                            if (string.IsNullOrEmpty(item) || item.IndexOf("|") == -1) continue;
                            attr = item.Split('|');
                            SetGLogProperty(attr[0], attr[1]);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (GLog.IsLogErrorEnabled) GLog.LogException(e);
                    goto GlogDefault;
                }
                GLog.Init();
                _isInit = true;
                return;
            }
            GlogDefault:
            if (GLog.IsLogInfoEnabled) GLog.LogInfo("Glog 编译设置");
#if DISABLE_LOG_ALL
            GLog.SetAllLogEnabled(false);
#endif
#if DISABLE_LOG_INFO
            GLog.SetLogInfoEnabled(false);
#endif
#if DISABLE_LOG_WARN
            GLog.SetLogWarningEnabled(false);
#endif
#if DISABLE_LOG_ERROR
            GLog.SetLogErrorEnabled(false);
#endif
#if DISABLE_LOG_CONSOLE
            GLog.SetLogToConsoleEnabled(false);
#endif
#if DISABLE_LOG_STACKTRACE
            GLog.SetLogStackTraceEnabled(false);
#endif
#if UNITY_IPHONE
            GLog.SetCallLogBack(CallLogBack);
#endif
#if !(DEVELOPMENT_BUILD || UNITY_EDITOR || ALWAYS_SHOW_LOG || DEBUG)
            GLog.SetCallLogBack(null);
            GLog.SetLogToConsoleEnabled(false);
            GLog.SetLogInfoEnabled(false);
            GLog.SetLogStackTraceEnabled(false);
#endif
            GLog.Init();
            _isInit = true;
        }
        /// <summary>
        /// 设置glog 属性
        /// </summary>
        public static void SetGLogProperty(string key, string v)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(v)) return;
            bool isActive = v == "1";
            switch (key)
            {
                case "IsAllLogEnabled":
                    GLog.SetAllLogEnabled(isActive);
                    return;
                case "IsLogStackTraceEnabled":
                    GLog.SetLogStackTraceEnabled(isActive);
                    return;
                case "IsLogInfoEnabled":
                    GLog.SetLogInfoEnabled(isActive);
                    return;
                case "IsLogWarningEnabled":
                    GLog.SetLogWarningEnabled(isActive);
                    return;
                case "IsLogErrorEnabled":
                    GLog.SetLogErrorEnabled(isActive);
                    return;
                case "IsLogToConsoleEnabled":
                    GLog.SetLogToConsoleEnabled(isActive);
                    return;
                case "IsLogToFileEnabled":
                    GLog.SetLogToFileEnabled(isActive);
                    return;
                case "IsOpenIosNSLog":
#if UNITY_IPHONE
                    if (isActive) GLog.SetCallLogBack(CallLogBack);
#endif
                    return;
            }
        }

        public static void LogInfo(this object obj)
        {
            GLog.LogInfo(obj);
        }

        public static void LogError(this object obj)
        {
            GLog.LogError(obj);
        }

        public static void LogWarning(this object obj)
        {
            GLog.LogWarning(obj);
        }

        #region 原生平台与平台处理相关接口
        public static int GetTotalMemory()
        {
#if UNITY_ANDROID
            try
            {
                AndroidJavaObject fileReader = new AndroidJavaObject("java.io.FileReader", "/proc/meminfo");
                AndroidJavaObject br = new AndroidJavaObject("java.io.BufferedReader", fileReader, 2048);
                string mline = br.Call<String>("readLine");
                br.Call("close");
                mline = mline.Substring(mline.IndexOf("MemTotal:"));
                mline = Regex.Match(mline, "(\\d+)").Groups[1].Value;
                return (int.Parse(mline) / 1024);
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogError("[QualityManager] GetTotalMemory 获取内存失败:" + e);
                return SystemInfo.systemMemorySize;
            }
#else
        return SystemInfo.systemMemorySize;
#endif
        }
        /// <summary>
        /// 判断是否运行在安卓模拟器上面
        /// 判断依据是 通过模拟器无法模拟 Mac地址的属性 通过mac地址是否存在来判断
        /// 注意的是可以以这样来判断 但是此处mac地址不能使用 
        /// 这个方法Android 7.0是获取不到的，都会返回“02:00:00:00:00:00”
        /// </summary>
        /// <returns></returns>
        public static bool IsRunAndroidSimulator()
        {
            if (_isRunAndroidSimulator == -1)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                _isRunAndroidSimulator = 0;
                try
                {
                    AndroidJavaObject fileReader = new AndroidJavaObject("java.io.FileReader", "/proc/diskstats");
                    AndroidJavaObject br = new AndroidJavaObject("java.io.BufferedReader", fileReader, 2048);
                    bool isMmcblk0 = false;
                    string mline = "";
                    while ((mline = br.Call<String>("readLine")) != null)
                    {
                        if (mline.IndexOf("mmcblk0") == -1) continue;
                        isMmcblk0 = true;
                        break;
                    }
                    br.Call("close");

                    if (!isMmcblk0)
                    {
                        fileReader = new AndroidJavaObject("java.io.FileReader", "/proc/cpuinfo");
                        br = new AndroidJavaObject("java.io.BufferedReader", fileReader, 2048);
                        mline = br.Call<String>("readLine");
                        br.Call("close");
                        if (string.IsNullOrEmpty(mline) || (mline.IndexOf(": 0") != -1) || (mline.IndexOf(":0") != -1))
                        {
                            _isRunAndroidSimulator = 1;
                        }
                        else
                        {
                            _isRunAndroidSimulator = 1;
                            System.Net.NetworkInformation.NetworkInterface[] nis = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                            System.Net.NetworkInformation.PhysicalAddress item;
                            foreach (System.Net.NetworkInformation.NetworkInterface ni in nis)
                            {
                                if (ni == null) continue;
                                item = ni.GetPhysicalAddress();
                                if (item == null || string.IsNullOrEmpty(item.ToString())) continue;
                                _isRunAndroidSimulator = 0;
                                break;
                            }
                        }
                    }
                }
                catch
                {

                }
#else
                _isRunAndroidSimulator = 0;
#endif
            }
            return _isRunAndroidSimulator == 1;
        }
        #endregion
        public static bool IsValueType(this Type type)
        {
            return type.IsValueType;
        }

        public static bool IsEnum(this Type type)
        {
            return type.IsEnum;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }

        public static bool ContainsGenericParameters(this Type type)
        {
            return type.ContainsGenericParameters;
        }

        public static StreamReader OpenReadText(this string path)
        {
            try
            {
                var stream = OpenRead(path);
                if (stream != null)
                {
                    return new StreamReader(stream);
                }
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException(e);
            }
            return null;
        }

        public static Stream OpenRead(this string path)
        {
            try
            {
                return File.OpenRead(path);
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException(e);
                return null;
            }
        }

        public static StreamWriter OpenWriteText(this string path)
        {
            try
            {
                var stream = OpenWrite(path);
                if (stream != null)
                {
                    return new StreamWriter(stream);
                }
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException(e);
            }
            return null;
        }

        public static Stream OpenWrite(this string path, bool isCheckAndCreateDir = true)
        {
            DeleteFile(path);
            return OpenAppend(path, isCheckAndCreateDir);
        }

        public static StreamWriter OpenAppendText(this string path, bool isCheckAndCreateDir = true)
        {
            try
            {
                var stream = OpenAppend(path, isCheckAndCreateDir);
                if (stream != null)
                {
                    return new StreamWriter(stream);
                }
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException(e);
            }
            return null;
        }
        /// <summary>
        /// 叠加方式打开文件
        /// </summary>
        /// <param name="path"></param>
        /// 海鸣不骑猪 2018.4.24 修改
        /// <param name="isCheckAndCreateDir">是否检查文件目录结构是否存在 如果不存在则进行创建文件</param>
        /// <returns></returns>
        public static Stream OpenAppend(this string path, bool isCheckAndCreateDir = true)
        {
            try
            {
                FileStream stream = null;
                if (isCheckAndCreateDir)
                {
                    CreateFolder(Path.GetDirectoryName(path));
                    stream = File.OpenWrite(path);
                }
                else ///如果是运行时不检查并创建文件夹 则需要做安全处理防止在运行过程中文件被销毁
                {
                    try
                    {
                        stream = File.OpenWrite(path);
                    }
                    catch (Exception e)
                    {
                        if (GLog.IsLogErrorEnabled) GLog.LogException(e);
                        CreateFolder(Path.GetDirectoryName(path));
                        stream = File.OpenWrite(path);
                    }
                }

                if (stream != null)
                {
                    if (stream.CanSeek)
                    {
                        stream.Seek(0, SeekOrigin.End);
                    }
                    else
                    {
                        if (GLog.IsLogErrorEnabled) GLog.LogError(path + " cannot append.");
                    }
                }
                return stream;
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException(e);
                return null;
            }
        }

        public static TypeCode GetTypeCode(this Type type)
        {
            return Type.GetTypeCode(type);
        }

        public static bool IsObjIConvertible(this object obj)
        {
            return obj is IConvertible;
        }

        public static bool IsTypeIConvertible(this Type type)
        {
            return typeof(IConvertible).IsAssignableFrom(type);
        }

        public static Type[] GetAllNestedTypes(this Type type)
        {
            HashSet<Type> types = new HashSet<Type>();
            while (type != null)
            {
                try
                {
                    types.UnionWith(type.GetNestedTypes());
                }
                catch (Exception e)
                {
                    if (GLog.IsLogErrorEnabled) GLog.LogException(e);
                }
                try
                {
                    type = type.BaseType;
                }
                catch (Exception e)
                {
                    if (GLog.IsLogErrorEnabled) GLog.LogException(e);
                    type = null;
                }
            }
            return types.ToArray();
        }

        public static MethodInfo GetDelegateMethod(this Delegate del)
        {
            return del.Method;
        }

        public static bool IsFileExist(this string path)
        {
            try
            {
                return File.Exists(path);
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException(e);
                return false;
            }
        }

        public static void DeleteFile(this string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (DirectoryNotFoundException) { }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException(e);
            }
        }

        public static string[] GetAllFiles(this string dir)
        {
            try
            {
                List<string> files = new List<string>();
                files.AddRange(Directory.GetFiles(dir));
                var subs = Directory.GetDirectories(dir);
                foreach (var sub in subs)
                {
                    files.AddRange(GetAllFiles(sub));
                }
                return files.ToArray();
            }
            catch (DirectoryNotFoundException) { }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException(e);
            }
            return new string[0];
        }

        public static void CreateFolder(this string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException(e);
            }
        }

        public static void CopyTo(this Stream src, Stream dst)
        {
            byte[] buffer = Dynamic.ObjectPool.GetDataBufferFromPool();
            try
            {
                int readcnt = 0;
                do
                {
                    readcnt = src.Read(buffer, 0, 1024 * 1024);
                    dst.Write(buffer, 0, readcnt);
                } while (readcnt != 0);
            }
            finally
            {
                Dynamic.ObjectPool.ReturnDataBufferToPool(buffer);
            }
        }

        public static bool IsFileSameName(this string src, string dst)
        {
            try
            {
                if (src == dst)
                {
                    return true;
                }
                if (string.IsNullOrEmpty(src))
                {
                    if (string.IsNullOrEmpty(dst))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (string.IsNullOrEmpty(dst))
                {
                    return false;
                }

                if (Path.GetFullPath(src) == Path.GetFullPath(dst))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException(e);
            }
            return false;
        }

        public static void CopyFile(this string src, string dst)
        {
            if (IsFileSameName(src, dst))
            {
                return;
            }

            if (!string.IsNullOrEmpty(src) && !string.IsNullOrEmpty(dst))
            {
                var srcs = OpenRead(src);
                if (srcs != null)
                {
                    try
                    {
                        using (var dsts = OpenWrite(dst))
                        {
                            CopyTo(srcs, dsts);
                        }
                    }
                    catch (Exception e)
                    {
                        if (GLog.IsLogErrorEnabled) GLog.LogException(e);
                        throw;
                    }
                    finally
                    {
                        srcs.Dispose();
                    }
                }
                else
                {
                    if (GLog.IsLogInfoEnabled) GLog.LogInfo(src + " cannot be read.");
                }
            }
        }
        public static void MoveFile(this string src, string dst)
        {
            if (IsFileSameName(src, dst))
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("Move same file: " + (src ?? "") + " -> " + (dst ?? ""));
                return;
            }

            if (!string.IsNullOrEmpty(src) && !string.IsNullOrEmpty(dst))
            {
                CreateFolder(Path.GetDirectoryName(dst));
                // try to lock src and delete dst.
                {
                    Stream srcfile = null;
                    try
                    {
                        srcfile = OpenRead(src);
                        DeleteFile(dst);
                    }
                    catch (Exception e)
                    {
                        if (GLog.IsLogErrorEnabled) GLog.LogException(e);
                    }
                    finally
                    {
                        if (srcfile != null)
                        {
                            srcfile.Dispose();
                        }
                    }
                }
                try
                {
                    File.Move(src, dst);
                }
                catch (Exception e)
                {
                    if (GLog.IsLogErrorEnabled) GLog.LogException(e);
                    throw;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(src))
                {
                    if (GLog.IsLogInfoEnabled) GLog.LogInfo("MoveFile, src is empty");
                }
                if (string.IsNullOrEmpty(dst))
                {
                    if (GLog.IsLogInfoEnabled) GLog.LogInfo("MoveFile, dst is empty");
                }
            }
        }

        public static byte[] EncryptPostData(byte[] data, string token, ulong seq)
        {
            var strkey = XLuaExt.LuaEvent.TrigClrEvent<string>("Get_EncryptKey");
            if (string.IsNullOrEmpty(strkey))
            {
                return null;
            }
            if (token == null)
            {
                token = "";
            }
            var aiv = new byte[16];
            var atoken = System.Text.Encoding.UTF8.GetBytes(token);
            for (int i = 0; i < 16; ++i)
            {
                aiv[i] = 0;
                if (i < atoken.Length) aiv[i] = atoken[i];
                aiv[i] ^= (byte)((seq & (0xFFUL << (4 * i))) >> (4 * i));
            }

            var key = System.Text.Encoding.UTF8.GetBytes(strkey);
            var aesAlgorithm = new System.Security.Cryptography.RijndaelManaged();
            aesAlgorithm.KeySize = 256;
            aesAlgorithm.BlockSize = 128;
            aesAlgorithm.Mode = CipherMode.CBC;
            aesAlgorithm.Padding = PaddingMode.PKCS7;
            aesAlgorithm.Key = key;
            aesAlgorithm.IV = aiv;

            return aesAlgorithm.CreateEncryptor().TransformFinalBlock(data, 0, data.Length);
        }

        public static byte[] DecryptPostData(byte[] data, string token, ulong seq)
        {
            var strkey = XLuaExt.LuaEvent.TrigClrEvent<string>("Get_EncryptKey");
            if (string.IsNullOrEmpty(strkey))
            {
                return null;
            }
            if (token == null)
            {
                token = "";
            }
            var aiv = new byte[16];
            var atoken = System.Text.Encoding.UTF8.GetBytes(token);
            for (int i = 0; i < 16; ++i)
            {
                aiv[i] = 0;
                if (i < atoken.Length) aiv[i] = atoken[i];
                aiv[i] ^= (byte)((seq & (0xFFUL << (4 * i))) >> (4 * i));
            }

            var key = System.Text.Encoding.UTF8.GetBytes(strkey);
            var aesAlgorithm = new System.Security.Cryptography.RijndaelManaged();
            aesAlgorithm.KeySize = 256;
            aesAlgorithm.BlockSize = 128;
            aesAlgorithm.Mode = CipherMode.CBC;
            aesAlgorithm.Padding = PaddingMode.PKCS7;
            aesAlgorithm.Key = key;
            aesAlgorithm.IV = aiv;

            return aesAlgorithm.CreateDecryptor().TransformFinalBlock(data, 0, data.Length);
        }

        [XLua.LuaCallCSharp]
        public class TaskProgress
        {
            public int Length = 0;
            public int Total = 0;
            public bool Done = false;
            public string Error = null;

            public Action OnCancel = null;
            public void Cancel()
            {
                if (OnCancel != null)
                {
                    OnCancel();
                }
            }
        }

        public static TaskProgress DownloadLargeFile(string uri, Stream dst)
        {
            return DownloadLargeFile(uri, dst, null);
        }

        public static TaskProgress DownloadLargeFile(string uri, string file)
        {
            var stream = OpenWrite(file);
            if (stream != null)
            {
                return DownloadLargeFile(uri, stream, prog => stream.Dispose());
            }
            return null;
        }

        public static TaskProgress DownloadLargeFile(string uri, System.IO.Stream dst, Action<TaskProgress> OnDone)
        {
            return DownloadLargeFile(uri, dst, false, OnDone);
        }

        /// <remarks>OnDone is called from ThreadPool thread, NOT the main thread.</remarks>
        //public static TaskProgress DownloadLargeFile(string uri, Stream dst, Action<TaskProgress> OnDone)
        //{
        //    // TODO: 断点续传
        //    var prog = new TaskProgress();

        //    UnityFramework.HttpRequest req = new UnityFramework.HttpRequest(uri);
        //    req.DestStream = dst;
        //    req.StartRequest();
        //    prog.OnCancel = () => req.StopRequest();
        //    ThreadPool.QueueUserWorkItem(state =>
        //{
        //    int unreachStartTime = Environment.TickCount;
        //    ulong lastLen = 0;
        //    bool timedout = false;
        //    while (!req.IsDone)
        //    {
        //        prog.Total = (int)req.Total;
        //        prog.Length = (int)req.Length;
        //        if (GLog.IsLogInfoEnabled) GLog.LogInfo(req.Length + "\n" + req.Total + "\n" + req.IsDone);
        //        if (req.Length > lastLen)
        //        {
        //            lastLen = req.Length;
        //            unreachStartTime = Environment.TickCount;
        //        }
        //        else
        //        {
        //            int deltatime = Environment.TickCount - unreachStartTime;
        //            if (deltatime > 15000)
        //            {
        //                timedout = true;
        //                break;
        //            }
        //        }
        //    }
        //    if (timedout)
        //    {
        //        req.StopRequest();
        //        prog.Error = "timedout";
        //        prog.Done = true;
        //    }
        //    else
        //    {
        //        prog.Error = req.Error;
        //        prog.Done = true;
        //    }

        //    if (OnDone != null)
        //    {
        //        OnDone(prog);
        //    }
        //});
        //    return prog;
        //}

        /// <remarks>OnDone is called from ThreadPool thread, NOT the main thread.</remarks>
        public static TaskProgress DownloadLargeFile(string uri, System.IO.Stream dst, bool rangeEnabled, Action<TaskProgress> OnDone)
        {
            // TODO: 断点续传
            var prog = new TaskProgress();
            UnityFramework.HttpRequest req = new UnityFramework.HttpRequest(uri);
            req.DestStream = dst;
            req.RangeEnabled = rangeEnabled;
            req.StartRequest();
            prog.OnCancel = () => req.StopRequest();
            System.Threading.ThreadPool.QueueUserWorkItem(state =>
            {
                int unreachStartTime = Environment.TickCount;
                ulong lastLen = 0;
                bool timedout = false;
                while (!req.IsDone)
                {
                    prog.Total = (int)req.Total;
                    prog.Length = (int)req.Length;
                    //LogInfo(req.Length);
                    //LogInfo(req.Total);
                    //LogInfo(req.IsDone);

                    if (req.Length > lastLen)
                    {
                        lastLen = req.Length;
                        unreachStartTime = Environment.TickCount;
                    }
                    else
                    {
                        int deltatime = Environment.TickCount - unreachStartTime;
                        if (deltatime > 15000)
                        {
                            timedout = true;
                            break;
                        }
                    }
                }
                if (timedout)
                {
                    req.StopRequest();
                    prog.Error = "timedout";
                    prog.Done = true;
                }
                else
                {
                    prog.Error = req.Error;
                    prog.Done = true;
                }

                if (OnDone != null)
                {
                    OnDone(prog);
                }
            });
            //#endif
            return prog;
        }

        public static TaskProgress RunBackground(Action<TaskProgress> work)
        {
            var prog = new TaskProgress();
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    work(prog);
                }
#if UNITY_EDITOR
                catch (ThreadAbortException e)
                {
                    // 此处吃掉异常，因为线程经常在UnityEditor内被杀掉
                }
#endif
                catch (Exception e)
                {
                    if (GLog.IsLogErrorEnabled) GLog.LogException(e);
                    prog.Error = e.Message;
                }
                finally
                {
                    prog.Done = true;
                }
            });
            return prog;
        }
    }
}
