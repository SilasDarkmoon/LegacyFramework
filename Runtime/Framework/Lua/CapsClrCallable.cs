using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Capstones.PlatExt;

namespace Capstones.Dynamic
{
    public struct Types : IList<Type>
    {
        private Type t0;
        private Type t1;
        private Type t2;
        private Type t3;
        private Type t4;
        private Type t5;
        private Type t6;
        private Type t7;
        private Type t8;
        private Type t9;
        private List<Type> tx;

        private int _cnt;

        #region static funcs for set and get
        private delegate Type GetTypeDel(ref Types types);
        private delegate void SetTypeDel(ref Types types, Type type);

        private static Type GetType0(ref Types types) { return types.t0; }
        private static Type GetType1(ref Types types) { return types.t1; }
        private static Type GetType2(ref Types types) { return types.t2; }
        private static Type GetType3(ref Types types) { return types.t3; }
        private static Type GetType4(ref Types types) { return types.t4; }
        private static Type GetType5(ref Types types) { return types.t5; }
        private static Type GetType6(ref Types types) { return types.t6; }
        private static Type GetType7(ref Types types) { return types.t7; }
        private static Type GetType8(ref Types types) { return types.t8; }
        private static Type GetType9(ref Types types) { return types.t9; }

        private static void SetType0(ref Types types, Type type) { types.t0 = type; }
        private static void SetType1(ref Types types, Type type) { types.t1 = type; }
        private static void SetType2(ref Types types, Type type) { types.t2 = type; }
        private static void SetType3(ref Types types, Type type) { types.t3 = type; }
        private static void SetType4(ref Types types, Type type) { types.t4 = type; }
        private static void SetType5(ref Types types, Type type) { types.t5 = type; }
        private static void SetType6(ref Types types, Type type) { types.t6 = type; }
        private static void SetType7(ref Types types, Type type) { types.t7 = type; }
        private static void SetType8(ref Types types, Type type) { types.t8 = type; }
        private static void SetType9(ref Types types, Type type) { types.t9 = type; }

        private static GetTypeDel[] GetTypeFuncs = new GetTypeDel[]
        {
            GetType0,
            GetType1,
            GetType2,
            GetType3,
            GetType4,
            GetType5,
            GetType6,
            GetType7,
            GetType8,
            GetType9,
        };
        private static SetTypeDel[] SetTypeFuncs = new SetTypeDel[]
        {
            SetType0,
            SetType1,
            SetType2,
            SetType3,
            SetType4,
            SetType5,
            SetType6,
            SetType7,
            SetType8,
            SetType9,
        };
        #endregion

        #region IList<Type>
        public int IndexOf(Type item)
        {
            for (int i = 0; i < _cnt; ++i)
            {
                if (this[i] == item)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, Type item)
        {
            if (index >= 0 && index <= _cnt)
            {
                this.Add(null);
                for (int i = _cnt - 1; i > index; --i)
                {
                    this[i] = this[i - 1];
                }
                this[index] = item;
            }
        }

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < _cnt)
            {
                for (int i = index + 1; i < _cnt; ++i)
                {
                    this[i - 1] = this[i];
                }
                this[_cnt - 1] = null;
                --_cnt;
            }
        }

        public Type this[int index]
        {
            get
            {
                if (index >= 0 && index < _cnt)
                {
                    if (index < GetTypeFuncs.Length)
                    {
                        return GetTypeFuncs[index](ref this);
                    }
                    else
                    {
                        if (tx != null)
                        {
                            var pindex = index - GetTypeFuncs.Length;
                            if (pindex < tx.Count)
                            {
                                return tx[pindex];
                            }
                        }
                    }
                }
                return null;
            }
            set
            {
                if (index >= 0 && index < _cnt)
                {
                    if (index < SetTypeFuncs.Length)
                    {
                        SetTypeFuncs[index](ref this, value);
                    }
                    else
                    {
                        if (tx != null)
                        {
                            var pindex = index - SetTypeFuncs.Length;
                            if (pindex < tx.Count)
                            {
                                tx[pindex] = value;
                            }
                        }
                    }
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Type item)
        {
            if (_cnt < SetTypeFuncs.Length)
            {
                this[_cnt++] = item;
            }
            else
            {
                ++_cnt;
                if (tx == null)
                {
                    tx = new List<Type>(8);
                }
                tx.Add(item);
            }
        }

        public void Clear()
        {
            _cnt = 0;
            t0 = null;
            t1 = null;
            t2 = null;
            t3 = null;
            t4 = null;
            t5 = null;
            t6 = null;
            t7 = null;
            t8 = null;
            t9 = null;
            tx = null;
        }

        public bool Contains(Type item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(Type[] array, int arrayIndex)
        {
            if (arrayIndex >= 0)
            {
                for (int i = 0; i < _cnt && i + arrayIndex < array.Length; ++i)
                {
                    array[arrayIndex + i] = this[i];
                }
            }
        }

        public int Count
        {
            get { return _cnt; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Type item)
        {
            var index = IndexOf(item);
            if (index >= 0 && index < _cnt)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public IEnumerator<Type> GetEnumerator()
        {
            for (int i = 0; i < _cnt; ++i)
            {
                yield return this[i];
            }
        }
        #endregion

        public override bool Equals(object obj)
        {
            if (obj is Types)
            {
                Types types2 = (Types)obj;
                if (types2._cnt == _cnt)
                {
                    for (int i = 0; i < _cnt; ++i)
                    {
                        if (this[i] != types2[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        internal static bool OpEquals(Types source, Types other)
        {
            if (!object.ReferenceEquals(source, null))
            {
                return source.Equals(other);
            }
            else if (!object.ReferenceEquals(other, null))
            {
                return other.Equals(source);
            }
            return true;
        }
        public static bool operator ==(Types source, Types other)
        {
            return OpEquals(source, other);
        }
        public static bool operator !=(Types source, Types other)
        {
            return !OpEquals(source, other);
        }

        // the greater weight means more detail and explicit
        public static int Compare(Types ta, Types tb)
        {
            // TODO : IComparable IComparable<T>
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
                    if (tya.IsAssignableFrom(tyb))
                    {
                        return -1;
                    }
                    if (tyb.IsAssignableFrom(tya))
                    {
                        return 1;
                    }
                    var a2b = DynamicHelper.CanConvertRaw(tya, tyb);
                    var b2a = DynamicHelper.CanConvertRaw(tyb, tya);
                    if (a2b && !b2a)
                    {
                        return 1;
                    }
                    if (!a2b && b2a)
                    {
                        return -1;
                    }
                    if (!a2b && !b2a)
                    {
                        return tya.GetHashCode() - tyb.GetHashCode();
                    }
                    else // (a2b && b2a)
                    {
                        var rv = DynamicHelper.GetPrimitiveTypeWeight(tya) - DynamicHelper.GetPrimitiveTypeWeight(tyb);
                        if (rv != 0)
                        {
                            return rv;
                        }
                    }
                }
            }
            return 0;
        }

        public override int GetHashCode()
        {
            int code = 0;
            for (int i = 0; i < Count; ++i)
            {
                code <<= 1;
                var type = this[i];
                if (type != null)
                {
                    code += type.GetHashCode();
                }
            }
            return code;
        }
    }

    public interface ICallableCore
    {
        object[] Call(object target, params object[] args);
        BaseDynamic CreateOverloadSelector(object tar);
    }

    internal class UniqueCallableCore : ICallableCore
    {
        protected internal MethodBase Method { get; set; }
        protected internal Types _MethodParamTypes;
        protected internal int[] _OutParamIndices;
        protected internal int _LastIsParams = -1;

        protected internal UniqueCallableCore(MethodBase minfo)
        {
            Method = minfo;
            var pars = minfo.GetParameters();
            Types types = new Types();
            if (pars != null)
            {

                List<int> oindices = new List<int>(4);
                for (int i = 0; i < pars.Length; ++i)
                {
                    var type = pars[i].ParameterType;
                    if (type.IsByRef)
                    {
                        types.Add(type.GetElementType());
                        oindices.Add(i);
                    }
                    else
                    {
                        types.Add(type);
                    }
                    if (i == pars.Length - 1 && type.IsArray)
                    {
                        var attrs = pars[i].GetCustomAttributes(typeof(ParamArrayAttribute), true);
#if NETFX_CORE
                        if (attrs != null && attrs.Count() > 0)
#else
                        if (attrs != null && attrs.Length > 0)
#endif
                        {
                            _LastIsParams = i;
                        }
                    }
                }
                _OutParamIndices = oindices.ToArray();
            }
            _MethodParamTypes = types;
        }

        protected internal virtual bool CanConvertParam(Type src, Type dst)
        {
            if (src.CanConvertRaw(dst))
            {
                return true;
            }
            else if (src != null && dst != null && src.IsSubclassOf(typeof(BaseDynamic)) && dst.IsSubclassOf(typeof(Delegate)))
            {
                return true;
            }
            return false;
        }
        public virtual int CanCall(Types pt)
        {
            int rv = 0;
            for (int i = 0; i < _MethodParamTypes.Count; ++i)
            {
                if (i == _LastIsParams)
                {
                    int ex = 1;
                    if (pt.Count == _MethodParamTypes.Count && _MethodParamTypes[i].IsAssignableFrom(pt[i]))
                    {
                        ex = 0;
                    }
                    else
                    {
                        for (int j = i; j < pt.Count; ++j)
                        {

                            if (!CanConvertParam(pt[j], _MethodParamTypes[i].GetElementType()))
                            {
                                return -1;
                            }
                        }
                        //ex = 1;
                    }
                    rv += ex << i;
                    return rv < 0 ? int.MaxValue : rv;
                }
                Type curtype = null;
                if (i < pt.Count)
                {
                    curtype = pt[i];
                }
                if (!_MethodParamTypes[i].IsAssignableFrom(curtype))
                {
                    rv += 1 << i;
                }
                if (!CanConvertParam(curtype, _MethodParamTypes[i]))
                {
                    return -1;
                }
            }
            for (int i = _MethodParamTypes.Count; i < pt.Count; ++i)
            {
                rv += 1 << i;
            }
            return rv < 0 ? int.MaxValue : rv;
        }
        public virtual object[] Call(object target, params object[] args)
        {
            try
            {
                object[] rargs;
                if (_LastIsParams >= 0)
                {
                    rargs = ObjectPool.GetParamsFromPool(_LastIsParams + 1);
                    for (int i = 0; i < _LastIsParams; ++i)
                    {
                        object arg = null;
                        if (i < args.Length)
                        {
                            arg = args[i];
                        }
                        rargs[i] = arg.ConvertTypeRaw(_MethodParamTypes[i]);
                    }
                    Array arr = null;
                    if (args.Length == _LastIsParams + 1)
                    {
                        var raw = args[_LastIsParams];//.UnwrapDynamic();
                        if (_MethodParamTypes[_LastIsParams].IsInstanceOfType(raw))
                        {
                            arr = raw as Array;
                            rargs[_LastIsParams] = arr;
                        }
                    }
                    if (arr == null)
                    {
                        int arrLen = 0;
                        if (args.Length > _LastIsParams)
                        {
                            arrLen = args.Length - _LastIsParams;
                        }
                        arr = Array.CreateInstance(_MethodParamTypes[_LastIsParams].GetElementType(), arrLen);
                        rargs[_LastIsParams] = arr;
                        for (int i = 0; i < arr.Length; ++i)
                        {
                            arr.SetValue(args[_LastIsParams + i].ConvertTypeRaw(_MethodParamTypes[_LastIsParams].GetElementType()), i);
                        }
                    }
                }
                else
                {
                    int len = _MethodParamTypes.Count;
                    rargs = ObjectPool.GetParamsFromPool(len);
                    for (int i = 0; i < len; ++i)
                    {
                        object arg = null;
                        if (i < args.Length)
                        {
                            arg = args[i];
                        }
                        rargs[i] = arg.ConvertTypeRaw(_MethodParamTypes[i]);
                    }
                }
                object result = null;
                if (Method is ConstructorInfo)
                {
                    result = ((ConstructorInfo)Method).Invoke(rargs);
                }
                else
                {
                    // ideally, we should not call the overridden method and call exactly the method provided by the MethodInfo.
                    // but the MethodInfo will always call the finally overridden method provided by the target object.
                    // there is a solution that creates delegate with RuntimeMethodHandle using Activator class.
                    // but the delegate itself is to be declared. so this is not the common solution.
                    // see http://stackoverflow.com/questions/4357729/use-reflection-to-invoke-an-overridden-base-method
                    // the temporary solution is we should declare public non-virtual method in the derived class and call base.XXX in this method and we can call this method.
                    result = Method.Invoke(target, rargs);
                }
                var rv = ObjectPool.GetReturnValueFromPool(1 + (_OutParamIndices == null ? 0 : _OutParamIndices.Length));
                rv[0] = result;
                var seq = 0;
                if (_OutParamIndices != null && rargs != null)
                {
                    foreach (var index in _OutParamIndices)
                    {
                        if (index >= 0 && index < rargs.Length)
                        {
                            rv[++seq] = rargs[index];
                        }
                    }
                }
                ObjectPool.ReturnParamsToPool(rargs);
                return rv;
            }
            catch (Exception e)
            {
                // TODO: the debug info will decrease the performance. we should develop a generic lua-call-clr log.
                // perhaps we should make a Call and a TryCall. perhaps we should show which lua-state is doing the log. perhaps we should should lua-stack 
                if(GLog.IsLogErrorEnabled) GLog.LogException("Unable To Call: " + Method.Name + "@" + Method.DeclaringType.Name + " \n" + e.ToString());
            }
            return null;
        }
        public virtual BaseDynamic CreateOverloadSelector(object tar)
        {
            return new UniqueCallableOverloadSelector(this, tar);
        }
    }

    internal class UniqueCallableOverloadSelector : NoBindingDynamic
    {
        internal UniqueCallableCore _Core;
        public object Target { get; set; }

        public override object[] Call(params object[] args)
        {
            Types types = new Types();
            if (args != null)
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    types.Add(args[i].UnwrapDynamic() as Type);
                }
            }
            if (types.Equals(_Core._MethodParamTypes))
            {
                var rv = ObjectPool.GetReturnValueFromPool(1);
                rv[0] = ClrCallable.GetFromPool(_Core, Target);
                return rv;
            }
            return null;
        }

        internal UniqueCallableOverloadSelector(UniqueCallableCore core, object tar)
        {
            _Core = core;
            Target = tar;
        }
    }

    internal class GroupCallableCore : ICallableCore
    {
        internal LinkedList<UniqueCallableCore> _SeqCache = new LinkedList<UniqueCallableCore>();
        internal Dictionary<Types, UniqueCallableCore> _TypedCache;

        protected internal GroupCallableCore(MethodBase[] minfos, Dictionary<Types, UniqueCallableCore> tcache)
        {
            if (minfos != null)
            {
                UniqueCallableCore[] callables = new UniqueCallableCore[minfos.Length];
                for (int i = 0; i < minfos.Length; ++i)
                {
                    callables[i] = new UniqueCallableCore(minfos[i]);
                }
                Array.Sort(callables, (ca, cb) =>
                {
                    return Types.Compare(ca._MethodParamTypes, cb._MethodParamTypes);
                });
                for (int i = 0; i < minfos.Length; ++i)
                {
                    _SeqCache.AddLast(callables[i]);
                }
            }
            if (tcache == null)
            {
                tcache = new Dictionary<Types, UniqueCallableCore>();
            }
            _TypedCache = tcache;
        }
        protected internal GroupCallableCore(MethodBase[] minfos)
            : this(minfos, null)
        {
        }

        protected internal UniqueCallableCore FindAppropriate(Types pt)
        {
            UniqueCallableCore ucore = null;
            if (_TypedCache.TryGetValue(pt, out ucore))
            {
                if (ucore == null)
                {
                    return null;
                }
                else
                {
                    return ucore;
                }
            }
            var node = _SeqCache.First;
            LinkedListNode<UniqueCallableCore> found = null;
            int foundw = int.MaxValue;
            while (node != null)
            {
                var core = node.Value;
                var cancall = core.CanCall(pt);
                if (cancall == 0)
                {
                    found = node;
                    foundw = 0;
                    break;
                }
                if (cancall > 0 && cancall <= foundw)
                {
                    found = node;
                    foundw = cancall;
                }
                node = node.Next;
            }
            if (found != null)
            {
                _TypedCache[pt] = found.Value;
                //_SeqCache.Remove(found);
                //_SeqCache.AddFirst(found);
                return found.Value;
            }
            else
            {
                _TypedCache[pt] = null;
                return null;
            }
        }
        public object[] Call(object target, params object[] args)
        {
            Types types = new Types();
            if (args != null)
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    if (args[i] != null)
                    {
                        types.Add(args[i].GetType());
                    }
                    else
                    {
                        types.Add(null);
                    }
                }
            }
            UniqueCallableCore ucore = FindAppropriate(types);
            if (ucore == null)
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("Cann't find method with appropriate params.");
                return null;
            }
            return ucore.Call(target, args);
        }
        public BaseDynamic CreateOverloadSelector(object tar)
        {
            return new GroupCallableOverloadSelector(this, tar);
        }
    }

    internal class GroupCallableOverloadSelector : NoBindingDynamic
    {
        internal GroupCallableCore _Core;
        public object Target { get; set; }

        public override object[] Call(params object[] args)
        {
            Types types = new Types();
            if (args != null)
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    types.Add(args[i].UnwrapDynamic() as Type);
                    if (types[i] == null)
                    {
                        return null;
                    }
                }
            }
            UniqueCallableCore ucore = _Core.FindAppropriate(types);
            if (ucore == null)
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("Cann't find method with appropriate params.");
                return null;
            }
            else
            {
                var rv = ObjectPool.GetReturnValueFromPool(1);
                rv[0] = ClrCallable.GetFromPool(ucore, Target);
                return rv;
            }
        }

        internal GroupCallableOverloadSelector(GroupCallableCore core, object tar)
        {
            _Core = core;
            Target = tar;
        }
    }

    internal class PackedCallableCore : ICallableCore
    {
        internal Dictionary<int, ICallableCore> _CallableGroups = new Dictionary<int, ICallableCore>();

        public object[] Call(object target, params object[] args)
        {
            var code = args.GetParamsCode();
            ICallableCore rcore = null;
            _CallableGroups.TryGetValue(code, out rcore);
            if (rcore != null)
            {
                return rcore.Call(target, args);
            }
            return null;
        }
        protected internal PackedCallableCore(MethodBase[] minfos, Dictionary<Types, UniqueCallableCore> tcache)
        {
            if (minfos != null)
            {
                Dictionary<int, List<MethodBase>> pmethods = new Dictionary<int, List<MethodBase>>();
                for (int i = 0; i < minfos.Length; ++i)
                {
                    var minfo = minfos[i];
                    var pars = minfo.GetParameters();
                    Type[] types = null;
                    if (pars != null)
                    {
                        types = ObjectPool.GetParamTypesFromPool(pars.Length);
                        for (int j = 0; j < pars.Length; ++j)
                        {
                            var ptype = pars[j].ParameterType;
                            if (ptype.IsByRef)
                            {
                                types[j] = ptype.GetElementType();
                            }
                            else
                            {
                                types[j] = ptype;
                            }
                        }
                    }
                    var code = types.GetParamsCode();
                    ObjectPool.ReturnParamTypesToPool(types);
                    List<MethodBase> arr = null;
                    if (!pmethods.TryGetValue(code, out arr))
                    {
                        arr = new List<MethodBase>();
                        pmethods[code] = arr;
                    }
                    arr.Add(minfo);
                }
                foreach (var kvp in pmethods)
                {
                    if (kvp.Value.Count > 1)
                    {
                        _CallableGroups[kvp.Key] = new GroupCallableCore(kvp.Value.ToArray(), tcache);
                    }
                    else
                    {
                        _CallableGroups[kvp.Key] = new UniqueCallableCore(kvp.Value[0]);
                    }
                }
            }
        }
        protected internal PackedCallableCore(MethodBase[] minfos)
            : this(minfos, null)
        {
        }

        public BaseDynamic CreateOverloadSelector(object tar)
        {
            return new PackedCallableOverloadSelector(this, tar);
        }
    }

    internal class PackedCallableOverloadSelector : NoBindingDynamic
    {
        internal PackedCallableCore _Core;
        public object Target { get; set; }

        public override object[] Call(params object[] args)
        {
            Type[] types = null;
            if (args != null)
            {
                types = ObjectPool.GetParamTypesFromPool(args.Length);
                for (int i = 0; i < args.Length; ++i)
                {
                    types[i] = args[i].UnwrapDynamic() as Type;
                    if (types[i] == null)
                    {
                        return null;
                    }
                }
            }
            ICallableCore ucore = null;
            if (!_Core._CallableGroups.TryGetValue(types.GetParamsCode(), out ucore))
            {
                ObjectPool.ReturnParamTypesToPool(types);
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("Cann't find method with appropriate params.");
                return null;
            }
            else
            {
                ObjectPool.ReturnParamTypesToPool(types);
                return ucore.CreateOverloadSelector(Target).Call(args);
            }
        }

        internal PackedCallableOverloadSelector(PackedCallableCore core, object tar)
        {
            _Core = core;
            Target = tar;
        }
    }

    internal class CallableRebinder : NoBindingDynamic
    {
        internal ICallableCore _Core;
        internal CallableRebinder(ICallableCore core)
        {
            _Core = core;
        }
        public override object[] Call(params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                var target = args[0].UnwrapDynamic();
                var rv = ObjectPool.GetReturnValueFromPool(1);
                rv[0] = ClrCallable.GetFromPool(_Core, target);
                return rv;
            }
            return null;
        }
    }

    public class ClrCallable : NoBindingDynamic
    {
#if ENABLE_OBJ_POOL
        [ThreadStatic] protected internal static ObjectPool.GenericInstancePool<ClrCallable> _Pool;
        protected internal static ObjectPool.GenericInstancePool<ClrCallable> Pool
        {
            get
            {
                if (_Pool == null)
                {
                    _Pool = new ObjectPool.GenericInstancePool<ClrCallable>();
                }
                return _Pool;
            }
        }
#endif
        public static ClrCallable GetFromPool(ICallableCore core, object tar)
        {
#if ENABLE_OBJ_POOL
            return Pool.GetFromPool(() => new ClrCallable(core) { Target = tar }, callable => { callable._Core = core; callable.Target = tar; });
#else
            return new ClrCallable(core) { Target = tar };
#endif
        }
        public override void ReturnToPool()
        {
            _Binding = this;
            _Core = null;
            Target = null;
#if ENABLE_OBJ_POOL
            Pool.ReturnToPool(this);
#endif
        }

        internal ICallableCore _Core;
        protected internal object _Target;
        public object Target
        {
            get
            {
                return _Target;
            }
            set
            {
                _Target = value;
            }
        }

        public override object[] Call(params object[] args)
        {
            object[] rargs = null;
            if (args != null)
            {
                rargs = ObjectPool.GetParamsFromPool(args.Length);
                for (int i = 0; i < args.Length; ++i)
                {
                    rargs[i] = args[i].UnwrapDynamic();
                }
            }
            var rv = _Core.Call(Target, rargs);
            if (rargs != null)
            {
                ObjectPool.ReturnParamsToPool(rargs);
            }
            return rv;
        }
        protected internal override object GetFieldImp(object key)
        {
            if (key != null)
            {
                var ukey = key.UnwrapDynamic();
                if (ukey == null)
                {
                    ukey = key;
                }
                if (ukey is string)
                {
                    string strkey = (string)ukey;
                    switch (strkey)
                    {
                        case "___ol":
                            return _Core.CreateOverloadSelector(Target);
                        case "___bind":
                            return new CallableRebinder(_Core);
                        default:
                            if (Binding is Delegate)
                            {
                                var tcore = ClrTypeCore.GetTypeCore(Binding.GetType());
                                return tcore.GetFieldFor(Binding, key);
                            }
                            break;
                    }
                }
                return base.GetFieldImp(key);
            }
            return null;
        }

        internal ClrCallable(ICallableCore core)
        {
            _Core = core;
        }
    }

    public class ClrEventWrapper : NoBindingDynamic
    {
        protected internal struct EventBinder
        {
            public object Target;
            public EventInfo Info;
        }
        protected internal class AddHandlerCallable : NoBindingDynamic
        {
            public EventBinder _Binder;
            public AddHandlerCallable(EventBinder binder)
            {
                _Binder = binder;
            }
            public override object[] Call(params object[] args)
            {
                if (args != null && args.Length > 0)
                {
                    var del = args[0];
                    ClrEventWrapper.AddHandler(_Binder, del);
                    return new[] { this };
                }
                return null;
            }
        }
        protected internal class RemoveHandlerCallable : NoBindingDynamic
        {
            public EventBinder _Binder;
            public RemoveHandlerCallable(EventBinder binder)
            {
                _Binder = binder;
            }
            public override object[] Call(params object[] args)
            {
                if (args != null && args.Length > 0)
                {
                    var del = args[0];
                    ClrEventWrapper.RemoveHandler(_Binder, del);
                    return new[] { this };
                }
                return null;
            }
        }

#if ENABLE_OBJ_POOL
        protected internal class ClrEventWrapperPool : ObjectPool.GenericInstancePool<ClrEventWrapper>
        {
            public ClrEventWrapper GetFromPool(EventInfo info, object tar)
            {
                var rv = this.GetFromPool();
                rv._Binder.Info = info;
                rv._Binder.Target = tar;
                return rv;
            }
            public void ReturnToPool(ClrEventWrapper wrapper)
            {
                wrapper._Binder = new EventBinder();
                this.ReturnToPool<ClrEventWrapper>(wrapper);
            }
        }
        [ThreadStatic] protected internal static ClrEventWrapperPool _Pool;
        protected internal static ClrEventWrapperPool Pool
        {
            get
            {
                if (_Pool == null)
                {
                    _Pool = new ClrEventWrapperPool();
                }
                return _Pool;
            }
        }
#endif
        public static ClrEventWrapper GetFromPool(EventInfo info, object tar)
        {
#if ENABLE_OBJ_POOL
            return Pool.GetFromPool(info, tar);
#else
            return new ClrEventWrapper() { _Binder = new EventBinder() { Info = info, Target = tar } };
#endif
        }
        public override void ReturnToPool()
        {
#if ENABLE_OBJ_POOL
            Pool.ReturnToPool(this);
#endif
        }

        protected internal EventBinder _Binder;

        public static void AddHandler(object tar, EventInfo e, object delobj)
        {
            Delegate del = delobj.UnwrapDynamic<Delegate>();
            if (del != null)
            {
                if (!e.EventHandlerType.IsInstanceOfType(del))
                {
                    del = null;
                }
            }
            else if (delobj is BaseDynamic)
            {
                del = Capstones.LuaWrap.CapsLuaDelegateGenerator.CreateDelegate(e.EventHandlerType, delobj as BaseDynamic);
            }
            if (del != null)
            {
                e.AddEventHandler(tar, del);
            }
        }
        public static void RemoveHandler(object tar, EventInfo e, object delobj)
        {
            Delegate del = delobj.UnwrapDynamic<Delegate>();
            if (del != null)
            {
                if (!e.EventHandlerType.IsInstanceOfType(del))
                {
                    del = null;
                }
            }
            else if (delobj is BaseDynamic)
            {
                del = Capstones.LuaWrap.CapsLuaDelegateGenerator.CreateDelegate(e.EventHandlerType, delobj as BaseDynamic);
            }
            if (del != null)
            {
                e.RemoveEventHandler(tar, del);
            }
        }
        protected internal static void AddHandler(EventBinder b, object delobj)
        {
            AddHandler(b.Target, b.Info, delobj);
        }
        protected internal static void RemoveHandler(EventBinder b, object delobj)
        {
            RemoveHandler(b.Target, b.Info, delobj);
        }

        public override object BinaryOp(string op, object other)
        {
            if (op == "+")
            {
                AddHandler(_Binder, other);
                return this;
            }
            else if (op == "-")
            {
                RemoveHandler(_Binder, other);
                return this;
            }
            return base.BinaryOp(op, other);
        }
        protected internal override object GetFieldImp(object key)
        {
            if (key != null)
            {
                var ukey = key.UnwrapDynamic();
                if (ukey == null)
                {
                    ukey = key;
                }
                if (ukey is string)
                {
                    string strkey = (string)ukey;
                    switch (strkey)
                    {
                        case "add":
                            return new AddHandlerCallable(_Binder);
                        case "remove":
                            return new RemoveHandlerCallable(_Binder);
                    }
                }
                return base.GetFieldImp(key);
            }
            return null;
        }

        public override string ToString()
        {
            if (_Binder.Info != null)
            {
                return _Binder.Info.Name;
            }
            else
            {
                return "EventWrapper(null)";
            }
        }
    }

    public static class CallableHelper
    {
        public static int GetParamsCode(this object[] args)
        {
            int code = 0;
            if (args != null)
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    int codepart = 0;
                    if (args[i] != null && args[i].GetType().IsValueType())
                    {
                        codepart = 1 << i;
                    }
                    code += codepart;
                }
            }
            return code;
        }
        public static int GetParamsCode(this Type[] types)
        {
            int code = 0;
            if (types != null)
            {
                for (int i = 0; i < types.Length; ++i)
                {
                    int codepart = 0;
                    if (types[i] != null && types[i].IsValueType())
                    {
                        codepart = 1 << i;
                    }
                    code += codepart;
                }
            }
            return code;
        }
        public static int GetTypesCode(this Type[] types)
        {
            int code = 0;
            if (types != null)
            {
                for (int i = 0; i < types.Length; ++i)
                {
                    code <<= 1;
                    if (types[i] != null)
                    {
                        code += types[i].GetHashCode();
                    }
                }
            }
            return code;
        }

        public static bool IsTypesEqual(this Type[] types, Type[] types2)
        {
            var tother = types2;
            var tthis = types;
            if (tother == null)
            {
                if (tthis == null || tthis.Length == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (tthis == null)
            {
                if (tother.Length == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (tthis.Length != tother.Length)
            {
                return false;
            }
            for (int i = 0; i < tthis.Length; ++i)
            {
                if (tthis[i] != tother[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static ICallableCore CreateCallableCore(this MethodBase[] minfos)
        {
            var rv = new CallableCoreWithGeneric(minfos);
            if (rv._GenericCore != null && rv._NormalCore != null)
            {
                return rv;
            }
            else if (rv._NormalCore != null)
            {
                return rv._NormalCore;
            }
            else
            {
                return rv._GenericCore;
            }
        }
        public static object UnwrapReturnValues(this object[] vals)
        {
            if (vals == null || vals.Length < 1)
                return null;
            return vals[0].UnwrapDynamic();
        }
        public static T UnwrapReturnValues<T>(this object[] vals)
        {
            return UnwrapReturnValues(vals).ConvertType<T>();
        }

        public static ClrCallable WrapDelegate(this Delegate del)
        {
            if (del != null)
            {
#if NETFX_CORE
                var core = new UniqueCallableCore(del.GetType().GetMethod("Invoke"));
                var rv = ClrCallable.GetFromPool(core, del);
#else
                var core = new UniqueCallableCore(del.GetDelegateMethod());
                var rv = ClrCallable.GetFromPool(core, del.Target);
#endif
                rv.Binding = del;
                return rv;
            }
            return null;
        }
    }
}