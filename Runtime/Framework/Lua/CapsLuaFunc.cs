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
    public class LuaOnStackFunc : BaseLuaOnStack
    {
        public LuaOnStackFunc(IntPtr l, int index)
        {
            L = l;
            StackPos = index;
        }

        public override object[] Call(params object[] args)
        {
            if (L != IntPtr.Zero)
            {
                if (L.isfunction(StackPos))
                {
                    L.pushvalue(StackPos);
                    return L.PushArgsAndCall(args);
                }
                else
                {
                    if (GLog.IsLogInfoEnabled) GLog.LogInfo("luafunc : the index is not a func.");
                }
            }
            else
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("luafunc : null state.");
            }
            return null;
        }
        public override string ToString()
        {
            return "LuaFuncOnStack:" + StackPos.ToString();
        }

        public static explicit operator lua.CFunction(LuaOnStackFunc val)
        {
            if (val != null && val.L != IntPtr.Zero)
            {
                if (val.L.iscfunction(val.StackPos))
                {
                    return val.L.tocfunction(val.StackPos);
                }
            }
            return null;
        }
    }

    public class LuaFunc : BaseLua
    {
        public LuaFunc(IntPtr l, int stackpos)
        {
            L = l;
            if (l != IntPtr.Zero)
            {
                if (l.isfunction(stackpos))
                {
                    l.pushvalue(stackpos);
                    Refid = l.refer();
                }
            }
        }
        protected internal LuaFunc()
        { }
        public override object[] Call(params object[] args)
        {
            if (L != IntPtr.Zero)
            {
                if (Refid != 0)
                {
                    L.getref(Refid);
                    return L.PushArgsAndCall(args);
                }
                else
                {
                    if (GLog.IsLogInfoEnabled) GLog.LogInfo("luafunc : null ref");
                }
            }
            else
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("luafunc : null state.");
            }
            return null;
        }
        public override string ToString()
        {
            return "LuaFunc:" + Refid.ToString();
        }
        public static implicit operator LuaFunc(LuaOnStackFunc val)
        {
            if (val != null && val.L != IntPtr.Zero)
                return new LuaFunc(val.L, val.StackPos);
            return null;
        }
        public static implicit operator LuaOnStackFunc(LuaFunc val)
        {
            if (val != null && val.L != IntPtr.Zero && val.Refid != 0)
            {
                val.L.getref(val.Refid);
                return new LuaOnStackFunc(val.L, val.L.gettop());
            }
            return null;
        }
        public static explicit operator lua.CFunction(LuaFunc val)
        {
            if (val != null && val.L != IntPtr.Zero && val.Refid != 0)
            {
                val.L.getref(val.Refid);
                lua.CFunction cfunc = null;
                if (val.L.iscfunction(-1))
                {
                    cfunc = val.L.tocfunction(-1);
                }
                val.L.pop(1);
                return cfunc;
            }
            return null;
        }
    }

    public static class LuaFuncHelper
    {
        public static int PushArgsAndCallRaw(this IntPtr l, params object[] args)
        {
            if (l != IntPtr.Zero)
            {
                var oldtop = l.gettop();
                l.pushcfunction(Capstones.LuaExt.LuaFramework.ClrDelErrorHandler);
                l.insert(oldtop);
                if (args != null)
                {
                    foreach (var arg in args)
                    {
                        l.PushLua(arg);
                    }
                }
                var argc = args == null ? 0 : args.Length;
                var code = l.pcall(argc, lua.LUA_MULTRET, oldtop);
                l.remove(oldtop);
                return code;
            }
            return lua.LUA_ERRERR;
        }
        public static object[] PushArgsAndCall(this IntPtr l, params object[] args)
        {
            if (l != IntPtr.Zero)
            {
                var oldtop = l.gettop() - 1;
                var code = PushArgsAndCallRaw(l, args);
                var newtop = l.gettop();
                object[] rv = null;
                if (code == 0 && newtop >= oldtop)
                {
                    rv = ObjectPool.GetReturnValueFromPool(newtop - oldtop);
                    for (int i = 0; i < (newtop - oldtop); ++i)
                    {
                        rv[i] = l.GetLua(i + oldtop + 1);
                    }
                }
                if (code != 0)
                {
                    if (GLog.IsLogErrorEnabled) GLog.LogError(l.GetLua(-1).UnwrapDynamic());
                }
                if (newtop >= oldtop)
                {
                    l.pop(newtop - oldtop);
                }
                return rv;
            }
            return null;
        }
    }
}