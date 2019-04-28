using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Capstones.Dynamic;
using Capstones.LuaLib;
using Capstones.LuaWrap;
using Capstones.LuaExt;

using lua = Capstones.LuaLib.LuaCoreLib;
using lual = Capstones.LuaLib.LuaAuxLib;
using luae = Capstones.LuaLib.LuaLibEx;

namespace Capstones.UnityFramework
{
    public static class UnityLua
    {
        public static LuaState GlobalLua;

        static UnityLua()
        {
            GlobalLua = new LuaState();
            Assembly2Lua.Init(GlobalLua);
            Json2Lua.Init(GlobalLua);
            LuaFramework.Init(GlobalLua);
        }

        public static Coroutine StartCoroutine(this IEnumerator work)
        {
            var go = new GameObject();
            ResManager.DontDestroyOnLoad(go);
            return go.AddComponent<DummyBehav>().StartCoroutine(CoroutineWorkAndDestroy(go, work));
        }

        public static IEnumerator CoroutineWorkAndDestroy(GameObject go, IEnumerator work)
        {
            if (work != null)
            {
                while (work.MoveNext())
                {
                    yield return work.Current;
                }
            }
            if (go != null)
            {
                GameObject.Destroy(go);
            }
        }

        public static Coroutine StartCoroutine(this IEnumerable work)
        {
            if (work == null)
            {
                return null;
            }
            return StartCoroutine(work.GetEnumerator());
        }

        public static IEnumerable GetEnumerable(this IEnumerator work)
        {
            if (work != null)
            {
                while (work.MoveNext())
                {
                    yield return work.Current;
                }
            }
        }

        public static Coroutine StartLuaCoroutine(LuaFunc lfunc)
        {
            return StartLuaCoroutineForBehav(null, lfunc);
        }

        public static Coroutine StartLuaCoroutineForBehav(this MonoBehaviour behav, LuaFunc lfunc)
        {
            if (behav != null)
            {
                //var go = new GameObject();
                //ResManager.DontDestroyOnLoad(go);
                //return StartLuaCoroutineForMonoBehaviourAndDestroy(go, behav, lfunc);
                return behav.StartCoroutine(EnumLuaCoroutine(lfunc));
            }
            else
            {
                var go = new GameObject();
                ResManager.DontDestroyOnLoad(go);
                return StartLuaCoroutineForGameObjectAndDestroy(go, lfunc);
            }
        }

        public static Coroutine StartLuaCoroutineForGameObjectAndDestroy(this GameObject go, LuaFunc lfunc)
        {
            var behav = go.AddComponent<DummyBehav>();
            return behav.StartCoroutine(EnumLuaCoroutineForGameObjectAndDestroy(go, behav, lfunc));
        }

        public static Coroutine StartLuaCoroutineForMonoBehaviourAndDestroy(this GameObject go, MonoBehaviour behav, LuaFunc lfunc)
        {
            var behav2 = go.AddComponent<DummyBehav>();
            return behav2.StartCoroutine(EnumLuaCoroutineForMonoBehaviourAndDestroy(go, behav, lfunc));
        }

        public static IEnumerator EnumLuaCoroutineForGameObjectAndDestroy(this GameObject go, MonoBehaviour behav, LuaFunc lfunc)
        {
            var work = EnumLuaCoroutine(lfunc);
            if (work != null)
            {
                while (work.MoveNext())
                {
                    yield return work.Current;
                }
            }
            if (go != null)
            {
                GameObject.Destroy(go);
            }
        }

        public static IEnumerator EnumLuaCoroutineForMonoBehaviourAndDestroy(this GameObject go, MonoBehaviour behav, LuaFunc lfunc)
        {
            if (behav != null)
            {
                var work = EnumLuaCoroutine(lfunc);
                if (work != null)
                {
                    while (behav != null && work.MoveNext())
                    {
                        yield return work.Current;
                    }
                }
            }
            if (go != null)
            {
                GameObject.Destroy(go);
            }
        }

        public static IEnumerator EnumLuaCoroutine(LuaFunc lfunc)
        {
            if (lfunc != null)
            {
                LuaThread lthd = new LuaThread(lfunc);
                return EnumLuaCoroutine(lthd);
            }
            else
            {
                return GetEmptyEnumerator();
            }
        }

        public static IEnumerator EnumLuaCoroutine(LuaOnStackThread lthd)
        {
            if (lthd != null)
            {
                while (true)
                {
                    var results = lthd.Resume();
                    if (results != null)
                    {
                        var result = results.UnwrapReturnValues();
                        if (results.Length >= 2 && results[1].UnwrapDynamic<bool>())
                        {
                            if (result is IEnumerator)
                            {
                                var etor = result as IEnumerator;
                                while (etor.MoveNext())
                                {
                                    yield return etor.Current;
                                }
                            }
                            else if (result is IEnumerable)
                            {
                                var enumerable = result as IEnumerable;
                                foreach (var obj in enumerable)
                                {
                                    yield return obj;
                                }
                            }
                            else
                            {
                                yield return result;
                            }
                        }
                        else
                        {
                            yield return result;
                        }
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
        }

        public static IEnumerator GetEmptyEnumerator()
        {
            yield break;
        }

        public static BaseLua CreateUserDataAndExpandExFields(this CapsUnityLuaBehav behav)
        {
            using (var lr = GlobalLua.CreateStackRecover())
            {
                var l = GlobalLua.L;
                var ud = new LuaOnStackUserData(l, behav);
                ud.PushToLua();
                l.pushvalue(-1);
                var refid = l.refer();
                l.newtable();
                l.SetField(-2, "___ex");
                foreach (var kvp in behav.ExpandExVal())
                {
                    if (!(kvp.Value is CapsUnityLuaBehav.NotAvailableExVal))
                    {
                        ud.SetHierarchical("___ex." + kvp.Key, kvp.Value);
                    }
                }
                return new BaseLua(l, refid);
            }
        }

        public static BaseLua BindBehav(this CapsUnityLuaBehav behav)
        {
            using (var lr = GlobalLua.CreateStackRecover())
            {
                var l = GlobalLua.L;
                if (behav == null)
                {
                    return null;
                }
                if (string.IsNullOrEmpty(behav.InitLuaPath))
                {
                    return CreateUserDataAndExpandExFields(behav);
                }

                int oldtop = lr.Top;
                bool luaFileDone = false;
                l.pushcfunction(LuaFramework.ClrDelErrorHandler);
                l.GetGlobal("require");
                l.PushString(behav.InitLuaPath);
                if (l.pcall(1, 1, -3) == 0)
                {
                    if (l.gettop() > oldtop + 1 && l.istable(oldtop + 2))
                    {
                        luaFileDone = true;
                    }
                    else
                    {
                        if (GLog.IsLogErrorEnabled) GLog.LogError("Failed to init script by require " + behav.InitLuaPath + ". (Not a table.) Now Init it by file.");
                    }
                }
                else
                {
                    if (GLog.IsLogErrorEnabled) GLog.LogError(l.GetLua(-1).UnwrapDynamic() + "\nFailed to init script by require " + behav.InitLuaPath + ". Now Init it by file.");
                }
                if (!luaFileDone)
                {
                    if (GLog.IsLogInfoEnabled) GLog.LogInfo("Init it by file. - Disabled");
                    //string path = behav.InitLuaPath;
                    //if (path.EndsWith(".lua"))
                    //{
                    //    path = path.Substring(0, path.Length - 4);
                    //}
                    //path = path.Replace('.', '/');
                    //path = path.Replace('\\', '/');
                    //if (!path.StartsWith("spt/"))
                    //{
                    //    path = "spt/" + path;
                    //}
                    //path += ".lua";
                    //path = ResManager.UpdatePath + "/" + path;

                    //l.settop(oldtop);
                    //if (l.dofile(path) == 0)
                    //{
                    //    if (l.gettop() > oldtop && l.istable(oldtop + 1))
                    //    {
                    //        luaFileDone = true;
                    //    }
                    //    else
                    //    {
                    //       if (GLog.IsLogInfoEnabled) GLog.LogInfo("Failed to load script " + path + ". (Not a table.)");
                    //    }
                    //}
                    //else
                    //{
                    //   if (GLog.IsLogInfoEnabled) GLog.LogInfo(l.GetLua(-1).UnwrapDynamic());
                    //   if (GLog.IsLogInfoEnabled) GLog.LogInfo("Failed to load script " + path);
                    //}
                }
                if (luaFileDone)
                {
                    l.GetField(oldtop + 2, "attach");
                    if (l.isfunction(-1))
                    {
                        var ud = CreateUserDataAndExpandExFields(behav);
                        l.getref(ud.Refid);
                        if (l.pcall(1, 0, oldtop + 1) == 0)
                        {
                        }
                        else
                        {
                            if (GLog.IsLogErrorEnabled) GLog.LogError(l.GetLua(-1).UnwrapDynamic());
                        }
                        return ud;
                    }
                }
                return CreateUserDataAndExpandExFields(behav);
            }
        }
    }
}
