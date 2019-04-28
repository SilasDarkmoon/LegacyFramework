using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Capstones.PlatExt;

namespace Capstones.Dynamic
{
    public interface IClrTypeCore : ICallableCore
    {
        Type BindingType { get; }
        object GetFieldFor(object tar, object key);
        bool SetFieldFor(object tar, object key, object val);
    }
    public interface IClrTypeCoreForTypeGroup
    {
        Type MajorType { get; }
        Type[] AllTypes { get; }
    }

    internal class ClrTypeCore : IClrTypeCore
    {
        internal static Dictionary<Type, IClrTypeCore> _TypeCoreCache = new Dictionary<Type, IClrTypeCore>();
        internal static IClrTypeCore GetTypeCore(Type type)
        {
            if (type != null)
            {
#if NETFX_CORE
                Capstones.LuaExt.Assembly2Lua._SearchAssemblies.Add(type.GetTypeInfo().Assembly);
#endif
                IClrTypeCore core = null;
                if (_TypeCoreCache.TryGetValue(type, out core))
                {
                    return core;
                }
                if (type.IsGenericType() && type.ContainsGenericParameters())
                {
                    core = new ClrGenericTypeCore(type);
                }
                else
                {
                    core = new ClrTypeCore(type);
                }
                _TypeCoreCache[type] = core;
                return core;
            }
            return null;
        }

        internal Type _BindingType;
        internal ICallableCore _ConstructorCallable = null;
        //#if NETFX_CORE
        //        internal Func<object> _DefaultCtor = null;
        //#endif
        internal Dictionary<string, object> _ExpandedFields = new Dictionary<string, object>();
        internal Dictionary<string, object> _StaticFields = new Dictionary<string, object>();

        public Type BindingType
        {
            get { return _BindingType; }
        }

        public ClrTypeCore(Type type)
        {
            _BindingType = type;
            if (type != null)
            {
                var ctors = type.GetConstructors();
                _ConstructorCallable = ctors.CreateCallableCore();
                //#if NETFX_CORE
                //                if (type.GetTypeInfo().IsValueType)
                //                {
                //                    try
                //                    {
                //                        _DefaultCtor = type.GetDefaultCtor();
                //                    }
                //                    catch { }
                //                }
                //#endif

                Dictionary<string, List<MethodBase>> methods = new Dictionary<string, List<MethodBase>>();
                Dictionary<string, List<MethodBase>> smethods = new Dictionary<string, List<MethodBase>>();
                foreach (var minfo in type.GetMethods()) // FlattenHierarchy?
                {
                    string name = minfo.Name;
                    List<MethodBase> list;
                    methods.TryGetValue(name, out list);
                    if (list == null)
                    {
                        list = new List<MethodBase>();
                        methods[name] = list;
                    }
                    list.Add(minfo);
                    if (minfo.IsStatic)
                    {
                        list = null;
                        smethods.TryGetValue(name, out list);
                        if (list == null)
                        {
                            list = new List<MethodBase>();
                            smethods[name] = list;
                        }
                        list.Add(minfo);
                    }
                }
                foreach (var kvp in methods)
                {
                    var ccore = kvp.Value.ToArray().CreateCallableCore();
                    _ExpandedFields[kvp.Key] = ccore;
                }
                foreach (var kvp in smethods)
                {
                    var ccore = kvp.Value.ToArray().CreateCallableCore();
                    _StaticFields[kvp.Key] = ccore;
                }
                foreach (var ntype in type.GetAllNestedTypes())
                {
                    _ExpandedFields[ntype.Name] = ntype;
                    _StaticFields[ntype.Name] = ntype;
                }
            }
        }

        protected internal object GetFieldDescFor(object tar, string key)
        {
            object finfo = null;
            if (!_ExpandedFields.TryGetValue(key, out finfo))
            {
                _ExpandedFields[key] = null;
                _StaticFields[key] = null;
                var pinfo = _BindingType.GetProperty(key);
                if (pinfo != null)
                {
                    bool isStatic = false;
                    var pminfo = pinfo.GetGetMethod();
                    if (pminfo != null)
                    {
                        isStatic = pminfo.IsStatic;
                    }
                    else
                    {
                        pminfo = pinfo.GetSetMethod();
                        if (pminfo != null)
                        {
                            isStatic = pminfo.IsStatic;
                        }
                    }

                    _ExpandedFields[key] = pinfo;
                    if (isStatic)
                    {
                        _StaticFields[key] = pinfo;
                    }
                    if (tar != null || isStatic)
                    {
                        finfo = pinfo;
                    }
                }
                else
                {
                    var finfo2 = _BindingType.GetField(key);
                    if (finfo2 != null)
                    {
                        _ExpandedFields[key] = finfo2;
                        if (finfo2.IsStatic)
                        {
                            _StaticFields[key] = finfo2;
                        }
                        if (tar != null || finfo2.IsStatic)
                        {
                            finfo = finfo2;
                        }
                    }
                    else
                    {
                        var finfo3 = _BindingType.GetEvent(key);
                        if (finfo3 != null)
                        {
                            _ExpandedFields[key] = finfo3;
                            bool isStatic = finfo3.GetAddMethod().IsStatic;
                            if (isStatic)
                            {
                                _StaticFields[key] = finfo3;
                            }
                            if (tar != null || isStatic)
                            {
                                finfo = finfo3;
                            }
                        }
                    }
                }
            }
            return finfo;
        }

        public object GetFieldFor(object tar, object key)
        {
            if (_BindingType == null)
                return null;
            tar = tar.UnwrapDynamic();
            key = key.UnwrapDynamic();

            var fields = _ExpandedFields;
            if (tar == null)
            {
                fields = _StaticFields;
            }
            object finfo = null;
            if (key is string)
            {
                finfo = GetFieldDescFor(tar, key as string);
                if (finfo is PropertyInfo)
                {
                    var pmethod = ((PropertyInfo)finfo).GetGetMethod();
                    if (pmethod != null)
                    {
                        try
                        {
                            //return ((PropertyInfo)finfo).GetValue(tar, null).WrapDynamic();
                            var args = ObjectPool.GetParamsFromPool(0);
                            var rv = pmethod.Invoke(tar, args);
                            ObjectPool.ReturnParamsToPool(args);
                            return rv;
                        }
                        catch
                        {
                            if (GLog.IsLogErrorEnabled) GLog.LogError("Unable to get property: " + key);
                            throw;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (finfo is FieldInfo)
                {
                    return ((FieldInfo)finfo).GetValue(tar);
                }
                else if (finfo is ICallableCore)
                {
                    var core = finfo as ICallableCore;
                    var callable = ClrCallable.GetFromPool(core, tar);
                    return callable;
                }
                else if (finfo is EventInfo)
                {
                    return ClrEventWrapper.GetFromPool(finfo as EventInfo, tar);
                }
            }
            if (finfo == null)
            {
                object indexmethod = null;
                if (fields.TryGetValue("get_Item", out indexmethod))
                {
                    if (indexmethod is ICallableCore)
                    {
                        var callable = ClrCallable.GetFromPool((ICallableCore)indexmethod, tar);
                        var rv = callable.Call(key);
                        callable.ReturnToPool();
                        if (rv != null && rv.Length > 0)
                        {
                            return rv[0];
                        }
                    }
                }
                else if (tar is IList)
                { // special treat to array.
                    var ikey = key.ConvertType(typeof(int));
                    if (ikey != null)
                    {
                        try
                        {
                            var rv = ((IList)tar)[(int)ikey];
                            return rv;
                        }
                        catch { }
                    }
                }
            }
            return finfo;
        }
        public bool SetFieldFor(object tar, object key, object val)
        {
            if (_BindingType == null)
                return false;
            tar = tar.UnwrapDynamic();
            key = key.UnwrapDynamic();
            var rval = val.UnwrapDynamic();

            var fields = _ExpandedFields;
            if (tar == null)
            {
                fields = _StaticFields;
            }
            object finfo = null;
            if (key is string)
            {
                finfo = GetFieldDescFor(tar, key as string);
                if (finfo is PropertyInfo)
                {
                    var pmethod = ((PropertyInfo)finfo).GetSetMethod();
                    if (pmethod != null)
                    {
                        try
                        {
                            Type type = ((PropertyInfo)finfo).PropertyType;
                            if (type.IsSubclassOf(typeof(Delegate)) && rval is BaseDynamic)
                            {
                                rval = Capstones.LuaWrap.CapsLuaDelegateGenerator.CreateDelegate(type, rval as BaseDynamic);
                            }
                            if (rval.CanConvertRawObj(type))
                            {
                                //((PropertyInfo)finfo).SetValue(tar, rval.ConvertType(type), null);
                                var args = ObjectPool.GetParamsFromPool(1);
                                args[0] = rval.ConvertType(type);
                                pmethod.Invoke(tar, args);
                                ObjectPool.ReturnParamsToPool(args);
                                return true;
                            }
                        }
                        catch (Exception e)
                        {
                            // TODO: the debug info will decrease the performance. we should develop a generic lua-call-clr log.
                            // perhaps we should make a Call and a TryCall. perhaps we should show which lua-state is doing the log. perhaps we should show lua-stack 
                            if (GLog.IsLogInfoEnabled) GLog.LogInfo("Unable to set property: " + key + '\n' + e.ToString());
                            throw;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else if (finfo is FieldInfo)
                {
                    Type type = ((FieldInfo)finfo).FieldType;
                    if (type.IsSubclassOf(typeof(Delegate)) && rval is BaseDynamic)
                    {
                        rval = Capstones.LuaWrap.CapsLuaDelegateGenerator.CreateDelegate(type, rval as BaseDynamic);
                    }
                    if (rval.CanConvertRawObj(type))
                    {
                        try
                        {
                            ((FieldInfo)finfo).SetValue(tar, rval.ConvertType(type));
                            return true;
                        }
                        catch
                        {
                            if (GLog.IsLogInfoEnabled) GLog.LogInfo("Unable to set field: " + key);
                            throw;
                        }
                    }
                    return true;
                }
                //else if (finfo is ICallableCore)
                //{
                //    // can't set a method;
                //    return true;
                //}
                else if (finfo is EventInfo)
                {
                    // we should only use +/- on EventInfo
                    return true;
                }
            }
            if (finfo == null)
            {
                object indexmethod = null;
                if (fields.TryGetValue("set_Item", out indexmethod))
                {
                    if (indexmethod is ICallableCore)
                    {
                        var callable = ClrCallable.GetFromPool((ICallableCore)indexmethod, tar);
                        var rv = callable.Call(key, rval);
                        callable.ReturnToPool();
                        if (rv != null)
                        {
                            return true;
                        }
                        return false;
                    }
                }
                else if (tar is IList)
                { // special treat to array.
                    var ikey = key.ConvertType(typeof(int));
                    if (ikey != null)
                    {
                        try
                        {
                            var cval = rval;
                            if (tar.GetType().HasElementType)
                            {
                                cval = rval.ConvertType(tar.GetType().GetElementType());
                            }

                            ((IList)tar)[(int)ikey] = cval;
                            return true;
                        }
                        catch { }
                    }
                }
            }
            return false;
        }

        public object[] Call(object target, params object[] args)
        {
            if (args == null || args.Length < 1)
            {
                if (_BindingType.IsValueType())
                {
                    try
                    {
                        var rv = ObjectPool.GetReturnValueFromPool(1);
                        rv[0] = Activator.CreateInstance(_BindingType);
                        return rv;
                    }
                    catch { }
                }
            }
            if (_ConstructorCallable != null)
            {
                return _ConstructorCallable.Call(target, args);
            }
            return null;
        }

        public BaseDynamic CreateOverloadSelector(object tar)
        {
            if (_ConstructorCallable != null)
            {
                return _ConstructorCallable.CreateOverloadSelector(tar);
            }
            return null;
        }
    }

    public class ClrTypeWrapper : BaseDynamic
    {
#if ENABLE_OBJ_POOL
        [ThreadStatic] protected internal static ObjectPool.GenericInstancePool<ClrTypeWrapper> _Pool;
        protected internal static ObjectPool.GenericInstancePool<ClrTypeWrapper> Pool
        {
            get
            {
                if (_Pool == null)
                {
                    _Pool = new ObjectPool.GenericInstancePool<ClrTypeWrapper>();
                }
                return _Pool;
            }
        }
#endif
        public static ClrTypeWrapper GetFromPool(Type type)
        {
#if ENABLE_OBJ_POOL
            return Pool.GetFromPool(() => new ClrTypeWrapper(type), wrapper => wrapper.Init(type));
#else
            return new ClrTypeWrapper(type);
#endif
        }
        public static ClrTypeWrapper GetFromPool(IClrTypeCore core)
        {
#if ENABLE_OBJ_POOL
            return Pool.GetFromPool(() => new ClrTypeWrapper(core), wrapper => wrapper.Init(core));
#else
            return new ClrTypeWrapper(core);
#endif
        }
        public override void ReturnToPool()
        {
            TypeObjectWrapper.ReturnToPool();
#if ENABLE_OBJ_POOL
            Pool.ReturnToPool(this);
#endif
        }

        public ClrObjectWrapper TypeObjectWrapper { get; internal set; }

        public ClrTypeWrapper(Type type)
        {
            Init(type);
        }
        public ClrTypeWrapper(IClrTypeCore core)
        {
            Init(core);
        }
        public void Init(Type type)
        {
            _Binding = type;
            BindingCore = ClrTypeCore.GetTypeCore(type);
            TypeObjectWrapper = ClrObjectWrapper.GetFromPool(type, null);
        }
        public void Init(IClrTypeCore core)
        {
            _Binding = this;
            BindingCore = core;
            if (core != null && core.BindingType != null)
            {
                _Binding = core.BindingType;
            }
            TypeObjectWrapper = ClrObjectWrapper.GetFromPool(BindingType, null);
        }

        private IClrTypeCore _BindingCore;
        protected internal IClrTypeCore BindingCore
        {
            set
            {
                _BindingCore = value;
            }
            get { return _BindingCore; }
        }
        public Type BindingType
        {
            get
            {
                var type = _Binding as Type;
                if (type != null)
                {
                    return type;
                }
                if (BindingCore.BindingType != null)
                {
                    return BindingCore.BindingType;
                }
                if (BindingCore is IClrTypeCoreForTypeGroup)
                {
                    return ((IClrTypeCoreForTypeGroup)BindingCore).MajorType;
                }
                return null;
            }
            //internal set
            //{
            //    _Binding = value;
            //}
        }

        protected internal override object GetFieldImp(object key)
        {
            if (BindingCore != null)
            {
                string strkey = key.UnwrapDynamic<string>();
                if ("___ol" == strkey)
                {
                    return BindingCore.CreateOverloadSelector(null);
                }
                else if ("___ctor" == strkey)
                {
                    return ClrCallable.GetFromPool(BindingCore, null);
                }
                var rv = BindingCore.GetFieldFor(null, key);
                if (rv != null)
                    return rv;
            }
            return TypeObjectWrapper.GetFieldImp(key);
        }
        protected internal override bool SetFieldImp(object key, object val)
        {
            if (BindingCore != null)
            {
                var rv = BindingCore.SetFieldFor(null, key, val);
                if (rv)
                {
                    return rv;
                }
            }
            return TypeObjectWrapper.SetFieldImp(key, val);
        }
        public override object[] Call(params object[] args)
        {
            if (BindingCore != null)
            {
                var callable = ClrCallable.GetFromPool(BindingCore, null);
                var rv = callable.Call(args);
                callable.ReturnToPool();
                return rv;
            }
            return null;
        }

        public override string ToString()
        {
            var type = BindingType;
            if (type != null)
            {
                return type.ToString();
            }
            return base.ToString();
        }
    }
}