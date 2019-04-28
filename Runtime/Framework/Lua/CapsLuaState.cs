using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Capstones.Dynamic;
using Capstones.LuaLib;
using Capstones.LuaWrap;

using lua = Capstones.LuaLib.LuaCoreLib;
using lual = Capstones.LuaLib.LuaAuxLib;
using luae = Capstones.LuaLib.LuaLibEx;

namespace Capstones.LuaWrap
{
    public class LuaState : BaseLua
    {
        public LuaState() : this(0)
        {
            L = lual.newstate();
            L.openlibs();
            _Closer = new LuaStateCloser() { _L = L };
        }
        public LuaState(IntPtr l) : this(0)
        {
            L = l;
        }
        protected internal LuaState(int preserved)
        {
            __UserDataCache = LuaOnStackUserData.ObjToCoreMap;
        }

        public LuaOnStackTable _G
        {
            get
            {
                return new LuaOnStackTable(L, lua.LUA_GLOBALSINDEX);
            }
        }
        public LuaStateRecover CreateStackRecover()
        {
            return new LuaStateRecover(L);
        }
        private object __UserDataCache; // this is for asset holder

        protected internal override object GetFieldImp(object key)
        {
            object rawkey = key.UnwrapDynamic();
            if (rawkey is string)
            {
                return _G.GetHierarchical(rawkey as string);
            }
            else
            {
                return _G[key];
            }
        }
        protected internal override bool SetFieldImp(object key, object val)
        {
            object rawkey = key.UnwrapDynamic();
            if (rawkey is string)
            {
                return _G.SetHierarchical(rawkey as string, val);
            }
            else
            {
                _G[key] = val;
                return true;
            }
        }

        public static implicit operator IntPtr(LuaState l)
        {
            if (l != null)
                return l.L;
            return IntPtr.Zero;
        }
        public static implicit operator LuaState(IntPtr l)
        {
            return new LuaState(l);
        }

        public override string ToString()
        {
            return "LuaState:" + L.ToString();
        }

        public static bool IgnoreDispose = false;

        protected internal class LuaStateCloser : IDisposable
        {
            [ThreadStatic] protected internal static LinkedList<IntPtr> DelayedCloser;
            protected internal LinkedList<IntPtr> _DelayedCloser;

            protected internal IntPtr _L;
            protected internal bool _Disposed;

            public LuaStateCloser()
            {
                if (DelayedCloser == null)
                    DelayedCloser = new LinkedList<IntPtr>();
                _DelayedCloser = DelayedCloser;
                RawDispose();
            }
            ~LuaStateCloser()
            {
                Dispose(false);
            }
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            protected internal void Dispose(bool includeManagedRes)
            {
                if (IgnoreDispose)
                {
                    _Disposed = true;
                    return;
                }
                if (!_Disposed)
                {
                    _Disposed = true;
                    if (_L != IntPtr.Zero)
                    {
                        if (_DelayedCloser != null)
                        {
                            lock (_DelayedCloser)
                            {
                                _DelayedCloser.AddLast(_L);
                            }
                        }
                    }
                }
                RawDispose();
            }

            public static void RawDispose()
            {
                if (IgnoreDispose) return;
                if (DelayedCloser != null)
                {
                    int tick = Environment.TickCount;
                    while (DelayedCloser.Count > 0)
                    {
                        IntPtr l = IntPtr.Zero;
                        lock (DelayedCloser)
                        {
                            if (DelayedCloser.Count > 0)
                            {
                                l = DelayedCloser.First.Value;
                                DelayedCloser.RemoveFirst();
                            }
                        }
                        if (l != IntPtr.Zero)
                        {
                            l.close();
                        }
                        var newtick = Environment.TickCount;
                        //if (newtick < tick || newtick - tick > 200)
                        //{
                        //    break;
                        //}
                    }
                }
            }
        }
        protected internal LuaStateCloser _Closer = null;
        public override void Dispose()
        {
            __UserDataCache = null;
            if (_Closer != null)
            {
                _Closer.Dispose();
            }
        }

        public void CallGlobalSelfVoid(string objname, string funcname, params object[] args)
        {
            var l = L;
            using (var lr = new LuaStateRecover(l))
            {
                if (!l.GetHierarchicalRaw(lua.LUA_GLOBALSINDEX, objname))
                    return;
                var _self = l.GetLuaOnStack(-1);
                if (!l.GetHierarchicalRaw(-1, funcname))
                    return;
                var paramCnt = 0;
                if (args != null)
                {
                    paramCnt = args.Length;
                }
                var rargs = ObjectPool.GetParamsFromPool(paramCnt + 1);
                rargs[0] = _self;
                for (int i = 0; i < paramCnt; ++i)
                {
                    rargs[i + 1] = args[i];
                }
                if (l.PushArgsAndCallRaw(rargs) != 0)
                {
                    if (GLog.IsLogErrorEnabled) GLog.LogError(l.GetLua(-1).UnwrapDynamic());
                }
                ObjectPool.ReturnParamsToPool(rargs);
            }
        }

        public void CallGlobalVoid(string funcname, params object[] args)
        {
            var l = L;
            using (var lr = new LuaStateRecover(l))
            {
                if (!l.GetHierarchicalRaw(lua.LUA_GLOBALSINDEX, funcname))
                    return;
                if (l.PushArgsAndCallRaw(args) != 0)
                {
                    if (GLog.IsLogErrorEnabled) GLog.LogError(l.GetLua(-1).UnwrapDynamic());
                }
            }
        }

        public object[] CallGlobalSelf(string objname, string funcname, params object[] args)
        {
            var l = L;
            using (var lr = new LuaStateRecover(l))
            {
                if (!l.GetHierarchicalRaw(lua.LUA_GLOBALSINDEX, objname))
                    return null;
                var _self = l.GetLuaOnStack(-1);
                if (!l.GetHierarchicalRaw(-1, funcname))
                    return null;
                var paramCnt = 0;
                if (args != null)
                {
                    paramCnt = args.Length;
                }
                var rargs = ObjectPool.GetParamsFromPool(paramCnt + 1);
                rargs[0] = _self;
                for (int i = 0; i < paramCnt; ++i)
                {
                    rargs[i + 1] = args[i];
                }
                var rv = l.PushArgsAndCall(rargs);
                ObjectPool.ReturnParamsToPool(rargs);
                return rv;
            }
        }

        public object[] CallGlobal(string funcname, params object[] args)
        {
            var l = L;
            using (var lr = new LuaStateRecover(l))
            {
                if (!l.GetHierarchicalRaw(lua.LUA_GLOBALSINDEX, funcname))
                    return null;
                return l.PushArgsAndCall(args);
            }
        }

        public T CallGlobalSelf<T>(string objname, string funcname, params object[] args)
        {
            return CallGlobalSelf(objname, funcname, args).UnwrapReturnValues<T>();
        }

        public T CallGlobal<T>(string funcname, params object[] args)
        {
            return CallGlobal(funcname, args).UnwrapReturnValues<T>();
        }

        // TODO: more func of lualib import here.
        public int DoFile(string filepath)
        {
            var l = L;
            var oldtop = l.gettop();
            l.pushcfunction(Capstones.LuaExt.LuaFramework.ClrDelErrorHandler);
            var code = l.dofile(filepath, oldtop + 1);
            l.remove(oldtop + 1);
            return code;
        }
    }

    public class LuaOnStackThread : LuaState
    {
        protected internal bool _IsDone = false;

        public override string ToString()
        {
            return "LuaThreadRaw:" + L.ToString() + ", of ref:" + Refid.ToString();
        }
        public LuaOnStackThread(IntPtr l) : base(0)
        {
            L = l;
        }

        protected internal LuaOnStackThread(IntPtr l, int refid) : base(0)
        {
            L = l;
            Refid = refid;
        }

        public virtual object[] Resume(params object[] args)
        {
            if (L != IntPtr.Zero)
            {
                using (var lr = new LuaStateRecover(L))
                {
                    return ResumeRaw(args);
                }
            }
            return null;
        }

        protected internal object[] ResumeRaw(params object[] args)
        {
            if (_IsDone)
            {
                return null;
            }
            if (args != null)
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    L.PushLua(args[i]);
                }
            }
            int status = L.resume(args == null ? 0 : args.Length);
            if (status == lua.LUA_YIELD || status == 0)
            {
                object[] rv = ObjectPool.GetReturnValueFromPool(L.gettop());
                for (int i = 0; i < rv.Length; ++i)
                {
                    rv[i] = L.GetLua(i + 1);
                }
                if (status == 0)
                {
                    _IsDone = true;
                }
                return rv;
            }
            else
            {
                L.pushcfunction(Capstones.LuaExt.LuaFramework.ClrDelErrorHandler);
                L.insert(-2);
                L.pcall(1, 1, 0);
                if (GLog.IsLogErrorEnabled) GLog.LogError(L.GetLua(-1).UnwrapDynamic());
            }
            return null;
        }

        public virtual bool IsRunning
        {
            get
            {
                if (L != IntPtr.Zero)
                {
                    return L.status() == lua.LUA_YIELD;
                }
                return false;
            }
        }
    }

    public class LuaThread : LuaOnStackThread
    {
        public override string ToString()
        {
            return "LuaThreadRestartable:" + L.ToString() + ", of ref:" + Refid.ToString();
        }

        protected internal LuaFunc _Func;
        protected internal bool _NeedRestart = true;

        public LuaThread(LuaFunc func) : base(IntPtr.Zero)
        {
            if (func != null && func.L != IntPtr.Zero)
            {
                L = func.L.newthread();
                Refid = func.L.refer();
                if (func.Refid != 0)
                {
                    L.getref(func.Refid);
                    _Func = new LuaFunc(L, -1);
                    L.pop(1);
                }
            }
        }

        public override object[] Resume(params object[] args)
        {
            if (_Func != null && L != IntPtr.Zero)
            {
                using (var lr = new LuaStateRecover(L))
                {
                    if (_NeedRestart)
                    {
                        _NeedRestart = false;
                        _IsDone = false;
                        if (_Func == null)
                            return null;
                        L.PushLua(_Func);
                        return ResumeRaw(args);
                    }
                    else if (IsRunning)
                    {
                        return ResumeRaw(args);
                    }
                }
            }
            return null;
        }

        public object[] Restart(params object[] args)
        {
            _NeedRestart = true;
            return Resume(args);
        }
    }

    internal class LuaThreadRefMan
    {
        protected internal LinkedList<WeakReference> _RefList = new LinkedList<WeakReference>();

        public void Close()
        {
            foreach (var node in _RefList)
            {
                var luaref = node.Target as LuaRef.RawLuaRef;
                if (luaref != null)
                {
                    luaref.l = IntPtr.Zero;
                    luaref.n = null;
                }
            }
            _RefList.Clear();
        }
    }

    public class LuaRef : IDisposable
    {
        protected internal class RawLuaRef
        {
            public IntPtr l;
            public int r;
            public int rl;
            public LinkedListNode<WeakReference> n;
        }
        [ThreadStatic] protected internal static LinkedList<RawLuaRef> DelayedFinalizer;
        protected internal LinkedList<RawLuaRef> _DelayedFinalizer;

        public IntPtr L
        {
            get { return RawRef.l; }
            set
            {
                if (!_Disposed)
                {
                    if (RawRef.l != value)
                    {
                        if (RawRef.n != null)
                        {
                            RawRef.n.List.Remove(RawRef.n);
                            RawRef.n = null;
                        }
                        if (RawRef.l != IntPtr.Zero)
                        {
                            if (RawRef.rl != 0)
                            {
                                RawRef.l.unref(RawRef.rl);
                                RawRef.rl = 0;
                            }
                            if (Refid != 0)
                            {
                                RawRef.l.unref(Refid);
                                Refid = 0;
                            }
                        }
                        if (value != IntPtr.Zero)
                        {
                            value.pushthread();
                            RawRef.rl = value.refer();
                        }
                        RawRef.l = value;
                        var man = RawRef.l.PrepareRefMan();
                        if (man != null)
                        {
                            RawRef.n = man._RefList.AddLast(new WeakReference(RawRef));
                        }
                        RawDispose();
                    }
                }
            }
        }

        protected internal RawLuaRef RawRef = new RawLuaRef();
        public int Refid
        {
            get { return RawRef.r; }
            set { RawRef.r = value; }
        }
        protected internal bool _Disposed;

        public LuaRef()
        {
            if (DelayedFinalizer == null)
                DelayedFinalizer = new LinkedList<RawLuaRef>();
            _DelayedFinalizer = DelayedFinalizer;
            RawDispose();
        }
        ~LuaRef()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected internal void Dispose(bool includeManagedRes)
        {
            if (LuaState.IgnoreDispose)
            {
                _Disposed = true;
                return;
            }
            if (!_Disposed)
            {
                _Disposed = true;
                var raw = RawRef;
                if (_DelayedFinalizer != null && raw != null)
                {
                    if (raw.l != IntPtr.Zero)
                    {
                        lock (_DelayedFinalizer)
                        {
                            _DelayedFinalizer.AddLast(raw);
                        }
                    }
                }
            }
            RawDispose();
        }

        public static void RawDispose()
        {
            if (LuaState.IgnoreDispose) return;
            if (DelayedFinalizer != null)
            {
                int tick = Environment.TickCount;
                while (DelayedFinalizer.Count > 0)
                {
                    RawLuaRef l = new RawLuaRef();
                    var locker = DelayedFinalizer;
                    if (System.Threading.Monitor.TryEnter(locker, 1))
                    {
                        try
                        {
                            if (DelayedFinalizer.Count > 0)
                            {
                                l = DelayedFinalizer.First.Value;
                                DelayedFinalizer.RemoveFirst();
                            }
                        }
                        catch { }
                        finally
                        {
                            System.Threading.Monitor.Exit(locker);
                        }
                    }
                    try
                    {
                        if (l.l != IntPtr.Zero)
                        {
                            if (l.r != 0)
                            {
                                l.l.unref(l.r);
                            }
                            if (l.rl != 0)
                            {
                                l.l.unref(l.rl);
                            }
                            l.l = IntPtr.Zero;
                        }
                        if (l.n != null)
                        {
                            l.n.List.Remove(l.n);
                            l.n = null;
                        }
                    }
                    catch { }
                    var newtick = Environment.TickCount;
                    //if (newtick < tick || newtick - tick > 20)
                    //{
                    //    break;
                    //}
                }
            }
        }
    }

    internal static class LuaThreadRefHelper
    {
        public static LuaThreadRefMan GetRefMan(this IntPtr l)
        {
            if (l != IntPtr.Zero)
            {
                using (var lr = new LuaStateRecover(l))
                {
                    l.GetField(lua.LUA_REGISTRYINDEX, "___thdreg");
                    IntPtr pud = l.touserdata(-1);
                    if (pud != IntPtr.Zero)
                    {
                        IntPtr hval = Marshal.ReadIntPtr(pud);
                        GCHandle handle = (GCHandle)hval;
                        LuaThreadRefMan ud = null;
                        try
                        {
                            ud = handle.Target as LuaThreadRefMan;
                        }
                        catch { }
                        return ud;
                    }
                }
            }
            return null;
        }

        public static LuaThreadRefMan PrepareRefMan(this IntPtr l)
        {
            if (l != IntPtr.Zero)
            {
                using (var lr = new LuaStateRecover(l))
                {
                    l.GetField(lua.LUA_REGISTRYINDEX, "___thdreg");
                    IntPtr pud = l.touserdata(-1);
                    GCHandle handle;
                    if (pud != IntPtr.Zero)
                    {
                        IntPtr hval = Marshal.ReadIntPtr(pud);
                        handle = (GCHandle)hval;
                        LuaThreadRefMan ud = null;
                        try
                        {
                            ud = handle.Target as LuaThreadRefMan;
                        }
                        catch { }
                        if (ud != null)
                        {
                            return ud;
                        }
                    }
                    l.pop(1);

                    l.newtable(); // meta
                    l.pushcfunction(FuncRefManGc); // meta, func
                    l.SetField(-2, "__gc"); // meta

                    LuaThreadRefMan man = new LuaThreadRefMan();
                    pud = l.newuserdata(new IntPtr(Marshal.SizeOf(typeof(IntPtr)))); // meta, man
                    handle = GCHandle.Alloc(man);
                    Marshal.WriteIntPtr(pud, (IntPtr)handle);

                    l.insert(-2); // man, meta
                    l.setmetatable(-2); // man
                    l.SetField(lua.LUA_REGISTRYINDEX, "___thdreg"); // (empty)
                    return man;
                }
            }
            return null;
        }

        internal static readonly lua.CFunction FuncRefManGc = new lua.CFunction(MetaRefManGc);

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int MetaRefManGc(IntPtr l)
        {
            using (var lr = new LuaStateRecover(l))
            {
                var oldtop = l.gettop();
                if (oldtop < 1)
                    return 0;

                if (l.isuserdata(1) && !l.islightuserdata(1))
                {
                    IntPtr pud = l.touserdata(1);
                    IntPtr hval = Marshal.ReadIntPtr(pud);
                    GCHandle handle = (GCHandle)hval;

                    LuaThreadRefMan ud = null;
                    try
                    {
                        ud = handle.Target as LuaThreadRefMan;
                    }
                    catch { }
                    if (!object.ReferenceEquals(ud, null))
                    {
                        ud.Close();
                    }

                    handle.Free();
                }
            }
            return 0;
        }
    }

    public static class LuaStateHelper
    {
        public static bool GetHierarchicalRaw(this IntPtr l, int index, string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            var hkeys = key.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (hkeys == null)
                return false;
            if (l == IntPtr.Zero)
                return false;

            //var oldtop = l.gettop();
            l.pushvalue(index); // table
            l.pushnil(); // table result
            l.insert(-2); // result table
            for (int i = 0; i < hkeys.Length; ++i)
            {
                if (l.istable(-1) || (l.isuserdata(-1) && !l.islightuserdata(-1)))
                {
                    l.GetField(-1, hkeys[i]); // result table newresult
                    l.replace(-3); // newresult table
                    l.pop(1); // newresult
                    l.pushvalue(-1); // newresult newtable
                }
                else
                {
                    break;
                }
            }
            l.pop(1); // result
            return true;
        }
        public static object GetHierarchical(this IntPtr l, int index, string key)
        {
            if (GetHierarchicalRaw(l, index, key))
            {
                var rv = l.GetLua(-1);
                l.pop(1);
                return rv;
            }
            return null;
        }
        public static object GetHierarchical(this LuaOnStackTable tab, string key)
        {
            if (tab == null || tab.L == IntPtr.Zero)
            {
                return null;
            }
            return GetHierarchical(tab.L, tab.StackPos, key);
        }
        public static object GetHierarchical(this LuaTable tab, string key)
        {
            if (tab == null || tab.L == IntPtr.Zero)
            {
                return null;
            }
            var l = tab.L;
            l.PushLua(tab);
            var rv = GetHierarchical(l, -1, key);
            l.pop(1);
            return rv;
        }
        public static object GetHierarchical(this LuaOnStackUserData ud, string key)
        {
            if (ud == null || ud.L == IntPtr.Zero)
            {
                return null;
            }
            var l = ud.L;
            using (var lr = new LuaStateRecover(l))
            {
                if (ud.PushToLua())
                {
                    return GetHierarchical(l, -1, key);
                }
                return null;
            }
        }

        public static bool SetHierarchicalRaw(this IntPtr l, int index, string key, int valindex)
        {
            var val = l.GetLuaOnStack(valindex);
            return SetHierarchical(l, index, key, val);
        }
        public static bool SetHierarchical(this IntPtr l, int index, string key, object val)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            var hkeys = key.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (hkeys == null || hkeys.Length < 1)
                return false;
            if (l == IntPtr.Zero)
                return false;

            l.pushvalue(index); // table
            for (int i = 0; i < hkeys.Length - 1; ++i)
            {
                if (l.istable(-1) || (l.isuserdata(-1) && !l.islightuserdata(-1)))
                {
                    l.GetField(-1, hkeys[i]); // table result
                    if (l.isnoneornil(-1))
                    {
                        l.pop(1); // table
                        l.newtable(); // table result
                        l.pushvalue(-1); // table result result
                        l.SetField(-3, hkeys[i]); // table result
                    }
                    l.remove(-2); // result
                }
                else
                {
                    l.pop(1);
                    return false;
                }
            }
            if (l.istable(-1) || (l.isuserdata(-1) && !l.islightuserdata(-1)))
            {
                l.PushLua(val); // table val
                l.SetField(-2, hkeys[hkeys.Length - 1]); // table
                l.pop(1);
                return true;
            }
            else
            {
                l.pop(1);
                return false;
            }
        }
        public static bool SetHierarchical(this LuaOnStackTable tab, string key, object val)
        {
            if (tab == null || tab.L == IntPtr.Zero)
            {
                return false;
            }
            return SetHierarchical(tab.L, tab.StackPos, key, val);
        }
        public static bool SetHierarchical(this LuaTable tab, string key, object val)
        {
            if (tab == null || tab.L == IntPtr.Zero)
            {
                return false;
            }
            var l = tab.L;
            l.PushLua(tab);
            var rv = SetHierarchical(l, -1, key, val);
            l.pop(1);
            return rv;
        }
        public static bool SetHierarchical(this LuaOnStackUserData ud, string key, object val)
        {
            if (ud == null || ud.L == IntPtr.Zero)
            {
                return false;
            }
            var l = ud.L;
            using (var lr = new LuaStateRecover(l))
            {
                if (ud.PushToLua())
                {
                    return SetHierarchical(l, -1, key, val);
                }
                return false;
            }
        }

        public static void Log(this IntPtr l, object message)
        {
            var oldtop = l.gettop();
            l.GetGlobal("dump");
            l.PushLua(message);
            l.pcall(1, 0, 0);
            l.settop(oldtop);
        }
        public static void LogWarning(this IntPtr l, object message)
        {
            var oldtop = l.gettop();
            l.GetGlobal("dumpw");
            l.PushLua(message);
            l.pcall(1, 0, 0);
            l.settop(oldtop);
        }
        public static void LogError(this IntPtr l, object message)
        {
            var oldtop = l.gettop();
            l.GetGlobal("dumpe");
            l.PushLua(message);
            l.pcall(1, 0, 0);
            l.settop(oldtop);
        }
    }
}