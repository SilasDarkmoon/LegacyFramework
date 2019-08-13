using UnityEngine;
using XLua;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// param PonterEventData
/// </summary>
/// <param name="self"></param>
/// <param name="eventData"></param>
[CSharpCallLua]
public delegate void EventHandlerDelegate(LuaTable self, UnityEngine.EventSystems.PointerEventData eventData);
/// <summary>
/// param BaseEventData
/// </summary>
/// <param name="self"></param>
/// <param name="eventData"></param>
[CSharpCallLua]
public delegate void EventHandlerDelegate1(LuaTable self, UnityEngine.EventSystems.BaseEventData eventData);
/// <summary>
/// param AxisEventData
/// </summary>
/// <param name="self"></param>
/// <param name="eventData"></param>
[CSharpCallLua]
public delegate void EventHandlerDelegate2(LuaTable self, UnityEngine.EventSystems.AxisEventData eventData);

/// <summary>
/// AnimationEvent 动画事件回调
/// </summary>
[CSharpCallLua]
public delegate void AnimationEventDelegate(LuaTable self, string strParam);

[CSharpCallLua]
public delegate void LuaBehaviourFunc(LuaTable self);

[CSharpCallLua]
public delegate LuaFunction GetMtFunc(LuaTable baseTable);

[LuaCallCSharp]
public class LuaBehaviour : MonoBehaviour
{
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
        if (!string.IsNullOrEmpty(key) && !(val == null ||
                                            (val is UnityEngine.Object && (UnityEngine.Object)val == null) ||
                                            (val is string && string.IsNullOrEmpty((string)val))))
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
    [XLua.LuaCallCSharp]
    public sealed class NotAvailableExVal
    {
        private int _Index;

        public int Index
        {
            get { return _Index; }
        }

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

            switch (type)
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

    /// <summary>
    /// For capslua call xlua, TODO: REMOVE AFTER CAPSLUA IS DELETED
    /// </summary>
    public object[] CallLuaFunc(string funcName, params object[] args)
    {
        if (lua != null)
        {
            var func = lua.Get<LuaFunction>(funcName);
            if (func != null)
            {
                return func.Call(lua, args);
            }
            else
            {
                if (GLog.IsLogWarningEnabled) GLog.LogWarning("Cannot find lua function '" + funcName + "'.");
            }
        }
        return null;
    }
    [LuaCallCSharp]
    public interface ILuaCoroutine
    {
        object Resume();
    }

    private static IEnumerator EnumLuaCoroutine(ILuaCoroutine coroutine)
    {
        while (true)
        {
            var result = coroutine.Resume();
            if (result != null)
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
                yield break;
            }
        }
    }

    public Coroutine BehavCoroutine(ILuaCoroutine coroutine)
    {
        return StartCoroutine(EnumLuaCoroutine(coroutine));
    }

    //all lua behaviour shared one luaenv only!
    private static LuaEnv _luaEnv;
    public static LuaEnv luaEnv
    {
        get
        {
            if (_luaEnv == null)
            {
                InitLuaEnv();
            }

            return _luaEnv;
        }
    }

    internal static float lastGCTime = 0;
    internal static bool isInitialized = false;
    internal const float GCInterval = 1; //1 second 

    public LuaTable lua { get; private set; }

    private LuaBehaviourFunc luaStart;
    private LuaBehaviourFunc luaOnDestroy;
    private LuaBehaviourFunc luaOnDestroyBase;

    private LuaTable ex;
    private LuaTable data;

    private bool selfBehaviourInitialized = false;

    private static void InitLuaEnv()
    {
        _luaEnv = new LuaEnv();
        // Load init.lua
        _luaEnv.DoString("require 'preinit'");
        isInitialized = true;
    }

    void Init()
    {
        if (selfBehaviourInitialized || string.IsNullOrEmpty(InitLuaPath))
        {
            return;
        }

        if (!isInitialized)
        {
            InitLuaEnv();
        }

        ex = luaEnv.NewTable();

        foreach (var injection in ExpandExVal())
        {
            ex.SetInPath(injection.Key, injection.Value);
        }

        var ret = luaEnv.DoString(@"return require '" + InitLuaPath + "'", InitLuaPath);

        LuaTable baseTable = null;

        if (ret.Length > 0)
        {
            baseTable = ret[0] as LuaTable;
        }

        if (baseTable != null)
        {
            lua = luaEnv.NewTable();
            lua.Set("___ex", ex);
            lua.Set("___cs", this);

            LuaTable mt = luaEnv.NewTable();

            var getmtFunc = luaEnv.Global.GetInPath<GetMtFunc>("behaviour.getmt");
            var indexFunc = getmtFunc.Invoke(baseTable);

            mt.Set("__index", indexFunc);

            lua.SetMetaTable(mt);
            LuaBehaviourFunc luaCtor = lua.Get<LuaBehaviourFunc>("ctor");
            lua.Get("start", out luaStart);
            lua.Get("onDestroy", out luaOnDestroy);
            lua.Get("_onDestroy", out luaOnDestroyBase);
            //if (luaOnDestroyBase == null)
            //{
            //    BuglyAgent.PrintLog(LogSeverity.LogWarning, "Can't find _onDestroy function. {0} should be based on behavior.lua", InitLuaPath);
            //}

            selfBehaviourInitialized = true;

            if (luaCtor != null)
            {
                luaCtor(lua);
            }
        }
    }

    void Awake()
    {
        LuaBehaviour[] luaBahaviourArray = this.GetComponentsInChildren<LuaBehaviour>(true);
        for (int i = 0; i < luaBahaviourArray.Length; i++)
        {
            luaBahaviourArray[i].Init();
        }
    }

    void Start()
    {
        if (luaStart != null)
        {
            luaStart(lua);
        }
    }

    void Update()
    {
        if (Time.time - LuaBehaviour.lastGCTime > GCInterval)
        {
            luaEnv.Tick();
            LuaBehaviour.lastGCTime = Time.time;
        }
    }

    [NonSerialized]
    protected internal bool _Destroyed = false;
    protected internal void OnDestroy()
    {
        if (!_Destroyed)
        {
            _Destroyed = true;
            if (luaOnDestroy != null)
            {
                luaOnDestroy(lua);
            }
            luaOnDestroy = null;

            if (luaOnDestroyBase != null)
            {
                luaOnDestroyBase(lua);
            }
            luaOnDestroyBase = null;

            luaStart = null;
            if (ex != null)
            {
                ex.Dispose();
                ex = null;
            }

            if (lua != null)
            {
                lua.Dispose();
                lua = null;
            }
        }
    }
}
