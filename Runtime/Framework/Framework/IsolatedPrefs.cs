using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

using Capstones.PlatExt;

namespace Capstones.UnityEngineEx
{
    public static class IsolatedPrefs
    {
#if UNITY_STANDALONE && !UNITY_EDITOR
#if NET_4_6
        private class IsolatedIDFileHolder
        {
            private int _InstanceID = 0;
            public int InstanceID { get { return _InstanceID; } }

            private System.Threading.Mutex _IsolatedIDMutex = new System.Threading.Mutex(false, "IsolatedIDMutex");
            private System.IO.FileStream _IsolatedIDHolder;

            public IsolatedIDFileHolder()
            {
                _IsolatedIDMutex.WaitOne();
                try
                {
                    var file = Application.persistentDataPath + "/iid.txt";
                    var fileh = Application.persistentDataPath + "/iidh.txt";
                    if (PlatDependant.IsFileExist(fileh))
                    {
                        bool shouldDeleteFile = true;
                        try
                        {
                            var hstream = System.IO.File.Open(fileh, System.IO.FileMode.Open, System.IO.FileAccess.Write, System.IO.FileShare.Read);
                            if (hstream == null)
                            {
                                shouldDeleteFile = false;
                            }
                            else
                            {
                                hstream.Dispose();
                            }
                        }
                        catch (Exception)
                        {
                            shouldDeleteFile = false;
                        }
                        if (shouldDeleteFile)
                        {
                            PlatDependant.DeleteFile(fileh);
                            PlatDependant.DeleteFile(file);
                        }
                    }
                    if (!PlatDependant.IsFileExist(fileh))
                    {
                        using (var sw = PlatDependant.OpenWriteText(fileh))
                        {
                            sw.Write(" ");
                        }
                    }
                    _IsolatedIDHolder = System.IO.File.Open(fileh, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                    if (PlatDependant.IsFileExist(file))
                    {
                        try
                        {
                            using (var sr = PlatDependant.OpenReadText(file))
                            {
                                var index = sr.ReadLine();
                                int.TryParse(index, out _InstanceID);
                            }
                        }
                        catch (Exception e)
                        {
                            PlatDependant.LogError(e);
                        }
                    }
                    using (var sw = PlatDependant.OpenWriteText(file))
                    {
                        sw.Write(_InstanceID + 1);
                    }
                }
                catch (Exception e)
                {
                    PlatDependant.LogError(e);
                }
                finally
                {
                    _IsolatedIDMutex.ReleaseMutex();
                }
            }

            ~IsolatedIDFileHolder()
            {
                _IsolatedIDMutex.WaitOne();
                try
                {
                    if (_IsolatedIDHolder != null)
                    {
                        _IsolatedIDHolder.Dispose();
                        _IsolatedIDHolder = null;
                    }
                    var file = Application.persistentDataPath + "/iid.txt";
                    int instanceid = 0;
                    if (PlatDependant.IsFileExist(file))
                    {
                        try
                        {
                            using (var sr = PlatDependant.OpenReadText(file))
                            {
                                var index = sr.ReadLine();
                                int.TryParse(index, out instanceid);
                            }
                        }
                        catch (Exception e)
                        {
                            PlatDependant.LogError(e);
                        }
                    }
                    if (instanceid <= 1)
                    {
                        PlatDependant.DeleteFile(file);
                    }
                    else
                    {
                        using (var sw = PlatDependant.OpenWriteText(file))
                        {
                            sw.Write(instanceid - 1);
                        }
                    }
                }
                catch (Exception e)
                {
                    PlatDependant.LogError(e);
                }
                finally
                {
                    _IsolatedIDMutex.ReleaseMutex();
                }
            }
        }
        private static IsolatedIDFileHolder _InstanceHolder = new IsolatedIDFileHolder();
#else
        private class IsolatedIDFileHolder
        {
            private int _InstanceID = 0;
            public int InstanceID { get { return _InstanceID; } }

            private System.IO.FileStream _IsolatedIDHolder;

            public IsolatedIDFileHolder()
            {
                var file = Application.persistentDataPath + "/iid.data";
                System.IO.FileStream sfile = null;
                while (sfile == null)
                {
                    try
                    {
                        if (!System.IO.Directory.Exists(Application.persistentDataPath))
                        {
                            System.IO.Directory.CreateDirectory(Application.persistentDataPath);
                        }
                        sfile = System.IO.File.Open(file, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                    }
                    catch { }
                    if (sfile == null)
                    {
                        System.Threading.Thread.Sleep(1);
                    }
                }
                try
                {
                    var fileh = Application.persistentDataPath + "/iidh.txt";
                    if (PlatDependant.IsFileExist(fileh))
                    {
                        bool shouldDeleteFile = true;
                        try
                        {
                            var hstream = System.IO.File.Open(fileh, System.IO.FileMode.Open, System.IO.FileAccess.Write, System.IO.FileShare.Read);
                            if (hstream == null)
                            {
                                shouldDeleteFile = false;
                            }
                            else
                            {
                                hstream.Dispose();
                            }
                        }
                        catch (Exception)
                        {
                            shouldDeleteFile = false;
                        }
                        if (shouldDeleteFile)
                        {
                            PlatDependant.DeleteFile(fileh);
                            sfile.Seek(0, System.IO.SeekOrigin.Begin);
                            sfile.SetLength(0);
                        }
                    }
                    if (!PlatDependant.IsFileExist(fileh))
                    {
                        using (var sw = PlatDependant.OpenWriteText(fileh))
                        {
                            sw.Write(" ");
                        }
                    }
                    _IsolatedIDHolder = System.IO.File.Open(fileh, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                    if (sfile.Length >= 4)
                    {
                        sfile.Seek(0, System.IO.SeekOrigin.Begin);
                        using (var br = new System.IO.BinaryReader(sfile, System.Text.Encoding.UTF8, true))
                        {
                            _InstanceID = br.ReadInt32();
                        }
                    }
                    sfile.Seek(0, System.IO.SeekOrigin.Begin);
                    sfile.SetLength(0);
                    using (var bw = new System.IO.BinaryWriter(sfile, System.Text.Encoding.UTF8, true))
                    {
                        bw.Write(_InstanceID + 1);
                    }
                }
                catch (Exception e)
                {
                    PlatDependant.LogError(e);
                }
                finally
                {
                    sfile.Dispose();
                }
            }

            ~IsolatedIDFileHolder()
            {
                var file = Application.persistentDataPath + "/iid.data";
                System.IO.FileStream sfile = null;
                while (sfile == null)
                {
                    try
                    {
                        if (!System.IO.Directory.Exists(Application.persistentDataPath))
                        {
                            System.IO.Directory.CreateDirectory(Application.persistentDataPath);
                        }
                        sfile = System.IO.File.Open(file, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                    }
                    catch { }
                    if (sfile == null)
                    {
                        System.Threading.Thread.Sleep(1);
                    }
                }
                try
                {
                    if (_IsolatedIDHolder != null)
                    {
                        _IsolatedIDHolder.Dispose();
                        _IsolatedIDHolder = null;
                    }
                    int instanceid = 0;
                    if (sfile.Length >= 4)
                    {
                        sfile.Seek(0, System.IO.SeekOrigin.Begin);
                        using (var br = new System.IO.BinaryReader(sfile, System.Text.Encoding.UTF8, true))
                        {
                            instanceid = br.ReadInt32();
                        }
                    }
                    sfile.Seek(0, System.IO.SeekOrigin.Begin);
                    sfile.SetLength(0);
                    if (instanceid > 1)
                    {
                        using (var bw = new System.IO.BinaryWriter(sfile, System.Text.Encoding.UTF8, true))
                        {
                            bw.Write(instanceid - 1);
                        }
                    }
                }
                catch (Exception e)
                {
                    PlatDependant.LogError(e);
                }
                finally
                {
                    sfile.Dispose();
                }
            }
        }
        private static IsolatedIDFileHolder _InstanceHolder = new IsolatedIDFileHolder();
#endif
#endif

        private static string _InstallID;
        private static string LoadInstallID()
        {
#if UNITY_EDITOR
            string capid = null;
            string capidfile = "EditorOutput/Runtime/capid.txt";
            if (PlatDependant.IsFileExist(capidfile))
            {
                try
                {
                    using (var sr = PlatDependant.OpenReadText(capidfile))
                    {
                        capid = sr.ReadLine().Trim();
                    }
                }
                catch (Exception e)
                {
                    PlatDependant.LogError(e);
                }
            }
            if (string.IsNullOrEmpty(capid))
            {
                capid = Guid.NewGuid().ToString("N");
                try
                {
                    using (var sw = PlatDependant.OpenWriteText(capidfile))
                    {
                        sw.WriteLine(capid);
                    }
                }
                catch (Exception e)
                {
                    PlatDependant.LogError(e);
                }
            }
            return capid;
#else
            string capid = null;
            if (PlayerPrefs.HasKey("___Pref__CapID"))
            {
                capid = PlayerPrefs.GetString("___Pref__CapID");
            }
            if (string.IsNullOrEmpty(capid))
            {
                capid = Guid.NewGuid().ToString("N");
                PlayerPrefs.SetString("___Pref__CapID", capid);
                PlayerPrefs.Save();
            }
            return capid;
#endif
        }
        public static void ReloadInstallID()
        {
            _InstallID = LoadInstallID();
        }
        public static string InstallID
        {
            get
            {
                if (_InstallID == null)
                {
                    ReloadInstallID();
                }
                return _InstallID;
            }
        }
        private static string _IsolatedID;
        private static string LoadIsolatedID()
        {
#if UNITY_EDITOR
            return InstallID;
#elif UNITY_STANDALONE
            if (_InstanceHolder.InstanceID == 0)
            {
                return InstallID;
            }
            else
            {
                return string.Format("{0}-{1}-", InstallID, _InstanceHolder.InstanceID);
            }
#else
            return InstallID;
#endif
        }
        public static void ReloadIsolatedID()
        {
            ReloadInstallID();
            _IsolatedID = LoadIsolatedID();
        }
        public static string IsolatedID
        {
            get
            {
                if (_InstallID == null || _IsolatedID == null)
                {
                    ReloadIsolatedID();
                }
                return _IsolatedID;
            }
        }

        public static int InstanceID
        {
            get
            {
#if UNITY_STANDALONE && !UNITY_EDITOR
                return _InstanceHolder.InstanceID;
#else
                return 0;
#endif
            }
        }

        public static string GetIsolatedPath()
        {
#if UNITY_EDITOR
            return "EditorOutput/Runtime";
#elif UNITY_STANDALONE
            if (_InstanceHolder.InstanceID == 0)
            {
                return UnityEngine.Application.temporaryCachePath;
            }
            else
            {
                return UnityEngine.Application.temporaryCachePath + "/instance" + _InstanceHolder.InstanceID.ToString();
            }
#elif UNITY_ANDROID
            return UnityEngine.Application.persistentDataPath;
#else
            return UnityEngine.Application.temporaryCachePath;
#endif
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        private static DataDictionary _Dict = new DataDictionary();
        static IsolatedPrefs()
        {
            string json = null;
            string file = GetIsolatedPath() + "/iprefs.txt";
            if (PlatDependant.IsFileExist(file))
            {
                try
                {
                    using (var sr = PlatDependant.OpenReadText(file))
                    {
                        json = sr.ReadToEnd();
                    }
                    if (!string.IsNullOrEmpty(json))
                    {
                        JsonUtility.FromJsonOverwrite(json, _Dict);
                    }
                }
                catch (Exception e)
                {
                    PlatDependant.LogError(e);
                }
            }
        }

        public static void DeleteAll()
        {
            _Dict.Clear();
        }
        public static void DeleteKey(string key)
        {
            _Dict.Remove(key);
        }
        public static double GetNumber(string key)
        {
            object val;
            if (_Dict.TryGetValue(key, out val))
            {
                if (val is double)
                {
                    return (double)val;
                }
            }
            return 0;
        }
        public static int GetInt(string key)
        {
            object val;
            if (_Dict.TryGetValue(key, out val))
            {
                if (val is int)
                {
                    return (int)val;
                }
            }
            return 0;
        }
        public static string GetString(string key)
        {
            object val;
            if (_Dict.TryGetValue(key, out val))
            {
                if (val is string)
                {
                    return (string)val;
                }
            }
            return null;
        }
        public static bool HasKey(string key)
        {
            return _Dict.ContainsKey(key);
        }
        public static void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(_Dict);
                string file = GetIsolatedPath() + "/iprefs.txt";
                if (string.IsNullOrEmpty(json))
                {
                    PlatDependant.DeleteFile(file);
                }
                else
                {
                    using (var sw = PlatDependant.OpenWriteText(file))
                    {
                        sw.Write(json);
                    }
                }
            }
            catch (Exception e)
            {
                PlatDependant.LogError(e);
            }
        }
        public static void SetNumber(string key, double value)
        {
            _Dict[key] = value;
        }
        public static void SetInt(string key, int value)
        {
            _Dict[key] = value;
        }
        public static void SetString(string key, string value)
        {
            _Dict[key] = value;
        }
#else
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }
        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }
        public static double GetNumber(string key)
        {
            return PlayerPrefs.GetFloat(key);
        }
        public static int GetInt(string key)
        {
            return PlayerPrefs.GetInt(key);
        }
        public static string GetString(string key)
        {
            return PlayerPrefs.GetString(key);
        }
        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }
        public static void Save()
        {
            PlayerPrefs.Save();
        }
        public static void SetNumber(string key, double value)
        {
            PlayerPrefs.SetFloat(key, (float)value);
        }
        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }
        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }
#endif
    }
}
