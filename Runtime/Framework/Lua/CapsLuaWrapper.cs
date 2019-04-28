using System;
using System.Collections;
using System.Collections.Generic;
using Capstones.Dynamic;
using Capstones.LuaLib;
using Capstones.PlatExt;

using lua = Capstones.LuaLib.LuaCoreLib;
using lual = Capstones.LuaLib.LuaAuxLib;
using luae = Capstones.LuaLib.LuaLibEx;

namespace Capstones.LuaWrap
{
    public struct LuaStateRecover : IDisposable
    {
        private IntPtr _l;
        private int _top;

        public int Top
        {
            get { return _top; }
        }

        public LuaStateRecover(IntPtr l)
        {
            _l = l;
            _top = l.gettop(); // this is dangerous, but the user should call it with available "l".
        }

        public void Dispose()
        {
            int top = _l.gettop();
            if (top < _top)
            {
                _l.LogWarning("lua stack top is lower than the prev top, there may be some mistake!");
            }
            else
            {
                _l.settop(_top);
            }
        }
    }

    public class BaseLua : ScriptDynamic, IDisposable
    {
        protected internal IntPtr _L;
        protected internal LuaRef Ref
        {
            get
            {
                return _Binding as LuaRef;
            }
            set
            {
                _Binding = value;
            }
        }
        public IntPtr L
        {
            get { return _L; }
            internal protected set
            {
                _L = value;
                if (Ref != null)
                {
                    Ref.L = value;
                }
            }
        }
        public override int Refid
        {
            get
            {
                if (Ref == null)
                {
                    return 0;
                }
                else
                {
                    return Ref.Refid;
                }
            }
            protected internal set
            {
                if (value == 0)
                {
                    if (Ref != null)
                    {
                        Ref.Refid = 0;
                        Ref.Dispose();
                        Ref = null;
                    }
                }
                else
                {
                    if (Ref == null)
                    {
                        Ref = new LuaRef();
                        Ref.L = _L;
                    }
                    Ref.Refid = value;
                }
            }
        }

        public override string ToString()
        {
            return "LuaRef:" + Refid.ToString();
        }
        public override bool Equals(object obj)
        {
            if (obj is BaseLua)
            {
                return Refid == ((BaseLua)obj).Refid && L == ((BaseLua)obj).L;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Refid ^ L.GetHashCode();
        }

        public BaseLua() { }
        public BaseLua(IntPtr l, int refid)
        {
            L = l;
            Refid = refid;
        }
        public virtual void Dispose()
        {
            if (Ref != null)
            {
                Ref.Dispose();
                Ref = null;
            }
        }

        protected internal override object ConvertBinding(Type type)
        {
            if (type == typeof(bool))
            {
                if (L != IntPtr.Zero && Refid != 0)
                {
                    L.getref(Refid);
                    bool rv = !L.isnoneornil(-1) && !(L.isboolean(-1) && !L.toboolean(-1));
                    L.pop(1);
                    return rv;
                }
                return false;
            }
            if (GLog.IsLogInfoEnabled) GLog.LogInfo("__convert(" + type.ToString() + ") meta-method Not Implemented.");
            return null;
        }
    }

    public class BaseLuaOnStack : BaseLua
    {
        public override string ToString()
        {
            return "LuaOnStack:" + StackPos.ToString();
        }
        public override bool Equals(object obj)
        {
            if (obj is BaseLuaOnStack)
            {
                return StackPos == ((BaseLuaOnStack)obj).StackPos && L == ((BaseLuaOnStack)obj).L;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return StackPos ^ L.GetHashCode();
        }
        public override int Refid
        {
            get
            {
                return 0;
            }
            protected internal set
            {
            }
        }

        public virtual int StackPos
        {
            get
            {
                if (_Binding == null)
                    return 0;
                if (_Binding is int)
                    return (int)_Binding;
                return 0;
            }
            protected internal set
            {
                _Binding = value;
            }
        }

        protected internal override object ConvertBinding(Type type)
        {
            if (type == typeof(bool))
            {
                if (L != IntPtr.Zero && StackPos != 0)
                {
                    bool rv = !L.isnoneornil(StackPos) && !(L.isboolean(StackPos) && !L.toboolean(StackPos));
                    return rv;
                }
                return false;
            }
            if (GLog.IsLogInfoEnabled) GLog.LogInfo("__convert(" + type.ToString() + ") meta-method Not Implemented.");
            return null;
        }
    }

    public static class LuaTransHelper
    {
        public static object GetLua(this IntPtr l, int index)
        {
            if (l != IntPtr.Zero)
            {
                using (var lr = new LuaStateRecover(l))
                {
                    l.pushvalue(index);
                    int typecode = l.type(-1);
                    switch (typecode)
                    {
                        case lua.LUA_TBOOLEAN:
                            return l.toboolean(-1);
                        case lua.LUA_TFUNCTION:
                            //if (l.iscfunction(-1))
                            //    return l.tocfunction(-1);
                            //else
                                return new LuaFunc(l, -1);
                        case lua.LUA_TLIGHTUSERDATA:
                            return l.touserdata(-1);
                        case lua.LUA_TUSERDATA:
                            return l.GetUserDataRaw(-1);
                        case lua.LUA_TNIL:
                            return null;
                        case lua.LUA_TNONE:
                            return null;
                        case lua.LUA_TNUMBER:
                            double num = l.tonumber(-1);
                            //int inum = (int)num;
                            //if (num == (double)inum)
                            //{
                            //    return inum;
                            //}
                            //else
                            {
                                return num;
                            }
                        case lua.LUA_TSTRING:
                            //return l.tostring(-1);
                            return l.GetString(-1);
                        case lua.LUA_TTABLE:
                            return new LuaTable(l, -1);
                        case lua.LUA_TTHREAD:
                            IntPtr lthd = l.tothread(-1);
                            int refid = l.refer();
                            return new LuaOnStackThread(lthd, refid);
                    }
                }
            }
            return null;
        }

        public static void PushLua(this IntPtr l, object val)
        {
            if (l != IntPtr.Zero)
            {
                object raw = val.UnwrapDynamic();
                BaseDynamic obj = val as BaseDynamic;
                if (obj is LuaOnStackUserData)
                {
                    l.PushUserData(obj);
                }
                else if (obj is LuaState)
                {
                    ((LuaState)obj).L.pushthread();
                }
                else if (obj is BaseLuaOnStack)
                {
                    l.pushvalue(((BaseLuaOnStack)obj).StackPos);
                }
                else if (obj is BaseLua)
                {
                    l.getref(((BaseLua)obj).Refid);
                }
                //else if (raw is lua.CFunction)
                //{

                //}
                else if (raw == null)
                {
                    l.pushnil();
                }
                else if (raw is bool)
                {
                    l.pushboolean((bool)raw);
                }
                else if (raw is string)
                {
                    //l.pushstring((string)raw);
                    l.PushString((string)raw);
                }
                else if (raw is byte[])
                {
                    l.pushbuffer((byte[])raw);
                }
                else if (raw is IntPtr)
                {
                    l.pushlightuserdata((IntPtr)raw);
                }
                else if (raw is Enum)
                {
                    l.PushUserData(raw.WrapDynamic());
                }
                else if (raw.IsObjIConvertible())
                {
                    l.pushnumber(Convert.ToDouble(raw));
                }
                else
                {
                    l.PushUserData(raw.WrapDynamic());
                }
            }
        }

        public static object GetLuaOnStack(this IntPtr l, int index)
        {
            if (l != IntPtr.Zero)
            {
                var top = l.gettop();
                if (index < 0 && -index <= top)
                {
                    index = top + 1 + index;
                }
                int typecode = l.type(index);
                switch (typecode)
                {
                    case lua.LUA_TBOOLEAN:
                        return l.toboolean(index);
                    case lua.LUA_TFUNCTION:
                        //if (l.iscfunction(index))
                        //    return l.tocfunction(index);
                        //else
                        return new LuaOnStackFunc(l, index);
                    case lua.LUA_TLIGHTUSERDATA:
                        return l.touserdata(index);
                    case lua.LUA_TUSERDATA:
                        return l.GetUserDataRaw(index);
                    case lua.LUA_TNIL:
                        return null;
                    case lua.LUA_TNONE:
                        return null;
                    case lua.LUA_TNUMBER:
                        double num = l.tonumber(index);
                        //int inum = (int)num;
                        //if (num == (double)inum)
                        //{
                        //    return inum;
                        //}
                        //else
                        {
                            return num;
                        }
                    case lua.LUA_TSTRING:
                        //return l.tostring(-1);
                        return l.GetString(index);
                    case lua.LUA_TTABLE:
                        return new LuaOnStackTable(l, index);
                    case lua.LUA_TTHREAD:
                        l.pushvalue(index);
                        IntPtr lthd = l.tothread(-1);
                        int refid = l.refer();
                        return new LuaOnStackThread(lthd, refid);
                }
            }
            return null;
        }

        private class LuaStringCache
        {
            public const int InternVisitCount = 100;
            public const int CacheMaxCount = 10000;
            public const int CachedStringMaxLen = 100;
            public static readonly byte[] LuaRefEntry = { 1, 255, 0 };

            public IntPtr L = IntPtr.Zero;
            public int LastId = 0;
            public LinkedList<LuaCachedStringInfo> CacheList = new LinkedList<LuaCachedStringInfo>();
            public Dictionary<string, LuaCachedStringInfo> CacheMap = new Dictionary<string, LuaCachedStringInfo>();
            public Dictionary<int, LuaCachedStringInfo> CacheRevMap = new Dictionary<int, LuaCachedStringInfo>();
            public LinkedListNode<LuaCachedStringInfo>[] CacheIndexStartNode = new LinkedListNode<LuaCachedStringInfo>[InternVisitCount];

            public class LuaCachedStringInfo
            {
                public LuaStringCache Cache;
                public string Str;
                //public byte[] Coded;
                public int Id;
                public LinkedListNode<LuaCachedStringInfo> Node;
                public int VisitCount;
                public bool IsInterned;

                public void Intern()
                {
                    if (!IsInterned)
                    {
                        IsInterned = true;
                        Str = string.Intern(Str);

                        if (Node != null)
                        {
                            Cache.RemoveListNode(Node);
                            Node = null;
                        }
                    }
                }
                public void AddVisitCount()
                {
                    if (Node != null)
                    {
                        Cache.RemoveListNode(Node);
                    }
                    ++VisitCount;
                    if (!IsInterned)
                    {
                        if (VisitCount >= InternVisitCount)
                        {
                            Node = null;
                            Intern();
                        }
                    }
                    if (Node != null)
                    {
                        if (Cache.CacheIndexStartNode[VisitCount] != null)
                        {
                            Node = Cache.CacheList.AddBefore(Cache.CacheIndexStartNode[VisitCount], this);
                            Cache.CacheIndexStartNode[VisitCount] = Node;
                        }
                        else
                        {
                            int vi = VisitCount - 1;
                            for (; vi >= 0; --vi)
                            {
                                if (Cache.CacheIndexStartNode[vi] != null)
                                {
                                    break;
                                }
                            }
                            if (vi >= 0)
                            {
                                Node = Cache.CacheList.AddBefore(Cache.CacheIndexStartNode[vi], this);
                                Cache.CacheIndexStartNode[VisitCount] = Node;
                            }
                            else
                            {
                                Node = Cache.CacheList.AddLast(this);
                                Cache.CacheIndexStartNode[VisitCount] = Node;
                            }
                        }
                    }
                }
            }

            private void RemoveListNode(LinkedListNode<LuaCachedStringInfo> node)
            {
                if (node != null)
                {
                    var info = node.Value;
                    if (CacheIndexStartNode[info.VisitCount] == node)
                    {
                        CacheIndexStartNode[info.VisitCount] = null;
                        var next = node.Next;
                        if (next != null)
                        {
                            if (next.Value.VisitCount == info.VisitCount)
                            {
                                CacheIndexStartNode[info.VisitCount] = next;
                            }
                        }
                    }
                    CacheList.Remove(node);
                }
            }

            public bool TryGetCacheInfo(string val, out LuaCachedStringInfo info)
            {
                var found = CacheMap.TryGetValue(val, out info);
                if (found)
                {
                    info.AddVisitCount();
                }
                return found;
            }
            public bool TryGetCacheInfo(int id, out LuaCachedStringInfo info)
            {
                var found = CacheRevMap.TryGetValue(id, out info);
                if (found)
                {
                    info.AddVisitCount();
                }
                return found;
            }
            public LuaCachedStringInfo PutIntoCache(string str)
            {
                if (str == null)
                {
                    return null;
                }
                LuaCachedStringInfo rv;
                if (TryGetCacheInfo(str, out rv))
                {
                    return rv;
                }
                if (str.Length > CachedStringMaxLen)
                {
                    return null;
                }
                rv = new LuaCachedStringInfo();
                rv.Str = str;
                rv.Cache = this;
                rv.Id = ++LastId;

                if (string.IsInterned(str) != null)
                {
                    rv.IsInterned = true;
                }
                else
                {
                    if (CacheList.Count >= CacheMaxCount)
                    {
                        var last = CacheList.Last.Value;
                        RemoveFromCache(last);
                    }
                    rv.Node = CacheList.AddLast(rv);
                }
                CacheMap[str] = rv;
                CacheRevMap[rv.Id] = rv;
                rv.AddVisitCount();

                var id = rv.Id;
                var l = L;
                using (var lr = new LuaStateRecover(l))
                {
                    l.pushbuffer(LuaStringCache.LuaRefEntry); // rkey
                    l.gettable(lua.LUA_REGISTRYINDEX); // reg
                    if (l.istable(-1))
                    {
                        l.pushnumber(1); // reg 1
                        l.gettable(-2); // reg map
                        if (!l.istable(-1))
                        {
                            l.pop(1); // reg
                            l.newtable(); // reg map
                            l.pushnumber(1); // reg map 1
                            l.pushvalue(-2); // reg map 1 map
                            l.settable(-4); // reg map
                        }
                        l.pushnumber(2); // reg map 2
                        l.gettable(-3); // reg map revmap
                        if (!l.istable(-1))
                        {
                            l.pop(1); // reg map
                            l.newtable(); // reg map revmap
                            l.pushnumber(2); // reg map revmap 2
                            l.pushvalue(-2); // reg map revmap 2 revmap
                            l.settable(-5); // reg map revmap
                        }

                        l.pushnumber(id); // reg map revmap id
                        l.pushstring(str); // reg map revmap id str
                        l.pushvalue(-1); // reg map revmap id str str
                        l.pushvalue(-3); // reg map revmap id str str id
                        l.settable(-5); // reg map revmap id str
                        l.settable(-4); // reg map revmap
                    }
                }

                return rv;
            }
            public void RemoveFromCache(LuaCachedStringInfo info)
            {
                if (info.Node != null)
                {
                    RemoveListNode(info.Node);
                    info.Node = null;
                }
                CacheMap.Remove(info.Str);
                CacheRevMap.Remove(info.Id);

                var id = info.Id;
                var l = L;
                using (var lr = new LuaStateRecover(l))
                {
                    l.pushbuffer(LuaStringCache.LuaRefEntry); // rkey
                    l.gettable(lua.LUA_REGISTRYINDEX); // reg
                    if (l.istable(-1))
                    {
                        l.pushnumber(1); // reg 1
                        l.gettable(-2); // reg map
                        l.pushnumber(2); // reg map 2
                        l.gettable(-3); // reg map revmap
                        if (l.istable(-2))
                        {
                            l.pushnumber(id); // reg map revmap id
                            l.pushvalue(-1); // reg map revmap id id
                            l.gettable(-4); // reg map revmap id str
                            l.pushvalue(-2); // reg map revmap id str id
                            l.pushnil(); // reg map revmap id str id nil
                            l.settable(-6); // reg map revmap id str
                            if (l.isstring(-1) && l.istable(-3))
                            {
                                l.pushnil(); // reg map revmap id str nil
                                l.settable(-4); // reg map revmap id
                            }
                        }
                    }
                }
            }
        }

        public static void PushString(this IntPtr l, string str)
        {
            l.pushbuffer(LuaStringCache.LuaRefEntry); // rkey
            l.gettable(lua.LUA_REGISTRYINDEX); // reg
            if (!l.istable(-1))
            {
                l.pop(1); // X
                l.newtable(); // reg
                l.pushbuffer(LuaStringCache.LuaRefEntry); // reg rkey
                l.pushvalue(-2); // reg rkey reg
                l.settable(lua.LUA_REGISTRYINDEX);
            }

            l.pushnumber(0); // reg 0
            l.gettable(-2); // reg cache
            var cache = l.GetRawObj(-1) as LuaStringCache;
            l.pop(1); // reg
            if (cache == null)
            {
                l.pushnumber(0); // reg 0
                cache = new LuaStringCache() { L = l };
                l.PushRawObj(cache); // reg 0 cache
                l.settable(-3); // reg
            }
            else
            {
                cache.L = l;
            }

            var info = cache.PutIntoCache(str);
            if (info == null)
            {
                l.pop(1); // X
                l.pushstring(str); // str
            }
            else
            {
                l.pushnumber(1); // reg 1
                l.gettable(-2); // reg map
                if (!l.istable(-1))
                {
                    l.pop(1); // reg
                    l.newtable(); // reg map
                    l.pushnumber(1); // reg map 1
                    l.pushvalue(-2); // reg map 1 map
                    l.settable(-4); // reg map
                }

                l.pushnumber(info.Id); // reg map id
                l.gettable(-2); // reg map str
                if (l.isstring(-1))
                {
                    l.insert(-3); // str reg map
                    l.pop(2); // str
                }
                else
                {
                    l.pop(3); // X
                    l.pushstring(str); // str

                    //// this should not happen!
                    //l.pop(1); // reg map
                    //l.pushnumber(2); // reg map 2
                    //l.gettable(-3); // reg map revmap
                    //if (!l.istable(-1))
                    //{
                    //    l.pop(1); // reg map
                    //    l.newtable(); // reg map revmap
                    //    l.pushnumber(2); // reg map revmap 2
                    //    l.pushvalue(-2); // reg map revmap 2 revmap
                    //    l.settable(-5); // reg map revmap
                    //}

                    //l.pushstring(str); // reg map revmap str
                    //l.pushnumber(info.Id); // reg map revmap str id
                    //l.pushvalue(-2); // reg map revmap str id str
                    //l.pushvalue(-1); // reg map revmap str id str str
                    //l.pushvalue(-3); // reg map revmap str id str str id
                    //l.settable(-6); // reg map revmap str id str
                    //l.settable(-5); // reg map revmap str
                    //l.insert(-4); // str reg map revmap
                    //l.pop(3); // str
                }
            }
        }

        public static string GetString(this IntPtr l, int index)
        {
            string rv = null;
            using (var lr = new LuaStateRecover(l))
            {
                if (l.isstring(index))
                {
                    l.pushvalue(index); // lstr
                    l.pushbuffer(LuaStringCache.LuaRefEntry); // lstr rkey
                    l.gettable(lua.LUA_REGISTRYINDEX); // lstr reg
                    if (l.istable(-1))
                    {
                        l.pushnumber(2); // lstr reg 2
                        l.gettable(-2); // lstr reg revmap
                        
                        if (l.istable(-1))
                        {
                            l.pushvalue(-3); // lstr reg revmap lstr
                            l.gettable(-2); // lstr reg revmap id

                            if (l.isnumber(-1))
                            {
                                var id = (int)l.tonumber(-1);
                                if (id != 0)
                                {
                                    l.pushnumber(0); // lstr reg revmap id 0
                                    l.gettable(-4); // lstr reg revmap id cache
                                    var cache = l.GetRawObj(-1) as LuaStringCache;
                                    if (cache != null)
                                    {
                                        cache.L = l;
                                        LuaStringCache.LuaCachedStringInfo info;
                                        if (cache.TryGetCacheInfo(id, out info))
                                        {
                                            return info.Str;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (l.isstring(index))
            {
                rv = l.tostring(index);

                l.pushbuffer(LuaStringCache.LuaRefEntry); // rkey
                l.gettable(lua.LUA_REGISTRYINDEX); // reg
                if (!l.istable(-1))
                {
                    l.pop(1); // X
                    l.newtable(); // reg
                    l.pushbuffer(LuaStringCache.LuaRefEntry); // reg rkey
                    l.pushvalue(-2); // reg rkey reg
                    l.settable(lua.LUA_REGISTRYINDEX);
                }

                l.pushnumber(0); // reg 0
                l.gettable(-2); // reg cache
                var cache = l.GetUserDataRaw(-1) as LuaStringCache;
                l.pop(1); // reg
                if (cache == null)
                {
                    l.pushnumber(0); // reg 0
                    cache = new LuaStringCache() { L = l };
                    l.PushRawObj(cache); // reg 0 cache
                    l.settable(-3); // reg
                }
                else
                {
                    cache.L = l;
                }
                l.pop(1); // X
                cache.PutIntoCache(rv);
            }
            return rv;
        }

        public static void GetField(this IntPtr l, int index, string key)
        {
            var top = l.gettop();
            if (index < 0 && -index <= top)
            {
                index = top + 1 + index;
            }

            l.PushString(key);
            l.gettable(index);
        }

        public static void SetField(this IntPtr l, int index, string key)
        {
            var top = l.gettop();
            if (index < 0 && -index <= top)
            {
                index = top + 1 + index;
            }

            l.PushString(key);
            l.insert(-2);
            l.settable(index);
        }

        public static void GetGlobal(this IntPtr l, string key)
        {
            GetField(l, lua.LUA_GLOBALSINDEX, key);
        }

        public static void SetGlobal(this IntPtr l, string key)
        {
            SetField(l, lua.LUA_GLOBALSINDEX, key);
        }

        public static bool IsString(this IntPtr l, int index)
        {
            return l.type(index) == LuaCoreLib.LUA_TSTRING;
        }

        public static bool IsNumber(this IntPtr l, int index)
        {
            return l.type(index) == LuaCoreLib.LUA_TNUMBER;
        }
    }
}