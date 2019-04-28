//#if UNITY_EDITOR
//#   define LOAD_TEX_FROM_PACK
//#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using Capstones.Dynamic;
//using Capstones.LuaWrap;
using System.Linq;
using System.Text;
using Unity.IO.Compression;
using Capstones.PlatExt;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Capstones.UnityFramework
{
    [XLua.LuaCallCSharp]
    public static class ResManager
    {
        [ThreadStatic]
        private static System.IO.Stream _AndroidApkFileStream;
        [ThreadStatic]
        private static ZipArchive _AndroidApkZipArchive;
        public static System.IO.Stream AndroidApkFileStream
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                try
                {
                    bool disposed = false;
                    try
                    {
                        if (_AndroidApkFileStream == null)
                        {
                            disposed = true;
                        }
                        else if (!_AndroidApkFileStream.CanSeek)
                        {
                            disposed = true;
                        }
                    }
                    catch
                    {
                        disposed = true;
                    }
                    if (disposed)
                    {
                        _AndroidApkFileStream = null;
                        _AndroidApkFileStream = PlatExt.PlatDependant.OpenRead(LuaExt.LuaFramework.AppDataPath);
                    }
                }
                catch (Exception e)
                {
                    if(GLog.IsLogErrorEnabled) GLog.LogException(e);
                }
#endif
                return _AndroidApkFileStream;
            }
        }
        public static ZipArchive AndroidApkZipArchive
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                try
                {
                    bool disposed = false;
                    try
                    {
                        if (_AndroidApkZipArchive == null)
                        {
                            disposed = true;
                        }
                        else
                        {
                            _AndroidApkZipArchive.ThrowIfDisposed();
                            if (_AndroidApkZipArchive.Mode == ZipArchiveMode.Create)
                            {
                                disposed = true;
                            }
                        }
                    }
                    catch
                    {
                        disposed = true;
                    }
                    if (disposed)
                    {
                        _AndroidApkZipArchive = null;
                        _AndroidApkZipArchive = new ZipArchive(AndroidApkFileStream);
                    }
                }
                catch (Exception e)
                {
                    if(GLog.IsLogErrorEnabled) GLog.LogException(e);
                }
#endif
                return _AndroidApkZipArchive;
            }
        }
        [ThreadStatic]
        private static System.IO.Stream _ObbFileStream;
        [ThreadStatic]
        private static ZipArchive _ObbZipArchive;
        public static System.IO.Stream ObbFileStream
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                if (_ObbPath != null)
                {
                    try
                    {
                        bool disposed = false;
                        try
                        {
                            if (_ObbFileStream == null)
                            {
                                disposed = true;
                            }
                            else if (!_ObbFileStream.CanSeek)
                            {
                                disposed = true;
                            }
                        }
                        catch
                        {
                            disposed = true;
                        }
                        if (disposed)
                        {
                            _ObbFileStream = null;
                            _ObbFileStream = PlatExt.PlatDependant.OpenRead(_ObbPath);
                        }
                    }
                    catch (Exception e)
                    {
                        if(GLog.IsLogErrorEnabled) GLog.LogException(e);
                    }
                }
                else
                {
                    _ObbFileStream = null;
                }
#endif
                return _ObbFileStream;
            }
        }
        public static ZipArchive ObbZipArchive
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                if (_ObbPath != null && ObbFileStream != null)
                {
                    try
                    {
                        bool disposed = false;
                        try
                        {
                            if (_ObbZipArchive == null)
                            {
                                disposed = true;
                            }
                            else
                            {
                                _ObbZipArchive.ThrowIfDisposed();
                                if (_ObbZipArchive.Mode == ZipArchiveMode.Create)
                                {
                                    disposed = true;
                                }
                            }
                        }
                        catch
                        {
                            disposed = true;
                        }
                        if (disposed)
                        {
                            _ObbZipArchive = null;
                            _ObbZipArchive = new ZipArchive(ObbFileStream);
                        }
                    }
                    catch (Exception e)
                    {
                        if(GLog.IsLogErrorEnabled) GLog.LogException(e);
                    }
                }
                else
                {
                    _ObbZipArchive = null;
                }
#endif
                return _ObbZipArchive;
            }
        }

#if (UNITY_5 || UNITY_5_3_OR_NEWER)
        private static AssetBundleManifest ResManifest;
        private static HashSet<string> AssetBundleNamesFromManifest;
#endif

        private static bool _LoadAssetsFromApk;
        public static bool LoadAssetsFromApk
        {
            get { return _LoadAssetsFromApk; }
        }
        private static bool _LoadAssetsFromObb;
        public static bool LoadAssetsFromObb
        {
            get { return _LoadAssetsFromObb; }
        }
        private static string _ObbPath;
        public static string ObbPath
        {
            get { return _ObbPath; }
        }

        static ResManager()
        {
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
#if !FORCE_DECOMPRESS_ASSETS_ON_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                _LoadAssetsFromApk = true;
            }
#endif

            Application.lowMemory += () =>
            {
                XLuaExt.LuaEvent.TrigClrEvent("LowMemory");
                GC.Collect();
                Resources.UnloadUnusedAssets();
                if (GLog.IsLogErrorEnabled) GLog.LogError("Application.lowMemory *******系统内存不足预警*******");
            };
#endif
        }

        public static void Init()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _ObbPath = XLuaExt.LuaEvent.TrigClrEvent<string>("GET_MAIN_OBB_PATH"); // TODO: To Deng: the listener to "ObbPath" in DistributePlugin
#if (UNITY_5 || UNITY_5_3_OR_NEWER) //&& LOAD_ASSETS_FROM_OBB
            _LoadAssetsFromObb = true;
#endif
#endif
        }

        public static IEnumerator GetEmptyEnumerator()
        {
            yield break;
        }

        public static IEnumerator DecompressScriptBundleAsync(AssetBundle bundle, Action<string, object> funcReport)
        {
            if (bundle != null)
            {
                string key = "";
                int ver = 0;
                if (IsCachedBundleOutOfDateThenUpdate(bundle, (key1, ver1) => { key = key1; ver = ver1; return true; }))
                {
                    // TODO: obsoleted. if we should use this again, we must review and test this (and the build procedure,) carefully.
                    // TODO: we should delete existing files first and then decompress.
                    if (funcReport != null)
                    {
                        funcReport("FirstLoad", true);
                    }
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
                    TextAsset txt = bundle.LoadAsset<TextAsset>("index");
#else
                    TextAsset txt = bundle.Load("index") as TextAsset;
#endif
                    if (txt != null)
                    {
                        int tick = Environment.TickCount;
                        var lines = txt.text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (funcReport != null)
                        {
                            funcReport("Count", lines.Length);
                        }
                        foreach (var line in lines)
                        {
                            if (funcReport != null)
                            {
                                funcReport("Progress", line);
                            }
                            string[] script = line.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                            if (script.Length >= 2)
                            {
                                string path = script[0];
                                string name = script[1];
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
                                TextAsset txtScript = bundle.LoadAsset<TextAsset>(name);
#else
                                TextAsset txtScript = (TextAsset)bundle.Load(name);
#endif
                                if (txtScript != null)
                                {
                                    string pathCached = ResManager.UpdatePath + "/spt/" + path;
                                    var stream = Capstones.PlatExt.PlatDependant.OpenWrite(pathCached + ".tmp");
                                    System.IO.BinaryWriter br = new System.IO.BinaryWriter(stream);
                                    br.Write(txtScript.bytes);
                                    br.Flush();
                                    stream.Dispose();
                                    Capstones.PlatExt.PlatDependant.MoveFile(pathCached + ".tmp", pathCached);
                                    GameObject.DestroyImmediate(txtScript, true);
                                }
                            }
                            int newtick = Environment.TickCount;
                            if (newtick - tick > 200)
                            {
                                yield return null;
                                tick = Environment.TickCount;
                            }
                        }
                    }
                    RecordCacheVersion(key, ver);
                }
                bundle.Unload(true);
            }
        }

        public static void DecompressScriptBundle(AssetBundle bundle)
        {
            var work = DecompressScriptBundleAsync(bundle, null);
            if (work != null)
            {
                while (work.MoveNext())
                {
                }
            }
        }

        public static IEnumerator CopyOrDeleteDecompressedScript(string bundleName, Action<string, object> funcReport)
        {
            //if (bundleName == "default")
            //{
            //    return DeleteDecompressedScript(bundleName, funcReport);
            //}
            //else
            //{
            //    //return CopyDecompressedScript(bundleName, funcReport);
            //    return GetEmptyEnumerator();
            //}
            return DeleteDecompressedScript(bundleName, funcReport);
        }

        public static IEnumerator CopyDecompressedScript(string bundleName, Action<string, object> funcReport)
        {
            var path = Application.streamingAssetsPath + "/spt/";
            var pathd = ResManager.UpdatePath + "/spt/";
            if (bundleName != "default")
            {
                path += bundleName + "/";
            }
            string[] allfiles = null;
            try
            {
                allfiles = Capstones.PlatExt.PlatDependant.GetAllFiles(path);
            }
            catch { }
            if (allfiles != null)
            {
                List<string> files = new List<string>();
                for (int j = 0; j < allfiles.Length; ++j)
                {
                    var file = allfiles[j];
                    if (file.EndsWith(".lua"))
                    {
                        files.Add(file);
                    }
                }
                if (funcReport != null)
                {
                    funcReport("Count", files.Count);
                }
                int tick = Environment.TickCount;
                for (int j = 0; j < files.Count; ++j)
                {
                    var file = files[j];
                    if (funcReport != null)
                    {
                        funcReport("Progress", file);
                    }
                    try
                    {
                        var filed = pathd + file.Substring(path.Length);
                        using (var fsrc = Capstones.PlatExt.PlatDependant.OpenRead(file))
                        {
                            using (var fdst = Capstones.PlatExt.PlatDependant.OpenWrite(filed + ".tmp"))
                            {
                                fsrc.CopyTo(fdst);
                            }
                        }
                        Capstones.PlatExt.PlatDependant.MoveFile(filed + ".tmp", filed);
                    }
                    catch { }
                    int newtick = Environment.TickCount;
                    if (newtick - tick > 200)
                    {
                        yield return null;
                        tick = Environment.TickCount;
                    }
                }
            }
        }

        public static IEnumerator DeleteDecompressedScript(string bundleName, Action<string, object> funcReport)
        {
            var path = ResManager.UpdatePath + "/spt/";
            if (bundleName != "default")
            {
                path += bundleName + "/";
            }
            string[] allfiles = null;
            try
            {
                allfiles = Capstones.PlatExt.PlatDependant.GetAllFiles(path);
            }
            catch { }
            if (allfiles != null)
            {
                var forbiddenDir = path + "distribute/";
                List<string> files = new List<string>();
                for (int j = 0; j < allfiles.Length; ++j)
                {
                    var file = allfiles[j];
                    if (bundleName != "default" || !file.StartsWith(forbiddenDir))
                    {
                        files.Add(file);
                    }
                }
                if (funcReport != null)
                {
                    funcReport("Count", files.Count);
                }
                int tick = Environment.TickCount;
                for (int j = 0; j < files.Count; ++j)
                {
                    var file = files[j];
                    if (funcReport != null)
                    {
                        funcReport("Progress", file);
                    }
                    try
                    {
                        Capstones.PlatExt.PlatDependant.DeleteFile(file);
                    }
                    catch { }
                    int newtick = Environment.TickCount;
                    if (newtick - tick > 200)
                    {
                        yield return null;
                        tick = Environment.TickCount;
                    }
                }
            }
        }

        /// <param name="bundleName">"default" or "distribute/XXX"</param>
        public static IEnumerator DecompressScriptBundle(string bundleName, Action<string, object> funcReport)
        {
            if (Application.streamingAssetsPath.Contains("://"))
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    var sptEntryDir = "assets/spt/";
                    if (bundleName != "default")
                    {
                        sptEntryDir = sptEntryDir + bundleName + "/";
                    }

                    int retryTimes = 10;
                    HashSet<string> successItems = new HashSet<string>();
                    string key = "";
                    int ver = 0;
                    for (int i = 0; i < retryTimes; ++i)
                    {
                        System.Exception error = null;
                        do
                        {
                            ZipArchive za = AndroidApkZipArchive;
                            if (za == null)
                            {
                                error = new Exception("Apk Archive Cannot be read.");
                                break;
                            }
                            try
                            {
                                var versionName = sptEntryDir + "version.txt";
                                if (!successItems.Contains(versionName))
                                {
                                    var versionEntry = za.GetEntry(versionName);
                                    if (versionEntry != null)
                                    {
                                        using (var streamVer = versionEntry.Open())
                                        {
                                            var readerVer = new System.IO.StreamReader(streamVer);

                                            var line = readerVer.ReadLine();
                                            if (line != null)
                                            {
                                                string[] version = line.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                                                if (version != null && version.Length >= 1)
                                                {
                                                    key = version[0];
                                                    if (version.Length >= 2)
                                                    {
                                                        int.TryParse(version[1], out ver);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    successItems.Add(versionName);
                                }
                            }
                            catch (Exception e)
                            {
                                error = e;
                                break;
                            }

                            if (!string.IsNullOrEmpty(key) && IsCachedAssetOutOfDate(key, ver))
                            {
                                if (funcReport != null)
                                {
                                    funcReport("FirstLoad", true);
                                }
                                if (!successItems.Contains("<delete old>"))
                                {
                                    var work = DeleteDecompressedScript(bundleName, funcReport); // first, delete the old files. To keep it clean.
                                    if (work != null)
                                    {
                                        while (work.MoveNext())
                                        {
                                            yield return work.Current;
                                        }
                                    }
                                    successItems.Add("<delete old>");
                                }

                                if (!_LoadAssetsFromApk)
                                {
                                    var entries = new List<ZipArchiveEntry>();
                                    try
                                    {
                                        foreach (var entry in za.Entries)
                                        {
                                            if (entry != null
                                                && entry.FullName.StartsWith(sptEntryDir)
                                                && (bundleName != "default" || !entry.FullName.StartsWith("assets/spt/distribute/")))
                                            {
                                                entries.Add(entry);
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        error = e;
                                        break;
                                    }
                                    if (funcReport != null)
                                    {
                                        funcReport("Count", entries.Count);
                                    }
                                    int tick = Environment.TickCount;
                                    for (int j = 0; j < entries.Count; ++j)
                                    {
                                        var entry = entries[j];
                                        var fullname = entry.FullName;
                                        if (funcReport != null)
                                        {
                                            funcReport("Progress", fullname);
                                        }
                                        if (!successItems.Contains(fullname))
                                        {
                                            try
                                            {
                                                using (var srcstream = entry.Open())
                                                {
                                                    var pathd = ResManager.UpdatePath + "/spt/" + fullname.Substring("assets/spt/".Length);
                                                    using (var dststream = PlatExt.PlatDependant.OpenWrite(pathd + ".tmp"))
                                                    {
                                                        srcstream.CopyTo(dststream);
                                                    }
                                                    PlatExt.PlatDependant.MoveFile(pathd + ".tmp", pathd);
                                                }
                                                successItems.Add(fullname);
                                            }
                                            catch (Exception e)
                                            {
                                                error = e;
                                                break;
                                            }
                                        }
                                        int newtick = Environment.TickCount;
                                        if (newtick - tick > 200)
                                        {
                                            yield return null;
                                            tick = Environment.TickCount;
                                        }
                                    }
                                    if (error != null)
                                    {
                                        break;
                                    }
                                }
                                RecordCacheVersion(key, ver);
                            }
                        } while (false);
                        if (error != null)
                        {
                            if (i == retryTimes - 1)
                            {
                                if (GLog.IsLogErrorEnabled) GLog.LogError(error);
                                throw error;
                            }
                            else
                            {
                                if (GLog.IsLogErrorEnabled) GLog.LogError(error + "\nNeed Retry " + i);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    AssetBundle bundle;
                    string pathPackedBundle = Application.streamingAssetsPath + "/spt/" + bundleName + ".ab";
                    WWW www = null;
                    try
                    {
                        www = new WWW(pathPackedBundle);
                    }
                    catch
                    {
                        yield break;
                    }
                    if (www != null)
                    {
                        yield return www;
                    }
                    else
                    {
                        yield break;
                    }
                    try
                    {
                        bundle = www.assetBundle;
                    }
                    catch
                    {
                        yield break;
                    }
                    if (www != null)
                    {
                        www.Dispose();
                    }
                    var work = ResManager.DecompressScriptBundleAsync(bundle, funcReport);
                    if (work != null)
                    {
                        while (work.MoveNext())
                        {
                            yield return work.Current;
                        }
                    }
                }
            }
            else
            {
                string key = "";
                int ver = 0;
                try
                {
                    string pathv = Application.streamingAssetsPath + "/spt/version.txt";
                    if (bundleName != "default")
                    {
                        pathv = Application.streamingAssetsPath + "/spt/" + bundleName + "/version.txt";
                    }
                    using (var sr = Capstones.PlatExt.PlatDependant.OpenReadText(pathv))
                    {
                        var line = sr.ReadLine();
                        if (line != null)
                        {
                            string[] version = line.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                            if (version != null && version.Length >= 1)
                            {
                                key = version[0];
                                if (version.Length >= 2)
                                {
                                    int.TryParse(version[1], out ver);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    yield break;
                }
                if (!string.IsNullOrEmpty(key))
                {
                    if (IsCachedAssetOutOfDate(key, ver))
                    {
                        if (funcReport != null)
                        {
                            funcReport("FirstLoad", true);
                        }
                        var work = CopyOrDeleteDecompressedScript(bundleName, funcReport);
                        if (work != null)
                        {
                            while (work.MoveNext())
                            {
                                yield return work.Current;
                            }
                        }

                        RecordCacheVersion(key, ver);
                    }
                }
            }
        }

        public static IEnumerator DecompressScriptBundle(string bundleName)
        {
            return DecompressScriptBundle(bundleName, null);
        }

        public static void MergeResIndexDiff(string oldpath, string diffpath, string newpath)
        {
            // old
            Dictionary<string, string> oldlines = new Dictionary<string, string>();
            List<KeyValuePair<string, string>> oldraw = new List<KeyValuePair<string, string>>();
            try
            {
                using (var sr = PlatDependant.OpenReadText(oldpath))
                {
                    while (true)
                    {
                        var line = sr.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        var infos = line.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        if (infos != null && infos.Length >= 1)
                        {
                            oldraw.Add(new KeyValuePair<string, string>(infos[0], line));
                            oldlines[infos[0]] = line;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException(e);
            }
            // diff
            Dictionary<string, string> difflines = new Dictionary<string, string>();
            List<KeyValuePair<string, string>> diffraw = new List<KeyValuePair<string, string>>();
            try
            {
                using (var sr = PlatDependant.OpenReadText(diffpath))
                {
                    while (true)
                    {
                        var line = sr.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        var infos = line.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        if (infos != null && infos.Length >= 1)
                        {
                            if (infos.Length > 1)
                            {
                                diffraw.Add(new KeyValuePair<string, string>(infos[0], line));
                                difflines[infos[0]] = line;
                            }
                            else
                            {
                                diffraw.Add(new KeyValuePair<string, string>(infos[0], ""));
                                difflines[infos[0]] = "";
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException(e);
            }
            // new
            try
            {
                using (var sw = PlatDependant.OpenWriteText(newpath + ".tmp"))
                {
                    foreach (var kvpline in oldraw)
                    {
                        if (kvpline.Value == oldlines[kvpline.Key])
                        {
                            var line = kvpline.Value;
                            if (difflines.ContainsKey(kvpline.Key))
                            {
                                line = difflines[kvpline.Key];
                            }
                            if (!string.IsNullOrEmpty(line))
                            {
                                sw.WriteLine(line);
                            }
                        }
                    }
                    foreach (var kvpline in diffraw)
                    {
                        if (kvpline.Value == difflines[kvpline.Key])
                        {
                            if (!oldlines.ContainsKey(kvpline.Key))
                            {
                                if (!string.IsNullOrEmpty(kvpline.Value))
                                {
                                    sw.WriteLine(kvpline.Value);
                                }
                            }
                        }
                    }
                }
                PlatDependant.MoveFile(newpath + ".tmp", newpath);
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException(e);
            }
        }

        public static void MovePendingUpdate()
        {
            var workpending = ResManager.MovePendingUpdate(null);
            if (workpending != null)
            {
                while (workpending.MoveNext())
                {
                }
            }
        }
        public static IEnumerator MovePendingUpdate(Action<string, object> funcReport)
        {
            var updatePath = Capstones.LuaExt.LuaFramework.AppUpdatePath;
            var pendingPath = updatePath + "/pending";
            string[] pendingFiles = null;
            try
            {
                pendingFiles = Capstones.PlatExt.PlatDependant.GetAllFiles(pendingPath);
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException(e);
            }
            if (pendingFiles != null && pendingFiles.Length > 0)
            {
                if (funcReport != null)
                {
                    funcReport("PendingUpdate", true);
                    funcReport("Count", pendingFiles.Length);
                }
                int tick = Environment.TickCount;
                foreach (var file in pendingFiles)
                {
                    if (GLog.IsLogInfoEnabled) GLog.LogInfo("Moving pending update: " + file);
                    if (funcReport != null)
                    {
                        funcReport("Progress", file);
                    }
                    if (file.EndsWith(".resindex.diff.txt"))
                    {
                        var part = file.Substring(pendingPath.Length, file.Length - pendingPath.Length - ".resindex.diff.txt".Length);
                        var dest = updatePath + part;
                        var oldpath = dest;
                        if (!PlatDependant.IsFileExist(oldpath))
                        {
                            oldpath = null;
                            if (!Application.streamingAssetsPath.Contains("://"))
                            {
                                oldpath = Application.streamingAssetsPath + part;
                                if (!PlatDependant.IsFileExist(oldpath))
                                {
                                    oldpath = null;
                                }
                            }
                            else
                            {
                                if (Application.platform == RuntimePlatform.Android && _LoadAssetsFromApk)
                                {
                                    int retryTimes = 10;
                                    for (int i = 0; i < retryTimes; ++i)
                                    {
                                        GC.Collect();
                                        Exception error = null;
                                        do
                                        {
                                            ZipArchive za = AndroidApkZipArchive;
                                            if (za == null)
                                            {
                                                error = new Exception("Apk Archive Cannot be read.");
                                                break;
                                            }
                                            var entryname = "assets" + part;
                                            try
                                            {
                                                var entry = za.GetEntry(entryname);
                                                if (entry != null)
                                                {
                                                    oldpath = dest;
                                                    using (var srcstream = entry.Open())
                                                    {
                                                        using (var dststream = PlatDependant.OpenWrite(dest + ".tmp"))
                                                        {
                                                            srcstream.CopyTo(dststream);
                                                        }
                                                    }
                                                    PlatDependant.MoveFile(dest + ".tmp", dest);
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
                                                if (GLog.IsLogErrorEnabled) GLog.LogError(error);
                                                throw error;
                                            }
                                            else
                                            {
                                                if (GLog.IsLogErrorEnabled) GLog.LogError(error + "\nNeed Retry " + i);
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (oldpath != null)
                        {
                            MergeResIndexDiff(oldpath, file, dest);
                            PlatDependant.DeleteFile(file);
                        }
                        else
                        {
                            PlatDependant.MoveFile(file, dest);
                        }
                    }
                    //else if (file.EndsWith(".delete")) // we'd better no use this. because this cannot delete the files in the apk or ipa.
                    //{
                    //    var part = file.Substring(pendingPath.Length, file.Length - pendingPath.Length - ".delete".Length);
                    //    var dest = updatePath + part;
                    //    PlatDependant.DeleteFile(dest);
                    //}
                    else
                    {
                        var dest = updatePath + file.Substring(pendingPath.Length);
                        Capstones.PlatExt.PlatDependant.MoveFile(file, dest);

                        int newtick = Environment.TickCount;
                        if (newtick - tick > 200)
                        {
                            yield return null;
                            tick = Environment.TickCount;
                        }
                    }
                }
            }
        }

        public static void UpdateResourceBundle(AssetBundle bundle, byte[] buffer)
        {
            if (bundle != null)
            {
                IsCachedBundleOutOfDateThenUpdate(bundle, (name, ver) => UpdateResourceBundle(name, buffer));
                bundle.Unload(true);
            }
        }

        public static bool UpdateResourceBundle(string name, byte[] buffer)
        {
            try
            {
                var pathb = ResManager.UpdatePath + "/res/" + name + ".ab";
                using (var sw = Capstones.PlatExt.PlatDependant.OpenWrite(pathb + ".tmp"))
                {
                    var bw = new System.IO.BinaryWriter(sw);
                    bw.Write(buffer);
                }
                Capstones.PlatExt.PlatDependant.MoveFile(pathb + ".tmp", pathb);
            }
            catch { }
            return false;
        }

        private static HashSet<string> NonExistingFiles = new HashSet<string>();
        public static bool IsNonExistingFiles(string file)
        {
#if NETFX_CORE
            return NonExistingFiles.Contains(file);
#else
            return false;
#endif
        }
        public static void RecordNonExistingFiles(string file)
        {
#if NETFX_CORE
            NonExistingFiles.Add(file);
#else
#endif
        }

        public static IEnumerator UpdateResourceBundleLocalAll()
        {
            return UpdateResourceBundleLocalAll(null);
        }

        public static IEnumerator UpdateResourceBundleLocalAll(Action<string, object> funcReport)
        {
            NonExistingFiles.Clear();

            var work = UpdateResourceBundleLocal("", funcReport);
            if (work != null)
            {
                while (work.MoveNext())
                {
                    yield return work.Current;
                }
            }
            //work = UpdateResourceBundleLocal("ex", funcReport);
            //if (work != null)
            //{
            //    while (work.MoveNext())
            //    {
            //        yield return work.Current;
            //    }
            //}
            foreach (var flag in GetDistributeFlags())
            {
                work = UpdateResourceBundleLocal(flag, funcReport);
                if (work != null)
                {
                    while (work.MoveNext())
                    {
                        yield return work.Current;
                    }
                }
                GC.Collect();
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            work = UpdateResourceBundleObb(funcReport);
            if (work != null)
            {
                while (work.MoveNext())
                {
                    yield return work.Current;
                }
            }
            GC.Collect();
#endif
        }

        public static bool IsFileInApp(string file)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                bool result = false;
                int retryTimes = 10;
                for (int i = 0; i < retryTimes; ++i)
                {
                    Exception error = null;
                    do
                    {
                        ZipArchive za = AndroidApkZipArchive;
                        if (za == null)
                        {
                            error = new Exception("Apk Archive Cannot be read.");
                            break;
                        }

                        try
                        {
                            var entry = za.GetEntry("assets/" + file);
                            if (entry != null)
                            {
                                result = true;
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
                            if (GLog.IsLogErrorEnabled) GLog.LogError(error);
                            throw error;
                        }
                        else
                        {
                            if (GLog.IsLogErrorEnabled) GLog.LogError(error + "\nNeed Retry " + i);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                return result;
            }
            else
            {
                var files = Application.streamingAssetsPath + "/" + file;
                if (!IsNonExistingFiles(files))
                {
                    if (PlatDependant.IsFileExist(files))
                    {
                        return true;
                    }
                    else
                    {
                        RecordNonExistingFiles(files);
                    }
                }
                return false;
            }
        }
        public static int ObbEntryType(string file)
        {
            int result = 0;
            if (ObbZipArchive != null)
            {
                int retryTimes = 10;
                for (int i = 0; i < retryTimes; ++i)
                {
                    Exception error = null;
                    do
                    {
                        ZipArchive za = ObbZipArchive;
                        if (za == null)
                        {
                            error = new Exception("Apk Archive Cannot be read.");
                            break;
                        }

                        try
                        {
                            var entry = za.GetEntry(file); // TODO: To Deng: the real path.
                            if (entry != null)
                            {
                                result = 1;
                                if (entry.CompressedLength == entry.Length)
                                {
                                    result = 2;
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
                            if (GLog.IsLogErrorEnabled) GLog.LogException(error);
                            throw error;
                        }
                        else
                        {
                            if (GLog.IsLogErrorEnabled) GLog.LogError(error + "\n" + "Need Retry " + i);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return result;
        }
        public static bool IsFileInObb(string file)
        {
            return ObbEntryType(file) != 0;
        }

        public static IEnumerator UpdateResourceBundleLocal(string dflag, Action<string, object> funcReport)
        {
            var tarkey = "res" + (dflag ?? "");
            int tarver = 0;

            bool firstLoad = false;
            var txtver = Resources.Load("res/version", typeof(TextAsset)) as TextAsset;
            if (txtver != null)
            {
                var strversion = txtver.text;
                if (strversion != null)
                {
                    var lines = strversion.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        string key = "";
                        string[] version = line.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (version.Length >= 1)
                        {
                            key = version[0];
                            if (version.Length >= 2)
                            {
                                int.TryParse(version[1], out tarver);
                            }
                        }
                        if (key == tarkey)
                        {
                            if (IsCachedAssetOutOfDate(key, tarver))
                            {
                                firstLoad = true;
                            }
                            break;
                        }
                    }
                }
            }

            if (firstLoad)
            {
                if (string.IsNullOrEmpty(dflag))
                {
                    var cachedVersion = GetCachedVersion();
                    int resexver = 0;
                    if (cachedVersion.ContainsKey("resex"))
                    {
                        resexver = cachedVersion.Get<int>("resex");
                        if (resexver < tarver)
                        {
                            RecordCacheVersion("resex", 0);
                        }
                    }
                }
                if (funcReport != null)
                {
                    funcReport("FirstLoad", true);
                }
                if (Application.streamingAssetsPath.Contains("://") && (Application.platform != RuntimePlatform.Android || !_LoadAssetsFromApk))
                {
                    if (Application.platform == RuntimePlatform.Android)
                    {
                        int retryTimes = 10;
                        HashSet<string> successItems = new HashSet<string>();
                        for (int i = 0; i < retryTimes; ++i)
                        {
                            Exception error = null;
                            do
                            {
                                ZipArchive za = AndroidApkZipArchive;
                                if (za == null)
                                {
                                    error = new Exception("Apk Archive Cannot be read.");
                                    break;
                                }
                                var pre1 = "assets/res/distribute/" + (dflag ?? "");
                                var pre2 = "assets/res/res_distribute_" + (dflag ?? "") + "_";
                                var pre3 = "assets/res/s_res_distribute_" + (dflag ?? "") + "_";

                                var entries = new List<ZipArchiveEntry>();
                                try
                                {
                                    foreach (var entry in za.Entries)
                                    {
                                        if (entry != null
                                            && entry.FullName.StartsWith("assets/res/")
                                            && (!entry.FullName.StartsWith("assets/res/distribute/") && !entry.FullName.StartsWith("assets/res/res_distribute_") && !entry.FullName.StartsWith("assets/res/s_res_distribute_") && string.IsNullOrEmpty(dflag)
                                                || !string.IsNullOrEmpty(dflag) && (entry.FullName.StartsWith(pre1) || entry.FullName.StartsWith(pre2) || entry.FullName.StartsWith(pre3))))
                                        {
                                            entries.Add(entry);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    error = e;
                                    break;
                                }
                                if (funcReport != null)
                                {
                                    funcReport("Count", entries.Count);
                                }
                                int tick = Environment.TickCount;
                                for (int j = 0; j < entries.Count; ++j)
                                {
                                    var entry = entries[j];
                                    var fullname = entry.FullName;
                                    if (funcReport != null)
                                    {
                                        funcReport("Progress", fullname);
                                    }
                                    if (!successItems.Contains(fullname))
                                    {
                                        try
                                        {
                                            using (var srcstream = entry.Open())
                                            {
                                                var pathd = ResManager.UpdatePath + fullname.Substring("assets".Length);
                                                using (var dststream = PlatExt.PlatDependant.OpenWrite(pathd + ".tmp"))
                                                {
                                                    srcstream.CopyTo(dststream);
                                                }
                                                PlatExt.PlatDependant.MoveFile(pathd + ".tmp", pathd);
                                            }
                                            successItems.Add(fullname);
                                        }
                                        catch (Exception e)
                                        {
                                            error = e;
                                            break;
                                        }
                                    }
                                    int newtick = Environment.TickCount;
                                    if (newtick - tick > 200)
                                    {
                                        yield return null;
                                        tick = Environment.TickCount;
                                    }
                                }
                            } while (false);
                            if (error != null)
                            {
                                if (i == retryTimes - 1)
                                {
                                    if (GLog.IsLogErrorEnabled) GLog.LogException(error);
                                    throw error;
                                }
                                else
                                {
                                    if (GLog.IsLogErrorEnabled) GLog.LogError(error + "\nNeed Retry " + i);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        var txtlist = Resources.Load("res/list", typeof(TextAsset)) as TextAsset;
                        if (txtlist != null)
                        {
                            // list
                            var strlist = txtlist.text;
                            // Copy
                            if (strlist != null)
                            {
                                var pre1 = "res/distribute/" + (dflag ?? "");
                                var pre2 = "res/res_distribute_" + (dflag ?? "") + "_";
                                var pre3 = "res/s_res_distribute_" + (dflag ?? "") + "_";
                                var alllines = strlist.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                                List<string> lines = new List<string>();
                                for (int j = 0; j < alllines.Length; ++j)
                                {
                                    var entry = alllines[j];
                                    if (entry != null
                                        && entry.StartsWith("res/")
                                        && (!entry.StartsWith("res/distribute/") && !entry.StartsWith("res/res_distribute_") && !entry.StartsWith("res/s_res_distribute_") && string.IsNullOrEmpty(dflag)
                                            || !string.IsNullOrEmpty(dflag) && (entry.StartsWith(pre1) || entry.StartsWith(pre2) || entry.StartsWith(pre3))))
                                    {
                                        lines.Add(entry);
                                    }
                                }
                                if (funcReport != null)
                                {
                                    funcReport("Count", lines.Count);
                                }
                                for (int j = 0; j < lines.Count; ++j)
                                {
                                    var line = lines[j];
                                    if (funcReport != null)
                                    {
                                        funcReport("Progress", line);
                                    }
                                    var work = CopyFromStreamingAssets(line);
                                    if (work != null)
                                    {
                                        while (work.MoveNext())
                                        {
                                            yield return work.Current;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    string[] allfiles = null;
                    try
                    {
                        allfiles = Capstones.PlatExt.PlatDependant.GetAllFiles(ResManager.UpdatePath + "/res");
                    }
                    catch { }
                    if (allfiles != null)
                    {
                        var pre1n = ResManager.UpdatePath + "/res/distribute/";
                        var pre2n = ResManager.UpdatePath + "/res/res_distribute_";
                        var pre3n = ResManager.UpdatePath + "/res/s_res_distribute_";

                        var pre1 = ResManager.UpdatePath + "/res/distribute/" + (dflag ?? "");
                        var pre2 = ResManager.UpdatePath + "/res/res_distribute_" + (dflag ?? "") + "_";
                        var pre3 = ResManager.UpdatePath + "/res/s_res_distribute_" + (dflag ?? "") + "_";

                        List<string> files = new List<string>();
                        for (int j = 0; j < allfiles.Length; ++j)
                        {
                            var entry = allfiles[j];
                            if (entry != null
                                && (!entry.StartsWith(pre1n) && !entry.StartsWith(pre2n) && !entry.StartsWith(pre3n) && string.IsNullOrEmpty(dflag)
                                    || !string.IsNullOrEmpty(dflag) && (entry.StartsWith(pre1) || entry.StartsWith(pre2) || entry.StartsWith(pre3))))
                            {
                                var file = entry.Substring(ResManager.UpdatePath.Length + 1);
                                if (IsFileInApp(file))
                                {
                                    files.Add(entry);
                                }
                            }
                        }
                        if (funcReport != null)
                        {
                            funcReport("Count", files.Count);
                        }
                        int tick = Environment.TickCount;
                        for (int j = 0; j < files.Count; ++j)
                        {
                            var file = files[j];
                            if (funcReport != null)
                            {
                                funcReport("Progress", file);
                            }
                            try
                            {
                                Capstones.PlatExt.PlatDependant.DeleteFile(file);
                            }
                            catch { }
                            int newtick = Environment.TickCount;
                            if (newtick - tick > 200)
                            {
                                yield return null;
                                tick = Environment.TickCount;
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(dflag))
                {
                    var descpath = ResManager.UpdatePath + "/resdesc/";
                    var forbidpath = descpath + "Assets/CapstonesRes/distribute/";
                    var files = GetAllFiles(descpath);
                    if (files != null)
                    {
                        if (funcReport != null)
                        {
                            funcReport("Count", files.Length);
                        }
                        int tick = Environment.TickCount;
                        for (int i = 0; i < files.Length; ++i)
                        {
                            var file = files[i];
                            if (funcReport != null)
                            {
                                funcReport("Progress", file);
                            }
                            if (file != null && !file.StartsWith(forbidpath))
                            {
                                PlatDependant.DeleteFile(file);
                            }
                            int newtick = Environment.TickCount;
                            if (newtick - tick > 200)
                            {
                                yield return null;
                                tick = Environment.TickCount;
                            }
                        }
                    }
                }
                else
                {
                    var descpath = ResManager.UpdatePath + "/resdesc/Assets/CapstonesRes/distribute/" + dflag + "/";
                    var files = GetAllFiles(descpath);
                    if (files != null)
                    {
                        if (funcReport != null)
                        {
                            funcReport("Count", files.Length);
                        }
                        int tick = Environment.TickCount;
                        for (int i = 0; i < files.Length; ++i)
                        {
                            var file = files[i];
                            if (funcReport != null)
                            {
                                funcReport("Progress", file);
                            }
                            if (file != null)
                            {
                                PlatDependant.DeleteFile(file);
                            }
                            int newtick = Environment.TickCount;
                            if (newtick - tick > 200)
                            {
                                yield return null;
                                tick = Environment.TickCount;
                            }
                        }
                    }
                }

                RecordCacheVersion(tarkey, tarver);
            }
        }
        public static IEnumerator UpdateResourceBundleObb(Action<string, object> funcReport)
        {
            if (ObbZipArchive != null)
            {
                bool firstLoad = false;
                var resvers = GetCachedVersion();
                var resexver = resvers.Get<int>("resex");
                var resver = resvers.Get<int>("res");
                if (resexver == 0)
                {
                    if (XLuaExt.LuaEvent.TrigClrEvent<bool>("CHECK_MAIN_OBB"))
                    {
                        firstLoad = true;
                    }
                }

                if (firstLoad)
                {
                    if (funcReport != null)
                    {
                        funcReport("FirstLoad", true);
                    }
                    {
                        int retryTimes = 10;
                        HashSet<string> successItems = new HashSet<string>();
                        for (int i = 0; i < retryTimes; ++i)
                        {
                            Exception error = null;
                            do
                            {
                                ZipArchive za = ObbZipArchive;
                                if (za == null)
                                {
                                    error = new Exception("Apk Archive Cannot be read.");
                                    break;
                                }

                                var entries = new List<ZipArchiveEntry>();
                                try
                                {
                                    foreach (var entry in za.Entries)
                                    {
                                        if (entry != null
                                            && entry.FullName.StartsWith("res/")) // TODO: To Deng: the real path.
                                        {
                                            entries.Add(entry);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    error = e;
                                    break;
                                }
                                if (funcReport != null)
                                {
                                    funcReport("Count", entries.Count);
                                }
                                int tick = Environment.TickCount;
                                for (int j = 0; j < entries.Count; ++j)
                                {
                                    var entry = entries[j];
                                    var fullname = entry.FullName;
                                    if (funcReport != null)
                                    {
                                        funcReport("Progress", fullname);
                                    }
                                    if (!successItems.Contains(fullname))
                                    {
                                        try
                                        {
                                            var pathd = ResManager.UpdatePath + "/" + fullname; // TODO: To Deng: the real path.
                                            bool uncompressed = false;
                                            try
                                            {
                                                uncompressed = entry.Length == entry.CompressedLength;
                                            }
                                            catch { }
                                            if (_LoadAssetsFromObb && uncompressed)
                                            {
                                                PlatExt.PlatDependant.DeleteFile(pathd);
                                            }
                                            else
                                            {
                                                if (_LoadAssetsFromObb)
                                                {
                                                    if (GLog.IsLogWarningEnabled) GLog.LogWarning("Compressed entry in obb file: " + fullname);
                                                }
                                                using (var srcstream = entry.Open())
                                                {
                                                    using (var dststream = PlatExt.PlatDependant.OpenWrite(pathd + ".tmp"))
                                                    {
                                                        srcstream.CopyTo(dststream);
                                                    }
                                                    PlatExt.PlatDependant.MoveFile(pathd + ".tmp", pathd);
                                                }
                                            }
                                            successItems.Add(fullname);
                                        }
                                        catch (Exception e)
                                        {
                                            error = e;
                                            break;
                                        }
                                    }
                                    int newtick = Environment.TickCount;
                                    if (newtick - tick > 200)
                                    {
                                        yield return null;
                                        tick = Environment.TickCount;
                                    }
                                }
                            } while (false);
                            if (error != null)
                            {
                                if (i == retryTimes - 1)
                                {
                                    if (GLog.IsLogErrorEnabled) GLog.LogException(error);
                                    throw error;
                                }
                                else
                                {
                                    if (GLog.IsLogErrorEnabled) GLog.LogError(error + "\nNeed Retry " + i);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    RecordCacheVersion("resex", resver);
                }
            }
        }

        public static IEnumerator CopyFromStreamingAssets(string path)
        {
            var paths = Application.streamingAssetsPath + "/" + path;
            var pathd = ResManager.UpdatePath + "/" + path;
            Capstones.PlatExt.PlatDependant.DeleteFile(pathd);
            Capstones.PlatExt.PlatDependant.CreateFolder(System.IO.Path.GetDirectoryName(pathd));
            if (Application.streamingAssetsPath.Contains("://"))
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    ZipArchive za = AndroidApkZipArchive;
                    if (za != null)
                    {
                        ZipArchiveEntry entry = null;
                        try
                        {
                            entry = za.GetEntry("assets/" + path);
                        }
                        catch { }
                        if (entry != null)
                        {
                            try
                            {
                                using (var srcstream = entry.Open())
                                {
                                    using (var dststream = PlatExt.PlatDependant.OpenWrite(pathd + ".tmp"))
                                    {
                                        srcstream.CopyTo(dststream);
                                    }
                                    PlatExt.PlatDependant.MoveFile(pathd + ".tmp", pathd);
                                }
                            }
                            catch { }
                        }
                    }
                }
                else
                {
                    var www = new WWW(paths);
                    yield return www;
                    byte[] data = null;
                    try
                    {
                        data = www.bytes;
                    }
                    catch
                    {
                        yield break;
                    }
                    if (www != null)
                    {
                        www.Dispose();
                    }
                    if (data != null)
                    {
                        try
                        {
                            using (var stream = Capstones.PlatExt.PlatDependant.OpenWrite(pathd + ".tmp"))
                            {
                                stream.Write(data, 0, data.Length);
                            }
                            Capstones.PlatExt.PlatDependant.MoveFile(pathd + ".tmp", pathd);
                        }
                        catch { }
                    }
                }
            }
            else
            {
                try
                {
                    using (var streams = Capstones.PlatExt.PlatDependant.OpenRead(paths))
                    {
                        using (var streamd = Capstones.PlatExt.PlatDependant.OpenWrite(pathd + ".tmp"))
                        {
                            streams.CopyTo(streamd);
                        }
                        Capstones.PlatExt.PlatDependant.MoveFile(pathd + ".tmp", pathd);
                    }
                }
                catch { }
            }
        }

        public static void UnzipPackage(byte[] data, string dir)
        {
            UnzipPackageRaw(data, dir);
        }
        public static void UnzipPackage(string src, string dst)
        {
            UnzipPackageRaw(src, dst);
        }
        public static void UnzipPackage(System.IO.Stream stream, string dst)
        {
            UnzipPackageRaw(stream, dst);
        }

        public static void UnzipPackageRaw(object src, string dir)
        {
            var path = dir;
            if (path == null)
            {
                path = ResManager.UpdatePath;
            }
            path = path.Replace('\\', '/');
            if (!path.EndsWith("/"))
            {
                path = path + "/";
            }

            int retryTimes = 3;
            HashSet<string> successEntries = new HashSet<string>();
            if (src is System.IO.Stream)
            {
                retryTimes = 1;
            }
            for (int i = 0; i < retryTimes; ++i)
            {
                System.IO.Stream stream = null;
                bool closeStreamAfterUse = true;
                try
                {
                    if (src is System.IO.Stream)
                    {
                        stream = src as System.IO.Stream;
                        closeStreamAfterUse = false;
                    }
                    else if (src is byte[])
                    {
                        stream = new System.IO.MemoryStream(src as byte[], false);
                    }
                    else if (src is string)
                    {
                        stream = Capstones.PlatExt.PlatDependant.OpenRead(src as string);
                    }
                    if (stream != null)
                    {
                        using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
                        {
                            foreach (var entry in zip.Entries)
                            {
                                var fullname = entry.FullName;
                                if (successEntries.Contains(fullname))
                                {
                                    continue;
                                }
                                if (GLog.IsLogInfoEnabled) GLog.LogInfo("Unzip entry: " + fullname + " to: " + dir);
                                if (fullname.EndsWith("/") || fullname.EndsWith("\\"))
                                {
                                    if (GLog.IsLogInfoEnabled) GLog.LogInfo("dir, skipped.");
                                    successEntries.Add(fullname);
                                    continue;
                                }
                                using (var streams = entry.Open())
                                {
                                    var pathd = path + entry.FullName;
                                    using (var streamd = Capstones.PlatExt.PlatDependant.OpenWrite(pathd + ".tmp"))
                                    {
                                        streams.CopyTo(streamd);
                                        streamd.Flush();
                                    }
                                    Capstones.PlatExt.PlatDependant.MoveFile(pathd + ".tmp", pathd);
                                }
                                successEntries.Add(fullname);
                            }
                        }
                    }
                    break;
                }
                catch (Exception e)
                {
                    if (i == retryTimes - 1)
                    {
                        if (GLog.IsLogErrorEnabled) GLog.LogException(e);
                        throw;
                    }
                    else
                    {
                        if (GLog.IsLogErrorEnabled) GLog.LogException(e + "\nNeed Retry " + i);
                    }
                }
                finally
                {
                    if (closeStreamAfterUse)
                    {
                        if (stream != null)
                        {
                            stream.Dispose();
                        }
                    }
                }
            }
        }

        public static Capstones.PlatExt.PlatDependant.TaskProgress UnzipPackageBackground(string src, string dst)
        {
            return Capstones.PlatExt.PlatDependant.RunBackground(prog =>
            {
                UnzipPackage(src, dst);
            });
        }

        public static XLua.LuaTable GetCachedVersion()
        {
            //LuaTable rv = UnityLua.GlobalLua["___resver"] as LuaTable;
            //if (rv != null)
            //{
            //    return rv;
            //}
            //rv = new LuaTable(UnityLua.GlobalLua.L);
            //UnityLua.GlobalLua["___resver"] = rv;

            XLua.LuaTable resVer = LuaBehaviour.luaEnv.Global.GetInPath<XLua.LuaTable>("exports.___resver");
            if (resVer != null)
            {
                return resVer;
            }
            resVer = LuaBehaviour.luaEnv.NewTable();
            LuaBehaviour.luaEnv.Global.SetInPath("exports.___resver", resVer);

            string pathCachedVersion = ResManager.UpdatePath + "/version.txt";
            if (Capstones.PlatExt.PlatDependant.IsFileExist(pathCachedVersion))
            {
                using (System.IO.StreamReader sr = Capstones.PlatExt.PlatDependant.OpenReadText(pathCachedVersion))
                {
                    while (true)
                    {
                        var line = sr.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        string[] version = line.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (version.Length >= 1)
                        {
                            string key = version[0];
                            int val = 0;
                            if (version.Length >= 2)
                            {
                                int.TryParse(version[1], out val);
                            }
                            resVer.Set(key, val);
                        }
                    }
                }
            }

            return resVer;
        }

        public static bool IsCachedAssetOutOfDate(string name, int version)
        {
            if (name == null)
            {
                name = "";
            }
            object cachedver = GetCachedVersion().Get<object>(name).ConvertType(typeof(int));
            if (!(cachedver is int))
            {
                return true;
            }
            int cv = (int)cachedver;
            return version < 0 || cv < version;
        }

        public static bool IsCachedBundleOutOfDate(AssetBundle bundle)
        {
            if (bundle == null)
            {
                return false;
            }
            string name = "";
            int ver = 0;
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
            TextAsset txt = bundle.LoadAsset<TextAsset>("version");
#else
            TextAsset txt = bundle.Load("version") as TextAsset;
#endif
            if (txt != null)
            {
                string[] version = txt.text.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (version.Length >= 1)
                {
                    name = version[0];
                    if (version.Length >= 2)
                    {
                        int.TryParse(version[1], out ver);
                    }
                }
            }
            return IsCachedAssetOutOfDate(name, ver);
        }

        public static void RecordCacheVersion(string name, int version)
        {
            if (!string.IsNullOrEmpty(name))
            {
                LinkedList<KeyValuePair<string, int>> list = new LinkedList<KeyValuePair<string, int>>();
                Dictionary<string, LinkedListNode<KeyValuePair<string, int>>> dict = new Dictionary<string, LinkedListNode<KeyValuePair<string, int>>>();

                string pathCachedVersion = ResManager.UpdatePath + "/version.txt";
                if (Capstones.PlatExt.PlatDependant.IsFileExist(pathCachedVersion))
                {
                    using (System.IO.StreamReader sr = Capstones.PlatExt.PlatDependant.OpenReadText(pathCachedVersion))
                    {
                        while (true)
                        {
                            var line = sr.ReadLine();
                            if (line == null)
                            {
                                break;
                            }
                            string[] arrversion = line.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                            if (arrversion.Length >= 1)
                            {
                                string key = arrversion[0];
                                int val = 0;
                                if (arrversion.Length >= 2)
                                {
                                    int.TryParse(arrversion[1], out val);
                                }

                                LinkedListNode<KeyValuePair<string, int>> node = null;
                                if (dict.TryGetValue(key, out node))
                                {
                                    if (node != null)
                                    {
                                        list.Remove(node);
                                    }
                                }
                                dict[key] = list.AddLast(new KeyValuePair<string, int>(key, val));
                            }
                        }
                    }
                }

                {
                    LinkedListNode<KeyValuePair<string, int>> node = null;
                    if (dict.TryGetValue(name, out node))
                    {
                        if (node != null)
                        {
                            list.Remove(node);
                        }
                    }
                    dict[name] = list.AddLast(new KeyValuePair<string, int>(name, version));
                }

                Capstones.PlatExt.PlatDependant.DeleteFile(pathCachedVersion);
                using (System.IO.StreamWriter sw = Capstones.PlatExt.PlatDependant.OpenWriteText(pathCachedVersion + ".tmp"))
                {
                    foreach (var node in list)
                    {
                        sw.Write(node.Key);
                        sw.Write("|");
                        sw.Write(node.Value);
                        sw.Write("\r\n");
                    }
                }
                Capstones.PlatExt.PlatDependant.MoveFile(pathCachedVersion + ".tmp", pathCachedVersion);
                //LuaTable rv = UnityLua.GlobalLua["___resver"] as LuaTable;
                //if (rv == null)
                //{
                //    rv = new LuaTable(UnityLua.GlobalLua.L);
                //    UnityLua.GlobalLua["___resver"] = rv;
                //}
                //rv[name] = version;

                XLua.LuaTable resVer = LuaBehaviour.luaEnv.Global.GetInPath<XLua.LuaTable>("exports.___resver");
                if (resVer == null)
                {
                    resVer = LuaBehaviour.luaEnv.NewTable();
                    LuaBehaviour.luaEnv.Global.SetInPath("exports.___resver", resVer);
                }
                resVer.Set(name, version);
            }
        }

        public static void ResetCacheVersion()
        {
            string pathCachedVersion = ResManager.UpdatePath + "/version.txt";
            Capstones.PlatExt.PlatDependant.DeleteFile(pathCachedVersion);
            //UnityLua.GlobalLua["___resver"] = null;
            LuaBehaviour.luaEnv.Global.SetInPath<object>("exports.___resver", null);
        }

        public static bool IsCachedBundleOutOfDateThenUpdate(AssetBundle bundle, Func<string, int, bool> doUpdate)
        {
            if (bundle == null)
            {
                return false;
            }
            string name = "";
            int ver = 0;
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
            TextAsset txt = bundle.LoadAsset<TextAsset>("version");
#else
            TextAsset txt = bundle.Load("version") as TextAsset;
#endif
            if (txt != null)
            {
                string[] version = txt.text.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (version.Length >= 1)
                {
                    name = version[0];
                    if (version.Length >= 2)
                    {
                        int.TryParse(version[1], out ver);
                    }
                }
            }
            if (IsCachedAssetOutOfDate(name, ver))
            {
                bool blockRecord = false;
                if (doUpdate != null)
                {
                    blockRecord = doUpdate(name, ver);
                }
                if (!blockRecord)
                {
                    RecordCacheVersion(name, ver);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsCachedBundleOutOfDateThenUpdate(AssetBundle bundle)
        {
            return IsCachedBundleOutOfDateThenUpdate(bundle, (key, ver) => true);
        }

        public static string[] GetAllFiles(string dir)
        {
            return Capstones.PlatExt.PlatDependant.GetAllFiles(dir);
        }
        [XLua.LuaCallCSharp]
        public class AssetDesc
        {
            public string Path;
            public string Name;
            public string[] DepBundles;
            public string[] Types;
            public bool Permanent = false;
        }
        [XLua.LuaCallCSharp]
        public class AssetDesc_DynTex : AssetDesc
        {
            public int Width;
            public int Height;
        }
        [XLua.LuaCallCSharp]
        public class AssetDesc_DynSprite : AssetDesc
        {
        }
        [XLua.LuaCallCSharp]
        public class AssetDesc_FromManifest : AssetDesc
        {
            public string BundleName;
        }
        [XLua.LuaCallCSharp]
        public class AssetDesc_PackedTex : AssetDesc
        {
        }

        [XLua.LuaCallCSharp]
        public class AssetDesc_NewPackedTex : AssetDesc
        {
        }

        [XLua.LuaCallCSharp]
        public class AssetBundleInfo
        {
            public AssetBundle Bundle = null;
            public int RefCnt = 0;
            public bool Permanent = false;

            public AssetBundleInfo(AssetBundle ab)
            {
                Bundle = ab;
                RefCnt = 0;
            }

            public int AddRef()
            {
                return ++RefCnt;
            }

            public int Release()
            {
                var rv = --RefCnt;
                if (rv <= 0 && !Permanent)
                {
                    if (Bundle != null)
                    {
                        Bundle.Unload(true);
                        Bundle = null;
                    }
                }
                return rv;
            }
        }

        [XLua.LuaCallCSharp]
        public class AssetInfo
        {
            public System.WeakReference Asset = null;
            public int AssetLiveRefCnt = 0;
            public int RefCnt = 0;
            public HashSet<AssetBundleInfo> DepBundles = null;
            public AssetBundleInfo ContainingBundle = null;
            public AssetDesc Desc = null;

            public Dictionary<Type, AssetInfo> SubAssets = null;

            public bool CheckAlive()
            {
                ///掩盖问题临时关闭
                return true;
                if (SubAssets != null)
                {
                    foreach (var kvpsub in SubAssets)
                    {
                        if (kvpsub.Value.CheckAlive())
                        {
                            return true;
                        }
                    }
                }
                ///目前这里的 Asset.IsAlive 在是 gameobject资源时候为false
                if (Asset == null || !Asset.IsAlive)
                {
                    AssetLiveRefCnt = 0;
                }
                else
                {
                    var asset = Asset.GetWeakReference<object>();
                    if (asset == null)
                    {
                        AssetLiveRefCnt = 0;
                    }
                    else if (asset is UnityEngine.Object)
                    {
                        if (((UnityEngine.Object)asset) == null)
                        {
                            AssetLiveRefCnt = 0;
                        }
                    }
                }
                if (AssetLiveRefCnt + RefCnt <= 0)
                {
                    if (DepBundles != null)
                    {
                        foreach (var abi in DepBundles)
                        {
                            abi.Release();
                        }
                        DepBundles = null;
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }

            public void Destroy()
            {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
                var asset = Asset.GetWeakReference<UnityEngine.Object>();
                if (asset != null)
                {
                    UnityEngine.Object.Destroy(asset);
                }
#endif
                Asset = null;

                if (SubAssets != null)
                {
                    foreach (var kvpsub in SubAssets)
                    {
                        if (kvpsub.Value != null)
                        {
                            kvpsub.Value.Destroy();
                        }
                    }
                    SubAssets = null;
                }
                if (DepBundles != null)
                {
                    foreach (var abi in DepBundles)
                    {
                        abi.Release();
                    }
                    DepBundles = null;
                }
            }

            public int AddRef()
            {
                return ++RefCnt;
            }

            public bool Release()
            {
                --RefCnt;
                return !CheckAlive();
            }
        }
        public static Dictionary<string, AssetBundleInfo> LoadedAssetBundles = new Dictionary<string, AssetBundleInfo>();
        public static Dictionary<string, AssetInfo> LoadedAssets = new Dictionary<string, AssetInfo>();
        public static Dictionary<string, AssetDesc> AssetDescs = new Dictionary<string, AssetDesc>();

        private static void DumpLoadedAssetsImpl(StringBuilder sb, AssetInfo assetInfo, int indent)
        {
            if (assetInfo.Desc != null)
            {
                for (int i = 0; i < indent; i++)
                {
                    sb.Append("    ");
                }
                StringBuilder sbTypes = new StringBuilder();
                if (assetInfo.Desc.Types != null)
                {
                    foreach (var type in assetInfo.Desc.Types)
                    {
                        sbTypes.AppendFormat("{0},", type.ToString());
                    }
                }

                StringBuilder sbBundles = new StringBuilder();
                if (assetInfo.Desc.DepBundles != null)
                {
                    foreach (var bundle in assetInfo.Desc.DepBundles)
                    {
                        sbBundles.AppendFormat("{0},", bundle);
                    }
                }

                sb.AppendFormat(
                    "Name={0}, RefCnt={1}, LiveRefCnt={2}, Permant={3}, Path={4}, Types={5}, DepBundles={6}\n",
                    assetInfo.Desc.Name, assetInfo.RefCnt, assetInfo.AssetLiveRefCnt, assetInfo.Desc.Permanent,
                    assetInfo.Desc.Path, sbTypes, sbBundles);

                if (assetInfo.SubAssets != null)
                {
                    foreach (var subAsset in assetInfo.SubAssets)
                    {
                        DumpLoadedAssetsImpl(sb, subAsset.Value, indent + 1);
                    }
                }
            }
        }

        public static void DumpLoadedAssets()
        {
            var logFilePath = string.Format("{0}/LoadedAssets_{1}.log", Application.persistentDataPath, System.DateTime.Now.ToString("yyyy_MM_dd_hh_MM_ss_fff"));

            StringBuilder sb = new StringBuilder();

            foreach (var kv in LoadedAssets)
            {
                DumpLoadedAssetsImpl(sb, kv.Value, 0);
            }

            using (var writer = PlatDependant.OpenWriteText(logFilePath))
            {
                writer.WriteLine(sb);
                writer.Flush();
            }
        }

        public static AssetDesc TryGetAssetDesc(string asset)
        {
            AssetDesc rv;
            if (AssetDescs.TryGetValue(asset, out rv))
            {
                return rv;
            }

#if (UNITY_5 || UNITY_5_3_OR_NEWER)
            if (ResManifest)
            {
                var ad = new AssetDesc_FromManifest();
                ad.Path = asset;
                AssetDescs[asset] = ad;
                return ad;
            }
            else
#endif
            {
                var path = asset;
                string fpath = null;
                string[] distributeFlags = GetDistributeFlags();
                if (path.StartsWith("Assets/CapstonesRes/"))
                {
                    var subpath = path.Substring("Assets/CapstonesRes/".Length);
                    for (int i = distributeFlags.Length - 1; i >= 0; --i)
                    {
                        var flag = distributeFlags[i];
                        if (!string.IsNullOrEmpty(flag))
                        {
                            var dpath = ResManager.UpdatePath + "/resdesc/Assets/CapstonesRes/distribute/" + flag + "/" + subpath + ".desc.txt";
                            if (!IsNonExistingFiles(dpath))
                            {
                                if (PlatDependant.IsFileExist(dpath))
                                {
                                    fpath = dpath;
                                    break;
                                }
                                else
                                {
                                    RecordNonExistingFiles(dpath);
                                }
                            }
                        }
                    }
                }
                if (fpath == null)
                {
                    var dpath = ResManager.UpdatePath + "/resdesc/" + path + ".desc.txt";
                    if (!IsNonExistingFiles(dpath))
                    {
                        if (PlatDependant.IsFileExist(dpath))
                        {
                            fpath = dpath;
                        }
                        else
                        {
                            RecordNonExistingFiles(dpath);
                        }
                    }
                }
                if (fpath != null)
                {
                    using (var sr = PlatDependant.OpenReadText(fpath))
                    {
                        try
                        {
                            var line = sr.ReadLine();
                            ParseAssetsDesc(line);
                            AssetDescs.TryGetValue(asset, out rv);
                            if (rv != null)
                            {
                                return rv;
                            }
                        }
                        catch (Exception e)
                        {
                            if (GLog.IsLogErrorEnabled) GLog.LogException(e);
                        }
                    }
                }
            }
            AssetDescs[asset] = null;
            return null;
        }
        private static AssetDesc GetAssetDesc(string asset)
        {
            AssetDesc rv = TryGetAssetDesc(asset);
            if (rv == null)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogError(asset + " not found.");
            }
            return rv;
        }

        public static void ParseAssetsDesc(string strdesc)
        {
            if (strdesc != null)
            {
                var lines = strdesc.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (lines != null)
                {
                    foreach (var line in lines)
                    {
                        string[] infos = line.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (infos.Length >= 2)
                        {
                            if (!string.IsNullOrEmpty(infos[0]) && !string.IsNullOrEmpty(infos[1]))
                            {
                                AssetDesc ad = null;
                                if (infos.Length >= 5)
                                {
                                    var tinfos = infos[4].Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                                    if (tinfos != null && tinfos.Length == 1 && tinfos[0] == "DynTex")
                                    {
                                        ad = new AssetDesc_DynTex();
                                        var addyn = ad as AssetDesc_DynTex;
                                        var sizeinfo = infos[3].Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                                        if (sizeinfo != null)
                                        {
                                            if (sizeinfo.Length >= 1)
                                            {
                                                int.TryParse(sizeinfo[0], out addyn.Width);
                                            }
                                            if (sizeinfo.Length >= 2)
                                            {
                                                int.TryParse(sizeinfo[1], out addyn.Height);
                                            }
                                        }
                                        // TODO: Load sub-sprites's info. But this seems useless.
                                    }
                                    else if (tinfos != null && tinfos.Length == 1 && tinfos[0] == "DynSprite")
                                    {
                                        ad = new AssetDesc_DynSprite();
                                    }
                                    else if (tinfos != null && tinfos.Length == 1 && tinfos[0] == "Redirect")
                                    {
                                        var realad = new AssetDesc_FromManifest();
                                        realad.Path = infos[0];
                                        realad.Name = infos[0].ToLower();
                                        if (infos.Length >= 3)
                                        {
                                            realad.BundleName = infos[2].ToLower().Replace('/', '_') + ".ab";
                                        }
                                        AssetDescs[GetUndistributedAssetPath(realad.Path)] = realad;
                                        continue;
                                    }
                                    else if (tinfos != null && tinfos.Length > 0 && tinfos[tinfos.Length - 1] == "PackedTex")
                                    {
                                        var realad = new AssetDesc_PackedTex();
                                        realad.Path = infos[0];
                                        realad.Name = infos[1];
                                        realad.DepBundles = new[] { infos[2] };
                                        if (tinfos.Length > 1)
                                        {
                                            realad.Types = tinfos[0].Split(new char[] { '*' }, System.StringSplitOptions.RemoveEmptyEntries);
                                        }
                                        AssetDescs[GetUndistributedAssetPath(realad.Path)] = realad;
                                        continue;
                                    }
                                    else if (tinfos != null && tinfos.Length > 0 && tinfos[tinfos.Length - 1] == "NewPackedTex")
                                    {
                                        var realad = new AssetDesc_NewPackedTex();
                                        realad.Path = infos[0];
                                        realad.Name = infos[1];
                                        realad.DepBundles = new[] { infos[2] };
                                        if (tinfos.Length > 1)
                                        {
                                            realad.Types = tinfos[0].Split(new char[] { '*' }, System.StringSplitOptions.RemoveEmptyEntries);
                                        }
                                        AssetDescs[GetUndistributedAssetPath(realad.Path)] = realad;
                                        continue;
                                    }
                                    else
                                    {
                                        ad = new AssetDesc();
                                    }
                                    ad.Types = tinfos;
                                }
                                if (ad == null)
                                {
                                    ad = new AssetDesc();
                                }
                                ad.Path = infos[0];
                                ad.Name = infos[1];
                                if (infos.Length >= 3)
                                {
                                    var dinfos = infos[2].Split(new char[] { '<' }, System.StringSplitOptions.RemoveEmptyEntries);
                                    ad.DepBundles = dinfos;
                                }
                                AssetDescs[GetUndistributedAssetPath(ad.Path)] = ad;
                            }
                        }
                    }
                }
            }
        }

        public static void UnloadResForDistribute(string flag)
        {
            string pre1 = "res/distribute/" + flag + "/";
            string pre2 = "s/res/distribute/" + flag + "/";
            //string pre3 = "Assets/CapstonesRes/distribute/" + flag + "/";
            List<string> unused = new List<string>();
            foreach (var kvpabi in LoadedAssetBundles)
            {
                var bname = kvpabi.Key;
                if (bname.StartsWith(pre1) || bname.StartsWith(pre2))
                {
                    unused.Add(bname);
                    var abi = kvpabi.Value;
                    if (abi != null && abi.Bundle != null)
                    {
                        abi.Bundle.Unload(true);
                    }
                }
            }
            for (int i = 0; i < unused.Count; ++i)
            {
                LoadedAssetBundles.Remove(unused[i]);
            }

            UpdateResInfo();
        }
        public static void ReinitResForDistribute(string flag)
        {
            UpdateResInfo();
            UnloadUnusedBundle();
        }
        public static void UpdateResInfo()
        {
            var oldAssetDescs = AssetDescs;
            AssetDescs = new Dictionary<string, AssetDesc>();
            var work = ResManager.SplitResIndexAsyncAll(null);
            if (work != null)
            {
                while (work.MoveNext())
                {
                }
            }
            foreach (var kvploaded in LoadedAssets)
            {
                GetAssetDesc(kvploaded.Key);
            }

            List<string> unused = new List<string>();
            foreach (var kvpold in oldAssetDescs)
            {
                AssetDesc oldad = kvpold.Value;
                AssetDesc newad;
                AssetDescs.TryGetValue(kvpold.Key, out newad);

                if (oldad == null && newad == null)
                {
                    continue;
                }
                else
                {
                    if (oldad != null && newad != null)
                    {
                        if (oldad.Path == newad.Path)
                        {
                            continue;
                        }
                    }
                }
                unused.Add(kvpold.Key);
            }
            for (int i = 0; i < unused.Count; ++i)
            {
                var asset = unused[i];
                AssetDescs.Remove(asset);
                AssetInfo ai;
                LoadedAssets.TryGetValue(asset, out ai);
                if (ai != null)
                {
                    ai.Destroy();
                }
                LoadedAssets.Remove(asset);
            }

            foreach (var kvpad in AssetDescs)
            {
                if (!oldAssetDescs.ContainsKey(kvpad.Key))
                {
                    oldAssetDescs[kvpad.Key] = kvpad.Value;
                }
            }
            AssetDescs = oldAssetDescs;
        }

        public static void SplitResIndex(string file)
        {
            var work = SplitResIndexAsync(file, null);
            if (work != null)
            {
                while (work.MoveNext())
                {
                }
            }
        }

        public static IEnumerator SplitResIndexAsync(string file, Action<string, object> funcReport)
        {
            var dest = ResManager.UpdatePath + "/" + file;
            var pathi = dest;
            if (!Capstones.PlatExt.PlatDependant.IsFileExist(pathi))
            {
                if (Application.streamingAssetsPath.Contains("://"))
                {
                    pathi = null;
                    if (Application.platform == RuntimePlatform.Android && _LoadAssetsFromApk)
                    {
                        int retryTimes = 10;
                        for (int i = 0; i < retryTimes; ++i)
                        {
                            GC.Collect();
                            Exception error = null;
                            do
                            {
                                ZipArchive za = AndroidApkZipArchive;
                                if (za == null)
                                {
                                    error = new Exception("Apk Archive Cannot be read.");
                                    break;
                                }
                                var entryname = "assets/" + file;
                                try
                                {
                                    var entry = za.GetEntry(entryname);
                                    if (entry != null)
                                    {
                                        pathi = dest;
                                        using (var srcstream = entry.Open())
                                        {
                                            using (var dststream = PlatDependant.OpenWrite(dest + ".tmp"))
                                            {
                                                srcstream.CopyTo(dststream);
                                            }
                                            PlatDependant.MoveFile(dest + ".tmp", dest);
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
                                    if (GLog.IsLogErrorEnabled) GLog.LogException(error);
                                    throw error;
                                }
                                else
                                {
                                    if (GLog.IsLogErrorEnabled) GLog.LogException(error + "\nNeed Retry " + i);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    pathi = Application.streamingAssetsPath + "/" + file;
                    if (!Capstones.PlatExt.PlatDependant.IsFileExist(pathi))
                    {
                        pathi = null;
                    }
                }
            }
            if (pathi != null)
            {
                List<string> lines = new List<string>(15000);
                using (var srsrc = PlatDependant.OpenReadText(pathi))
                {
                    while (true)
                    {
                        var line = srsrc.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        if (!string.IsNullOrEmpty(line))
                        {
                            lines.Add(line);
                        }
                    }
                }
                int tick = Environment.TickCount;
                if (funcReport != null)
                {
                    funcReport("Count", lines.Count);
                }
                if (lines.Count > 0)
                {
                    if (funcReport != null)
                    {
                        funcReport("FirstLoad", true);
                    }
                }
                for (int i = 0; i < lines.Count; ++i)
                {
                    var line = lines[i];
                    if (funcReport != null)
                    {
                        funcReport("Progress", line);
                    }
                    if (line != "")
                    {
                        var infos = line.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        if (infos != null && infos.Length > 0)
                        {
                            var asset = infos[0];
                            if (!string.IsNullOrEmpty(asset))
                            {
                                var descfile = ResManager.UpdatePath + "/resdesc/" + asset + ".desc.txt";
                                if (infos.Length > 1)
                                {
                                    using (var swdesc = PlatDependant.OpenWriteText(descfile + ".tmp"))
                                    {
                                        swdesc.Write(line);
                                    }
                                    PlatDependant.MoveFile(descfile + ".tmp", descfile);
                                }
                                else
                                {
                                    PlatDependant.DeleteFile(descfile);
                                }
                            }
                        }
                    }
                    int newtick = Environment.TickCount;
                    if (newtick - tick > 200)
                    {
                        yield return null;
                        tick = Environment.TickCount;
                    }
                }
                using (var sdest = PlatDependant.OpenWrite(dest))
                {
                }
            }
        }

        public static IEnumerator SplitResIndexAsyncFor(string dflag, Action<string, object> funcReport)
        {
            var indexfile = string.IsNullOrEmpty(dflag) ? "res/index.txt" : "res/distribute/" + dflag + ".txt";
            return SplitResIndexAsync(indexfile, funcReport);
        }

        public static IEnumerator SplitResIndexAsyncAll(Action<string, object> funcReport)
        {
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
            if (ResManifest) GameObject.Destroy(ResManifest);
#endif
            AssetDescs.Clear();

            double mem = XLuaExt.LuaEvent.TrigClrEvent<double>("GetMemTotal");
            if (GLog.IsLogInfoEnabled) GLog.LogInfo("SplitResIndexAsyncAll ===> " + mem);
            //if (Application.streamingAssetsPath.Contains("://") && mem > 0 && mem < 1)
            //{
            //    var work = SplitResIndexAsyncFor("", funcReport);
            //    if (work != null)
            //    {
            //        while (work.MoveNext())
            //        {
            //            yield return work.Current;
            //        }
            //    }
            //    foreach (var flag in GetDistributeFlags())
            //    {
            //        work = SplitResIndexAsyncFor(flag, funcReport);
            //        if (work != null)
            //        {
            //            while (work.MoveNext())
            //            {
            //                yield return work.Current;
            //            }
            //        }
            //    }
            //}
            //else
            {
                // index
                LinkedList<string> indexFiles = new LinkedList<string>();
                indexFiles.AddLast("res/index.txt");
                foreach (var flag in GetDistributeFlags())
                {
                    indexFiles.AddLast("res/distribute/" + flag + ".txt");
                }

                foreach (var file in indexFiles)
                {
                    GC.Collect();
                    var pathi = ResManager.UpdatePath + "/" + file;
                    if (!Capstones.PlatExt.PlatDependant.IsFileExist(pathi))
                    {
                        if (Application.streamingAssetsPath.Contains("://"))
                        {
                            pathi = null;
                        }
                        else
                        {
                            pathi = Application.streamingAssetsPath + "/" + file;
                            if (!Capstones.PlatExt.PlatDependant.IsFileExist(pathi))
                            {
                                pathi = null;
                            }
                        }
                    }
                    if (pathi != null)
                    {
                        string strindex = "";
                        try
                        {
                            using (var sr = Capstones.PlatExt.PlatDependant.OpenReadText(pathi))
                            {
                                strindex = sr.ReadToEnd();
                            }
                        }
                        catch { }
                        ParseAssetsDesc(strindex);
                    }
                    else
                    {
                        if (Application.platform == RuntimePlatform.Android && _LoadAssetsFromApk)
                        {
                            int retryTimes = 10;
                            for (int i = 0; i < retryTimes; ++i)
                            {
                                GC.Collect();
                                Exception error = null;
                                do
                                {
                                    ZipArchive za = AndroidApkZipArchive;
                                    if (za == null)
                                    {
                                        error = new Exception("Apk Archive Cannot be read.");
                                        break;
                                    }
                                    var entryname = "assets/" + file;
                                    try
                                    {
                                        var entry = za.GetEntry(entryname);
                                        if (entry != null)
                                        {
                                            using (var srcstream = entry.Open())
                                            {
                                                using (var sr = new System.IO.StreamReader(srcstream))
                                                {
                                                    var strindex = sr.ReadToEnd();
                                                    ParseAssetsDesc(strindex);
                                                }
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
                                        if (GLog.IsLogErrorEnabled) GLog.LogException(error);
                                        throw error;
                                    }
                                    else
                                    {
                                        if (GLog.IsLogErrorEnabled) GLog.LogException(error + "\nNeed Retry " + i);
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

#if (UNITY_5 || UNITY_5_3_OR_NEWER)
            var descbase = GetAssetDesc("base");
            if (descbase.Name == "assetbundlemanifest")
            {
                // this is built with the new buildpipeline.
                var baseab = LoadAssetBundle("?res");
                if (baseab != null && baseab.Bundle != null)
                {
                    var baseassets = baseab.Bundle.LoadAllAssets();
                    if (baseassets != null && baseassets.Length > 0)
                    {
                        ResManifest = GameObject.Instantiate(baseassets[0]) as AssetBundleManifest;
                        if (ResManifest)
                        {
                            AssetBundleNamesFromManifest = new HashSet<string>(ResManifest.GetAllAssetBundles());
                        }
                    }
                    baseab.Bundle.Unload(true);
                }
                LoadedAssetBundles.Remove("?res");
            }
#endif
            Capstones.UnityFramework.LanguageConverter.InitData();
            yield break;
        }

        public static string GetUndistributedAssetPath(string path)
        {
            if (path != null)
            {
                if (path.StartsWith("Assets/CapstonesRes/distribute/"))
                {
                    var i4 = path.IndexOf('/', "Assets/CapstonesRes/distribute/".Length);
                    if (i4 > 0)
                    {
                        return "Assets/CapstonesRes/" + path.Substring(i4 + 1);
                    }
                }
            }
            return path;
        }

        public static void DestroyAllHard()
        {
            UnloadAllRes(true);

            var xluaBehavs = Resources.FindObjectsOfTypeAll<LuaBehaviour>();
            foreach (var behav in xluaBehavs)
            {
                if (behav != null && behav.lua != null)
                {
                    behav.OnDestroy();
                }
            }

            //#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER

            //            var oldObjs = Resources.FindObjectsOfTypeAll<GameObject>().Where(go => string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(go)));
            //            //var oldObjs = GameObject.FindObjectsOfType<GameObject>();
            //            foreach (var obj in oldObjs)
            //            {
            //                GameObject.Destroy(obj);
            //            }
            //#else
            foreach (var obj in DontDestroyOnLoadObjs)
            {
                GameObject.Destroy(obj);
            }
            DontDestroyOnLoadObjs.Clear();

            var oldObjs = FindAllGameObject();

            foreach (var obj in oldObjs)
            {
                GameObject.Destroy(obj);
            }
            //#endif
            DontDestroyOnLoadObjs.Clear();

            ObjectPool.DropAllPool();
        }

        public static void DestroyAll()
        {
            UnloadAllRes();

            var sgos = PackSceneObj();
            if (sgos != null)
            {
                foreach (var sgo in sgos)
                {
                    if (sgo == null) continue;
                    GameObject.Destroy(sgo);
                }
            }

            foreach (var obj in CanDestroyAllObjs)
            {
                GameObject.Destroy(obj);
            }
            CanDestroyAllObjs.Clear();
            AudioManager.RemoveUnusedKeys();
            DontDestroyOnLoadObjs.RemoveWhere(obj => obj == null);

            ObjectPool.DropAllPool();
        }

        #region For Font Leak - dangerous, donot use these methods.
        //#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
        public static void SaveCurObjects() { }
        public static void DestroyAllExceptSaved()
        {
            DestroyAllHard();
        }
        //#else
        //        // This is for font leak. see http://issuetracker.unity3d.com/issues/resources-dot-unloadunusedassets-doesnt-unload-fonts-if-they-were-loaded-from-an-asset-bundle
        //        private static List<WeakReference> _SavedObjects = new List<WeakReference>();
        //        public static void SaveCurObjects()
        //        {
        //            foreach(var wr in _SavedObjects)
        //            {
        //                ObjectPool.WeakReferencePool.ReturnToPool(wr);
        //            }
        //            _SavedObjects.Clear();
        //            var objs = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
        //            foreach(var obj in objs)
        //            {
        //                _SavedObjects.Add(ObjectPool.WeakReferencePool.GetFromPool(() => new WeakReference(obj), wr => wr.Target = obj));
        //            }
        //        }
        //        public static void DestroyAllExceptSaved()
        //        {
        //            DestroyAll();
        //            HashSet<UnityEngine.Object> objs = new HashSet<UnityEngine.Object>();
        //            foreach (var wr in _SavedObjects)
        //            {
        //                var obj = wr.GetWeakReference<UnityEngine.Object>();
        //                if (obj != null)
        //                {
        //                    objs.Add(obj);
        //                }
        //            }
        //            var oldObjs = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
        //            foreach (var obj in oldObjs)
        //            {
        //                if (!objs.Contains(obj))
        //                {
        //                    GameObject.Destroy(obj);
        //                }
        //            }
        //        }
        //#endif
        #endregion

        public static void UnloadAllRes()
        {
            UnloadAllRes(false);
        }

        public static void UnloadAllRes(bool unloadPermanentBundle)
        {
            var newLoadedAssetBundles = new Dictionary<string, AssetBundleInfo>();
            var newLoadedAssets = new Dictionary<string, AssetInfo>();
            foreach (var ai in LoadedAssets)
            {
                if (unloadPermanentBundle || !ai.Value.Desc.Permanent)
                {
                    ai.Value.Destroy();
                }
                else
                {
                    newLoadedAssets[ai.Key] = ai.Value;
                }
            }
            foreach (var abi in LoadedAssetBundles)
            {
                if (unloadPermanentBundle || !abi.Value.Permanent)
                {
                    if (abi.Value.Bundle != null)
                    {
                        abi.Value.Bundle.Unload(true);
                        abi.Value.Bundle = null;
                    }
                }
                else
                {
                    newLoadedAssetBundles[abi.Key] = abi.Value;
                }
            }
            LoadedAssetBundles = newLoadedAssetBundles;
            LoadedAssets = newLoadedAssets;
            Resources.UnloadUnusedAssets();
        }

        public static IEnumerator UnloadUnusedResAsync()
        {
            yield return Resources.UnloadUnusedAssets();
            UnloadUnusedRes();
        }

        public static void UnloadUnusedRes()
        {
            var keys = LoadedAssets.Keys.ToArray();
            foreach (var key in keys)
            {
                AssetInfo ai = null;
                if (LoadedAssets.TryGetValue(key, out ai))
                {
                    if (ai == null || !ai.CheckAlive())
                    {
                        LoadedAssets.Remove(key);
                    }
                }
            }
            UnloadUnusedBundle();
        }

        public static void UnloadUnusedBundle()
        {
            foreach (var kvpb in LoadedAssetBundles)
            {
                var abi = kvpb.Value;
                if (!abi.Permanent && abi.RefCnt <= 0)
                {
                    if (abi.Bundle != null)
                    {
                        abi.Bundle.Unload(true); // Because it is dangerous, we disable this and it would be turn on again when we fix all memory leak / asset leak.
                        //abi.Bundle.Unload(false); // Dangerous - may leak fonts (or other assets - now, only font leak is confirmed)
                        abi.Bundle = null;
                    }
                }
            }
        }
        /// <summary>
        ///Unload(false) 这个接口调用容易造成 unity 对资源的引用关系断裂 造成的后果就是同一个asset 会出现两个
        ///比如某贴图在显示使用或是被某变量引用，调用 Unload(false) 时候不会清理掉该贴图的asset，但是如果有
        ///新的显示对象依赖加载了该贴图asset的时候 unity 会重新加载一张贴图出来 内存中会出现两张.
        /// 后面会废弃该接口的
        /// </summary>
        public static void UnloadAllBundleSoft()
        {
            var newLoadedAssetBundles = new Dictionary<string, AssetBundleInfo>();
            foreach (var abi in LoadedAssetBundles)
            {
                if (!abi.Value.Permanent)
                {
                    if (abi.Value.Bundle != null)
                    {
                        abi.Value.Bundle.Unload(false);
                        abi.Value.Bundle = null;
                    }
                }
                else
                {
                    newLoadedAssetBundles[abi.Key] = abi.Value;
                }
            }
            LoadedAssetBundles = newLoadedAssetBundles;
        }

        public static void MarkPermanent(string assetname)
        {
#if !UNITY_EDITOR || USE_CLIENT_RES_MANAGER
            var desc = GetAssetDesc(assetname);
            if (desc != null)
            {
                desc.Permanent = true;
            }
            AssetInfo info = null;
            if (LoadedAssets.TryGetValue(assetname, out info))
            {
                if (info != null)
                {
                    if (info.DepBundles != null)
                    {
                        foreach (var bundle in info.DepBundles)
                        {
                            bundle.Permanent = true;
                        }
                    }
                    // TODO: make the info.Asset's weak-ref strong.
                }
            }
#endif
        }

#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
        public static UnityEngine.Object LoadMainAsset(string name)
        {
            UnityEngine.Object rv = null;
            try
            {
                rv = UnityEditor.AssetDatabase.LoadMainAssetAtPath(name);
            }
            catch { }
            if (rv == null || rv is GameObject || rv is Font)
            {
                return rv;
            }
            if (rv is Texture2D)
            {
                var assets = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(name);
                if (assets != null && assets.Length > 0)
                {
                    return assets[0];
                }
            }
            return rv;
        }

        public static string GetDistributeAssetName(string name)
        {
            string found = null;

            var path = name;
            string[] distributeFlags = GetDistributeFlags();
            path = path.Replace('\\', '/');
            path = path.TrimEnd('/');
            if (path.StartsWith("Assets/CapstonesRes/"))
            {
                var subpath = path.Substring("Assets/CapstonesRes/".Length);
                for (int i = distributeFlags.Length - 1; i >= 0; --i)
                {
                    var flag = distributeFlags[i];
                    if (!string.IsNullOrEmpty(flag))
                    {
                        var dpath = "Assets/CapstonesRes/distribute/" + flag + "/" + subpath;
                        var upath = "Assets/UITextures/CapstonesRes/distribute/" + flag + "/" + subpath;
                        if (System.IO.File.Exists(upath))
                        {
                            if (found == null)
                            {
                                found = upath;
                            }
                            else
                            {
                                if (GLog.IsLogWarningEnabled) GLog.LogWarning("Duplicated asset: " + found + "\n Replaces: " + upath);
                            }
                        }
                        if (System.IO.File.Exists(dpath))
                        {
                            if (found == null)
                            {
                                found = dpath;
                            }
                            else
                            {
                                if (GLog.IsLogWarningEnabled) GLog.LogWarning("Duplicated asset: " + found + "\n Replaces: " + dpath);
                            }
                        }
                    }
                }
            }
            if (path.StartsWith("Assets/"))
            {
                var upath = "Assets/UITextures/" + path.Substring(7);
                if (System.IO.File.Exists(upath))
                {
                    if (found == null)
                    {
                        found = upath;
                    }
                    else
                    {
                        if (GLog.IsLogWarningEnabled) GLog.LogWarning("Duplicated asset: " + found + "\nReplaces: " + upath);
                    }
                }
            }
            if (System.IO.File.Exists(path))
            {
                if (found == null)
                {
                    found = path;
                }
                else
                {
                    if (GLog.IsLogWarningEnabled) GLog.LogWarning("Duplicated asset: " + found + "\nReplaces: " + path);
                }
            }

            if (found == null)
            {
                //if (GLog.IsLogWarningEnabled) GLog.LogWarning("Asset not found: " + name);
                found = path;
            }
            else
            {
                var realPath = UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.AssetPathToGUID(found));
                if (realPath != found)
                {
                    if (GLog.IsLogErrorEnabled) GLog.LogError("File name case error. Loading: " + found + "\nOnDisk: " + realPath);
                }
            }

            return found;
        }
        private static bool IsEditorRunning = false;
#endif

        public static UnityEngine.Object LoadRes(string name)
        {
            return LoadRes(name, null);
        }

        private static AssetBundleInfo LoadAssetBundleFor(string asset, Func<AssetBundleInfo, AssetDesc, Dictionary<string, AssetBundleInfo>, bool> funcCheck)
        {
            Dictionary<string, AssetBundleInfo> loaded = new Dictionary<string, AssetBundleInfo>();
            AssetBundleInfo abi = null;
            AssetDesc ad = GetAssetDesc(asset);
            if (ad != null)
            {
                bool found = false;
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
                if (ad is AssetDesc_FromManifest)
                {
                    var adm = ad as AssetDesc_FromManifest;
                    if (adm.BundleName != "")
                    {
                        if (adm.BundleName == null)
                        {
                            var path = ad.Path;
                            if (path.StartsWith("Assets/CapstonesRes/"))
                            {
                                var subpath = path.Substring("Assets/CapstonesRes/".Length);
                                string[] distributeFlags = GetDistributeFlags();
                                for (int i = distributeFlags.Length - 1; i >= 0; --i)
                                {
                                    var flag = distributeFlags[i];
                                    if (!string.IsNullOrEmpty(flag))
                                    {
                                        var dpath = "Assets/CapstonesRes/distribute/" + flag + "/" + subpath;
                                        var bname = EncodeBundleName(dpath).Replace('/', '_').ToLower() + ".ab";
                                        if (AssetBundleNamesFromManifest.Contains(bname))
                                        {
                                            abi = LoadAssetBundleAndAddRef(bname);
                                            if (abi != null && abi.Bundle != null)
                                            {
                                                var aname = dpath.ToLower();
                                                if (asset.EndsWith(".unity") || abi.Bundle.Contains(aname))
                                                {
                                                    if (ad.Permanent)
                                                    {
                                                        abi.Permanent = true;
                                                    }
                                                    loaded[bname] = abi;
                                                    adm.BundleName = bname;
                                                    adm.Name = aname;
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                {
                                                    abi.Release();
                                                    abi = null;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (!found)
                            {
                                var bname = EncodeBundleName(path).Replace('/', '_').ToLower() + ".ab";
                                if (AssetBundleNamesFromManifest.Contains(bname))
                                {
                                    abi = LoadAssetBundleAndAddRef(bname);
                                    if (abi != null && abi.Bundle != null)
                                    {
                                        var aname = path.ToLower();
                                        if (asset.EndsWith(".unity") || abi.Bundle.Contains(aname))
                                        {
                                            if (ad.Permanent)
                                            {
                                                abi.Permanent = true;
                                            }
                                            loaded[bname] = abi;
                                            adm.BundleName = bname;
                                            adm.Name = aname;
                                            found = true;
                                        }
                                        else
                                        {
                                            abi.Release();
                                            abi = null;
                                        }
                                    }
                                }
                            }
                            if (!found)
                            {
                                adm.BundleName = "";
                            }
                        }
                        else
                        {
                            var dep = adm.BundleName;
                            if (!loaded.TryGetValue(dep, out abi))
                            {
                                abi = LoadAssetBundleAndAddRef(dep);
                                if (abi == null)
                                {
                                    if (GLog.IsLogErrorEnabled) GLog.LogError("Unable to load assetbundle: " + dep);
                                }
                                else
                                {
                                    if (ad.Permanent)
                                    {
                                        abi.Permanent = true;
                                    }
                                    loaded[dep] = abi;
                                }
                            }
                            if (abi != null && abi.Bundle != null)
                            {
                                found = true;
                            }
                        }
                        if (found)
                        {
                            var deps = ResManifest.GetAllDependencies(adm.BundleName);
                            foreach (var dep in deps)
                            {
                                if (string.IsNullOrEmpty(dep))
                                {
                                    if (GLog.IsLogErrorEnabled) GLog.LogError(adm.BundleName + " has empty dep.");
                                    continue;
                                }
                                if (dep.StartsWith("fonts"))
                                {
                                    continue;
                                }
                                AssetBundleInfo dabi;
                                if (!loaded.TryGetValue(dep, out dabi))
                                {
                                    dabi = LoadAssetBundleAndAddRef(dep);
                                    if (dabi == null)
                                    {
                                        if (GLog.IsLogErrorEnabled) GLog.LogError("Unable to load assetbundle: " + dep);
                                    }
                                    else
                                    {
                                        if (ad.Permanent)
                                        {
                                            dabi.Permanent = true;
                                        }
                                        loaded[dep] = dabi;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (ad is AssetDesc_PackedTex || ad is AssetDesc_NewPackedTex || ad.Name == "?")
                {
                    var bname = EncodeBundleName(ad.DepBundles[0]).Replace('/', '_').ToLower() + ".ab";
                    {
                        var dep = bname;
                        if (!loaded.TryGetValue(dep, out abi))
                        {
                            abi = LoadAssetBundleAndAddRef(dep);
                            if (abi == null)
                            {
                                if (GLog.IsLogErrorEnabled) GLog.LogError("Unable to load assetbundle: " + dep);
                            }
                            else
                            {
                                if (ad.Permanent)
                                {
                                    abi.Permanent = true;
                                }
                                loaded[dep] = abi;
                            }
                        }
                        if (abi != null && abi.Bundle != null)
                        {
                            found = true;
                        }
                    }
                    if (found)
                    {
                        var deps = ResManifest.GetAllDependencies(bname);
                        foreach (var dep in deps)
                        {
                            if (dep.StartsWith("fonts"))
                            {
                                continue;
                            }
                            AssetBundleInfo dabi;
                            if (!loaded.TryGetValue(dep, out dabi))
                            {
                                dabi = LoadAssetBundleAndAddRef(dep);
                                if (dabi == null)
                                {
                                    if (GLog.IsLogErrorEnabled) GLog.LogError("Unable to load assetbundle: " + dep);
                                }
                                else
                                {
                                    if (ad.Permanent)
                                    {
                                        dabi.Permanent = true;
                                    }
                                    loaded[dep] = dabi;
                                }
                            }
                        }
                    }
                }
                else
#endif
                {
                    if (ad.DepBundles != null)
                    {
                        var deps = ad.DepBundles;
                        foreach (var dep in deps)
                        {
                            found = false;
                            if (!loaded.TryGetValue(dep, out abi))
                            {
                                abi = LoadAssetBundleAndAddRef(dep);
                                if (abi == null)
                                {
                                    if (GLog.IsLogErrorEnabled) GLog.LogError("Unable to load assetbundle: " + dep);
                                }
                                else
                                {
                                    if (ad.Permanent)
                                    {
                                        abi.Permanent = true;
                                    }
                                    loaded[dep] = abi;
                                }
                            }
                            if (abi != null && abi.Bundle != null)
                            {
                                found = true;
                            }
                        }
                    }
                }
                if (found)
                {
                    if (funcCheck != null)
                    {
                        found = funcCheck(abi, ad, loaded);
                    }
                }
                if (found)
                {
                    return abi;
                }
            }

            foreach (var abid in loaded)
            {
                abid.Value.Release();
            }
            return null;
        }

        public static UnityEngine.Object LoadResDistribute(string name, Type type)
        {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
#if !LOAD_TEX_FROM_PACK
            // the shader keyword
            if (!IsEditorRunning && UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                IsEditorRunning = true;
                Shader.EnableKeyword("NOT_USE_CLIENT_RES_MANAGER");
                Shader.DisableKeyword("USE_CLIENT_RES_MANAGER");

                //EditorBridge.Init();
                Action onPlayModeChanged = null;
                onPlayModeChanged = () =>
                {
                    if (!Application.isPlaying)
                    {
                        Shader.EnableKeyword("USE_CLIENT_RES_MANAGER");
                        Shader.DisableKeyword("NOT_USE_CLIENT_RES_MANAGER");
                        EditorBridge.OnPlayModeChanged -= onPlayModeChanged;
                        IsEditorRunning = false;
                    }
                };
                EditorBridge.OnPlayModeChanged += onPlayModeChanged;
            }

#endif
#if LOAD_TEX_FROM_PACK
            if (name.StartsWith("Assets/UITextures/") && type == null)
            {
                {
                    Sprite sprite;
                    Texture2D alphaTexture;
                    UnityEngine.UI.CapstoneImage.LoadSpriteAndAlphaTexture(name, out sprite, out alphaTexture);

                    if (sprite != null)
                    {
                        return sprite;
                    }
                }

                var predir = "Assets/" + name.Substring("Assets/UITextures/".Length);
                predir = System.IO.Path.GetDirectoryName(predir);
                var candi = System.IO.Directory.GetFiles(predir, "*.jpg", System.IO.SearchOption.AllDirectories);
                foreach (var candipath in candi)
                {
                    var cpath = candipath.Replace('\\', '/');
                    var candiam = UnityEditor.AssetDatabase.LoadMainAssetAtPath(cpath);
                    if (candiam is Texture2D)
                    {
                        // packed?
                        var guid = UnityEditor.AssetDatabase.AssetPathToGUID(name);
                        var candias = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(cpath);
                        string texname = null;
                        Sprite candis = null;
                        foreach (var candia in candias)
                        {
                            if (candia is Sprite)
                            {
                                var sprite = candia as Sprite;
                                if (sprite.name.EndsWith("?" + guid))
                                {
                                    candis = sprite;
                                    texname = sprite.name.Substring(0, sprite.name.Length - 1 - guid.Length);
                                    break;
                                }
                            }
                        }

                        if (texname != null)
                        {
                            foreach (var candia in candias)
                            {
                                if (candia is Sprite)
                                {
                                    var sprite = candia as Sprite;
                                    if (sprite.name.StartsWith(System.IO.Path.GetFileNameWithoutExtension(name) + "*" + texname))
                                    {
                                        return sprite;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
#endif
            if (type == null)
            {
                var rv = LoadMainAsset(name);
                return rv;
            }
            else
            {
                var rv = UnityEditor.AssetDatabase.LoadAssetAtPath(name, type);
                return rv;
            }
#else
            //如果缓存存在则直接获取缓存中的数据进行返回
            AssetInfo ai = null;
            if (LoadedAssets.TryGetValue(name, out ai))
            {
                if (ai != null && ai.CheckAlive())
                {
                    if (type == null)
                    {
                        var existing = ai.Asset.GetWeakReference<UnityEngine.Object>();
                        if (existing != null) return existing;
                    }
                    else
                    {
                        AssetInfo subai = null;
                        if (ai.SubAssets != null && ai.SubAssets.TryGetValue(type, out subai) && subai != null && subai.CheckAlive())
                        {
                            var existing = subai.Asset.GetWeakReference<UnityEngine.Object>();
                            if (existing != null) return existing;
                        }
                    }
                }
            }
            ///如果缓存中不存在或是缓存已经被释放了
            var oldai = ai;
            ai = null;
            LoadAssetBundleFor(name, (abi, ad, loaded) =>
            {
                if (oldai == null)
                {
                    ai = new AssetInfo();
                    LoadedAssets[name] = ai;
                    ai.DepBundles = new HashSet<AssetBundleInfo>(loaded.Values);
                    ai.Desc = ad;
                    ai.ContainingBundle = abi;
                }
                else
                {
                    ai = oldai;
                    ai.DepBundles = new HashSet<AssetBundleInfo>(loaded.Values);
                    ai.ContainingBundle = abi;
                }
                return true;
            });
            return GetAssetAndAddRef(ai, type);
#endif
        }
        #region 获取asset 接口
        /// <summary>
        /// 获取asset 并添加ref
        /// </summary>
        /// <returns></returns>
        private static UnityEngine.Object GetAssetAndAddRef(AssetInfo ai, Type type)
        {
            if (ai == null || ai.Desc == null || ai.ContainingBundle == null || ai.ContainingBundle.Bundle == null) return null;
            if (ai.Desc is AssetDesc_FromManifest) return GetAssetAndAddRefBy_FromManifest(ai, type);
            if (ai.Desc is AssetDesc_PackedTex) return GetAssetAndAddRefBy_PackedTex(ai, type);
            if (ai.Desc is AssetDesc_NewPackedTex) return GetAssetAndAddRefBy_NewPackedTex(ai, type);
            if (ai.Desc is AssetDesc_DynTex) return GetAssetAndAddRefBy_DynTex(ai, type);
            if (ai.Desc is AssetDesc_DynSprite) return GetAssetAndAddRefBy_DynSprite(ai, type);
            return GetAssetAndAddRefBy_Other(ai, type);
        }

        private static UnityEngine.Object GetAssetAndAddRefBy_FromManifest(AssetInfo ai, Type type)
        {
            if (ai == null || ai.Desc == null || ai.ContainingBundle == null || ai.ContainingBundle.Bundle == null) return null;
            var ad = ai.Desc as AssetDesc_FromManifest;
            if (ad == null) return null;
            UnityEngine.Object rv;
            if (type == null)
            {
                rv = ai.ContainingBundle.Bundle.LoadAsset(ad.Name);
                if (rv == null) return null;
                if (rv is Texture2D)
                {
                    var rvsp = ai.ContainingBundle.Bundle.LoadAsset<Sprite>(ad.Name);
                    if (rvsp) rv = rvsp;
                }
                ai.Asset = new WeakReference(rv);
                ai.AssetLiveRefCnt = 1;
                return rv;
            }
            rv = ai.ContainingBundle.Bundle.LoadAsset(ad.Name, type);
            if (rv == null) return null;
            if (ai.SubAssets == null) ai.SubAssets = new Dictionary<Type, AssetInfo>();
            var subai = new AssetInfo();
            subai.Asset = new WeakReference(rv);
            subai.AssetLiveRefCnt = 1;
            ai.SubAssets[type] = subai;
            return rv;
        }

        private static UnityEngine.Object GetAssetAndAddRefBy_PackedTex(AssetInfo ai, Type type)
        {
            if (ai == null || ai.Desc == null || ai.ContainingBundle == null || ai.ContainingBundle.Bundle == null) return null;
            var ad = ai.Desc as AssetDesc_PackedTex;
            if (ad == null) return null;
            var namepre = ad.Name;
            if (ad.Types != null && ad.Types.Length > 0) namepre = ad.Types[0] + "*" + namepre;
            var assets = ai.ContainingBundle.Bundle.LoadAssetWithSubAssets<Sprite>(ad.DepBundles[0].ToLower());
            if (assets == null) return null;
            Sprite rv = null;
            for (int i = 0; i < assets.Length; ++i)
            {
                rv = assets[i];
                if (rv == null || !rv.name.StartsWith(namepre)) continue;
                ai.Asset = new WeakReference(rv);
                ai.AssetLiveRefCnt = 1;
                return rv;
            }
            return null;
        }

        private static UnityEngine.Object GetAssetAndAddRefBy_NewPackedTex(AssetInfo ai, Type type)
        {
            if (ai == null || ai.Desc == null || ai.ContainingBundle == null || ai.ContainingBundle.Bundle == null) return null;
            var ad = ai.Desc as AssetDesc_NewPackedTex;
            if (ad == null) return null;
            var namepre = ad.Name;
            var assets = ai.ContainingBundle.Bundle.LoadAssetWithSubAssets<Sprite>(ad.DepBundles[0].ToLower());
            if (assets == null) return null;
            Sprite rv = null;
            for (int i = 0; i < assets.Length; ++i)
            {
                rv = assets[i];
                if (rv == null || !rv.name.StartsWith(namepre)) continue;
                ai.Asset = new WeakReference(rv);
                ai.AssetLiveRefCnt = 1;
                return rv;
            }
            return null;
        }

        private static UnityEngine.Object GetAssetAndAddRefBy_DynTex(AssetInfo ai, Type type)
        {
            if (ai == null || ai.Desc == null || ai.ContainingBundle == null || ai.ContainingBundle.Bundle == null) return null;
            AssetInfo subai = null;
            Sprite rv = null;
            Texture2D tex = null;
            var desc = ai.Desc as AssetDesc_DynTex;
            if (desc == null) return null;
            if (ai.SubAssets == null)
            {
                ai.SubAssets = new Dictionary<Type, AssetInfo>();
            }
            else
            {
                if (ai.SubAssets.TryGetValue(typeof(Texture2D), out subai) && subai != null && subai.CheckAlive())
                {
                    tex = subai.Asset.GetWeakReference<Texture2D>();
                    if (tex != null)
                    {
                        if (type == typeof(Texture2D)) return tex;
                        rv = Sprite.Create(tex, new Rect(0, 0, desc.Width, desc.Height), new Vector2(0.5f, 0.5f));
                        rv.name = tex.name;
                        ai.Asset = new WeakReference(rv);
                        ai.AssetLiveRefCnt = 1;
                        return rv;
                    }
                }
            }
            TextAsset raw;
            if (desc.Name == "?")
            {
                raw = ai.ContainingBundle.Bundle.LoadAsset<UnityEngine.TextAsset>(desc.DepBundles[0].ToLower());
            }
            else
            {
                raw = ai.ContainingBundle.Bundle.LoadAsset<UnityEngine.TextAsset>(ai.Desc.Name);
            }
            if (raw == null) return null;
            var texta = raw as TextAsset;
            if (texta == null) return null;
            tex = LoadTexFromBytes(texta);
            Resources.UnloadAsset(texta);
            if (tex == null) return null;
            subai = new AssetInfo();
            subai.Asset = new WeakReference(tex);
            subai.AssetLiveRefCnt = 1;
            ai.SubAssets[typeof(Texture2D)] = subai;
            if (type == typeof(Texture2D)) return tex;
            rv = Sprite.Create(tex, new Rect(0, 0, desc.Width, desc.Height), new Vector2(0.5f, 0.5f));
            rv.name = tex.name;
            ai.Asset = new WeakReference(rv);
            ai.AssetLiveRefCnt = 1;
            return rv;
        }

        private static UnityEngine.Object GetAssetAndAddRefBy_DynSprite(AssetInfo ai, Type type)
        {
            if (ai == null || ai.Desc == null || ai.ContainingBundle == null || ai.ContainingBundle.Bundle == null) return null;
            BytesImagePartInfo raw;
            if (ai.Desc.Name == "?")
            {
                raw = ai.ContainingBundle.Bundle.LoadAsset<BytesImagePartInfo>(ai.Desc.DepBundles[0].ToLower());
            }
            else
            {
                raw = ai.ContainingBundle.Bundle.LoadAsset<BytesImagePartInfo>(ai.Desc.Name);
            }
            if (raw == null) return null;
            var info = raw as BytesImagePartInfo;
            if (info == null) return null;
            if (info.SpriteInfos != null && info.SpriteInfos.Count == 1 && info.SpriteInfos[0].SpriteName == info.FileName)
            {
                var rv = info.SpriteInfos[0].CreateSprite();
                if (rv == null) return null;
                ai.Asset = new WeakReference(rv);
                ai.AssetLiveRefCnt = 1;
                return rv;
            }
            else if (info.MainSprite != null)
            {
                var rv = info.MainSprite.CreateSprite();
                if (rv == null) return null;
                ai.Asset = new WeakReference(rv);
                ai.AssetLiveRefCnt = 1;
                return rv;
            }
            return null;
        }

        private static UnityEngine.Object GetAssetAndAddRefBy_Other(AssetInfo ai, Type type)
        {
            if (ai == null || ai.Desc == null || ai.ContainingBundle == null || ai.ContainingBundle.Bundle == null) return null;
            if (type == null)
            {
                var rv = ai.ContainingBundle.Bundle.LoadAsset(ai.Desc.Name);
                if (rv == null) return null;
                ai.Asset = new WeakReference(rv);
                ai.AssetLiveRefCnt = 1;
                return rv;
            }
            var ad = ai.Desc;
            if (ad == null || ad.Types == null) return null;
            for (int i = 0; i < ad.Types.Length; ++i)
            {
                if (ad.Types[i] != type.Name) continue;
                var aname = ad.Name;
                if (i > 0) aname += "\uEE2A" + i.ToString();
                var rv = ai.ContainingBundle.Bundle.LoadAsset(aname);
                if (rv == null) continue;
                if (ai.SubAssets == null) ai.SubAssets = new Dictionary<Type, AssetInfo>();
                var subai = new AssetInfo();
                subai.Asset = new WeakReference(rv);
                subai.AssetLiveRefCnt = 1;
                ai.SubAssets[type] = subai;
                return rv;
            }
            return null;
        }
        #endregion

        public static UnityEngine.Object LoadRes(string name, Type type)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            if (name[0] == '?')
            {
                if (name.StartsWith("?Resources:"))
                {
                    var realname = name.Substring("?Resources:".Length);
                    return LoadFromResource(realname, type);
                }
                return null;
            }
            UnityEngine.Object rv;
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            rv = LoadResDistribute(GetDistributeAssetName(name), type);
#else
            rv = LoadResDistribute(name, type);
#endif
            return rv;
        }

        public static UnityEngine.Object LoadFromResource(string name, Type type)
        {
            if (!string.IsNullOrEmpty(name))
            {
                string[] distributeFlags = GetDistributeFlags();
                if (distributeFlags != null)
                {
                    for (int i = distributeFlags.Length - 1; i >= 0; --i)
                    {
                        var flag = distributeFlags[i];
                        UnityEngine.Object loaded;
                        if (type == null)
                        {
                            loaded = Resources.Load("distribute/" + flag + "/" + name.Replace('\\', '/'));
                        }
                        else
                        {
                            loaded = Resources.Load("distribute/" + flag + "/" + name.Replace('\\', '/'), type);
                        }
                        if (loaded)
                        {
                            return loaded;
                        }
                    }
                }
                if (type == null)
                {
                    return Resources.Load(name);
                }
                else
                {
                    return Resources.Load(name, type);
                }
            }
            return null;
        }

        [XLua.LuaCallCSharp]
        public class LoadResAsyncRequest
        {
            private UnityEngine.Object _loadedObj = null;
            private bool _loaded = false;
            private AssetInfo _info = null;
            private Func<UnityEngine.Object, UnityEngine.Object> _funcConvert = null;
            public AsyncOperation asyncOp { get; private set; }

            public UnityEngine.Object asset
            {
                get
                {
                    SaveLoadedAsset();
                    return _loadedObj;
                }
            }

            public bool isDone
            {
                get
                {
                    SaveLoadedAsset();
                    return asyncOp == null || asyncOp.isDone;
                }
            }

            private void SaveLoadedAsset()
            {

            }

            public LoadResAsyncRequest(AsyncOperation request, AssetInfo info)
            {
                asyncOp = request;
                _info = info;
            }
            public LoadResAsyncRequest(AsyncOperation request, AssetInfo info, Func<UnityEngine.Object, UnityEngine.Object> funcConvert)
            {
                asyncOp = request;
                _info = info;
                _funcConvert = funcConvert;
            }
            public LoadResAsyncRequest(UnityEngine.Object obj)
            {
                _loadedObj = obj;
            }
        }

        public static LoadResAsyncRequest LoadResDistributeAsync(string name, Type type)
        {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            var rv = LoadResDistribute(name, type);
            return new LoadResAsyncRequest(rv);
#else
            AssetInfo ai = null;
            if (LoadedAssets.TryGetValue(name, out ai))
            {
                if (ai != null)
                {
                    if (ai.CheckAlive())
                    {
                        if (type == null)
                        {
                            var req = ai.Asset.GetWeakReference<LoadResAsyncRequest>();
                            if (req != null)
                            {
                                return req;
                            }

                            var existing = ai.Asset.GetWeakReference<UnityEngine.Object>();
                            if (existing != null)
                            {
                                return new LoadResAsyncRequest(existing);
                            }
                        }
                        else
                        {
                            if (ai.SubAssets != null)
                            {
                                AssetInfo subai = null;
                                if (ai.SubAssets.TryGetValue(type, out subai))
                                {
                                    if (subai != null)
                                    {
                                        if (subai.CheckAlive())
                                        {
                                            var req = subai.Asset.GetWeakReference<LoadResAsyncRequest>();
                                            if (req != null)
                                            {
                                                return req;
                                            }

                                            var existing = subai.Asset.GetWeakReference<UnityEngine.Object>();
                                            if (existing != null)
                                            {
                                                return new LoadResAsyncRequest(existing);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var oldai = ai;
            ai = null;
            LoadAssetBundleFor(name, (abi, ad, loaded) =>
            {
                if (oldai == null)
                {
                    ai = new AssetInfo();
                    LoadedAssets[name] = ai;
                    ai.DepBundles = new HashSet<AssetBundleInfo>(loaded.Values);
                    ai.Desc = ad;
                    ai.ContainingBundle = abi;
                }
                else
                {
                    ai = oldai;
                }
                return true;
            });
            if (ai != null)
            {
                if (ai.ContainingBundle != null && ai.ContainingBundle.Bundle != null && ai.Desc != null)
                {
                    if (ai.Desc is AssetDesc_FromManifest)
                    {
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
                        var ad = ai.Desc as AssetDesc_FromManifest;
                        if (type == null)
                        {
                            var rv = ai.ContainingBundle.Bundle.LoadAssetAsync(ad.Name);
                            if (rv != null)
                            {
                                var req = new LoadResAsyncRequest(rv, ai, raw =>
                                {
                                    if (raw is Texture2D)
                                    {
                                        var rvsp = ai.ContainingBundle.Bundle.LoadAsset<Sprite>(ad.Name);
                                        if (rvsp)
                                        {
                                            return rvsp;
                                        }
                                    }
                                    return raw;
                                });
                                ai.Asset = new WeakReference(req);
                                ai.AssetLiveRefCnt = 1;
                                return req;
                            }
                        }
                        else
                        {
                            var rv = ai.ContainingBundle.Bundle.LoadAssetAsync(ad.Name, type);
                            if (rv != null)
                            {
                                if (ai.SubAssets == null)
                                {
                                    ai.SubAssets = new Dictionary<Type, AssetInfo>();
                                }
                                var subai = new AssetInfo();
                                var req = new LoadResAsyncRequest(rv, subai, raw =>
                                {
                                    return raw;
                                });
                                subai.Asset = new WeakReference(req);
                                subai.AssetLiveRefCnt = 1;
                                ai.SubAssets[type] = subai;
                                return req;
                            }
                        }
#endif
                    }
                    else if (ai.Desc is AssetDesc_PackedTex)
                    {
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
                        var ad = ai.Desc as AssetDesc_PackedTex;
                        var namepre = ad.Name;
                        if (ad.Types != null && ad.Types.Length > 0)
                        {
                            namepre = ad.Types[0] + "*" + namepre;
                        }
                        var loadwork = ai.ContainingBundle.Bundle.LoadAssetWithSubAssetsAsync<Sprite>(ad.DepBundles[0].ToLower());
                        if (loadwork != null)
                        {
                            var req = new LoadResAsyncRequest(loadwork, ai, raw =>
                            {
                                var assets = loadwork.allAssets;
                                for (int i = 0; i < assets.Length; ++i)
                                {
                                    var rv = assets[i];
                                    if (rv.name.StartsWith(namepre))
                                    {
                                        return rv;
                                    }
                                }
                                return null;
                            });
                            ai.Asset = new WeakReference(req);
                            ai.AssetLiveRefCnt = 1;
                            return req;
                        }
#endif
                    }
                    else if (ai.Desc is AssetDesc_NewPackedTex)
                    {
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
                        var ad = ai.Desc as AssetDesc_NewPackedTex;
                        var namepre = ad.Name;
                        var loadwork = ai.ContainingBundle.Bundle.LoadAssetWithSubAssetsAsync<Sprite>(ad.DepBundles[0].ToLower());
                        if (loadwork != null)
                        {
                            var req = new LoadResAsyncRequest(loadwork, ai, raw =>
                            {
                                var assets = loadwork.allAssets;
                                for (int i = 0; i < assets.Length; ++i)
                                {
                                    var rv = assets[i];
                                    if (rv.name.StartsWith(namepre))
                                    {
                                        return rv;
                                    }
                                }
                                return null;
                            });
                            ai.Asset = new WeakReference(req);
                            ai.AssetLiveRefCnt = 1;
                            return req;
                        }
#endif
                    }
                    else if (ai.Desc is AssetDesc_DynTex)
                    {
                        var desc = ai.Desc as AssetDesc_DynTex;
                        if (ai.SubAssets == null)
                        {
                            ai.SubAssets = new Dictionary<Type, AssetInfo>();
                        }
                        else
                        {
                            AssetInfo subai = null;
                            if (ai.SubAssets.TryGetValue(typeof(Texture2D), out subai))
                            {
                                if (subai != null)
                                {
                                    if (subai.CheckAlive())
                                    {
                                        var tex = subai.Asset.GetWeakReference<Texture2D>();
                                        if (tex != null)
                                        {
                                            if (type == typeof(Texture2D))
                                            {
                                                return new LoadResAsyncRequest(tex);
                                            }
                                            else
                                            {
                                                var rv = Sprite.Create(tex, new Rect(0, 0, desc.Width, desc.Height), new Vector2(0.5f, 0.5f));
                                                rv.name = tex.name;
                                                ai.Asset = new WeakReference(rv);
                                                ai.AssetLiveRefCnt = 1;
                                                return new LoadResAsyncRequest(rv);
                                            }
                                        }

                                        var req = subai.Asset.GetWeakReference<LoadResAsyncRequest>();
                                        if (req != null)
                                        {
                                            if (type == typeof(Texture2D))
                                            {
                                                return req;
                                            }
                                            else
                                            {
                                                var rv = new LoadResAsyncRequest(req.asyncOp, ai, loaded =>
                                                {
                                                    var tex2d = req.asset as Texture2D;
                                                    if (tex2d != null)
                                                    {
                                                        var sprite = Sprite.Create(tex2d, new Rect(0, 0, desc.Width, desc.Height), new Vector2(0.5f, 0.5f));
                                                        sprite.name = tex2d.name;
                                                        return sprite;
                                                    }
                                                    return null;
                                                });
                                                ai.Asset = new WeakReference(rv);
                                                ai.AssetLiveRefCnt = 1;
                                                return rv;
                                            }
                                        }
                                    }
                                }
                            }
                        }
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
                        AssetBundleRequest aop;
                        if (ai.Desc.Name == "?")
                        {
                            aop = ai.ContainingBundle.Bundle.LoadAssetAsync(ai.Desc.DepBundles[0].ToLower(), typeof(UnityEngine.TextAsset));
                        }
                        else
                        {
                            aop = ai.ContainingBundle.Bundle.LoadAssetAsync(ai.Desc.Name, typeof(UnityEngine.TextAsset));
                        }
#else
                        var aop = ai.ContainingBundle.Bundle.LoadAsync(ai.Desc.Name, typeof(UnityEngine.TextAsset));
#endif
                        if (aop != null)
                        {
                            var subai = new AssetInfo();
                            var req = new LoadResAsyncRequest(aop, subai, raw =>
                            {
                                var texta = raw as TextAsset;
                                if (texta != null)
                                {
                                    var tex = LoadTexFromBytes(texta);
                                    Resources.UnloadAsset(texta);
                                    if (tex != null)
                                    {
                                        return tex;
                                    }
                                }
                                return null;
                            });
                            subai.Asset = new WeakReference(req);
                            subai.AssetLiveRefCnt = 1;
                            ai.SubAssets[typeof(Texture2D)] = subai;

                            if (type == typeof(Texture2D))
                            {
                                return req;
                            }
                            else
                            {
                                var rv = new LoadResAsyncRequest(req.asyncOp, ai, loaded =>
                                {
                                    var tex2d = req.asset as Texture2D;
                                    if (tex2d != null)
                                    {
                                        var sprite = Sprite.Create(tex2d, new Rect(0, 0, desc.Width, desc.Height), new Vector2(0.5f, 0.5f));
                                        sprite.name = tex2d.name;
                                        return sprite;
                                    }
                                    return null;
                                });
                                ai.Asset = new WeakReference(rv);
                                ai.AssetLiveRefCnt = 1;
                                return rv;
                            }
                        }
                    }
                    else if (ai.Desc is AssetDesc_DynSprite)
                    {
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
                        AssetBundleRequest aop;
                        if (ai.Desc.Name == "?")
                        {
                            aop = ai.ContainingBundle.Bundle.LoadAssetAsync(ai.Desc.DepBundles[0].ToLower(), typeof(BytesImagePartInfo));
                        }
                        else
                        {
                            aop = ai.ContainingBundle.Bundle.LoadAssetAsync(ai.Desc.Name, typeof(BytesImagePartInfo));
                        }
#else
                        var aop = ai.ContainingBundle.Bundle.LoadAsync(ai.Desc.Name, typeof(BytesImagePartInfo));
#endif
                        if (aop != null)
                        {
                            var req = new LoadResAsyncRequest(aop, ai, raw =>
                            {
                                var info = raw as BytesImagePartInfo;
                                if (info != null)
                                {
                                    if (info.SpriteInfos != null && info.SpriteInfos.Count == 1 && info.SpriteInfos[0].SpriteName == info.FileName)
                                    {
                                        var rv = info.SpriteInfos[0].CreateSprite();
                                        if (rv != null)
                                        {
                                            ai.Asset = new WeakReference(rv);
                                            ai.AssetLiveRefCnt = 1;
                                            return rv;
                                        }
                                    }
                                    else if (info.MainSprite != null)
                                    {
                                        var rv = info.MainSprite.CreateSprite();
                                        if (rv != null)
                                        {
                                            ai.Asset = new WeakReference(rv);
                                            ai.AssetLiveRefCnt = 1;
                                            return rv;
                                        }
                                    }
                                }
                                return null;
                            });
                            ai.Asset = new WeakReference(req);
                            ai.AssetLiveRefCnt = 1;
                            return req;
                        }
                    }
                    else
                    {
                        if (type == null)
                        {
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
                            var rv = ai.ContainingBundle.Bundle.LoadAssetAsync(ai.Desc.Name, typeof(UnityEngine.Object));
#else
                            var rv = ai.ContainingBundle.Bundle.LoadAsync(ai.Desc.Name, typeof(UnityEngine.Object));
#endif
                            if (rv != null)
                            {
                                var req = new LoadResAsyncRequest(rv, ai, raw =>
                                {
                                    return raw;
                                });
                                ai.Asset = new WeakReference(req);
                                ai.AssetLiveRefCnt = 1;
                                return req;
                            }
                        }
                        else
                        {
                            var ad = ai.Desc;
                            if (ad.Types != null)
                            {
                                for (int i = 0; i < ad.Types.Length; ++i)
                                {
                                    if (ad.Types[i] == type.Name)
                                    {
                                        var aname = ad.Name;
                                        if (i > 0)
                                        {
                                            aname += "\uEE2A" + i.ToString();
                                        }
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
                                        var rv = ai.ContainingBundle.Bundle.LoadAssetAsync(aname, type);
#else
                                        var rv = ai.ContainingBundle.Bundle.LoadAsync(aname, type);
#endif
                                        if (rv != null)
                                        {
                                            if (ai.SubAssets == null)
                                            {
                                                ai.SubAssets = new Dictionary<Type, AssetInfo>();
                                            }
                                            var subai = new AssetInfo();
                                            var req = new LoadResAsyncRequest(rv, subai, raw =>
                                            {
                                                return raw;
                                            });
                                            subai.Asset = new WeakReference(req);
                                            subai.AssetLiveRefCnt = 1;
                                            ai.SubAssets[type] = subai;
                                            return req;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
#endif
        }

        public static LoadResAsyncRequest LoadResAsync(string name, Type type)
        {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            return LoadResDistributeAsync(GetDistributeAssetName(name), type);
#else
            return LoadResDistributeAsync(name, type);
#endif
        }

        public static Texture2D LoadTexFromBytes(TextAsset textAsset)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            tex.LoadImage(textAsset.bytes);
            tex.name = textAsset.name;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply(false, true);
            return tex;
        }

        public static string EncodeBundleName(string path)
        {
            if (string.IsNullOrEmpty(path)) return "Common";
            path = path.Replace('\\', '/');
            path = path.TrimEnd('/');
            if (path.ToLower().EndsWith(".unity"))
            {
                if (path.StartsWith("Assets/CapstonesRes/"))
                {
                    return "s/res/" + path.Substring("Assets/CapstonesRes/".Length, path.Length - ".unity".Length - "Assets/CapstonesRes/".Length);
                }
                else if (path.StartsWith("Assets/"))
                {
                    return "s/a/" + path.Substring("Assets/".Length, path.Length - ".unity".Length - "Assets/".Length);
                }
                else
                {
                    return "s/n/" + path.Substring(0, path.Length - ".unity".Length);
                }
            }
            else
            {
                if (path.StartsWith("Assets/CapstonesRes/"))
                {
                    path = path.Substring("Assets/CapstonesRes/".Length);
                    var dindex = path.LastIndexOf('/');
                    if (dindex >= 0)
                    {
                        path = path.Substring(0, dindex);
                    }
                    if (string.IsNullOrEmpty(path)) return "Common";
                    return "res/" + path;
                }
                return "Common";
            }
        }

        public static AssetBundleInfo LoadAssetBundle(string name)
        {
            AssetBundleInfo abi = null;
            if (LoadedAssetBundles.TryGetValue(name, out abi))
            {
                if (abi != null && abi.Bundle != null)
                {
                    return abi;
                }
            }
            abi = null;

            AssetBundle bundle = null;
            string realname, suffix;
            if (name.StartsWith("?"))
            {
                realname = name.Substring(1);
                suffix = "";
            }
            else
            {
                realname = name.Replace('/', '_');
                if (realname.EndsWith(".ab"))
                {
                    suffix = "";
                }
                else
                {
                    suffix = ".ab";
                }
            }
            string path = ResManager.UpdatePath + "/res/" + realname + suffix;
            int loadfrom = 0;
            if (Application.streamingAssetsPath.Contains("://"))
            {
                if (Application.platform == RuntimePlatform.Android && _LoadAssetsFromApk)
                {
                    if (IsNonExistingFiles(path) || !Capstones.PlatExt.PlatDependant.IsFileExist(path))
                    {
                        RecordNonExistingFiles(path);
                        var realpath = "res/" + realname + suffix; // TODO: To Deng: realPath
                        var obbtag = "obb:" + realpath;
                        if (_LoadAssetsFromObb && !IsNonExistingFiles(obbtag) && ObbEntryType(realpath) == 2)
                        {
                            path = realpath; // TODO: To Deng: realPath
                            loadfrom = 2;
                        }
                        else
                        {
                            RecordNonExistingFiles(obbtag);
                            path = Application.dataPath + "!assets/res/" + realname + suffix;
                            if (IsNonExistingFiles(path))
                            {
                                loadfrom = 3;
                            }
                            else
                            {
                                loadfrom = 1;
                            }
                        }
                    }
                }
            }
            else
            {
                if (IsNonExistingFiles(path) || !Capstones.PlatExt.PlatDependant.IsFileExist(path))
                {
                    RecordNonExistingFiles(path);
                    path = Application.streamingAssetsPath + "/res/" + realname + suffix;
                }
            }
            try
            {
                if (loadfrom != 0 || !IsNonExistingFiles(path) && Capstones.PlatExt.PlatDependant.IsFileExist(path))
                {
                    if (loadfrom == 0 || loadfrom == 1)
                    {
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
                        bundle = AssetBundle.LoadFromFile(path);
#else
                        bundle = AssetBundle.CreateFromFile(path);
#endif
                        if (loadfrom == 1 && bundle == null)
                        {
                            RecordNonExistingFiles(path);
                        }
                    }
                    else if (loadfrom == 2)
                    {
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
                        int retryTimes = 10;
                        long offset = -1;
                        for (int i = 0; i < retryTimes; ++i)
                        {
                            Exception error = null;
                            do
                            {
                                ZipArchive za = ObbZipArchive;
                                if (za == null)
                                {
                                    error = new Exception("Apk Archive Cannot be read.");
                                    break;
                                }
                                try
                                {
                                    var entry = za.GetEntry(path);
                                    using (var srcstream = entry.Open())
                                    {
                                        offset = ObbFileStream.Position;
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
                                    if (GLog.IsLogErrorEnabled) GLog.LogException(error);
                                    //throw error; // what should I do?
                                }
                                else
                                {
                                    if (GLog.IsLogErrorEnabled) GLog.LogException(error + "\nNeed Retry " + i);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (offset >= 0)
                        {
                            bundle = AssetBundle.LoadFromFile(ObbPath, 0, (ulong)offset);
                        }
#endif
                    }
                }
                else
                {
                    RecordNonExistingFiles(path);
                }
            }
            catch { }
            if (bundle != null)
            {
                abi = new AssetBundleInfo(bundle);
                LoadedAssetBundles[name] = abi;
            }
            return abi;
        }

        public static AssetBundleInfo LoadAssetBundleAndAddRef(string name)
        {
            var rv = LoadAssetBundle(name);
            if (rv != null && rv.Bundle != null)
            {
                rv.AddRef();
            }
            return rv;
        }

        private static AssetInfo PreloadResDistribute(string name)
        {
            AssetInfo ai = null;
            LoadedAssets.TryGetValue(name, out ai);

            if (ai == null)
            {
                LoadAssetBundleFor(name, (abi, ad, loaded) =>
                {
                    ai = new AssetInfo();
                    LoadedAssets[name] = ai;
                    ai.DepBundles = new HashSet<AssetBundleInfo>(loaded.Values);
                    ai.ContainingBundle = abi;
                    ai.Desc = ad;
                    ai.RefCnt = 1;
                    return true;
                });
            }
            else
            {
                ai.AddRef();
            }
            return ai;
        }

        public static AssetInfo PreloadRes(string name)
        {
#if !UNITY_EDITOR || USE_CLIENT_RES_MANAGER
            return PreloadResDistribute(name);
#else
            return null;
#endif
        }

#if !UNITY_EDITOR || USE_CLIENT_RES_MANAGER
        private static LinkedList<AssetInfo> RunningScene = new LinkedList<AssetInfo>();
        public static void UnloadCurrentScene(bool isAtEndOfFrame)
        {
            if (isAtEndOfFrame)
            {
                UnloadSceneList(RunningScene);
            }
            else
            {
                LinkedList<AssetInfo> copy = new LinkedList<AssetInfo>(RunningScene);
                UnloadSceneListWork(copy).StartCoroutine();
            }
            RunningScene.Clear();
        }
        private static void UnloadSceneList(LinkedList<AssetInfo> list)
        {
            foreach (var scene in list)
            {
                if (scene != null)
                {
                    scene.Release();
                }
            }
        }
        private static IEnumerator UnloadSceneListWork(LinkedList<AssetInfo> list)
        {
            yield return new WaitForEndOfFrame();
            UnloadSceneList(list);
        }

        public static void UnloadCurrentScene()
        {
            UnloadCurrentScene(false);
        }

        private static bool LoadSceneDistribute(string path, bool additive)
        {
            return LoadSceneDistribute(path, additive, false);
        }

        private static bool LoadSceneDistribute(string path, bool additive, bool isAtEndOfFrame)
        {
            AssetInfo ai = PreloadResDistribute(path);

            if (ai != null)
            {
                string levelName = System.IO.Path.GetFileNameWithoutExtension(path);

                if (additive)
                {
                    Application.LoadLevelAdditive(levelName);
                }
                else
                {
                    Application.LoadLevel(levelName);
                    UnloadCurrentScene(isAtEndOfFrame);
                }
                RunningScene.AddFirst(ai);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static LoadResAsyncRequest LoadSceneDistributeAsync(string path, bool additive)
        {
            AssetInfo ai = PreloadResDistribute(path);
            LoadResAsyncRequest req = null;
            if (ai != null)
            {

                string levelName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (additive)
                {
                    var operation = Application.LoadLevelAdditiveAsync(levelName);
                    req = new LoadResAsyncRequest(operation, null);
                    RunningScene.AddFirst(ai);
                }
                else
                {
                    var operation = Application.LoadLevelAsync(levelName);
                    req = new LoadResAsyncRequest(operation, null, raw =>
                    {
                        UnloadCurrentScene();
                        RunningScene.AddFirst(ai);
                        return raw;
                    });
                }
            }
            return req;
        }
#endif

        private static Camera _UIDialogCameraInstance = null;
        private static Camera _UIMainCameraInstance = null;
        private static AudioListener _UIAudioListener = null;

        public static AudioListener GetUIAudioListener()
        {
            if (_UIAudioListener == null)
            {
                var uiMainCamera = GetUIMainCamera();
                _UIAudioListener = uiMainCamera.gameObject.GetComponent<AudioListener>();
                if (_UIAudioListener == null)
                {
                    _UIAudioListener = uiMainCamera.gameObject.AddComponent<AudioListener>();
                }
            }
            return _UIAudioListener;
        }

        public static Camera GetUIDialogCamera()
        {
            if (_UIDialogCameraInstance == null)
            {
                Camera dialogCamera = null;
                var dialogCameraObj = GameObject.FindWithTag("UIDialogCamera");
                if (dialogCameraObj == null)
                {
                    dialogCameraObj = new GameObject("UI Dialog Camera");
                    dialogCameraObj.tag = "UIDialogCamera";
                    dialogCamera = dialogCameraObj.AddComponent<Camera>();
                    dialogCamera.depth = 1;
                    dialogCamera.clearFlags = CameraClearFlags.Depth;
                    dialogCamera.cullingMask = 1 << LayerMask.NameToLayer("Dialog");
                    dialogCamera.useOcclusionCulling = false;
                    dialogCamera.allowHDR = false;
                    dialogCamera.allowMSAA = false;

                    ResManager.DontDestroyOnLoad(dialogCameraObj);

                    _UIDialogCameraInstance = dialogCamera;
                }
                else
                {
                    dialogCamera = dialogCameraObj.GetComponent<Camera>();
                    _UIDialogCameraInstance = dialogCamera;
                }
            }

            return _UIDialogCameraInstance;
        }

        public static Camera GetUIMainCamera()
        {
            if (_UIMainCameraInstance == null)
            {
                Camera uiMainCamera = null;
                var uiMainCameraObj = GameObject.FindWithTag("UIMainCamera");
                if (uiMainCameraObj == null)
                {
                    uiMainCameraObj = new GameObject("UI Main Camera");
                    uiMainCameraObj.tag = "UIMainCamera";
                    uiMainCamera = uiMainCameraObj.AddComponent<Camera>();
#if UNITY_EDITOR
                    if (Application.isEditor && SceneManager.GetActiveScene().name != "match_main")
#endif
                    {
                        uiMainCameraObj.AddComponent<AudioListener>();
                    }
                    uiMainCamera.depth = 0;
                    uiMainCamera.clearFlags = CameraClearFlags.Depth;
                    uiMainCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
                    uiMainCamera.useOcclusionCulling = false;
                    uiMainCamera.allowHDR = false;
                    uiMainCamera.allowMSAA = false;

                    ResManager.DontDestroyOnLoad(uiMainCameraObj);

                    _UIMainCameraInstance = uiMainCamera;
                }
                else
                {
                    uiMainCamera = uiMainCameraObj.GetComponent<Camera>();
                    _UIMainCameraInstance = uiMainCamera;
                }
            }

            return _UIMainCameraInstance;
        }

        public static void ChangeGameObjectLayer(GameObject go, int layer)
        {
            //遍历当前物体及其所有子物体
            foreach (Transform tran in go.GetComponentsInChildren<Transform>(true))
            {
                tran.gameObject.layer = layer;//更改物体的Layer层
            }
        }

        private static IEnumerable<GameObject> FindAllGameObject()
        {
            var count = SceneManager.sceneCount;
            Scene sceneItem;
            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                sceneItem = SceneManager.GetSceneAt(i);
                list.AddRange(sceneItem.GetRootGameObjects());
            }
            return list;
        }
        #region 界面cache
        private static int Scene_Obj_Layer = LayerMask.NameToLayer("UI");
        private static int Dialog_Obj_Layer = LayerMask.NameToLayer("Dialog");
        private static int Scene_Cache_Obj_Layer = LayerMask.NameToLayer("SceneCacheObjLayer");
        public static int SceneCacheObjLayer { get { return Scene_Cache_Obj_Layer; } }
        private static int Dialog_Cache_Obj_Layer = LayerMask.NameToLayer("DialogCacheObjLayer");
        public static int DialogCacheObjLayer { get { return Dialog_Cache_Obj_Layer; } }
        private static Transform _cacheToDontDestroyRoot = null;
        /// <summary>
        /// 检查对象是否在uicache 中
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool HasOnUICache(UnityEngine.Object obj)
        {
            if (obj == null) return false;
            GameObject item = null;
            if (obj is GameObject)
            {
                item = obj as GameObject;
            }
            else if (obj is Component)
            {
                item = (obj as Component).gameObject;
            }
            else
            {
                return false;
            }
            if (item.layer == Scene_Cache_Obj_Layer || item.layer == Dialog_Cache_Obj_Layer) return true;
            Transform itemParent = item.transform.parent;
            int layer = 0;
            while (itemParent != null)
            {
                layer = itemParent.gameObject.layer;
                if (layer == Scene_Cache_Obj_Layer || layer == Dialog_Cache_Obj_Layer) return true;
                itemParent = itemParent.parent;
            }
            return false;
        }

        [XLua.LuaCallCSharp]
        public class PackedSceneObjs
        {
            public List<GameObject> sceneObj { private set; get; }
            public List<GameObject> dialogObjs { private set; get; }
            public PackedSceneObjs()
            {
                sceneObj = new List<GameObject>();
                dialogObjs = new List<GameObject>();
            }
        }

        public static PackedSceneObjs PackSceneAndDialogs(GameObject[] dialogObjs)
        {
            var rootDontDestroyOnLoadObjs = GetRootDontDestroyOnLoadObjs();
            var oldObjs = FindAllGameObject();

            var ret = new PackedSceneObjs();
            foreach (var obj in oldObjs)
            {
                if (rootDontDestroyOnLoadObjs.Contains(obj)) continue;
                var canvas = obj.GetComponent<Canvas>();
                if (canvas != null && canvas.sortingLayerName == "Dialog")
                {
                    bool find = false;
                    foreach (var dialogObj in dialogObjs)
                    {
                        if (dialogObj != obj) continue;
                        find = true;
                        break;
                    }
                    if (find)
                    {
                        ret.dialogObjs.Add(obj);
                        continue;
                    }
                    ret.sceneObj.Add(obj);
                    continue;
                }
                var canvasGroup = obj.GetComponent<CanvasGroup>();
                if (canvasGroup != null && canvasGroup.blocksRaycasts == false)
                {
                    GameObject.Destroy(obj);
                    continue;
                }
                ret.sceneObj.Add(obj);
            }
            // 按order从小到大排序，这样才能和lua中的记录对应上
            ret.dialogObjs.Sort((x, y) =>
            {
                var orderX = x.transform.GetComponent<Canvas>().sortingOrder;
                var orderY = y.transform.GetComponent<Canvas>().sortingOrder;
                return orderX - orderY;
            });
            return ret;
        }

        public static List<GameObject> PackSceneObj()
        {
            var rootDontDestroyOnLoadObjs = GetRootDontDestroyOnLoadObjs();
            var oldObjs = FindAllGameObject();
            List<GameObject> objs = new List<GameObject>();
            foreach (var obj in oldObjs)
            {
                if (rootDontDestroyOnLoadObjs.Contains(obj)) continue;
                objs.Add(obj);
            }
            return objs;
        }
        public static void UnpackSceneObj(GameObject sgo)
        {
            if (sgo == null) return;
            SetCacheActive(sgo, false);
        }

        public static void UnpackSceneObj(List<GameObject> sgos)
        {
            if (sgos == null) return;
            for (int i = 0; i < sgos.Count; i++) UnpackSceneObj(sgos[i]);
        }

        public static void CacheToDontDestroyRoot(List<GameObject> objs)
        {
            if (objs == null) return;
            if (_cacheToDontDestroyRoot == null)
            {
                _cacheToDontDestroyRoot = new GameObject("Cache_Dont_Destroy_Root").transform;
                DontDestroyOnLoad(_cacheToDontDestroyRoot);
            }
            foreach (var obj in objs) obj.transform.parent = _cacheToDontDestroyRoot;
        }

        public static void SetCacheActive(List<GameObject> objs, bool isActive)
        {
            if (objs == null) return;
            foreach (var obj in objs) SetCacheActive(obj, isActive);
        }

        public static void SetCacheActive(GameObject obj, bool isActive)
        {
            if (obj == null) return;
            //----------------------添加到不清除列表中-------------------------------
            if (isActive)
            {
                DontDestroyOnLoadObjs.Add(obj);
            }
            else
            {
                DontDestroyOnLoadObjs.Remove(obj);
            }
            //----------------------组件设置-------------------------------
            ProfilerUtl.BeginSample("SetCacheActive enabled ====>");
            var enabled = !isActive;
            var capsCache = obj.GetComponent<CapsGameObjectCache>();
            if (capsCache == null) capsCache = obj.AddComponent<CapsGameObjectCache>();
            if (capsCache != null) capsCache.SetEnabled(enabled);
            ProfilerUtl.EndSample("SetCacheActive enabled ====>");
            //-----------------------------layer层级调整----------------------------
            ProfilerUtl.BeginSample("SetCacheActive SET layer ====>");
            int currLayer = 0;
            var transformAttr = obj.GetComponentsInChildren<Transform>(true);
            if (transformAttr == null) return;
            GameObject gItem;
            int gItemLayer = 0;
            foreach (var transform in transformAttr)
            {
                gItem = transform.gameObject;
                gItemLayer = gItem.layer;
                if (isActive)
                {
                    if (gItemLayer == Scene_Obj_Layer)
                    {
                        currLayer = Scene_Cache_Obj_Layer;
                    }
                    else if (gItemLayer == Dialog_Obj_Layer)
                    {
                        currLayer = Dialog_Cache_Obj_Layer;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    if (gItemLayer == Scene_Cache_Obj_Layer)
                    {
                        currLayer = Scene_Obj_Layer;
                    }
                    else if (gItemLayer == Dialog_Cache_Obj_Layer)
                    {
                        currLayer = Dialog_Obj_Layer;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (gItemLayer != currLayer) gItem.layer = currLayer;
            }
            ProfilerUtl.EndSample("SetCacheActive SET layer ====>");
        }

        private static HashSet<UnityEngine.Object> GetRootDontDestroyOnLoadObjs()
        {
            DontDestroyOnLoadObjs.RemoveWhere(obj => obj == null);
            HashSet<UnityEngine.Object> RootDontDestroyOnLoadObjs = new HashSet<UnityEngine.Object>();
            Transform t = null;
            foreach (var dobj in DontDestroyOnLoadObjs)
            {
                t = null;
                if (dobj is GameObject)
                {
                    t = (dobj as GameObject).transform;
                }
                else if (dobj is Component)
                {
                    t = (dobj as Component).transform;
                }
                if (t == null)
                {
                    RootDontDestroyOnLoadObjs.Add(dobj);
                    continue;
                }
                while (t.parent != null) t = t.parent;
                RootDontDestroyOnLoadObjs.Add(t.gameObject);
            }
            return RootDontDestroyOnLoadObjs;
        }
        #endregion

#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
        private static void LoadSceneImmediateRaw(string name, bool additive, bool isAtEndOfFrame)
        {
            DontDestroyOnLoadObjs.RemoveWhere(obj => obj == null);

#if (UNITY_5 || UNITY_5_3_OR_NEWER)
#else
            if (!additive)
            {
                var sgo = PackSceneObj();
                GameObject.DestroyImmediate(sgo);
            }
#endif

            var path = name;
            string[] distributeFlags = GetDistributeFlags();
            path = path.Replace('\\', '/');
            path = path.TrimEnd('/');
            if (path.StartsWith("Assets/CapstonesRes/"))
            {
                var subpath = path.Substring("Assets/CapstonesRes/".Length);
                for (int i = distributeFlags.Length - 1; i >= 0; --i)
                {
                    var flag = distributeFlags[i];
                    if (!string.IsNullOrEmpty(flag))
                    {
                        var dpath = "Assets/CapstonesRes/distribute/" + flag + "/" + subpath;
                        if (System.IO.File.Exists(dpath))
                        {
                            path = dpath;
                            break;
                        }
                    }
                }
            }
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
            if (additive)
            {
                UnityEditor.EditorApplication.LoadLevelAdditiveInPlayMode(path);
            }
            else
            {
                UnityEditor.EditorApplication.LoadLevelInPlayMode(path);
            }
#else
            UnityEditor.EditorApplication.OpenSceneAdditive(path);
#endif
        }
        private class LoadSceneImmediateRawInfo
        {
            public string name;
            public bool additive;
            public bool isAtEndOfFrame;
        }
        private static LinkedList<LoadSceneImmediateRawInfo> LoadSceneImmediateRawQueue = new LinkedList<LoadSceneImmediateRawInfo>();
        private static void LoadSceneImmediateRawAll()
        {
            while (LoadSceneImmediateRawQueue.Count > 0)
            {
                var first = LoadSceneImmediateRawQueue.First.Value;
                LoadSceneImmediateRaw(first.name, first.additive, first.isAtEndOfFrame);
                LoadSceneImmediateRawQueue.RemoveFirst();
            }
        }
#endif
        internal static HashSet<UnityEngine.Object> DontDestroyOnLoadObjs = new HashSet<UnityEngine.Object>();
        public static void DontDestroyOnLoad(UnityEngine.Object obj, bool isDontDestroy = true)
        {
            if (isDontDestroy) GameObject.DontDestroyOnLoad(obj);
            if (obj != null) DontDestroyOnLoadObjs.Add(obj);
        }
        internal static HashSet<UnityEngine.Object> CanDestroyAllObjs = new HashSet<UnityEngine.Object>();
        public static void CanDestroyAll(UnityEngine.Object objp)
        {
            if (objp != null)
            {
                bool dontDestroy = false;
                HashSet<Transform> hie = new HashSet<Transform>();
                Transform cur = null;
                if (objp is Component)
                {
                    cur = ((Component)objp).transform;
                }
                else if (objp is GameObject)
                {
                    cur = ((GameObject)objp).transform;
                }
                while (cur != null)
                {
                    hie.Add(cur);
                    cur = cur.parent;
                }
                foreach (var obj in ResManager.DontDestroyOnLoadObjs)
                {
                    if (obj != null)
                    {
                        Transform trans = null;
                        if (obj is Transform)
                        {
                            trans = (Transform)obj;
                        }
                        else if (obj is Component)
                        {
                            trans = ((Component)obj).transform;
                        }
                        else if (obj is GameObject)
                        {
                            trans = ((GameObject)obj).transform;
                        }
                        if (trans != null && hie.Contains(trans))
                        {
                            dontDestroy = true;
                        }
                    }
                }
                if (dontDestroy)
                {
                    CanDestroyAllObjs.Add(objp);
                }
            }
        }
        public static void DontDestroyOnLoadCanDestroyAll(UnityEngine.Object obj)
        {
            DontDestroyOnLoad(obj);
            CanDestroyAll(obj);
        }

        public static void LoadSceneImmediate(string name, bool additive)
        {
            LoadSceneImmediate(name, additive, false);
        }

        public static void LoadSceneImmediate(string name, bool additive, bool isAtEndOfFrame)
        {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            LoadSceneImmediateRawQueue.AddLast(new LoadSceneImmediateRawInfo() { name = name, additive = additive, isAtEndOfFrame = isAtEndOfFrame });
            if (LoadSceneImmediateRawQueue.Count == 1)
            {
                LoadSceneImmediateRawAll();
            }
#else
            LoadSceneDistribute(name, additive, isAtEndOfFrame);
#endif
        }
        public static void LoadSceneImmediate(string name)
        {
            LoadSceneImmediate(name, false);
        }
        private static IEnumerator LoadSceneAtEndOfFrameWork(string name, bool additive)
        {
            yield return new WaitForEndOfFrame();
            LoadSceneImmediate(name, additive, true);
        }
        public static void LoadScene(string name, bool additive)
        {
            LoadSceneAtEndOfFrameWork(name, additive).StartCoroutine();
        }
        public static void LoadScene(string name)
        {
            LoadScene(name, false);
        }

        public static LoadResAsyncRequest LoadSceneAsync(string name, bool additive)
        {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            LoadScene(name, additive);
            return null;
#else
            return LoadSceneDistributeAsync(name, additive);
#endif
        }

        public static LoadResAsyncRequest LoadSceneAsync(string name)
        {
            return LoadSceneAsync(name, false);
        }

        public static bool IsDistributeFlagSelected(string flag)
        {
            string[] distFlags = GetDistributeFlags();
            if (distFlags != null)
            {
                for (int i = 0; i < distFlags.Length; i++)
                {
                    if (flag == distFlags[i])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static string[] DistributeFlags = null;
        public static string[] GetDistributeFlags()
        {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            DistributeFlags = null;
            var sr = PlatDependant.OpenReadText("Assets/Resources/DistributeFlags.txt");
            if (sr != null)
            {
                var strflags = sr.ReadToEnd().Trim();
                if (!string.IsNullOrEmpty(strflags))
                {
                    DistributeFlags = strflags.Split(new[] { '<' }, System.StringSplitOptions.RemoveEmptyEntries);
                }
                sr.Dispose();
            }
            else
            {
                // TODO: default dflags
                DistributeFlags = new[] { "ex" };
            }
            if (DistributeFlags == null)
            {
                DistributeFlags = new string[0];
            }
#else
            if (DistributeFlags == null)
            {
                HashSet<string> forbiddenFlags = new HashSet<string>();
                {
                    var strflags = PlayerPrefs.GetString("___Pref__ForbiddenDistributeFlags").Trim();
                    if (!string.IsNullOrEmpty(strflags))
                    {
                        var cflags = strflags.Split(new[] { '<' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (cflags != null)
                        {
                            forbiddenFlags.UnionWith(cflags);
                        }
                    }
                }

                List<string> flags = new List<string>();
                TextAsset txt = Resources.Load("DistributeFlags", typeof(TextAsset)) as TextAsset;
                if (txt != null)
                {
                    var strflags = txt.text.Trim();
                    if (!string.IsNullOrEmpty(strflags))
                    {
                        var cflags = strflags.Split(new[] { '<' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (cflags != null)
                        {
                            foreach (var flag in cflags)
                            {
                                if (!forbiddenFlags.Contains(flag))
                                {
                                    flags.Add(flag);
                                }
                            }
                        }
                    }
                }
                {
                    var strflags = PlayerPrefs.GetString("___Pref__OptionalDistributeFlags").Trim();
                    if (!string.IsNullOrEmpty(strflags))
                    {
                        var cflags = strflags.Split(new[] { '<' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (cflags != null)
                        {
                            foreach (var flag in cflags)
                            {
                                if (!forbiddenFlags.Contains(flag))
                                {
                                    flags.Add(flag);
                                }
                            }
                        }
                    }
                }
                DistributeFlags = flags.ToArray();
            }
#endif
            return DistributeFlags;
        }
        public static string[] ReloadDistributeFlags()
        {
            DistributeFlags = null;
            return GetDistributeFlags();
        }
        public static string[] AddDistributeFlag(string flag)
        {
            var oldflags = GetDistributeFlags();
            if (string.IsNullOrEmpty(flag))
            {
                return oldflags;
            }
            if (oldflags.Contains(flag))
            {
                return oldflags;
            }
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            DistributeFlags = oldflags.Concat(new[] { flag }).ToArray();
            var sb = new System.Text.StringBuilder();
            foreach (var nflag in DistributeFlags)
            {
                sb.Append('<');
                sb.Append(nflag);
            }
            using (var sw = PlatDependant.OpenWriteText("Assets/Resources/DistributeFlags.txt"))
            {
                sw.Write(sb.ToString());
            }
#else
            HashSet<string> forbiddenFlags = new HashSet<string>();
            {
                var strflags = PlayerPrefs.GetString("___Pref__ForbiddenDistributeFlags");
                if (!string.IsNullOrEmpty(strflags))
                {
                    var cflags = strflags.Split(new[] { '<' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (cflags != null)
                    {
                        forbiddenFlags.UnionWith(cflags);
                    }
                }
            }
            if (forbiddenFlags.Remove(flag))
            {
                var sb = new System.Text.StringBuilder();
                foreach (var nflag in forbiddenFlags)
                {
                    sb.Append('<');
                    sb.Append(nflag);
                }
                PlayerPrefs.SetString("___Pref__ForbiddenDistributeFlags", sb.ToString());
                PlayerPrefs.Save();
            }

            List<string> flags = new List<string>();
            TextAsset txt = Resources.Load("DistributeFlags", typeof(TextAsset)) as TextAsset;
            if (txt != null)
            {
                var strflags = txt.text;
                if (!string.IsNullOrEmpty(strflags))
                {
                    var cflags = strflags.Split(new[] { '<' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (cflags != null)
                    {
                        foreach (var nflag in cflags)
                        {
                            if (!forbiddenFlags.Contains(nflag))
                            {
                                flags.Add(nflag);
                            }
                        }
                    }
                }
            }
            List<string> oflags = new List<string>();
            {
                var strflags = PlayerPrefs.GetString("___Pref__OptionalDistributeFlags");
                if (!string.IsNullOrEmpty(strflags))
                {
                    var cflags = strflags.Split(new[] { '<' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (cflags != null)
                    {
                        foreach (var nflag in cflags)
                        {
                            if (!forbiddenFlags.Contains(nflag))
                            {
                                flags.Add(nflag);
                                oflags.Add(nflag);
                            }
                        }
                    }
                }
            }
            if (!flags.Contains(flag))
            {
                flags.Add(flag);
                oflags.Add(flag);
                var sb = new System.Text.StringBuilder();
                foreach (var nflag in oflags)
                {
                    sb.Append('<');
                    sb.Append(nflag);
                }
                PlayerPrefs.SetString("___Pref__OptionalDistributeFlags", sb.ToString());
                PlayerPrefs.Save();
            }
            DistributeFlags = flags.ToArray();
#endif
            return DistributeFlags;
        }
        public static string[] RemoveDistributeFlag(string flag)
        {
            var oldflags = GetDistributeFlags();
            if (string.IsNullOrEmpty(flag))
            {
                return oldflags;
            }
            if (!oldflags.Contains(flag))
            {
                return oldflags;
            }
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            DistributeFlags = oldflags.Except(new[] { flag }).ToArray();
            var sb = new System.Text.StringBuilder();
            foreach (var nflag in DistributeFlags)
            {
                sb.Append('<');
                sb.Append(nflag);
            }
            using (var sw = PlatDependant.OpenWriteText("Assets/Resources/DistributeFlags.txt"))
            {
                sw.Write(sb.ToString());
            }
            return DistributeFlags;
#else
            {
                var strflags = PlayerPrefs.GetString("___Pref__OptionalDistributeFlags");
                if (!string.IsNullOrEmpty(strflags))
                {
                    var cflags = strflags.Split(new[] { '<' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (cflags != null)
                    {
                        if (cflags.Contains(flag))
                        {
                            var sb = new System.Text.StringBuilder();
                            foreach (var nflag in cflags)
                            {
                                if (nflag != flag)
                                {
                                    sb.Append('<');
                                    sb.Append(nflag);
                                }
                            }
                            PlayerPrefs.SetString("___Pref__OptionalDistributeFlags", sb.ToString());
                            PlayerPrefs.Save();
                        }
                    }
                }
            }
            TextAsset txt = Resources.Load("DistributeFlags", typeof(TextAsset)) as TextAsset;
            if (txt != null)
            {
                var strflags = txt.text;
                if (!string.IsNullOrEmpty(strflags))
                {
                    var cflags = strflags.Split(new[] { '<' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (cflags != null)
                    {
                        if (cflags.Contains(flag))
                        {
                            HashSet<string> forbiddenFlags = new HashSet<string>();
                            strflags = PlayerPrefs.GetString("___Pref__ForbiddenDistributeFlags");
                            if (!string.IsNullOrEmpty(strflags))
                            {
                                cflags = strflags.Split(new[] { '<' }, System.StringSplitOptions.RemoveEmptyEntries);
                                if (cflags != null)
                                {
                                    forbiddenFlags.UnionWith(cflags);
                                }
                            }
                            if (forbiddenFlags.Add(flag))
                            {
                                var sb = new System.Text.StringBuilder();
                                foreach (var nflag in forbiddenFlags)
                                {
                                    sb.Append('<');
                                    sb.Append(nflag);
                                }
                                PlayerPrefs.SetString("___Pref__ForbiddenDistributeFlags", sb.ToString());
                                PlayerPrefs.Save();
                            }
                        }
                    }
                }
            }
            return ReloadDistributeFlags();
#endif
        }

        /// <summary>
        /// 切换语言使用
        /// </summary>
        /// <param name="dels">要屏蔽的flag</param>
        /// <param name="adds">要增加的flag</param>
        /// <returns></returns>
        public static string[] ChangeDistributeFlag(string dels, string adds)
        {
            string[] delList = dels.Split(new[] { '<' }, System.StringSplitOptions.RemoveEmptyEntries);
            string[] addList = adds.Split(new[] { '<' }, System.StringSplitOptions.RemoveEmptyEntries);
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            var oldflags = GetDistributeFlags();
            DistributeFlags = oldflags.Except(delList).Union(addList).ToArray();

            var sb = new System.Text.StringBuilder();
            foreach (var nflag in DistributeFlags)
            {
                sb.Append('<');
                sb.Append(nflag);
            }
            using (var sw = PlatDependant.OpenWriteText("Assets/Resources/DistributeFlags.txt"))
            {
                sw.Write(sb.ToString());
            }
            return DistributeFlags;
#else
            string[] forbiddenFlags = null;
            {
                // 要屏蔽的flag
                var strflags = PlayerPrefs.GetString("___Pref__ForbiddenDistributeFlags");
                if (!string.IsNullOrEmpty(strflags))
                {
                    var cflags = strflags.Split(new[] { '<' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (cflags != null)
                    {
                        forbiddenFlags = cflags.Union(delList).Except(addList).ToArray();
                        var sb = new System.Text.StringBuilder();
                        foreach (var nflag in forbiddenFlags)
                        {
                            sb.Append('<');
                            sb.Append(nflag);
                        }
                        PlayerPrefs.SetString("___Pref__ForbiddenDistributeFlags", sb.ToString());
                        PlayerPrefs.Save();
                    }
                }
                else
                {
                    var addForbiddenFlags = delList.Except(addList).ToArray();
                    var sb = new System.Text.StringBuilder();
                    for (int i = 0; i < addForbiddenFlags.Length; i++)
                    {
                        string delFlag = addForbiddenFlags[i];
                        sb.Append('<');
                        sb.Append(delFlag);
                    }
                    forbiddenFlags = addForbiddenFlags;
                    PlayerPrefs.SetString("___Pref__ForbiddenDistributeFlags", sb.ToString());
                    PlayerPrefs.Save();
                }
            }

            TextAsset txt = Resources.Load("DistributeFlags", typeof(TextAsset)) as TextAsset;
            if (txt != null)
            {
                var strflags = txt.text;
                if (!string.IsNullOrEmpty(strflags))
                {
                    var cflags = strflags.Split(new[] { '<' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (cflags != null)
                    {
                        DistributeFlags = cflags.Except(forbiddenFlags).ToArray();
                        return DistributeFlags;
                    }
                }
            }
            return ReloadDistributeFlags();
#endif
        }

        private static string _UpdatePath;
        public static string UpdatePath
        {
            get
            {
                if (_UpdatePath == null)
                {
                    _UpdatePath = UnityEngineEx.IsolatedPrefs.GetIsolatedPath();
                }
                return _UpdatePath;
            }
        }
    }
}
