using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Capstones.Dynamic;
using Capstones.LuaLib;

using lua = Capstones.LuaLib.LuaCoreLib;
using lual = Capstones.LuaLib.LuaAuxLib;
using luae = Capstones.LuaLib.LuaLibEx;

namespace Capstones.LuaWrap
{
    public class LuaOnStackUserData : BaseLua, IExpando
    {
        #region Core
        protected internal class LuaUserDataCore
        {
#if ENABLE_OBJ_POOL
            protected internal class LuaUserDataCorePool : ObjectPool.GenericInstancePool<LuaUserDataCore>
            {
                public LuaUserDataCore GetFromPool()
                {
                    var core = this.TryGetFromPool();
                    if (object.ReferenceEquals(core, null))
                    {
                        core = new LuaUserDataCore();
                    }
                    else
                    {
                        core.Init();
                    }
                    return core;
                }
            }
            [ThreadStatic] protected internal static LuaUserDataCorePool _Pool;
            protected internal static LuaUserDataCorePool Pool
            {
                get
                {
                    if (_Pool == null)
                    {
                        _Pool = new LuaUserDataCorePool();
                    }
                    return _Pool;
                }
            }
#endif
            protected internal static LuaUserDataCore GetFromPool()
            {
#if ENABLE_OBJ_POOL
                return Pool.GetFromPool();
#else
                return new LuaUserDataCore();
#endif
            }

            public void ReturnToPool()
            {
                ClearRef();
#if ENABLE_OBJ_POOL
                Pool.ReturnToPool(this);
#endif
            }
            protected internal void ClearRef()
            {
                if (_Raw != null)
                {
                    if (_ObjToCoreMap != null)
                    {
                        _ObjToCoreMap.Remove(_Raw);
                    }
                    _Raw = null;
                }
                if (!object.ReferenceEquals(_Core, null))
                {
                    _Core.ReturnToPool();
                    _Core = null;
                }
            }
            public int AddRef()
            {
                int rv;
                rv = ++RefCnt;
                return rv;
            }
            public int Release()
            {
                int rv;
                rv = --RefCnt;
                if (rv == 0)
                {
                    ReturnToPool();
                }
                return rv;
            }

            protected internal static int _NextUdRefid = 1;
            protected internal int _UdRefid;
            protected internal BaseDynamic _Core;
            protected internal object _Raw;
            protected internal int RefCnt = 1;

            protected internal LuaUserDataCore()
            {
                Init();
            }
            protected internal LuaUserDataCore(object obj)
            {
                Init(obj);
            }
            protected internal void Init()
            {
                _Raw = null;
                _Core = null;
                RefCnt = 1;
                _UdRefid = System.Threading.Interlocked.Increment(ref _NextUdRefid);
            }
            protected internal void Init(object obj)
            {
                Init();
                SetTarget(obj);
            }
            protected internal void SetTarget(object obj)
            {
                //_Raw = obj; // _Raw is not null means the obj is in the cache
                _Core = obj.WrapDynamic();
            }
        }
#endregion

#region Map
        [ThreadStatic] protected internal static Dictionary<object, LuaUserDataCore> _ObjToCoreMap;
        protected internal static Dictionary<object, LuaUserDataCore> ObjToCoreMap
        {
            get
            {
                if (_ObjToCoreMap == null)
                {
                    _ObjToCoreMap = new Dictionary<object, LuaUserDataCore>();
                }
                return _ObjToCoreMap;
            }
        }

        protected internal static LuaUserDataCore GetUdCore(object obj)
        {
            bool shouldCache = !(obj is ValueType || obj is BaseDynamic);
            if (shouldCache)
            {
                LuaUserDataCore core;
                var key = obj;
                var map = ObjToCoreMap;
                map.TryGetValue(key, out core);
                if (core != null)
                {
                    core.AddRef();
                    return core;
                }

                core = LuaUserDataCore.GetFromPool();
                core.SetTarget(obj);
                core._Raw = key;
                map[key] = core;
                return core;
            }
            else
            {
                LuaUserDataCore core = LuaUserDataCore.GetFromPool();
                core.SetTarget(obj);
                return core;
            }
        }

        protected internal static void ShrinkUdCache()
        {
            if (_ObjToCoreMap != null)
            {
                List<LuaUserDataCore> uselessCore = new List<LuaUserDataCore>();
                foreach (var kvpCache in _ObjToCoreMap)
                {
                    if (kvpCache.Key == null)
                    {
                        uselessCore.Add(kvpCache.Value);
                    }
                    else
                    {
                        if (kvpCache.Key is UnityEngine.Object)
                        {
                            var obj = kvpCache.Key as UnityEngine.Object;
                            if (obj == null)
                            {
                                uselessCore.Add(kvpCache.Value);
                            }
                        }
                    }
                }
                foreach (var core in uselessCore)
                {
                    // dangerous!
                    //core.RefCnt = 0;
                    //core.ReturnToPool();
                    core.ClearRef();
                }
            }
        }
#endregion

        protected internal LuaUserDataCore _UdCore;

#region IExpando
        public BaseDynamic Core
        {
            get
            {
                if (_UdCore != null && !object.ReferenceEquals(_UdCore._Core, null))
                {
                    return _UdCore._Core;
                }
                return BaseDynamic.Empty;
            }
        }
        public IFieldsProvider Extra
        {
            get
            {
                if (_L != IntPtr.Zero)
                {
                    var lr = new LuaStateRecover(_L);
                    PushToLua(); // ud
                    if (_L.gettop() == lr.Top + 1 && _L.isuserdata(-1))
                    {
                        _L.getfenv(-1); // ud, ex
                        //_L.remove(-2); // ex
                        return new UdExTableFieldsProvider(new LuaOnStackRawTable(_L, _L.gettop())) { lr = lr };
                    }
                }
                return null;
            }
        }

        internal class UdExTableFieldsProvider : Capstones.LuaWrap.LuaOnStackTable.LuaTableFieldsProvider
        {
            public UdExTableFieldsProvider(BaseLuaOnStack tab) : base(tab)
            { }
            internal LuaStateRecover lr;
            public override void Dispose()
            {
                if (lr.Top > 0)
                {
                    lr.Dispose();
                }
            }
        }
#endregion

#region overrides
        public override object BinaryOp(string op, object other)
        {
            return Core.BinaryOp(op, other);
        }
        public override object[] Call(params object[] args)
        {
            return Core.Call(args);
        }
        protected internal override object ConvertBinding(Type type)
        {
            return Core.ConvertBinding(type);
        }
        public override bool Equals(object obj)
        {
            return Core.Equals(obj);
        }
        protected internal override object GetFieldImp(object key)
        {
            if (_L != IntPtr.Zero)
            {
                using (var lr = new LuaStateRecover(_L))
                {
                    PushToLua();
                    if (_L.gettop() == lr.Top + 1 && _L.isuserdata(-1))
                    {
                        _L.PushLua(key);
                        _L.gettable(-2);
                        return _L.GetLua(-1);
                    }
                }
            }
            return null;
        }
        public override int GetHashCode()
        {
            return Core.GetHashCode();
        }
        protected internal override bool SetFieldImp(object key, object val)
        {
            if (_L != IntPtr.Zero)
            {
                using (var lr = new LuaStateRecover(_L))
                {
                    PushToLua();
                    if (_L.gettop() == lr.Top + 1 && _L.isuserdata(-1))
                    {
                        _L.PushLua(key);
                        _L.PushLua(val);
                        _L.settable(-3);
                        return true;
                    }
                }
            }
            return false;
        }
        public override string ToString()
        {
            return Core.ToString();
        }
        public override object UnaryOp(string op)
        {
            return Core.UnaryOp(op);
        }
#endregion

        protected internal LuaOnStackUserData(IntPtr l, object obj)
        {
            Init(l, obj);
        }
        protected internal LuaOnStackUserData(IntPtr l, LuaOnStackUserData ud)
        {
            L = l;
            if (ud != null && ud._UdCore != null)
            {
                _UdCore = ud._UdCore;
            }
        }
        protected internal LuaOnStackUserData(IntPtr l, LuaUserDataCore core)
        {
            Init(l, core);
        }
        protected internal LuaOnStackUserData()
        {
        }
        protected internal void Init(IntPtr l, object obj)
        {
            Refid = 0;
            var robj = obj.UnwrapDynamic();
            if (robj == null && obj != null)
            {
                robj = obj;
            }
            L = l;
            if (obj is LuaOnStackUserData)
            {
                LuaOnStackUserData ud = (LuaOnStackUserData)obj;
                if (ud != null && ud._UdCore != null)
                {
                    _UdCore = ud._UdCore;
                }
            }
            else
            {
                _UdCore = GetUdCore(robj);
            }
        }
        protected internal void Init(IntPtr l, LuaUserDataCore core)
        {
            L = l;
            if (core != null)
            {
                _UdCore = core;
            }
        }

        protected internal virtual bool PushToLua()
        {
            if (_L != IntPtr.Zero)
            {
                if (_UdCore != null)
                {
                    _L.PrepareUserDataReg();
                    var exists = GetExistingUd();
                    if (exists)
                    {
                        return exists;
                    }

                    // reg
                    _L.pushnumber(_UdCore._UdRefid); // reg, id
                    IntPtr pud = CreateExtraTable(); // reg, id, ud
                    var handle = GCHandle.Alloc(this);
                    Marshal.WriteIntPtr(pud, (IntPtr)handle);
                    CreateMetaTable();

                    // reg, id, ud
                    _L.pushvalue(-1); // reg, id, ud, ud
                    _L.insert(-4); // ud, reg, id, ud
                    _L.settable(-3); // ud, reg
                    _L.pop(1); // ud

                    return true;
                }
                else
                {
                    _L.pushnil();
                    return false;
                }
            }
            return false;
        }

        protected bool GetExistingUd()
        {
            _L.GetField(lua.LUA_REGISTRYINDEX, "___udreg"); // reg

            _L.pushnumber(_UdCore._UdRefid); // reg, id
            _L.gettable(-2); // reg, ud
            if (_L.isuserdata(-1))
            {
                LuaOnStackUserData ud = _L.GetUserData(-1);
                if (ud != null)
                {
                    _L.remove(-2); // ud
                    return true;
                }
            }
            _L.pop(1); // reg
            return false;
        }

        protected IntPtr CreateExtraTable()
        {
            IntPtr pud = _L.newuserdata(new IntPtr(Marshal.SizeOf(typeof(IntPtr)))); // ud
            _L.newtable(); // ud, ex
            //_L.pushnumber(1); // ud, ex, type
            //_L.SetField(-2, "___udtype"); // ud, ex
            _L.setfenv(-2); // ud
            return pud;
        }

        protected void CreateMetaTable()
        {
            // ud
            _L.PushUserDataMeta(); // ud, meta
            _L.setmetatable(-2); // ud
        }

#region Helper Functions
        public object[] CallSelf(string funcName, params object[] args)
        {
            LuaFunc func = this[funcName] as LuaFunc;
            if (func != null)
            {
                return func.Call(args);
            }
            return null;
        }

        public T CallSelf<T>(string funcName, params object[] args)
        {
            return CallSelf(funcName, args).UnwrapReturnValues<T>();
        }

        public object RawGet(object key)
        {
            object val = null;
            if (Core != null)
            {
                val = Core[key];
            }
            return val;
        }

        public bool RawSet(object key, object val)
        {
            if (Core != null)
            {
                return Core.SetFieldImp(key, val);
            }
            return false;
        }
        #endregion

#if ENABLE_OBJ_POOL
        [ThreadStatic] protected internal static ObjectPool.GenericInstancePool<LuaOnStackUserData> _Pool;
        protected internal static ObjectPool.GenericInstancePool<LuaOnStackUserData> Pool
        {
            get
            {
                if (_Pool == null)
                {
                    _Pool = new ObjectPool.GenericInstancePool<LuaOnStackUserData>();
                }
                return _Pool;
            }
        }
#endif
        public static LuaOnStackUserData GetFromPool(IntPtr l, object obj)
        {
#if ENABLE_OBJ_POOL
            return Pool.GetFromPool(() => new LuaOnStackUserData(l, obj), ud => ud.Init(l, obj));
#else
            return new LuaOnStackUserData(l, obj);
#endif
        }
        protected internal static LuaOnStackUserData GetFromPool(IntPtr l, LuaUserDataCore core)
        {
#if ENABLE_OBJ_POOL
            return Pool.GetFromPool(() => new LuaOnStackUserData(l, core), ud => ud.Init(l, core));
#else
            return new LuaOnStackUserData(l, core);
#endif
        }
        public override void ReturnToPool()
        {
            if (!object.ReferenceEquals(_UdCore, null))
            {
                _UdCore.Release();
                _UdCore = null;
            }
#if ENABLE_OBJ_POOL
            Pool.ReturnToPool(this);
#endif
        }
    }

    public class LuaOnStackUserDataShadow : LuaOnStackUserData
    {
        protected internal LuaOnStackUserDataShadow() { }
        protected internal LuaOnStackUserDataShadow(IntPtr l, object obj) : base(l, obj) { }
        protected internal LuaOnStackUserDataShadow(IntPtr l, LuaOnStackUserData ud) : base(l, ud) { }
        protected internal LuaOnStackUserDataShadow(IntPtr l, LuaUserDataCore core) : base(l, core) { }
        protected internal LuaOnStackUserDataShadow(IntPtr l, object obj, Type type)
        {
            Init(l, obj, type);
        }
        protected internal void Init(IntPtr l, object obj, Type type)
        {
            L = l;
            if (obj is LuaOnStackUserData
                && ((LuaOnStackUserData)obj)._UdCore != null
                && ((LuaOnStackUserData)obj)._UdCore._Core is ClrObjectWrapper
                && ((ClrObjectWrapper)((LuaOnStackUserData)obj)._UdCore._Core).BindingCore != null
                && ((ClrObjectWrapper)((LuaOnStackUserData)obj)._UdCore._Core).BindingCore.BindingType == type)
            {
                LuaOnStackUserData ud = (LuaOnStackUserData)obj;
                _UdCore = ud._UdCore;
            }
            else
            {
                _UdCore = LuaUserDataCore.GetFromPool();
                _UdCore._Core = ClrObjectWrapper.GetFromPool(obj, type);
            }
        }

#if ENABLE_OBJ_POOL
        [ThreadStatic] protected internal new static ObjectPool.GenericInstancePool<LuaOnStackUserDataShadow> _Pool;
        protected internal new static ObjectPool.GenericInstancePool<LuaOnStackUserDataShadow> Pool
        {
            get
            {
                if (_Pool == null)
                {
                    _Pool = new ObjectPool.GenericInstancePool<LuaOnStackUserDataShadow>();
                }
                return _Pool;
            }
        }
#endif
        public static LuaOnStackUserDataShadow GetFromPool(IntPtr l, object obj, Type type)
        {
#if ENABLE_OBJ_POOL
            return Pool.GetFromPool(() => new LuaOnStackUserDataShadow(l, obj, type), ud => ud.Init(l, obj, type));
#else
            return new LuaOnStackUserDataShadow(l, obj, type);
#endif
        }
        public override void ReturnToPool()
        {
            if (!object.ReferenceEquals(_UdCore, null))
            {
                _UdCore.Release();
                _UdCore = null;
            }
#if ENABLE_OBJ_POOL
            Pool.ReturnToPool(this);
#endif
        }
    }

    //public class LuaUserData : LuaOnStackUserData
    //{
    //    protected internal LuaUserData() { }
    //    public LuaUserData(IntPtr l, object obj) : base(l, obj) { }
    //    protected internal LuaUserData(IntPtr l, LuaOnStackUserData ud) : base(l, ud) { }
    //    protected internal LuaUserData(IntPtr l, LuaUserDataCore core) : base(l, core) { }
    //    protected internal LuaUserData(LuaOnStackUserData ud) : base(ud._L, ud) { }

    //    protected internal override bool PushToLua()
    //    {
    //        if (_L != IntPtr.Zero)
    //        {
    //            if (Refid != 0)
    //            {
    //                _L.getref(Refid);
    //                return true;
    //            }
    //            else if (_UdCore != null)
    //            {
    //                _L.PrepareUserDataReg();
    //                var exists = GetExistingUd();
    //                if (exists)
    //                {
    //                    // ud
    //                    _L.getfenv(-1); // ud, ex
    //                    _L.GetField(-1, "___udtype"); // ud, ex, type
    //                    var udtype = (int)_L.tonumber(-1);
    //                    _L.pop(1); // ud, ex
    //                    if (udtype != 2)
    //                    {
    //                        IntPtr pud = _L.touserdata(-2);
    //                        IntPtr hval = Marshal.ReadIntPtr(pud);
    //                        GCHandle handle = (GCHandle)hval;
    //                        var ud = handle.Target as LuaOnStackUserData;
    //                        if (!object.ReferenceEquals(ud, null))
    //                        {
    //                            ud.ReturnToPool();
    //                        }
    //                        handle.Free();
    //                        var wr = ObjectPool.WeakReferencePool.GetFromPool(() => new WeakReference(this), wr1 => wr1.Target = this);
    //                        handle = GCHandle.Alloc(wr);
    //                        Marshal.WriteIntPtr(pud, (IntPtr)handle); // make it weak.

    //                        _L.pushnumber(2); // ud, ex, type
    //                        _L.SetField(-2, "___udtype"); // ud, ex
    //                    }
    //                    _L.pop(1); // ud
    //                }
    //                else
    //                {
    //                    // reg
    //                    _L.pushnumber(_UdCore._UdRefid); // reg, id
    //                    IntPtr pud = CreateExtraTable(); // reg, id, ud
    //                    var wr = ObjectPool.WeakReferencePool.GetFromPool(() => new WeakReference(this), wr1 => wr1.Target = this);
    //                    var handle = GCHandle.Alloc(wr);
    //                    Marshal.WriteIntPtr(pud, (IntPtr)handle);
    //                    CreateMetaTable();

    //                    // reg, id, ud
    //                    _L.pushvalue(-1); // reg, id, ud, ud
    //                    _L.insert(-4); // ud, reg, id, ud
    //                    _L.settable(-3); // ud, reg
    //                    _L.pop(1); // ud

    //                    _L.getfenv(-1); // ud, ex
    //                    _L.pushnumber(2); // ud, ex, type
    //                    _L.SetField(-2, "___udtype"); // ud, ex
    //                    _L.pop(1); // ud
    //                }
    //                _L.pushvalue(-1); // ud, ud
    //                Refid = _L.refer(); // ud

    //                return true;
    //            }
    //            else
    //            {
    //                _L.pushnil();
    //                return false;
    //            }
    //        }
    //        return false;
    //    }

    //    [ThreadStatic] protected internal new static ObjectPool.GenericInstancePool<LuaUserData> _Pool;
    //    protected internal new static ObjectPool.GenericInstancePool<LuaUserData> Pool
    //    {
    //        get
    //        {
    //            if (_Pool == null)
    //            {
    //                _Pool = new ObjectPool.GenericInstancePool<LuaUserData>();
    //            }
    //            return _Pool;
    //        }
    //    }
    //    public override void ReturnToPool()
    //    {
    //    }
    //    public override void Dispose()
    //    {
    //        if (L != IntPtr.Zero)
    //        {
    //            if (Refid != 0)
    //            {
    //                var l = L;
    //                l.getref(Refid);
    //                if (l.isuserdata(-1))
    //                {
    //                    IntPtr pud = l.touserdata(-1);
    //                    IntPtr hval = Marshal.ReadIntPtr(pud);
    //                    try
    //                    {
    //                        GCHandle handle = (GCHandle)hval;
    //                        handle.Free();
    //                    }
    //                    catch { }
    //                    //l.newtable();
    //                    //l.setfenv(-2);
    //                    //Marshal.WriteIntPtr(pud, IntPtr.Zero);
    //                    l.getfenv(-1);
    //                    l.pushnumber(1);
    //                    l.SetField(-2, "___udtype");
    //                    l.pop(1);
    //                    var shadow = LuaOnStackUserData.GetFromPool(l, _UdCore);
    //                    var shandle = GCHandle.Alloc(shadow);
    //                    Marshal.WriteIntPtr(pud, (IntPtr)shandle);
    //                }
    //                l.pop(1);
    //            }
    //        }

    //        base.Dispose();
    //        //if (!object.ReferenceEquals(_UdCore, null))
    //        //{
    //        //    _UdCore.Release();
    //        //    _UdCore = null;
    //        //}
    //        Pool.ReturnToPool(this);
    //    }
    //    public static new LuaUserData GetFromPool(IntPtr l, object obj)
    //    {
    //        return Pool.GetFromPool(() => new LuaUserData(l, obj), ud => ud.Init(l, obj));
    //    }
    //}

    public static class LuaUserDataHelper
    {
        public static void PrepareUserDataReg(this IntPtr l)
        {
            //var regid = 0;
            if (l != IntPtr.Zero)
            {
                using (var lr = new LuaStateRecover(l))
                {
                    l.GetField(lua.LUA_REGISTRYINDEX, "___udreg");
                    if (!l.istable(-1))
                    {
                        //regid = _NextUdRegId++;
                        l.newtable(); // reg
                        l.newtable(); // reg meta
                        l.PushString("v"); // reg meta 'v'
                        l.SetField(-2, "__mode"); // reg meta
                        //l.pushnumber(regid); // reg meta id
                        //l.setfield(-2, "___regid"); // reg meta
                        l.setmetatable(-2); // reg
                        l.SetField(lua.LUA_REGISTRYINDEX, "___udreg"); // X
                    }
                    //else
                    //{
                    //    l.getmetatable(-1);
                    //    l.getfield(-1, "___regid");
                    //    regid = (int)l.tonumber(-1);
                    //}
                }
            }
            //return regid;
        }

        public static void ShrinkUserDataReg(this IntPtr l)
        {
            if (l != IntPtr.Zero)
            {
                using (var lr = new LuaStateRecover(l))
                {
                    l.GetField(lua.LUA_REGISTRYINDEX, "___udreg");
                    if (l.istable(-1))
                    {
                        l.pushnil();
                        while (l.next(-2))
                        {
                            if (l.isuserdata(-1))
                            {
                                IntPtr pud = l.touserdata(-1);
                                IntPtr hval = Marshal.ReadIntPtr(pud);

                                var raw = l.GetUserDataRaw(-1).UnwrapDynamic();
                                if ((raw is UnityEngine.Object) && ((UnityEngine.Object)raw) == null)
                                {
                                    try
                                    {
                                        GCHandle handle = (GCHandle)hval;
                                        handle.Free();
                                    }
                                    catch { }
                                    l.newtable();
                                    l.setfenv(-2);
                                    Marshal.WriteIntPtr(pud, IntPtr.Zero);
                                }
                            }
                            l.pop(1);
                        }
                    }
                }
            }
        }

        public static void PushUserDataMeta(this IntPtr l)
        {
            if (l != IntPtr.Zero)
            {
                l.GetField(lua.LUA_REGISTRYINDEX, "___udmeta");
                if (!l.istable(-1))
                {
                    l.pop(1);
                    l.newtable(); // meta
                    l.pushvalue(-1); // meta meta

                    l.pushcfunction(LuaUserDataHelper.FuncLuaIndex);
                    l.SetField(-2, "__index");
                    l.pushcfunction(LuaUserDataHelper.FuncLuaNewIndex);
                    l.SetField(-2, "__newindex");
                    l.pushcfunction(LuaUserDataHelper.FuncLuaGc);
                    l.SetField(-2, "__gc");
                    l.pushcfunction(LuaUserDataHelper.FuncLuaCall);
                    l.SetField(-2, "__call");
                    l.pushcfunction(LuaUserDataHelper.LuaMetaUnm);
                    l.SetField(-2, "__unm");
                    //l.pushcfunction(LuaUserDataHelper.FuncLuaLen);
                    //l.setfield(-2, "__len");
                    l.pushcfunction(LuaUserDataHelper.FuncLuaAdd);
                    l.SetField(-2, "__add");
                    l.pushcfunction(LuaUserDataHelper.FuncLuaSub);
                    l.SetField(-2, "__sub");
                    l.pushcfunction(LuaUserDataHelper.FuncLuaMul);
                    l.SetField(-2, "__mul");
                    l.pushcfunction(LuaUserDataHelper.FuncLuaDiv);
                    l.SetField(-2, "__div");
                    //l.pushcfunction(LuaUserDataHelper.FuncLuaMod); 
                    //l.setfield(-2, "__mod");
                    //l.pushcfunction(LuaUserDataHelper.FuncLuaPow); 
                    //l.setfield(-2, "__pow");
                    //l.pushcfunction(LuaUserDataHelper.FuncLuaCon); 
                    //l.setfield(-2, "__concat");
                    l.pushcfunction(LuaUserDataHelper.FuncLuaEq);
                    l.SetField(-2, "__eq");
                    //l.pushcfunction(LuaUserDataHelper.FuncLuaLt);
                    //l.setfield(-2, "__lt");
                    //l.pushcfunction(LuaUserDataHelper.FuncLuaLe);
                    //l.setfield(-2, "__le");

                    l.SetField(lua.LUA_REGISTRYINDEX, "___udmeta"); // meta
                }
            }
        }

        //internal static int GetUserDataType(this IntPtr l, int index)
        //{
        //    if (l != IntPtr.Zero)
        //    {
        //        if (l.isuserdata(index))
        //        {
        //            if (l.islightuserdata(index))
        //            {
        //                return 4;
        //            }
        //            else
        //            {
        //                int udtype = 0;
        //                IntPtr pud = l.touserdata(index);
        //                if (pud != IntPtr.Zero)
        //                {
        //                    l.getfenv(index);
        //                    if (l.istable(-1))
        //                    {
        //                        l.GetField(-1, "___udtype");
        //                        if (l.isnumber(-1))
        //                        {
        //                            udtype = (int)l.tonumber(-1);
        //                        }
        //                        l.pop(1);
        //                    }
        //                    l.pop(1);

        //                }
        //                return udtype;
        //            }
        //        }
        //    }
        //    return -1;
        //}

        public static object GetUserDataRaw(this IntPtr l, int index)
        {
            if (l != IntPtr.Zero)
            {
                if (l.isuserdata(index))
                {
                    if (l.islightuserdata(index))
                    {
                        var phandle = l.touserdata(index);
                        GCHandle handle = (GCHandle)phandle;
                        object ud = null;
                        try
                        {
                            ud = handle.Target;
                        }
                        catch { }
                        if (ud != null)
                        {
                            return ud;
                        }
                        else
                        {
                            return phandle;
                        }
                    }
                    else
                    {
                        IntPtr pud = l.touserdata(index);
                        if (pud != IntPtr.Zero)
                        {
                            IntPtr hval = Marshal.ReadIntPtr(pud);
                            object ud = hval;
                            try
                            {
                                GCHandle handle = (GCHandle)hval;
                                ud = handle.Target;
                                if (ud is WeakReference)
                                {
                                    var wr = ud as WeakReference;
                                    ud = wr.GetWeakReference<object>();
                                }
                            }
                            catch { }
                            return ud;
                        }
                    }
                }
            }
            return null;
        }
        public static LuaOnStackUserData GetUserData(this IntPtr l, int index)
        {
            return GetUserDataRaw(l, index) as LuaOnStackUserData;
        }

        public static LuaOnStackUserData PushUserData(this IntPtr l, object obj)
        {
            if (obj is LuaOnStackUserDataShadow)
            {
                if (((LuaOnStackUserDataShadow)obj)._L == l)
                {
                    var ud = (LuaOnStackUserDataShadow)obj;
                    ud.PushToLua();
                    return ud;
                }
                else
                {
                    var ud = LuaOnStackUserDataShadow.GetFromPool(l, obj);
                    ud.PushToLua();
                    return ud;
                }
            }
            else if (obj is LuaOnStackUserData && ((LuaOnStackUserData)obj)._L == l)
            {
                var ud = (LuaOnStackUserData)obj;
                ud.PushToLua();
                var real = l.GetUserData(-1);
                if (!Object.ReferenceEquals(real, ud))
                {
                    ud.ReturnToPool();
                    return real;
                }
                return ud;
            }
            else
            {
                var ud = LuaOnStackUserData.GetFromPool(l, obj);
                ud.PushToLua();
                var real = l.GetUserData(-1);
                if (!Object.ReferenceEquals(real, ud))
                {
                    ud.ReturnToPool();
                    return real;
                }
                return ud;
            }
        }

        public static LuaOnStackUserData PushUserDataOfType(this IntPtr l, object obj, Type type)
        {
            if (type == null)
                return PushUserData(l, obj);
            else
            {
                var robj = obj.UnwrapDynamic();
                if (robj != null && robj.GetType() == type)
                    return PushUserData(l, obj);
                else
                {
                    var ud = LuaOnStackUserDataShadow.GetFromPool(l, obj, type);
                    ud.PushToLua();
                    return ud;
                }
            }
        }

#region UserData MetaFuncs
        internal static readonly lua.CFunction FuncLuaIndex = new lua.CFunction(LuaMetaIndex);
        internal static readonly lua.CFunction FuncLuaNewIndex = new lua.CFunction(LuaMetaNewIndex);
        internal static readonly lua.CFunction FuncLuaCall = new lua.CFunction(LuaMetaCall);
        internal static readonly lua.CFunction FuncLuaGc = new lua.CFunction(LuaMetaGc);

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaGc(IntPtr l)
        {
            using (var lr = new LuaStateRecover(l))
            {
                var oldtop = l.gettop();
                if (oldtop < 1)
                    return 0;

                if (l.isuserdata(1))
                {
                    if (l.islightuserdata(1))
                    {

                    }
                    else
                    {
                        IntPtr pud = l.touserdata(1);
                        if (pud != IntPtr.Zero)
                        {
                            IntPtr hval = Marshal.ReadIntPtr(pud);
                            if (hval != IntPtr.Zero)
                            {
                                try
                                {
                                    GCHandle handle = (GCHandle)hval;
                                    try
                                    {
                                        object ud = handle.Target;
                                        if (ud is WeakReference)
                                        {
                                            var wr = ud as WeakReference;
                                            ud = wr.GetWeakReference<object>();
                                            wr.Target = null;
                                            ObjectPool.ReturnWeakReferenceToPool(wr);
                                        }
                                        if (ud is LuaOnStackUserData)
                                        {
                                            ((LuaOnStackUserData)ud).ReturnToPool();
                                        }
                                    }
                                    catch { }
                                    handle.Free();
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaIndex(IntPtr l)
        {
            var oldtop = l.gettop();
            switch (0)
            {
                default:
                    if (oldtop < 2)
                        break;
                    if (!l.isuserdata(1))
                        break;
                    LuaOnStackUserData ud = l.GetUserData(1);
                    if (Object.ReferenceEquals(ud, null))
                        break;

                    l.getfenv(1); // ud, key, ex
                    if (l.istable(-1))
                    {
                        l.pushvalue(2); // ud, key, ex, key
                        l.gettable(-2); // ud, key, ex, val
                        l.remove(-2); // ud, key, val
                        if (!l.isnoneornil(-1))
                        {
                            break;
                        }
                    }
                    l.pop(1); // ud, key

                    var key = l.GetLua(2);

                    object val = null;
                    var strkey = key.UnwrapDynamic<string>();

                    if (strkey != null && strkey.StartsWith("_"))
                    {
                        if ("___rawget" == strkey)
                        {
                            val = new Func<object, object>(ud.RawGet).WrapDelegate();
                        }
                        else if ("___rawset" == strkey)
                        {
                            val = new Func<object, object, bool>(ud.RawSet).WrapDelegate();
                        }
                    }
                    if (object.ReferenceEquals(val, null))
                    {
                        if (!Object.ReferenceEquals(ud.Core, null))
                        {
                            try
                            {
                                val = ud.Core[key];
                            }
                            catch (Exception e)
                            {
                                l.pushcfunction(Capstones.LuaExt.LuaFramework.ClrDelErrorHandler);
                                l.PushString(e.ToString());
                                l.pcall(1, 0, 0);
                            }
                        }
                    }

                    l.PushLua(val); // val
                    break;
            }

            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaNewIndex(IntPtr l)
        {
            using (var lr = new LuaStateRecover(l))
            {
                var oldtop = l.gettop();
                if (oldtop < 2)
                    return 0;
                if (!l.isuserdata(1))
                    return 0;
                LuaOnStackUserData ud = l.GetUserData(1);
                if (Object.ReferenceEquals(ud, null))
                    return 0;

                l.getfenv(1); // ud, key, val, ex
                if (l.istable(-1))
                {
                    l.pushvalue(2); // ud, key, val, ex, key
                    l.gettable(-2); // ud, key, val, ex, old
                    if (!l.isnoneornil(-1))
                    {
                        l.pop(1); // ud, key, val, ex
                        l.pushvalue(2); // ud, key, val, ex, key
                        if (oldtop >= 3)
                        {
                            l.pushvalue(3); // ud, key, val, ex, key, val
                        }
                        else
                        {
                            l.pushnil(); // ud, key, val, ex, key, val
                        }
                        l.settable(-3); // ud, key, val, ex
                        return 0;
                    }
                    l.pop(1); // ud, key, val, ex
                }
                l.pop(1); // ud, key, val

                var key = l.GetLua(2);

                object val = null;
                if (oldtop >= 3)
                {
                    val = l.GetLua(3);
                }

                if (!Object.ReferenceEquals(ud.Core, null))
                {
                    try
                    {
                        if (ud.Core.SetFieldImp(key, val))
                        {
                            return 0;
                        }
                    }
                    catch (Exception e)
                    {
                        l.pushcfunction(Capstones.LuaExt.LuaFramework.ClrDelErrorHandler);
                        l.PushString(e.ToString());
                        l.pcall(1, 0, 0);
                        return 0;
                    }
                }

                l.getfenv(1); // ud, key, val, ex
                if (l.istable(-1))
                {
                    l.pushvalue(2); // ud, key, val, ex, key
                    if (oldtop >= 3)
                    {
                        l.pushvalue(3); // ud, key, val, ex, key, val
                    }
                    else
                    {
                        l.pushnil(); // ud, key, val, ex, key, val
                    }
                    l.settable(-3); // ud, key, val, ex
                }
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaCall(IntPtr l)
        {
            var oldtop = l.gettop();
            if (oldtop >= 1)
            {
                LuaOnStackUserData ud = l.GetUserData(1);
                if (!object.ReferenceEquals(ud, null))
                {
                    var args = ObjectPool.GetParamsFromPool(oldtop - 1);
                    for (int i = 2; i <= oldtop; ++i)
                    {
                        var arg = l.GetLua(i);
                        args[i - 2] = arg;
                    }
                    try
                    {
                        var results = ud.Call(args);
                        ObjectPool.ReturnParamsToPool(args);
                        int cnt = 0;
                        if (results != null)
                        {
                            cnt = results.Length;
                            foreach (var result in results)
                            {
                                l.PushLua(result);
                            }
                        }
                        else
                        {
                            l.pushcfunction(Capstones.LuaExt.LuaFramework.ClrDelErrorHandler);
                            l.PushString("Unable To Call. See Details Below:");
                            l.pcall(1, 0, 0);
                        }
                        int diff = l.gettop() - oldtop - cnt;
                        if (diff > 0)
                        {
                            // the stack is not balance while doing the call!
                            for (int i = 0; i < diff; ++i)
                            {
                                l.remove(-cnt - 1);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        l.pushcfunction(Capstones.LuaExt.LuaFramework.ClrDelErrorHandler);
                        l.PushString(e.ToString());
                        l.pcall(1, 0, 0);
                    }
                }
            }
            return l.gettop() - oldtop;
        }
#endregion

#region UserData MetaFuncs For Operators
        internal static readonly lua.CFunction FuncLuaUnm = new lua.CFunction(LuaMetaUnm);
        internal static readonly lua.CFunction FuncLuaLen = new lua.CFunction(LuaMetaLen);
        internal static readonly lua.CFunction FuncLuaAdd = new lua.CFunction(LuaMetaAdd);
        internal static readonly lua.CFunction FuncLuaSub = new lua.CFunction(LuaMetaSub);
        internal static readonly lua.CFunction FuncLuaMul = new lua.CFunction(LuaMetaMul);
        internal static readonly lua.CFunction FuncLuaDiv = new lua.CFunction(LuaMetaDiv);
        internal static readonly lua.CFunction FuncLuaMod = new lua.CFunction(LuaMetaMod);
        internal static readonly lua.CFunction FuncLuaPow = new lua.CFunction(LuaMetaPow);
        internal static readonly lua.CFunction FuncLuaCon = new lua.CFunction(LuaMetaCon);
        internal static readonly lua.CFunction FuncLuaEq = new lua.CFunction(LuaMetaEq);
        internal static readonly lua.CFunction FuncLuaLt = new lua.CFunction(LuaMetaLt);
        internal static readonly lua.CFunction FuncLuaLe = new lua.CFunction(LuaMetaLe);

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaUnm(IntPtr l)
        {
            var oldtop = l.gettop();
            if (oldtop >= 1)
            {
                LuaOnStackUserData ud1 = l.GetUserData(1);
                if (ud1 != null)
                {
                    l.PushLua(ud1.UnaryOp("-"));
                }
            }
            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaLen(IntPtr l)
        {
            var oldtop = l.gettop();
            if (oldtop >= 1)
            {
                LuaOnStackUserData ud1 = l.GetUserData(1);
                if (ud1 != null)
                {
                    l.PushLua(ud1.UnaryOp("#"));
                }
            }
            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaAdd(IntPtr l)
        {
            var oldtop = l.gettop();
            if (oldtop >= 2)
            {
                LuaOnStackUserData ud1 = l.GetUserData(1);
                var obj2 = l.GetLua(2);
                if (ud1 != null)
                {
                    l.PushLua(ud1.BinaryOp("+", obj2));
                }
            }
            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaSub(IntPtr l)
        {
            var oldtop = l.gettop();
            if (oldtop >= 2)
            {
                LuaOnStackUserData ud1 = l.GetUserData(1);
                var obj2 = l.GetLua(2);
                if (ud1 != null)
                {
                    l.PushLua(ud1.BinaryOp("-", obj2));
                }
            }
            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaMul(IntPtr l)
        {
            var oldtop = l.gettop();
            if (oldtop >= 2)
            {
                LuaOnStackUserData ud1 = l.GetUserData(1);
                var obj2 = l.GetLua(2);
                if (ud1 != null)
                {
                    l.PushLua(ud1.BinaryOp("*", obj2));
                }
            }
            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaDiv(IntPtr l)
        {
            var oldtop = l.gettop();
            if (oldtop >= 2)
            {
                LuaOnStackUserData ud1 = l.GetUserData(1);
                var obj2 = l.GetLua(2);
                if (ud1 != null)
                {
                    l.PushLua(ud1.BinaryOp("/", obj2));
                }
            }
            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaMod(IntPtr l)
        {
            var oldtop = l.gettop();
            if (oldtop >= 2)
            {
                LuaOnStackUserData ud1 = l.GetUserData(1);
                var obj2 = l.GetLua(2);
                if (ud1 != null)
                {
                    l.PushLua(ud1.BinaryOp("%", obj2));
                }
            }
            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaPow(IntPtr l)
        {
            var oldtop = l.gettop();
            if (oldtop >= 2)
            {
                LuaOnStackUserData ud1 = l.GetUserData(1);
                var obj2 = l.GetLua(2);
                if (ud1 != null)
                {
                    l.PushLua(ud1.BinaryOp("^", obj2));
                }
            }
            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaCon(IntPtr l)
        {
            var oldtop = l.gettop();
            if (oldtop >= 2)
            {
                LuaOnStackUserData ud1 = l.GetUserData(1);
                var obj2 = l.GetLua(2);
                if (ud1 != null)
                {
                    l.PushLua(ud1.BinaryOp("..", obj2));
                }
            }
            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaEq(IntPtr l)
        {
            var oldtop = l.gettop();
            LuaOnStackUserData ud1 = null;
            object obj2 = null;
            if (oldtop >= 1)
            {
                ud1 = l.GetUserData(1);
            }
            if (oldtop >= 2)
            {
                obj2 = l.GetLua(2);
            }
            if (ud1 != null)
            {
                l.PushLua(ud1.BinaryOp("==", obj2));
            }
            else
            {
                obj2 = obj2.UnwrapDynamic();
                l.pushboolean(obj2 == null || obj2.Equals(null));
            }
            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaLt(IntPtr l)
        {
            var oldtop = l.gettop();
            if (oldtop >= 2)
            {
                LuaOnStackUserData ud1 = l.GetUserData(1);
                var obj2 = l.GetLua(2);
                if (ud1 != null)
                {
                    l.PushLua(ud1.BinaryOp("<", obj2));
                }
            }
            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaLe(IntPtr l)
        {
            var oldtop = l.gettop();
            if (oldtop >= 2)
            {
                LuaOnStackUserData ud1 = l.GetUserData(1);
                var obj2 = l.GetLua(2);
                if (ud1 != null)
                {
                    l.PushLua(ud1.BinaryOp("<=", obj2));
                }
            }
            return l.gettop() - oldtop;
        }
#endregion
    }

    public static class LuaRawClrObjectHelper
    {
        public static byte[] ESTRING___GC = new byte[] { (byte)'_', (byte)'_', (byte)'g', (byte)'c', 0 };

        public static IntPtr PushRawObj(this IntPtr l, object obj)
        {
            l.newtable(); // meta
            l.pushbuffer(ESTRING___GC); // meta, "__gc"
            l.pushcfunction(LuaUserDataHelper.FuncLuaGc); // meta, "__gc", func
            l.settable(-3); // meta
            IntPtr pud = l.newuserdata(new IntPtr(Marshal.SizeOf(typeof(IntPtr)))); // meta, ud
            var handle = GCHandle.Alloc(obj);
            Marshal.WriteIntPtr(pud, (IntPtr)handle);
            l.insert(-2); // ud meta
            l.setmetatable(-2); // ud
            return pud;
        }

        public static object GetRawObj(this IntPtr l, int pos)
        {
            if (l != IntPtr.Zero)
            {
                if (l.isuserdata(pos))
                {
                    if (l.islightuserdata(pos))
                    {
                        var phandle = l.touserdata(pos);
                        GCHandle handle = (GCHandle)phandle;
                        try
                        {
                            return handle.Target;
                        }
                        catch { }
                        return phandle;
                    }
                    else
                    {
                        IntPtr pud = l.touserdata(pos);
                        if (pud != IntPtr.Zero)
                        {
                            IntPtr hval = Marshal.ReadIntPtr(pud);
                            GCHandle handle = (GCHandle)hval;
                            try
                            {
                                return handle.Target;
                            }
                            catch { }
                        }
                    }
                }
            }
            return null;
        }

        public static IntPtr PushDisposableObj(this IntPtr l, IDisposable obj)
        {
            l.newtable(); // meta
            l.pushcfunction(FuncLuaDisposeObj); // meta, func
            l.SetField(-2, "__gc"); // meta
            IntPtr pud = l.newuserdata(new IntPtr(Marshal.SizeOf(typeof(IntPtr)))); // meta, ud
            var handle = GCHandle.Alloc(obj);
            Marshal.WriteIntPtr(pud, (IntPtr)handle);
            l.insert(-2); // ud meta
            l.setmetatable(-2); // ud
            return pud;
        }

        internal static readonly lua.CFunction FuncLuaDisposeObj = new lua.CFunction(LuaDisposeObjMeta);

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaDisposeObjMeta(IntPtr l)
        {
            using (var lr = new LuaStateRecover(l))
            {
                var oldtop = l.gettop();
                if (oldtop < 1)
                    return 0;

                IntPtr pud = l.touserdata(1);
                IntPtr hval = Marshal.ReadIntPtr(pud);
                GCHandle handle = (GCHandle)hval;
                try
                {
                    var disobj = handle.Target as IDisposable;
                    if (disobj != null)
                    {
                        disobj.Dispose();
                    }
                }
                catch { }
                handle.Free();
            }
            return 0;
        }
    }

    public static class LuaUserDataDebugHelper
    {
        public static int CachedObjectCnt
        {
            get
            {
                var dict = LuaOnStackUserData.ObjToCoreMap;
                var sb = new System.Text.StringBuilder();
                foreach (var kvp in dict)
                {
                    sb.AppendLine(kvp.Key.ToString());
                }
                if (GLog.IsLogInfoEnabled) GLog.LogInfo(sb);
                return LuaOnStackUserData.ObjToCoreMap.Count;
            }
        }

        public static int CachedFontCnt
        {
            get
            {
                int cnt = 0;
                var dict = LuaOnStackUserData.ObjToCoreMap;
                foreach (var kvp in dict)
                {
                    if (kvp.Key is UnityEngine.Font)
                    {
                        ++cnt;
                    }
                }
                return cnt;
            }
        }

        public static void ShrinkUdCache()
        {
            LuaOnStackUserData.ShrinkUdCache();
        }

        public static int GetLuaMemory(IntPtr l)
        {
            l.GetGlobal("collectgarbage");
            l.PushString("count");
            l.pcall(1, 1, 0);
            var mem = (int)(l.tonumber(-1) / 1024);
            l.pop(1);
            return mem;
        }
        public static int GetLuaMemory()
        {
            return GetLuaMemory(UnityFramework.UnityLua.GlobalLua.L);
        }

        public static void UserdataDump()
        {
            var path = UnityFramework.ResManager.UpdatePath + "/log/UserdataDump.txt";
#if UNITY_EDITOR
            path = "EditorOutput/Log/UserdataDump.txt";
#endif
            using (var sw = Capstones.PlatExt.PlatDependant.OpenWriteText(path))
            {
                var l = UnityFramework.UnityLua.GlobalLua.L;
                l.GetGlobal("collectgarbage");
                l.PushString("count");
                l.pcall(1, 1, 0);
                var mem = l.tonumber(-1);
                l.pop(1);
                sw.Write("Lua Memory: ");
                sw.Write(mem);
                sw.WriteLine("KB");

                l.newtable();
                var cacheindex = l.gettop();
                l.getglobal("_G");
                UserdataDump(sw, "", cacheindex);
                l.pop(1);
                l.newtable();
                l.pushnil();
                while(l.next(lua.LUA_REGISTRYINDEX))
                {
                    l.pushvalue(-2);
                    l.pushvalue(-2);
                    l.settable(-5);
                    l.pop(1);
                }
                UserdataDump(sw, "{ref}", cacheindex);
                l.pop(2);
            }
        }
        public static string FormatUserdata(IntPtr l, int index)
        {
            var obj = l.GetLua(index).UnwrapDynamic();
            if (object.ReferenceEquals(obj, null))
            {
                return "nullptr";
            }
            return obj.ToString() + "@" + obj.GetType().ToString();
        }
        public static void UserdataDump(System.IO.StreamWriter sw, string prev, int cacheindex)
        {
            var l = UnityFramework.UnityLua.GlobalLua.L;

            l.pushvalue(-1);
            l.gettable(cacheindex);
            var done = l.toboolean(-1);
            l.pop(1);
            if (done)
            {
                return;
            }
            l.pushvalue(-1);
            l.pushboolean(true);
            l.settable(cacheindex);

            l.pushnil();
            while (l.next(-2))
            {
                string key = prev;
                if (l.type(-2) == lua.LUA_TUSERDATA)
                {
                    // key is userdata.
                    var str = FormatUserdata(l, -2);
                    key += "[key:" + str + "]";
                    var nxt = key;
                    sw.WriteLine(nxt);
                    sw.Flush();
                    nxt += ".";
                    l.getfenv(-2);
                    UserdataDump(sw, nxt, cacheindex);
                    l.pop(1);
                }
                else if (l.type(-2) == lua.LUA_TTABLE)
                {
                    // key is table
                    key += "{key:tab}";
                    var nxt = key + ".";
                    l.pushvalue(-2);
                    UserdataDump(sw, nxt, cacheindex);
                    l.pop(1);
                }
                else if (l.isfunction(-2))
                {
                    // key is func
                    key += "<key:func>";

                    l.pushvalue(-2);
                    l.gettable(cacheindex);
                    done = l.toboolean(-1);
                    l.pop(1);
                    if (!done)
                    {
                        l.pushvalue(-2);
                        l.pushboolean(true);
                        l.settable(cacheindex);

                        l.newtable();
                        for (int i = 1; ; ++i)
                        {
                            var uname = l.getupvalue(-3, i);
                            if (uname == null)
                            {
                                break;
                            }
                            l.setfield(-2, "<u" + i + ":" + uname + ">");
                        }
                        var nxt = key + ".";
                        UserdataDump(sw, nxt, cacheindex);
                        l.pop(1);
                    }
                }
                else
                {
                    key += (l.GetLua(-2) ?? "null").ToString();
                }
                if (l.type(-1) == lua.LUA_TUSERDATA)
                {
                    // value is userdata.
                    var valstr = FormatUserdata(l, -1);
                    var nxt = key;
                    sw.Write(nxt);
                    sw.Write(":[");
                    sw.Write(valstr);
                    sw.WriteLine("]");
                    sw.Flush();
                    nxt += ".";
                    l.getfenv(-1);
                    UserdataDump(sw, nxt, cacheindex);
                    l.pop(1);
                }
                else if (l.type(-1) == lua.LUA_TTABLE)
                {
                    // value is table
                    var nxt = key + ".";
                    l.pushvalue(-1);
                    UserdataDump(sw, nxt, cacheindex);
                    l.pop(1);
                }
                else if (l.isfunction(-1))
                {
                    // value is func
                    l.pushvalue(-1);
                    l.gettable(cacheindex);
                    done = l.toboolean(-1);
                    l.pop(1);
                    if (!done)
                    {
                        l.pushvalue(-1);
                        l.pushboolean(true);
                        l.settable(cacheindex);
                        l.newtable();

                        for (int i = 1; ; ++i)
                        {
                            var uname = l.getupvalue(-2, i);
                            if (uname == null)
                            {
                                break;
                            }
                            l.setfield(-2, "<u" + i + ":" + uname + ">");
                        }
                        var nxt = key + ":<func>.";
                        UserdataDump(sw, nxt, cacheindex);
                        l.pop(1);
                    }
                }

                l.pop(1);
            }
        }
    }
}