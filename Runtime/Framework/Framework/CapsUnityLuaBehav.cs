using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Capstones.LuaWrap;
using Capstones.UnityFramework;
using Capstones.Dynamic;
using Capstones.LuaExt;
using Capstones.LuaLib;
using Capstones.PlatExt;
using lua = Capstones.LuaLib.LuaCoreLib;
using lual = Capstones.LuaLib.LuaAuxLib;
using luae = Capstones.LuaLib.LuaLibEx;

public class CapsUnityLuaBehav : MonoBehaviour
{
    protected internal class CapsUnityLuaBehavWrapper : ClrObjectWrapper
    {
#if ENABLE_OBJ_POOL
        protected internal class CapsUnityLuaBehavWrapperPool : ObjectPool.GenericInstancePool<CapsUnityLuaBehavWrapper>
        {
            public CapsUnityLuaBehavWrapper GetFromPool(object obj)
            {
                var rv = this.TryGetFromPool();
                if (object.ReferenceEquals(rv, null))
                {
                    rv = new CapsUnityLuaBehavWrapper(obj);
                }
                else
                {
                    rv.Init(obj);
                }
                return rv;
            }
        }
        [ThreadStatic] protected internal new static CapsUnityLuaBehavWrapperPool _Pool;
        protected internal new static CapsUnityLuaBehavWrapperPool Pool
        {
            get
            {
                if (_Pool == null)
                {
                    _Pool = new CapsUnityLuaBehavWrapperPool();
                }
                return _Pool;
            }
        }
#endif
        public static CapsUnityLuaBehavWrapper GetFromPool(object obj)
        {
#if ENABLE_OBJ_POOL
            return Pool.GetFromPool(obj);
#else
            return new CapsUnityLuaBehavWrapper(obj);
#endif
        }
        public override void ReturnToPool()
        {
#if ENABLE_OBJ_POOL
            Pool.ReturnToPool(this);
#endif
        }

        public CapsUnityLuaBehavWrapper(object obj)
            : base(obj)
        {
            var behav = Binding as CapsUnityLuaBehav;
            if (behav != null)
            {
                behav.BindLua();
            }
        }
        public new void Init(object obj)
        {
            base.Init(obj);
            var behav = Binding as CapsUnityLuaBehav;
            if (behav != null)
            {
                behav.BindLua();
            }
        }

        protected internal override object GetFieldImp(object key)
        {
            var rv = base.GetFieldImp(key);
            if (rv != null)
            {
                return rv;
            }
            var behav = Binding as CapsUnityLuaBehav;
            if (behav != null)
            {
                if (behav.RegAllNeighbours() != null)
                {
                    foreach (var neighbour in behav._Neighbours)
                    {
                        if (neighbour != null)
                        {
                            rv = neighbour.GetFieldImp(key);
                            if (rv != null)
                            {
                                return rv;
                            }
                        }
                    }
                }
            }
            return null;
        }
        protected internal override bool SetFieldImp(object key, object val)
        {
            if (base.SetFieldImp(key, val))
            {
                return true;
            }
            var behav = Binding as CapsUnityLuaBehav;
            if (behav != null)
            {
                if (behav.RegAllNeighbours() != null)
                {
                    foreach (var neighbour in behav._Neighbours)
                    {
                        if (neighbour != null)
                        {
                            if (neighbour.SetFieldImp(key, val))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        protected internal override object ConvertBinding(Type type)
        {
            var rv = base.ConvertBinding(type);
            if (rv != null)
            {
                return rv;
            }
            if (type != null)
            {
                var behav = Binding as CapsUnityLuaBehav;
                if (behav != null)
                {
                    if (behav.RegAllNeighbours() != null)
                    {
                        foreach (var neighbour in behav._Neighbours)
                        {
                            if (neighbour != null)
                            {
                                var nbehav = neighbour.UnwrapDynamic();
                                if (nbehav != null)
                                {
                                    if (type.IsInstanceOfType(nbehav))
                                    {
                                        return nbehav;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
    static CapsUnityLuaBehav()
    {
        DynamicHelper.DynamicWrapperFuncs.Insert(0, new KeyValuePair<Predicate<object>, Func<object, BaseDynamic>>(obj => (obj is CapsUnityLuaBehav), obj => CapsUnityLuaBehavWrapper.GetFromPool(obj)));
    }
#region Component Data
    public string InitLuaPath;
    public List<string> ExFieldKeys = new List<string>();
    public List<int> ExFieldTypes = new List<int>();
    public List<int> ExFieldIndices = new List<int>();
    public List<bool> ExFieldValsBool = new List<bool>();
    public List<int> ExFieldValsInt = new List<int>();
    public List<double> ExFieldValsDouble = new List<double>();
    public List<string> ExFieldValsString = new List<string>();
    public List<UnityEngine.Object> ExFieldValsObj = new List<UnityEngine.Object>();
#endregion
#region Methods on Ex-Fields
    public static object ParseExVal(object val, out int vtype)
    {
        if (!(val is string))
        {
            if (val == null || val is UnityEngine.Object)
            {
                vtype = 4;
                return val;
            }
            else
            {
                val = val.ToString();
            }
        }
        var str = val as string;
        int ival;
        if (int.TryParse(str, out ival))
        {
            vtype = 1;
            return ival;
        }
        bool bval;
        if (bool.TryParse(str, out bval))
        {
            vtype = 0;
            return bval;
        }
        double dval;
        if (double.TryParse(str, out dval))
        {
            vtype = 2;
            return dval;
        }
        vtype = 3;
        return val;
    }
    public object ParseExVal(int index)
    {
        if (index >= 0)
        {
            if (index < ExFieldIndices.Count && index < ExFieldTypes.Count)
            {
                var vindex = ExFieldIndices[index];
                var vtype = ExFieldTypes[index];
                if (vindex >= 0)
                {
                    switch (vtype)
                    {
                        case 0:
                            if (vindex < ExFieldValsBool.Count)
                            {
                                return ExFieldValsBool[vindex];
                            }
                            break;
                        case 1:
                            if (vindex < ExFieldValsInt.Count)
                            {
                                return ExFieldValsInt[vindex];
                            }
                            break;
                        case 2:
                            if (vindex < ExFieldValsDouble.Count)
                            {
                                return ExFieldValsDouble[vindex];
                            }
                            break;
                        case 3:
                            if (vindex < ExFieldValsString.Count)
                            {
                                return ExFieldValsString[vindex];
                            }
                            break;
                        case 4:
                            if (vindex < ExFieldValsObj.Count)
                            {
                                var rv = ExFieldValsObj[vindex];
                                if (rv != null)
                                {
                                    return rv;
                                }
                                else
                                {
                                    return new NotAvailableExVal(index);
                                }
                            }
                            break;
                    }
                }
            }
        }
        return null;
    }
    public void AddExVal(string key, object val)
    {
        if (!string.IsNullOrEmpty(key) && !(val == null || (val is UnityEngine.Object && (UnityEngine.Object)val == null) || (val is string && string.IsNullOrEmpty((string)val))))
        {
            ExFieldKeys.Add(key);
            int vtype;
            var realval = ParseExVal(val, out vtype);
            ExFieldTypes.Add(vtype);
            switch (vtype)
            {
                case 0:
                    ExFieldIndices.Add(ExFieldValsBool.Count);
                    ExFieldValsBool.Add((bool)realval);
                    break;
                case 1:
                    ExFieldIndices.Add(ExFieldValsInt.Count);
                    ExFieldValsInt.Add((int)realval);
                    break;
                case 2:
                    ExFieldIndices.Add(ExFieldValsDouble.Count);
                    ExFieldValsDouble.Add((double)realval);
                    break;
                case 3:
                    ExFieldIndices.Add(ExFieldValsString.Count);
                    ExFieldValsString.Add((string)realval);
                    break;
                case 4:
                    ExFieldIndices.Add(ExFieldValsObj.Count);
                    ExFieldValsObj.Add((UnityEngine.Object)realval);
                    break;
            }
        }
    }
    public void AddExValStr(string key, string val)
    {
        ExFieldKeys.Add(key);
        ExFieldTypes.Add(3);
        ExFieldIndices.Add(ExFieldValsString.Count);
        ExFieldValsString.Add(val);
    }
    public void ClearExVal()
    {
        ExFieldKeys.Clear();
        ExFieldTypes.Clear();
        ExFieldIndices.Clear();
        ExFieldValsBool.Clear();
        ExFieldValsInt.Clear();
        ExFieldValsDouble.Clear();
        ExFieldValsString.Clear();
        ExFieldValsObj.Clear();
    }
    public Dictionary<string, object> ExpandExVal()
    {
        var rv = new Dictionary<string, object>();
        for (int i = 0; i < ExFieldKeys.Count; ++i)
        {
            rv[ExFieldKeys[i]] = ParseExVal(i);
        }
        return rv;
    }
    public sealed class NotAvailableExVal
    {
        private int _Index;
        public int Index { get { return _Index; } }

        public NotAvailableExVal(int index)
        {
            _Index = index;
        }

        public override bool Equals(object obj)
        {
            if (obj is NotAvailableExVal)
            {
                return _Index == ((NotAvailableExVal)obj)._Index;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _Index;
        }
    }
    public int DeleteExKey(string key)
    {
        int index = -1;
        for (int i = 0; i < ExFieldKeys.Count; ++i)
        {
            if (key == ExFieldKeys[i])
            {
                index = i;
                break;
            }
        }
        if (index >= 0)
        {
            ExFieldKeys.RemoveAt(index);
            var type = ExFieldTypes[index];
            ExFieldTypes.RemoveAt(index);
            var vindex = ExFieldIndices[index];
            ExFieldIndices.RemoveAt(index);

            for (int i = 0; i < ExFieldIndices.Count; ++i)
            {
                if (ExFieldTypes[i] == type && ExFieldIndices[i] > vindex)
                {
                    --ExFieldIndices[i];
                }
            }

            switch(type)
            {
                case 0:
                    if (vindex < ExFieldValsBool.Count)
                    {
                        ExFieldValsBool.RemoveAt(vindex);
                    }
                    break;
                case 1:
                    if (vindex < ExFieldValsInt.Count)
                    {
                        ExFieldValsInt.RemoveAt(vindex);
                    }
                    break;
                case 2:
                    if (vindex < ExFieldValsDouble.Count)
                    {
                        ExFieldValsDouble.RemoveAt(vindex);
                    }
                    break;
                case 3:
                    if (vindex < ExFieldValsString.Count)
                    {
                        ExFieldValsString.RemoveAt(vindex);
                    }
                    break;
                case 4:
                    if (vindex < ExFieldValsObj.Count)
                    {
                        ExFieldValsObj.RemoveAt(vindex);
                    }
                    break;
            }
        }
        return index;
    }
    public void ChangeExKey(int index, string newKey)
    {
        if (newKey != null && index >= 0 && index < ExFieldKeys.Count)
        {
            var oldKey = ExFieldKeys[index];
            if (oldKey != newKey)
            {
                int index2 = DeleteExKey(newKey);
                if (index2 >= 0)
                {
                    if (index > index2)
                    {
                        --index;
                    }
                }
                ExFieldKeys[index] = newKey;
            }
        }
    }
#endregion
    [NonSerialized] protected internal BaseLua _Self = null;
    public BaseLua HostingUserData
    {
        get { return _Self; }
    }
    [NonSerialized] public LinkedList<BaseDynamic> _Neighbours = null;
    [NonSerialized] protected internal bool _NeighboursReady = false;
    [NonSerialized] protected internal bool _LuaBinded = false;
    [NonSerialized] protected internal bool _Destroyed = false;

    [NonSerialized] protected internal bool _Awaken = false;
    protected internal static Dictionary<int, CapsUnityLuaBehav> _DestroyReg = new Dictionary<int, CapsUnityLuaBehav>();
    protected internal static int _DestroyRegNextIndex = 1;
    [NonSerialized] protected internal int _DestroyRegIndex = 0;
    protected internal static void CheckDestroyed()
    {
        for(int i = 1; i < _DestroyRegNextIndex; ++i)
        {
            if (_DestroyReg.ContainsKey(i))
            {
                var behav = _DestroyReg[i];
                if (object.ReferenceEquals(behav, null))
                {
                    RemoveDestroyRegIndex(i);
                }
                else if (behav == null)
                {
                    behav.OnDestroy();
                }
            }
        }
    }
    protected internal static void RemoveDestroyRegIndex(int index)
    {
        if (_DestroyReg.Remove(index))
        {
            if (index == _DestroyRegNextIndex - 1)
            {
                var max = 0;
                foreach (var kvp in _DestroyReg)
                {
                    if (kvp.Key > max)
                    {
                        max = kvp.Key;
                    }
                }
                _DestroyRegNextIndex = max + 1;
            }
        }
    }

    public void BindLua()
    {
        if (!_LuaBinded && ReferenceEquals(_Self, null) && this != null && !_Destroyed)
        {
            CheckDestroyed();
            _LuaBinded = true;
            _Self = this.BindBehav();
            _NeighboursReady = true;
            if (!_Awaken && !(enabled && gameObject.activeInHierarchy))
            {
                if (_DestroyRegIndex <= 0)
                {
                    _DestroyRegIndex = _DestroyRegNextIndex++;
                    _DestroyReg[_DestroyRegIndex] = this;
                }
            }
        }
    }
    protected internal LinkedList<BaseDynamic> RegAllNeighbours()
    {
        if (_NeighboursReady && _Neighbours == null)
        {
            _Neighbours = new LinkedList<BaseDynamic>();
            foreach (var comp in this.GetComponents<Component>())
            {
                if (comp is ICapsUnityLuaBehavNeighbour)
                {
                    var neighbour = (ICapsUnityLuaBehavNeighbour)comp;
                    _Neighbours.AddLast(neighbour.WrapDynamic());
                    neighbour.Major = this;
                }
            }
        }
        return _Neighbours;
    }
    public object[] CallLuaFunc(string name, params object[] args)
    {
        if (!ReferenceEquals(_Self, null))
        {
            var l = _Self._L;
            using (var lr = new LuaStateRecover(l))
            {
                l.getref(_Self.Refid);
                l.GetField(-1, name);
                var func = l.GetLuaOnStack(-1).WrapDynamic();
                if (!ReferenceEquals(func, null))
                {
                    if (args != null)
                    {
                        object[] rargs = ObjectPool.GetParamsFromPool(args.Length + 1);
                        rargs[0] = _Self;
                        for (int i = 0; i < args.Length; ++i)
                        {
                            rargs[i + 1] = args[i];
                        }
                        var rv = func.Call(rargs);
                        ObjectPool.ReturnParamsToPool(rargs);
                        return rv;
                    }
                    else
                    {
                        return func.Call(_Self);
                    }
                }
            }
        }
        return null;
    }
    void Awake()
    {
#if UNITY_EDITOR
        var awaken = UnityLua.GlobalLua["___EDITOR_AWAKEN"].ConvertType<int>();
        if (awaken == 0)
        {
            UnityLua.GlobalLua["___EDITOR_AWAKEN"] = 1.WrapDynamic();

            ResManager.RecordCacheVersion("editor", int.MaxValue);
            LanguageConverter.InitData();
            string pathCachedMain = LuaFramework.AppDataPath + "/CapstonesScripts/spt/init.lua";
            if (UnityLua.GlobalLua.DoFile(pathCachedMain) == 0)
            {
            }
            else
            {
                if (GLog.IsLogErrorEnabled) GLog.LogError(UnityLua.GlobalLua.L.GetLua(-1).UnwrapDynamic());
            }
        }
#endif
        BindLua();
        _Awaken = true;
        if (_DestroyRegIndex > 0)
        {
            RemoveDestroyRegIndex(_DestroyRegIndex);
            _DestroyRegIndex = 0;
        }
        CallLuaFunc("awake"); // Notice! The awake will NOT be called for the runtime binded behaviours!
    }
    protected internal void OnDestroy()
    {
        if (!_Destroyed)
        {
            _Destroyed = true;
            CallLuaFunc("onDestroy");
            _NeighboursReady = false;
            _Neighbours = null;
            if (!object.ReferenceEquals(_Self, null))
            {
                var l = _Self.L;
                if (l != IntPtr.Zero)
                {
                    l.getref(_Self.Refid); // self
                    l.getfenv(-1); // self ex
                    l.ClearTable(-1);
                    l.pop(2);
                }
                _Self.Dispose();
                _Self = null;
            }
            if (_DestroyRegIndex > 0)
            {
                RemoveDestroyRegIndex(_DestroyRegIndex);
                _DestroyRegIndex = 0;
            }
            ClearExVal();
            CheckDestroyed();
        }
    }
}
public interface ICapsUnityLuaBehavNeighbour
{
    CapsUnityLuaBehav Major { get; set; }
}
