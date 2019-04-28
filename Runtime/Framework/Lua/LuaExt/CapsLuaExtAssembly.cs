using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using Capstones.Dynamic;
using Capstones.LuaLib;
using Capstones.LuaWrap;
using Capstones.PlatExt;

using lua = Capstones.LuaLib.LuaCoreLib;
using lual = Capstones.LuaLib.LuaAuxLib;
using luae = Capstones.LuaLib.LuaLibEx;

namespace Capstones.LuaExt
{
    public static class Assembly2Lua
    {
        public static void Init(IntPtr L)
        {
#if NETFX_CORE
            //// .NET Core
            _SearchAssemblies.Add(typeof(List<>).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(System.Collections.Concurrent.ConcurrentBag<>).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(System.Dynamic.DynamicObject).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(System.Globalization.Calendar).GetTypeInfo().Assembly);
            _SearchAssemblies.Add(typeof(System.IO.Stream).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(System.IO.Compression.ZipArchive).GetTypeInfo().Assembly);
            _SearchAssemblies.Add(typeof(System.Linq.Enumerable).GetTypeInfo().Assembly);
            _SearchAssemblies.Add(typeof(System.Linq.Expressions.Expression).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(System.Linq.ParallelEnumerable).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(System.Linq.Queryable).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(System.Net.Http.HttpClient).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(System.Net.Cookie).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(System.Net.WebRequest).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(Assembly).GetTypeInfo().Assembly);
            _SearchAssemblies.Add(typeof(int).GetTypeInfo().Assembly);
            _SearchAssemblies.Add(typeof(Random).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(Marshal).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(System.Numerics.Complex).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(System.Xml.XmlDictionary).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(Windows.UI.Color).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(Windows.UI.Xaml.Media.Media3D.Matrix3D).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(System.Text.Encoding).GetTypeInfo().Assembly);
            _SearchAssemblies.Add(typeof(System.Text.UTF8Encoding).GetTypeInfo().Assembly);
            _SearchAssemblies.Add(typeof(System.Text.RegularExpressions.Regex).GetTypeInfo().Assembly);
            _SearchAssemblies.Add(typeof(System.Threading.Interlocked).GetTypeInfo().Assembly);
            _SearchAssemblies.Add(typeof(System.Threading.Tasks.Task).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(System.Threading.Tasks.Parallel).GetTypeInfo().Assembly);
            _SearchAssemblies.Add(typeof(System.Threading.Timer).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(System.Xml.XmlReader).GetTypeInfo().Assembly);
            //_SearchAssemblies.Add(typeof(System.Xml.Linq.XNode).GetTypeInfo().Assembly);

            //// Unity Engine
            _SearchAssemblies.Add(typeof(UnityEngine.WWW).GetTypeInfo().Assembly);
            _SearchAssemblies.Add(typeof(UnityEngine.UI.Button).GetTypeInfo().Assembly);

            //// Self
            _SearchAssemblies.Add(typeof(Assembly2Lua).GetTypeInfo().Assembly);
#endif
            L.newtable(); // clr
            L.pushvalue(-1); // clr clr
            L.SetGlobal("clr"); // clr
            L.PushString(""); // clr ""
            L.SetField(-2, "___path"); // clr
            L.PushClrHierarchyMetatable(); // clr meta
            L.setmetatable(-2); // clr
            L.pushcfunction(ClrDelWrap); // clr func
            L.SetField(-2, "wrap"); // clr
            L.pushcfunction(ClrDelUnwrap); // clr func
            L.SetField(-2, "unwrap"); // clr
            L.pushcfunction(ClrDelConvert); // clr func
            L.SetField(-2, "as"); // clr
            L.pushcfunction(ClrDelIs); // clr func
            L.SetField(-2, "is"); // clr
            L.pushcfunction(ClrDelArray); // clr func
            L.SetField(-2, "array"); // clr
            L.pushcfunction(ClrDelDict); // clr func
            L.SetField(-2, "dict"); // clr
            L.pushcfunction(ClrDelTable); // clr func
            L.SetField(-2, "table"); // clr
            L.pushcfunction(ClrDelNext); // clr func
            L.SetField(-2, "next"); // clr
            L.pushcfunction(ClrDelPairs); // clr func
            L.SetField(-2, "pairs"); // clr
            L.pushcfunction(ClrDelEx); // clr func
            L.SetField(-2, "ex"); // clr
            L.PushUserDataOfType(null, typeof(object));
            L.SetField(-2, "null");
            L.pop(1);
        }

        internal static readonly lua.CFunction ClrFuncIndex = new lua.CFunction(ClrMetaIndex);
        internal static readonly lua.CFunction ClrDelWrap = new lua.CFunction(ClrFuncWrap);
        internal static readonly lua.CFunction ClrDelUnwrap = new lua.CFunction(ClrFuncUnwrap);
        internal static readonly lua.CFunction ClrDelConvert = new lua.CFunction(ClrFuncConvert);
        internal static readonly lua.CFunction ClrDelIs = new lua.CFunction(ClrFuncIs);
        internal static readonly lua.CFunction ClrDelArray = new lua.CFunction(ClrFuncArray);
        internal static readonly lua.CFunction ClrDelDict = new lua.CFunction(ClrFuncDict);
        internal static readonly lua.CFunction ClrDelTable = new lua.CFunction(ClrFuncTable);
        internal static readonly lua.CFunction ClrDelNext = new lua.CFunction(ClrFuncNext);
        internal static readonly lua.CFunction ClrDelPairs = new lua.CFunction(ClrFuncPairs);
        internal static readonly lua.CFunction ClrDelEx = new lua.CFunction(ClrFuncEx);

        public static void PushClrHierarchyMetatable(this IntPtr l)
        {
            if (l != IntPtr.Zero)
            {
                l.GetField(lua.LUA_REGISTRYINDEX, "___clrmeta");
                if (l.istable(-1))
                    return;
                l.pop(1);
                CreateClrHierarchyMetatable(l);
                l.pushvalue(-1);
                l.SetField(lua.LUA_REGISTRYINDEX, "___clrmeta");
            }
        }

        internal static void CreateClrHierarchyMetatable(this IntPtr l)
        {
            l.newtable(); // meta
            l.pushcfunction(ClrFuncIndex); // meta func
            l.SetField(-2, "__index"); // meta
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrMetaIndex(IntPtr l)
        {
            // ... = tab key
            var oldtop = l.gettop();
            switch (0)
            {
                default:
                    if (oldtop < 2)
                        break;
                    if (!l.istable(1))
                        break;
                    string key = l.GetString(2);
                    if (key == null)
                    {
                        key = "";
                    }
                    l.PushString("___path"); // ... "___path"
                    l.rawget(1);  // ... path
                    string path = l.GetString(-1);
                    if (path == null)
                    {
                        path = "";
                    }
                    string full = path + key;
                    l.pop(1); // ...

                    Type ftype = null;
                    List<Type> gtypes = new List<Type>(2);
#if NETFX_CORE
                    foreach (var asm in _SearchAssemblies)
#else
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
#endif
                    {
                        if (ftype == null)
                        {
                            try
                            {
#if NETFX_CORE
                                ftype = asm.GetType(full);
#else
                                ftype = asm.GetType(full, false);
#endif
                            }
                            catch { }
                        }
                        for(int i = 0; i < _MaxGenericParamCount; ++i)
                        {
                            try
                            {
#if NETFX_CORE
                                var gtype = asm.GetType(full + "`" + i.ToString());
#else
                                var gtype = asm.GetType(full + "`" + i.ToString(), false);
#endif
                                if (gtype != null)
                                {
                                    gtypes.Add(gtype);
                                }
                            }
                            catch { }
                        }
                    }
                    var typewrapper = GenericHelper.GetTypeWrapper(ftype, gtypes.ToArray());
                    if (object.ReferenceEquals(typewrapper, null))
                    {
                        l.newtable(); // ... ntab
                        l.pushvalue(-1); // ... ntab ntab
                        l.SetField(1, key); // ... ntab
                        l.PushString(full + "."); // ... ntab npath
                        l.SetField(-2, "___path"); // ... ntab
                        l.PushClrHierarchyMetatable(); // ... ntab meta
                        l.setmetatable(-2); // ... ntab
                    }
                    else
                    {
                        l.PushUserData(typewrapper); // ... type
                        l.pushvalue(-1); // ... type type
                        l.SetField(1, key); // ... type
                    }
                    break;
            }

            return l.gettop() - oldtop;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncWrap(IntPtr l)
        {
            l.PushUserData(l.GetLua(1));
            return 1;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncUnwrap(IntPtr l)
        {
            l.PushLua(l.GetLua(1).UnwrapDynamic());
            return 1;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncConvert(IntPtr l)
        {
            var obj = l.GetLua(1);//.UnwrapDynamic();
            var otype = l.GetLua(2).ConvertType(typeof(Type)) as Type;
            l.PushUserDataOfType(obj.ConvertTypeEx(otype), otype);
            return 1;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncIs(IntPtr l)
        {
            var obj = l.GetLua(1).UnwrapDynamic();
            var otype = l.GetLua(2).ConvertType(typeof(Type)) as Type;
            bool rv = otype != null && otype.IsInstanceOfType(obj);
            l.pushboolean(rv);
            return 1;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncArray(IntPtr l)
        {
            if (l.istable(1))
            {
                Array arr = null;
                using(var lr = new LuaStateRecover(l))
                {
                    LuaOnStackTable tab = new LuaOnStackTable(l, 1);
                    var len = ((LuaOnStackTable.LuaTableFieldsProvider)tab.FieldsProvider).ArrLength;
                    var otype = l.GetLua(2).ConvertType(typeof(Type));
                    if (otype == null)
                    {
                        otype = typeof(object);
                    }
                    arr = Array.CreateInstance((Type)otype, len);
                    for(int i = 0; i < len; ++i)
                    {
                        arr.SetValue(tab[i + 1].UnwrapDynamic().ConvertType((Type)otype), i);
                    }
                }
                l.PushUserDataOfType(arr, arr.GetType());
                return 1;
            }
            else if (l.IsString(1))
            {
                var bytes = l.tolstring(1);
                l.PushUserDataOfType(bytes, typeof(byte[]));
                return 1;
            }
            else if (l.IsNumber(1))
            {
                int len = (int)l.tonumber(1);
                if (len < 0)
                {
                    len = 0;
                }
                var otype = l.GetLua(2).ConvertType(typeof(Type));
                if (otype == null)
                {
                    otype = typeof(object);
                }
                Array arr = Array.CreateInstance((Type)otype, len);
                l.PushUserDataOfType(arr, arr.GetType());
                return 1;
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncDict(IntPtr l)
        {
            if (l.istable(1))
            {
                IDictionary dict = null;
                using (var lr = new LuaStateRecover(l))
                {
                    LuaOnStackTable tab = new LuaOnStackTable(l, 1);
                    var ktype = l.GetLua(2).ConvertType(typeof(Type)) as Type;
                    var vtype = l.GetLua(3).ConvertType(typeof(Type)) as Type;
                    if (ktype == null)
                    {
                        ktype = typeof(object);
                    }
                    if (vtype == null)
                    {
                        vtype = typeof(object);
                    }
                    dict = typeof(Dictionary<,>).MakeGenericType(ktype, vtype).GetConstructor(new Type[0]).Invoke(null) as IDictionary;
                    l.PushLua(tab);
                    l.pushnil();
                    while (l.next(-2))
                    {
                        object key = l.GetLua(-2);
                        object val = l.GetLua(-1);
                        dict.Add(key.UnwrapDynamic().ConvertType(ktype), val.UnwrapDynamic().ConvertType(vtype));
                        l.pop(1);
                    }
                    //l.pop(1);
                }
                l.PushLua(dict);
                return 1;
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncTable(IntPtr l)
        {
            if (l.istable(1))
            {
                l.pushvalue(1);
                return 1;
            }
            var obj = l.GetLua(1);
            var lobj = obj.ConvertType(typeof(IList)) as IList;
            if (lobj != null)
            {
                l.newtable();
                for (int i = 0; i < lobj.Count; ++i)
                {
                    l.pushnumber(i + 1);
                    l.PushLua(lobj[i]);
                    l.settable(-3);
                }
                return 1;
            }
            var dobj = obj.ConvertType(typeof(IDictionary)) as IDictionary;
            if (dobj != null)
            {
                l.newtable();
                foreach (DictionaryEntry kvp in dobj)
                {
                    l.PushLua(kvp.Key);
                    l.PushLua(kvp.Value);
                    l.settable(-3);
                }
                return 1;
            }
            if (l.isuserdata(1))
            {
                l.getfenv(1); // ud, ex
                return 1;
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncNext(IntPtr l)
        {
            var list = l.GetLua(1).ConvertType(typeof(IList)) as IList;
            if (list != null)
            {
                var key = l.GetLua(2).UnwrapDynamic();
                object oindex = null;
                if (key != null)
                {
                    oindex = key.ConvertTypeRaw(typeof(int));
                }
                if (key == null || oindex != null)
                {
                    int index = 0;
                    if (oindex != null)
                    {
                        index = (int)oindex + 1;
                    }
                    if (index >= 0 && index < list.Count)
                    {
                        l.pushnumber(index);
                        l.PushLua(list[index]);
                        return 2;
                    }
                }
                return 0;
            }
            var detor = l.GetLua(1).ConvertType(typeof(IDictionaryEnumerator)) as IDictionaryEnumerator;
            if (detor != null)
            {
                var key = l.GetLua(2).UnwrapDynamic();
                if(key == null)
                {
                    detor.Reset();
                }
                if (detor.MoveNext())
                {
                    l.PushLua(detor.Entry.Key);
                    l.PushLua(detor.Entry.Value);
                    return 2;
                }
                return 0;
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncPairs(IntPtr l)
        {
            var list = l.GetLua(1).ConvertType(typeof(IList)) as IList;
            if (list != null)
            {
                l.pushcfunction(ClrDelNext);
                l.pushvalue(1);
                l.pushnil();
                return 3;
            }
            var dict = l.GetLua(1).ConvertType(typeof(IDictionary)) as IDictionary;
            if (dict != null)
            {
                l.pushcfunction(ClrDelNext);
                l.PushLua(dict.GetEnumerator());
                l.pushnil();
                return 3;
            }
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int ClrFuncEx(IntPtr l)
        {
            l.getfenv(1);
            return 1;
        }

        internal const int _MaxGenericParamCount = 5;

#if NETFX_CORE
        internal static HashSet<Assembly> _SearchAssemblies = new HashSet<Assembly>();
#endif
    }
}