using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Capstones.PlatExt;

namespace Capstones.Dynamic
{
    public interface IGenericCore
    {
        ICallableCore BindTypes(Types genericTypes);
        ICallableCore DetermineTypes(Types types);
    }

    internal class GenericCallableCore : UniqueCallableCore, IGenericCore
    {
        protected internal Dictionary<string, int> _GenericParams = new Dictionary<string, int>();
        protected internal Dictionary<Types, UniqueCallableCore> _BindedGenericCache;

        protected internal GenericCallableCore(MethodBase minfo)
            : this(minfo, null)
        {
        }

        protected internal GenericCallableCore(MethodBase minfo, Dictionary<Types, UniqueCallableCore> cache)
            : base(minfo)
        {
            //if (minfo.IsGenericMethod && minfo.ContainsGenericParameters) // should only be created with generic methods
            {
                var pars = minfo.GetGenericArguments();
                if (pars != null)
                {
                    for (int i = 0; i < pars.Length; ++i)
                    {
                        _GenericParams[pars[i].Name] = i;
                    }
                }
            }
            if (cache == null)
            {
                cache = new Dictionary<Types, UniqueCallableCore>();
            }
            _BindedGenericCache = cache;
        }
        protected internal GenericCallableCore(MethodBase minfo, int reserved)
            : base(minfo)
        {
        }

        public virtual ICallableCore BindTypes(Types genericTypes)
        {
            if (genericTypes.Count == _GenericParams.Count)
            {
                UniqueCallableCore core = null;
                _BindedGenericCache.TryGetValue(genericTypes, out core);
                if (core != null)
                {
                    return core;
                }
                else
                {
                    MethodInfo mi = null;
                    try
                    {
                        mi = ((MethodInfo)Method).MakeGenericMethod(genericTypes.ToArray());
                    }
                    catch { }
                    if (mi != null)
                    {
                        core = new UniqueCallableCore(mi);
                        _BindedGenericCache[genericTypes] = core;
                        return core;
                    }
                    else
                    {
                        if (GLog.IsLogInfoEnabled) GLog.LogInfo("Unable to bind types to generic method.");
                    }
                }
            }
            return null;
        }

        protected internal override bool CanConvertParam(Type src, Type dst)
        {
            if (dst != null && dst.IsGenericParameter)
            {
                if (src == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return base.CanConvertParam(src, dst);
        }
        public override int CanCall(Types pt)
        {
            if (pt.Count == _GenericParams.Count)
            { // check if this is the generic call
                bool allTypes = true;
                foreach (var curType in pt)
                {
                    if (!typeof(Type).IsAssignableFrom(curType))
                    {
                        allTypes = false;
                        break;
                    }
                }
                if (allTypes)
                {
                    return 0;
                }
            }
            return base.CanCall(pt);
        }
        public override object[] Call(object target, params object[] args)
        {
            if (args == null || args.Length <= 0)
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("Unable to determine types of the generic method from the calling args - empty args.");
            }
            else
            {
                if (args.Length == _GenericParams.Count)
                {
                    Types types = new Types();
                    bool allTypes = true;
                    foreach (var arg in args)
                    {
                        if (!(arg is Type))
                        {
                            allTypes = false;
                            break;
                        }
                    }
                    if (allTypes)
                    {
                        for (int i = 0; i < args.Length; ++i)
                        {
                            types.Add(args[i] as Type);
                        }
                        ICallableCore core = BindTypes(types);
                        if (core != null)
                        {
                            var rv = ObjectPool.GetReturnValueFromPool(1);
                            rv[0] = ClrCallable.GetFromPool(core, target);
                            return rv;
                        }
                    }
                }

                // try to determine generic type params.
                var cored = this.DetermineGenericTypeArg(args);
                if (cored != null)
                {
                    return cored.Call(target, args);
                }
                else
                {
                    if (GLog.IsLogInfoEnabled) GLog.LogInfo("Unable to determine types of the generic method from the calling args.");
                }
            }
            return null;
        }
        public override BaseDynamic CreateOverloadSelector(object tar)
        {
            return new GenericSelector(this, tar);
        }

        public ICallableCore DetermineTypes(Types types)
        {
            if (types != null)
            {
                Dictionary<string, Type> determined = new Dictionary<string, Type>();
                int mlen = Math.Max(_MethodParamTypes.Count, types.Count);
                for (int i = 0; i < mlen; ++i)
                {
                    Type argtype = null;
                    if (i < _MethodParamTypes.Count)
                    {
                        argtype = _MethodParamTypes[i];
                    }
                    Type argt = null;
                    if (i < types.Count)
                    {
                        argt = types[i];
                    }
                    if (argtype == null)
                    {
                        if (argt != null)
                        {
                            return null;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    if (argt != null)
                    {
                        if (i == _LastIsParams)
                        {
                            var etype = argtype.GetElementType();
                            if (argt.IsArray && types.Count == i + 1)
                            {
                                var dict = argtype.DetermineGenericTypeArg(argt);
                                if (dict == null || determined.MergeDeterminedType(dict) == null || dict.Count == 0 && !argt.CanConvertRaw(argtype))
                                {
                                    //if (GLog.IsLogInfoEnabled)GLog.LogInfo("Unable to determine types of the generic method from the calling args - not match");
                                    return null;
                                }
                            }
                            else
                            {
                                for (int j = i; j < types.Count; ++j)
                                {
                                    var dict = etype.DetermineGenericTypeArg(types[j]);
                                    if (dict == null || determined.MergeDeterminedType(dict) == null || dict.Count == 0 && !argt.CanConvertRaw(argtype))
                                    {
                                        //if (GLog.IsLogInfoEnabled)GLog.LogInfo("Unable to determine types of the generic method from the calling args - not match");
                                        return null;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var dict = argtype.DetermineGenericTypeArg(argt);
                            if (dict == null || determined.MergeDeterminedType(dict) == null || dict.Count == 0 && !argt.CanConvertRaw(argtype))
                            {
                                //if (GLog.IsLogInfoEnabled)GLog.LogInfo("Unable to determine types of the generic method from the calling args - not match");
                                return null;
                            }
                        }
                    }
                }
                if (determined.Count == _GenericParams.Count)
                {
                    Types typesd = new Types();
                    for (int i = 0; i < _GenericParams.Count; ++i)
                    {
                        typesd.Add(null);
                    }
                    foreach (var kvp in _GenericParams)
                    {
                        typesd[kvp.Value] = determined[kvp.Key];
                    }
                    ICallableCore core = BindTypes(typesd);
                    if (core != null)
                    {
                        return core;
                    }
                    //else
                    //{
                    //    if (GLog.IsLogInfoEnabled)GLog.LogInfo("Unable to determine types of the generic method from the calling args - may be refused by constraints");
                    //}
                }
                //else
                //{
                //    if (GLog.IsLogInfoEnabled)GLog.LogInfo("Unable to determine types of the generic method from the calling args - some generic arg can't be determined");
                //}
            }
            return null;
        }
    }

    internal class GenericSelector : NoBindingDynamic
    {
        internal IGenericCore _Core;
        public object Target { get; set; }

        public override object[] Call(params object[] args)
        {
            Types types = new Types();
            if (args != null)
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    var type = args[i].UnwrapDynamic() as Type;
                    if (type == null)
                    {
                        return null;
                    }
                    types.Add(type);
                }
            }
            var core = _Core.BindTypes(types);
            if (core != null)
            {
                var rv = ObjectPool.GetReturnValueFromPool(1);
                rv[0] = ClrCallable.GetFromPool(core, Target);
                return rv;
            }
            return null;
        }

        internal GenericSelector(IGenericCore core, object tar)
        {
            _Core = core;
            Target = tar;
        }
    }

    internal class GroupGenericCore : ICallableCore, IGenericCore
    {
        internal LinkedList<GenericCallableCore> _SeqCache = new LinkedList<GenericCallableCore>();
        internal Dictionary<int, LinkedListNode<GenericCallableCore>> _GenericParamCountCache = new Dictionary<int, LinkedListNode<GenericCallableCore>>();
        //internal Dictionary<ParamTypes, ICallableCore> _BindedGenericCache = new Dictionary<ParamTypes,ICallableCore>();

        protected internal GroupGenericCore(MethodBase[] minfos)
        {
            if (minfos != null)
            {
                GenericCallableCore[] callables = new GenericCallableCore[minfos.Length];
                for (int i = 0; i < minfos.Length; ++i)
                {
                    callables[i] = new GenericCallableCore(minfos[i]);
                }
                Array.Sort(callables, (ca, cb) =>
                {
                    var pa = ca.Method.GetGenericArguments();
                    var pb = cb.Method.GetGenericArguments();

                    var npa = pa == null ? 0 : pa.Length;
                    var npb = pb == null ? 0 : pb.Length;

                    if (npa != npb)
                    {
                        return npa - npb;
                    }

                    return GenericHelper.CompareGenericParamTypes(ca._MethodParamTypes, cb._MethodParamTypes);
                });
                for (int i = 0; i < minfos.Length; ++i)
                {
                    var core = callables[i];
                    var cnt = core._GenericParams.Count;
                    var node = _SeqCache.AddLast(callables[i]);
                    if (!_GenericParamCountCache.ContainsKey(cnt))
                    {
                        _GenericParamCountCache[cnt] = node;
                    }
                }
            }
        }

        public ICallableCore BindTypes(Types genericTypes)
        {
            //{
            //    ICallableCore core;
            //    if (_BindedGenericCache.TryGetValue(genericTypes, out core))
            //    {
            //        return core;
            //    }
            //}

            int cnt = genericTypes.Count;
            LinkedListNode<GenericCallableCore> node;
            _GenericParamCountCache.TryGetValue(cnt, out node);
            if (node == null)
            {
                //_BindedGenericCache[genericTypes] = null;
                return null;
            }
            List<UniqueCallableCore> methods = new List<UniqueCallableCore>();
            do
            {
                UniqueCallableCore core = node.Value.BindTypes(genericTypes) as UniqueCallableCore;
                if (core != null)
                {
                    methods.Add(core);
                }
                node = node.Next;
            } while (node != null && node.Value._GenericParams.Count == cnt);
            if (methods.Count <= 0)
            {
                //_BindedGenericCache[genericTypes] = null;
                return null;
            }
            else if (methods.Count == 1)
            {
                var core = methods[0];
                //_BindedGenericCache[genericTypes] = core;
                return core;
            }
            else
            {
                var core = new GroupCallableCore(null);
                for (int i = 0; i < methods.Count; ++i)
                {
                    core._SeqCache.AddLast(methods[i]);
                }
                //_BindedGenericCache[genericTypes] = core;
                return core;
            }
        }

        public ICallableCore DetermineTypes(Types types)
        {
            foreach (var gcore in _SeqCache)
            {
                var core = gcore.DetermineTypes(types);
                if (core != null)
                {
                    // TODO: 1, return a group core if more than one gcore is matched. 2, cache the result.
                    return core;
                }
            }
            return null;
        }

        public object[] Call(object target, params object[] args)
        {
            if (args == null || args.Length <= 0)
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("Unable to determine types of the generic method from the calling args - empty args.");
            }
            else
            {
                Types types = new Types();
                bool allTypes = true;
                foreach (var arg in args)
                {
                    if (!(arg is Type))
                    {
                        allTypes = false;
                        break;
                    }
                }
                if (allTypes)
                {
                    for (int i = 0; i < args.Length; ++i)
                    {
                        types.Add(args[i] as Type);
                    }
                    ICallableCore core = BindTypes(types);
                    if (core != null)
                    {
                        var rv = ObjectPool.GetReturnValueFromPool(1);
                        rv[0] = ClrCallable.GetFromPool(core, target);
                        return rv;
                    }
                }

                // try to determine generic type params.
                var cored = this.DetermineGenericTypeArg(args);
                if (cored != null)
                {
                    return cored.Call(target, args);
                }
                else
                {
                    if (GLog.IsLogInfoEnabled) GLog.LogInfo("Unable to determine types of the generic method from the calling args.");
                }
            }
            return null;
        }

        public BaseDynamic CreateOverloadSelector(object tar)
        {
            return new GenericSelector(this, tar);
        }
    }

    internal class CallableCoreWithGeneric : ICallableCore, IGenericCore
    {
        internal ICallableCore _NormalCore;
        internal ICallableCore _GenericCore;

        protected internal CallableCoreWithGeneric(MethodBase[] minfos, Dictionary<Types, UniqueCallableCore> cache)
        {
            List<MethodBase> nmethods = new List<MethodBase>();
            List<MethodBase> gmethods = new List<MethodBase>();

            if (minfos != null)
            {
                foreach (var method in minfos)
                {
                    if (method != null)
                    {
                        if (method.IsGenericMethod && method.ContainsGenericParameters)
                        {
                            gmethods.Add(method);
                        }
                        else
                        {
                            nmethods.Add(method);
                        }
                    }
                }
            }

            if (gmethods.Count > 0)
            {
                if (gmethods.Count > 1)
                {
                    _GenericCore = new GroupGenericCore(gmethods.ToArray());
                }
                else
                {
                    _GenericCore = new GenericCallableCore(gmethods[0]);
                }
            }
            var rv = new PackedCallableCore(nmethods.ToArray());
            if (rv._CallableGroups.Count > 0)
            {
                if (rv._CallableGroups.Count > 1)
                {
                    _NormalCore = rv;
                }
                else
                {
                    _NormalCore = rv._CallableGroups.First().Value;
                }
            }
        }
        protected internal CallableCoreWithGeneric(MethodBase[] minfos)
            : this(minfos, null)
        {
        }

        public ICallableCore BindTypes(Types genericTypes)
        {
            var gcore = _GenericCore as IGenericCore;
            if (gcore != null)
            {
                return gcore.BindTypes(genericTypes);
            }
            return null;
        }

        public ICallableCore DetermineTypes(Types types)
        {
            var gcore = _GenericCore as IGenericCore;
            if (gcore != null)
            {
                return gcore.DetermineTypes(types);
            }
            return null;
        }

        public object[] Call(object target, params object[] args)
        {
            if (_GenericCore != null && args != null)
            {
                Types types = new Types();
                bool allTypes = true;
                foreach (var arg in args)
                {
                    if (!(arg is Type))
                    {
                        allTypes = false;
                        break;
                    }
                }
                if (allTypes)
                {
                    for (int i = 0; i < args.Length; ++i)
                    {
                        types.Add(args[i] as Type);
                    }
                    ICallableCore core = BindTypes(types);
                    if (core != null)
                    {
                        var rv = ObjectPool.GetReturnValueFromPool(1);
                        rv[0] = ClrCallable.GetFromPool(core, target);
                        return rv;
                    }
                }
            }

            if (_NormalCore != null)
            {
                var rv = _NormalCore.Call(target, args);
                if (rv != null)
                {
                    return rv;
                }
            }
            if (_GenericCore != null)
            {
                return _GenericCore.Call(target, args);
            }
            return null;
        }

        public BaseDynamic CreateOverloadSelector(object tar)
        {
            return new CallableOrGenericSelector(this, tar);;
        }
    }

    internal class CallableOrGenericSelector : NoBindingDynamic
    {
        internal CallableCoreWithGeneric _Core;
        public object Target { get; set; }

        public override object[] Call(params object[] args)
        {
            Types types = new Types();
            bool allTypes = true;
            if (args != null)
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    var type = args[i].UnwrapDynamic() as Type;
                    if (type == null)
                    {
                        allTypes = false;
                        break;
                    }
                    types.Add(type);
                }
            }
            if (allTypes)
            {
                var core = _Core.BindTypes(types);
                if (core != null)
                {
                    var rv = ObjectPool.GetReturnValueFromPool(1);
                    rv[0] = ClrCallable.GetFromPool(core, Target);
                    return rv;
                }
            }
            if (_Core._NormalCore != null)
            {
                return _Core._NormalCore.CreateOverloadSelector(Target).Call(args);
            }
            else
            {
                return null;
            }
        }

        internal CallableOrGenericSelector(CallableCoreWithGeneric core, object tar)
        {
            _Core = core;
            Target = tar;
        }
    }

    internal class GenericConstructorCore : GenericCallableCore
    {
        protected internal ClrGenericTypeCore _GenericTypeCore;
        protected internal GenericConstructorCore(ClrGenericTypeCore gtcore, MethodBase minfo)
            : base(minfo, 0)
        {
            _GenericTypeCore = gtcore;
            _GenericParams = gtcore._GenericParams;
        }

        public override ICallableCore BindTypes(Types genericTypes)
        {
            return _GenericTypeCore.BindTypes(genericTypes);
        }
    }

    internal class ClrGenericTypeCore : IClrTypeCore, IGenericCore
    {
        internal static ClrGenericTypeCore GetGenericTypeCore(Type type, Dictionary<Types, ClrTypeCore> cache)
        {
            if (type != null)
            {
                {
                    IClrTypeCore core = null;
                    if (ClrTypeCore._TypeCoreCache.TryGetValue(type, out core))
                    {
                        var rv = core as ClrGenericTypeCore;
                        if (rv != null)
                            return rv;
                    }
                }
                {
                    var core = new ClrGenericTypeCore(type, cache);
                    ClrTypeCore._TypeCoreCache[type] = core;
                    return core;
                }
            }
            return null;
        }

        protected internal Dictionary<Types, ClrTypeCore> _BindedGenericCache;
        protected internal Dictionary<string, int> _GenericParams = new Dictionary<string, int>();
        protected internal LinkedList<GenericConstructorCore> _Constructors = new LinkedList<GenericConstructorCore>();
        protected internal Type _BindingType;

        public ClrGenericTypeCore(Type type, Dictionary<Types, ClrTypeCore> cache)
        {
            if (cache == null)
            {
                cache = new Dictionary<Types, ClrTypeCore>();
            }
            _BindedGenericCache = cache;

            _BindingType = type;
            var pars = type.GetGenericArguments();
            if (pars != null)
            {
                for (int i = 0; i < pars.Length; ++i)
                {
                    _GenericParams[pars[i].Name] = i;
                }
            }

            var ctors = type.GetConstructors();
            if (ctors != null)
            {
                GenericConstructorCore[] ctorcores = new GenericConstructorCore[ctors.Length];
                for (int i = 0; i < ctors.Length; ++i)
                {
                    var core = new GenericConstructorCore(this, ctors[i]);
                    ctorcores[i] = core;
                }
                Array.Sort(ctorcores, (ca, cb) =>
                {
                    return GenericHelper.CompareGenericParamTypes(ca._MethodParamTypes, cb._MethodParamTypes);
                });
                for (int i = 0; i < ctorcores.Length; ++i)
                {
                    _Constructors.AddLast(ctorcores[i]);
                }
            }
        }
        public ClrGenericTypeCore(Type type)
            : this(type, null)
        {
        }

        public Type BindingType
        {
            get { return _BindingType; }
        }

        public ICallableCore BindTypes(Types genericTypes)
        {
            ClrTypeCore core;
            _BindedGenericCache.TryGetValue(genericTypes, out core);
            if (core != null)
            {
                return core;
            }

            var type = BindingType;
            if (type != null)
            {
                if (genericTypes.Count == _GenericParams.Count)
                {
                    try
                    {
                        var btype = type.MakeGenericType(genericTypes.ToArray());
                        if (btype != null)
                        {
                            var rv = ClrTypeCore.GetTypeCore(btype);
                            if (rv != null)
                            {
                                _BindedGenericCache[genericTypes] = rv as ClrTypeCore;
                                return rv;
                            }
                        }
                    }
                    catch { }
                }
            }
            return null;
        }
        public ICallableCore DetermineTypes(Types types)
        {
            foreach (var gcore in _Constructors)
            {
                var core = gcore.DetermineTypes(types);
                if (core != null)
                {
                    return core;
                }
            }
            return null;
        }

        public object GetFieldFor(object tar, object key)
        {
            //if (GLog.IsLogInfoEnabled)GLog.LogInfo("Unable to get a field on unbinded generic type.");
            return null;
        }

        public bool SetFieldFor(object tar, object key, object val)
        {
            //if (GLog.IsLogInfoEnabled)GLog.LogInfo("Unable to set a field on unbinded generic type.");
            return false;
        }

        public object[] Call(object target, params object[] args)
        {
            if (args == null || args.Length <= 0)
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("Unable to determine types of the generic type from the calling args - empty args.");
            }
            else
            {
                Types types = new Types();
                bool allTypes = true;
                foreach (var arg in args)
                {
                    if (!(arg is Type))
                    {
                        allTypes = false;
                        break;
                    }
                }
                if (allTypes)
                {
                    for (int i = 0; i < args.Length; ++i)
                    {
                        types.Add(args[i] as Type);
                    }
                    var core = BindTypes(types) as IClrTypeCore;
                    if (core != null)
                    {
                        var rv = ObjectPool.GetReturnValueFromPool(1);
                        rv[0] = ClrTypeWrapper.GetFromPool(core);
                        return rv;
                    }
                }

                // try to determine generic type params.
                var cored = this.DetermineGenericTypeArg(args);
                if (cored != null)
                {
                    return cored.Call(target, args);
                }
                else
                {
                    if (GLog.IsLogInfoEnabled) GLog.LogInfo("Unable to determine types of the generic type from the calling args.");
                }
            }
            return null;
        }

        public BaseDynamic CreateOverloadSelector(object tar)
        {
            return new GenericTypeSelector(this, tar);
        }
    }

    internal class GenericTypeSelector : NoBindingDynamic
    {
        internal IGenericCore _Core;
        public object Target { get; set; }

        public override object[] Call(params object[] args)
        {
            Types types = new Types();
            if (args != null)
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    var type = args[i].UnwrapDynamic() as Type;
                    if (type == null)
                    {
                        return null;
                    }
                    types.Add(type);
                }
            }
            var core = _Core.BindTypes(types) as IClrTypeCore;
            if (core != null)
            {
                var rv = ObjectPool.GetReturnValueFromPool(1);
                rv[0] = ClrTypeWrapper.GetFromPool(core);
                return rv;
            }
            return null;
        }

        internal GenericTypeSelector(IGenericCore core, object tar)
        {
            _Core = core;
            Target = tar;
        }
    }

    internal class GroupGenericTypeCore : IClrTypeCore, IGenericCore, IClrTypeCoreForTypeGroup
    {
        internal Type[] _GroupTypes;
        internal SortedDictionary<int, ClrGenericTypeCore> _GenericTypeCores = new SortedDictionary<int, ClrGenericTypeCore>();
        protected internal Dictionary<Types, ClrTypeCore> _BindedGenericCache;

        protected internal GroupGenericTypeCore(Type[] types, Dictionary<Types, ClrTypeCore> cache)
        {
            if (cache == null)
            {
                cache = new Dictionary<Types, ClrTypeCore>();
            }
            _BindedGenericCache = cache;

            _GroupTypes = types;
            if (types != null)
            {
                foreach(var type in types)
                {
                    if (type != null && type.IsGenericType() && type.ContainsGenericParameters())
                    {
                        var cnt = type.GetGenericArguments().Length;
                        var core = ClrGenericTypeCore.GetGenericTypeCore(type, cache);
                        _GenericTypeCores[cnt] = core;
                    }
                }
            }
        }
        protected internal GroupGenericTypeCore(Type[] types)
            : this(types, null)
        {
        }

        public ICallableCore BindTypes(Types genericTypes)
        {
            {
                ClrTypeCore core;
                _BindedGenericCache.TryGetValue(genericTypes, out core);
                if (core != null)
                {
                    return core;
                }
            }

            int cnt = genericTypes.Count;
            ClrGenericTypeCore gcore;
            _GenericTypeCores.TryGetValue(cnt, out gcore);
            if (gcore == null)
            {
                return null;
            }

            return gcore.BindTypes(genericTypes);
        }

        public ICallableCore DetermineTypes(Types types)
        {
            foreach (var kvp in _GenericTypeCores)
            {
                var core = kvp.Value.DetermineTypes(types);
                if (core != null)
                {
                    return core;
                }
            }
            return null;
        }

        public object[] Call(object target, params object[] args)
        {
            if (args == null || args.Length <= 0)
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("Unable to determine types of the generic method from the calling args - empty args.");
            }
            else
            {
                Types types = new Types();
                bool allTypes = true;
                foreach (var arg in args)
                {
                    if (!(arg is Type))
                    {
                        allTypes = false;
                        break;
                    }
                }
                if (allTypes)
                {
                    for (int i = 0; i < args.Length; ++i)
                    {
                        types.Add(args[i] as Type);
                    }
                    var core = BindTypes(types) as IClrTypeCore;
                    if (core != null)
                    {
                        var rv = ObjectPool.GetReturnValueFromPool(1);
                        rv[0] = ClrTypeWrapper.GetFromPool(core);
                        return rv;
                    }
                }

                // try to determine generic type params.
                var cored = this.DetermineGenericTypeArg(args);
                if (cored != null)
                {
                    return cored.Call(target, args);
                }
                else
                {
                    if (GLog.IsLogInfoEnabled) GLog.LogInfo("Unable to determine types of the generic method from the calling args.");
                }
            }
            return null;
        }

        public BaseDynamic CreateOverloadSelector(object tar)
        {
            return new GenericTypeSelector(this, tar);
        }

        public Type BindingType
        {
            get
            {
                //if (GLog.IsLogInfoEnabled)GLog.LogInfo("Unable to get type of a group of generic types.");
                return null;
            }
        }

        public object GetFieldFor(object tar, object key)
        {
            //if (GLog.IsLogInfoEnabled)GLog.LogInfo("Unable to get a field on unbinded generic type.");
            return null;
        }

        public bool SetFieldFor(object tar, object key, object val)
        {
            //if (GLog.IsLogInfoEnabled)GLog.LogInfo("Unable to set a field on unbinded generic type.");
            return false;
        }

        public Type MajorType
        {
            get
            {
                if (_GroupTypes != null && _GroupTypes.Length > 0)
                {
                    return _GroupTypes[0];
                }
                return null;
            }
        }

        public Type[] AllTypes
        {
            get
            {
                return _GroupTypes;
            }
        }
    }

    internal class ClrTypeCoreWithGeneric : IClrTypeCore, IGenericCore, IClrTypeCoreForTypeGroup
    {
        internal IClrTypeCore _NormalCore;
        internal IClrTypeCore _GenericCore;
        internal Type[] _GroupTypes;

        protected internal ClrTypeCoreWithGeneric(Type ntype, Type[] gtypes, Dictionary<Types, ClrTypeCore> cache)
        {
            List<Type> types = new List<Type>();
            if (ntype != null)
            {
                _NormalCore = ClrTypeCore.GetTypeCore(ntype);
                types.Add(ntype);
            }
            if (gtypes != null && gtypes.Length > 0)
            {
                if (gtypes.Length > 1)
                {
                    _GenericCore = new GroupGenericTypeCore(gtypes, cache);
                }
                else
                {
                    _GenericCore = ClrGenericTypeCore.GetGenericTypeCore(gtypes[0], cache);
                }
                types.AddRange(gtypes);
            }
            _GroupTypes = types.ToArray();
        }
        protected internal ClrTypeCoreWithGeneric(Type ntype, Type[] gtypes)
            : this(ntype, gtypes, null)
        {
        }

        public ICallableCore BindTypes(Types genericTypes)
        {
            var gcore = _GenericCore as IGenericCore;
            if (gcore != null)
            {
                return gcore.BindTypes(genericTypes);
            }
            return null;
        }

        public ICallableCore DetermineTypes(Types types)
        {
            var gcore = _GenericCore as IGenericCore;
            if (gcore != null)
            {
                return gcore.DetermineTypes(types);
            }
            return null;
        }

        public object[] Call(object target, params object[] args)
        {
            if (_GenericCore != null && args != null)
            {
                Types types = new Types();
                bool allTypes = true;
                foreach (var arg in args)
                {
                    if (!(arg is Type))
                    {
                        allTypes = false;
                        break;
                    }
                }
                if (allTypes)
                {
                    for (int i = 0; i < args.Length; ++i)
                    {
                        types.Add(args[i] as Type);
                    }
                    var core = BindTypes(types) as IClrTypeCore;
                    if (core != null)
                    {
                        var rv = ObjectPool.GetReturnValueFromPool(1);
                        rv[0] = ClrTypeWrapper.GetFromPool(core);
                        return rv;
                    }
                }
            }

            if (_NormalCore != null)
            {
                var rv = _NormalCore.Call(target, args);
                if (rv != null)
                {
                    return rv;
                }
            }
            if (_GenericCore != null)
            {
                return _GenericCore.Call(target, args);
            }
            return null;
        }

        public BaseDynamic CreateOverloadSelector(object tar)
        {
            return new ClrTypeOrGenericSelector(this, tar);;
        }

        public Type BindingType
        {
            get
            {
                if (_NormalCore != null && _GenericCore != null)
                {
                    //if (GLog.IsLogInfoEnabled)GLog.LogInfo("Unable to get type of a group of types.");
                    return null;
                }
                else if (_NormalCore != null)
                {
                    return _NormalCore.BindingType;
                }
                else if (_GenericCore != null)
                {
                    return _GenericCore.BindingType;
                }
                return null;
            }
        }

        public object GetFieldFor(object tar, object key)
        {
            if (_NormalCore != null)
            {
                return _NormalCore.GetFieldFor(tar, key);
            }
            return null;
        }

        public bool SetFieldFor(object tar, object key, object val)
        {
            if (_NormalCore != null)
            {
                return _NormalCore.SetFieldFor(tar, key, val);
            }
            return false;
        }

        public Type MajorType
        {
            get
            {
                if (_GroupTypes != null && _GroupTypes.Length > 0)
                {
                    return _GroupTypes[0];
                }
                return null;
            }
        }

        public Type[] AllTypes
        {
            get
            {
                return _GroupTypes;
            }
        }
    }

    internal class ClrTypeOrGenericSelector : NoBindingDynamic
    {
        internal ClrTypeCoreWithGeneric _Core;
        public object Target { get; set; }

        public override object[] Call(params object[] args)
        {
            Types types = new Types();
            bool allTypes = true;
            if (args != null)
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    var type = args[i].UnwrapDynamic() as Type;
                    if (type == null)
                    {
                        allTypes = false;
                        break;
                    }
                    types.Add(type);
                }
            }
            if (allTypes)
            {
                var core = _Core.BindTypes(types) as IClrTypeCore;
                if (core != null)
                {
                    var rv = ObjectPool.GetReturnValueFromPool(1);
                    rv[0] = ClrTypeWrapper.GetFromPool(core);
                    return rv;
                }
            }
            if (_Core._NormalCore != null)
            {
                return _Core._NormalCore.CreateOverloadSelector(Target).Call(args);
            }
            else
            {
                return null;
            }
        }

        internal ClrTypeOrGenericSelector(ClrTypeCoreWithGeneric core, object tar)
        {
            _Core = core;
            Target = tar;
        }
    }

    public static class GenericHelper
    {
        public static Dictionary<string, Type> MergeDeterminedType(this Dictionary<string, Type> todict, Dictionary<string, Type> fromdict)
        {
            if (fromdict == null)
            {
                return todict;
            }
            if (todict == null)
            {
                todict = new Dictionary<string, Type>();
            }
            foreach (var kvp in fromdict)
            {
                Type oldType = null;
                todict.TryGetValue(kvp.Key, out oldType);
                if (oldType == null)
                {
                    todict[kvp.Key] = kvp.Value;
                }
                else if (kvp.Value != null)
                {
                    var newType = kvp.Value;
                    if (newType.IsAssignableFrom(oldType))
                    {
                        todict[kvp.Key] = newType;
                    }
                    else if (oldType.IsAssignableFrom(newType))
                    {
                    }
                    else
                    {
                        todict[kvp.Key] = null;
                        //if (GLog.IsLogInfoEnabled)GLog.LogInfo("Unable to determine types of the generic method from the calling args (" + kvp.Key + ") - obfuscated arg.");
                        return null;
                    }
                }
            }
            return todict;
        }

        public static Dictionary<string, Type> DetermineGenericTypeArg(this Type gtype, Type otype)
        {
            Dictionary<string, Type> dict = new Dictionary<string, Type>();
            if (gtype == null || otype == null)
            {
                return dict;
            }
            if (gtype.IsGenericParameter)
            {
                dict[gtype.Name] = otype;
                return dict;
            }
            else if (gtype.IsArray)
            {
                if (otype.IsArray)
                {
                    if (gtype.GetArrayRank() == otype.GetArrayRank())
                    {
                        return DetermineGenericTypeArg(gtype.GetElementType(), otype.GetElementType());
                    }
                }
                return null;
            }
            else if (gtype.IsGenericType() && gtype.ContainsGenericParameters())
            {
                if (otype.IsGenericType())
                {
                    var gtypepars = gtype.GetGenericArguments();
                    var otypepars = otype.GetGenericArguments();
                    if (gtypepars.Length == otypepars.Length)
                    {
                        for (int i = 0; i < gtypepars.Length; ++i)
                        {
                            var dictpart = DetermineGenericTypeArg(gtypepars[i], otypepars[i]);
                            if (dictpart == null)
                            {
                                return null;
                            }
                            if (MergeDeterminedType(dict, dictpart) == null)
                            {
                                return null;
                            }
                        }
                        return dict;
                    }
                }
                return null;
            }
            return dict;
        }

        public static Dictionary<string, Type> DetermineGenericTypeArg(this Type gtype, object obj)
        {
            if (obj == null)
            {
                return new Dictionary<string, Type>();
            }
            return DetermineGenericTypeArg(gtype, obj.GetType());
        }

        public static ICallableCore DetermineGenericTypeArg(this IGenericCore core, params object[] args)
        {
            if (core != null && args != null)
            {
                Types types = new Types();
                for (int i = 0; i < args.Length; ++i)
                {
                    var arg = args[i];
                    if (arg != null)
                    {
                        types.Add(arg.GetType());
                    }
                    else
                    {
                        types.Add(null);
                    }
                }
                var rv = core.DetermineTypes(types);
                return rv;
            }
            return null;
        }

        public static bool ContainsGenericParametersHierachical(this Type t)
        {
            if (t == null)
            {
                return false;
            }
            if (t.IsGenericParameter)
            {
                return true;
            }
            if (t.IsArray)
            {
                return ContainsGenericParametersHierachical(t.GetElementType());
            }
            if (t.IsGenericType())
            {
                return t.ContainsGenericParameters();
            }
            return false;
        }

        public static int CompareGenericParamTypes(this Types ta, Types tb)
        {
            if (ta.Count != tb.Count)
            {
                return ta.Count - tb.Count;
            }
            for (int i = 0; i < ta.Count; ++i)
            {
                var tya = ta[i];
                var tyb = tb[i];
                if (tya != tyb)
                {
                    if (tya == null)
                    {
                        return -1;
                    }
                    if (tyb == null)
                    {
                        return 1;
                    }
                    var ga = ContainsGenericParametersHierachical(tya);
                    var gb = ContainsGenericParametersHierachical(tyb);
                    if (ga && !gb)
                    {
                        return 1;
                    }
                    else if (!ga && gb)
                    {
                        return -1;
                    }
                    else if (ga && gb)
                    {
                        return tya.GetHashCode() - tyb.GetHashCode();
                    }
                    else
                    {
                        if (tya.IsAssignableFrom(tyb))
                        {
                            return 1;
                        }
                        if (tyb.IsAssignableFrom(tya))
                        {
                            return -1;
                        }
                        var a2b = tya.CanConvertRaw(tyb);
                        var b2a = tyb.CanConvertRaw(tya);
                        if (a2b && !b2a)
                        {
                            return -1;
                        }
                        if (!a2b && b2a)
                        {
                            return 1;
                        }
                        if (!a2b && !b2a)
                        {
                            return tya.GetHashCode() - tyb.GetHashCode();
                        }
                        else // (a2b && b2a)
                        {
                            var rv = tyb.GetPrimitiveTypeWeight() - tya.GetPrimitiveTypeWeight();
                            if (rv != 0)
                            {
                                return rv;
                            }
                        }
                    }
                }
            }
            return 0;
        }

        public static ClrTypeWrapper GetTypeWrapper(Type ntype, Type[] gtypes)
        {
            var core = new ClrTypeCoreWithGeneric(ntype, gtypes);
            if (core != null)
            {
                if (core._NormalCore != null && core._GenericCore != null)
                {
                    return ClrTypeWrapper.GetFromPool(core);
                }
                else if (core._NormalCore != null)
                {
                    return ClrTypeWrapper.GetFromPool(core._NormalCore);
                }
                else if (core._GenericCore != null)
                {
                    return ClrTypeWrapper.GetFromPool(core._GenericCore);
                }
            }
            return null;
        }
    }
}