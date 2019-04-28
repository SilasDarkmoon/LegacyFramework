using System;
using System.Collections.Generic;
using Capstones.Dynamic;
using Capstones.LuaLib;
using Capstones.PlatExt;

using lua = Capstones.LuaLib.LuaCoreLib;
using lual = Capstones.LuaLib.LuaAuxLib;
using luae = Capstones.LuaLib.LuaLibEx;

namespace Capstones.LuaWrap
{
    public static class CapsLuaDelegateGenerator
    {
        public interface IDelegateDynamicWrapper
        {
            BaseDynamic Target { set; get; }
            Delegate MakeDelegate(Type deltype);
        }
        public abstract class BaseDelegateDynamicWrapper : IDelegateDynamicWrapper
        {
#if NETFX_CORE
            private Dictionary<Type, Delegate> _Cache = new Dictionary<Type, Delegate>();
#endif
            public BaseDynamic Target { get; set; }
            public Delegate MakeDelegate(Type deltype)
            {
#if NETFX_CORE
                Delegate rv = null;
                if (_Cache.TryGetValue(deltype, out rv))
                {
                    return rv;
                }

                System.Linq.Expressions.ParameterExpression[] expars = new System.Linq.Expressions.ParameterExpression[0];
                var pars = deltype.GetMethod("Invoke").GetParameters();
                if (pars != null)
                {
                    expars = new System.Linq.Expressions.ParameterExpression[pars.Length];
                    for (int i = 0; i < pars.Length; ++i)
                    {
                        expars[i] = System.Linq.Expressions.Expression.Parameter(pars[i].ParameterType, pars[i].Name);
                    }
                }
                System.Linq.Expressions.ConstantExpression extar = System.Linq.Expressions.Expression.Constant(this);
                rv = System.Linq.Expressions.Expression.Lambda(
                    deltype,
                    System.Linq.Expressions.Expression.Call(extar, "Call", null, expars),
                    true,
                    expars
                    ).Compile();
                _Cache[deltype] = rv;
                return rv;
#else
                return Delegate.CreateDelegate(deltype, this, "Call");
#endif
            }
        }

        public class ActionDynamicWrapper : BaseDelegateDynamicWrapper
        {
            public void Call()
            {
                Target.Call();
            }
        }
        public class ActionDynamicWrapper<T> : BaseDelegateDynamicWrapper
        {
            public void Call(T arg)
            {
                Target.Call(arg);
            }
        }
        public class ActionDynamicWrapper<T1, T2> : BaseDelegateDynamicWrapper
        {
            public void Call(T1 arg1, T2 arg2)
            {
                Target.Call(arg1, arg2);
            }
        }
        public class ActionDynamicWrapper<T1, T2, T3> : BaseDelegateDynamicWrapper
        {
            public void Call(T1 arg1, T2 arg2, T3 arg3)
            {
                Target.Call(arg1, arg2, arg3);
            }
        }
        public class ActionDynamicWrapper<T1, T2, T3, T4> : BaseDelegateDynamicWrapper
        {
            public void Call(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                Target.Call(arg1, arg2, arg3, arg4);
            }
        }
        public class ActionDynamicWrapper<T1, T2, T3, T4, T5> : BaseDelegateDynamicWrapper
        {
            public void Call(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            {
                Target.Call(arg1, arg2, arg3, arg4, arg5);
            }
        }
        public class ActionDynamicWrapper<T1, T2, T3, T4, T5, T6> : BaseDelegateDynamicWrapper
        {
            public void Call(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
            {
                Target.Call(arg1, arg2, arg3, arg4, arg5, arg6);
            }
        }
        public class ActionDynamicWrapper<T1, T2, T3, T4, T5, T6, T7> : BaseDelegateDynamicWrapper
        {
            public void Call(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
            {
                Target.Call(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }
        }
        public class ActionDynamicWrapper<T1, T2, T3, T4, T5, T6, T7, T8> : BaseDelegateDynamicWrapper
        {
            public void Call(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
            {
                Target.Call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            }
        }
        public class ActionDynamicWrapper<T1, T2, T3, T4, T5, T6, T7, T8, T9> : BaseDelegateDynamicWrapper
        {
            public void Call(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
            {
                Target.Call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
            }
        }

        public class FuncDynamicWrapper<R> : BaseDelegateDynamicWrapper
        {
            public R Call()
            {
                return Target.Call().UnwrapReturnValues<R>();
            }
        }
        public class FuncDynamicWrapper<R, T> : BaseDelegateDynamicWrapper
        {
            public R Call(T arg)
            {
                return Target.Call(arg).UnwrapReturnValues<R>();
            }
        }
        public class FuncDynamicWrapper<R, T1, T2> : BaseDelegateDynamicWrapper
        {
            public R Call(T1 arg1, T2 arg2)
            {
                return Target.Call(arg1, arg2).UnwrapReturnValues<R>();
            }
        }
        public class FuncDynamicWrapper<R, T1, T2, T3> : BaseDelegateDynamicWrapper
        {
            public R Call(T1 arg1, T2 arg2, T3 arg3)
            {
                return Target.Call(arg1, arg2, arg3).UnwrapReturnValues<R>();
            }
        }
        public class FuncDynamicWrapper<R, T1, T2, T3, T4> : BaseDelegateDynamicWrapper
        {
            public R Call(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                return Target.Call(arg1, arg2, arg3, arg4).UnwrapReturnValues<R>();
            }
        }
        public class FuncDynamicWrapper<R, T1, T2, T3, T4, T5> : BaseDelegateDynamicWrapper
        {
            public R Call(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            {
                return Target.Call(arg1, arg2, arg3, arg4, arg5).UnwrapReturnValues<R>();
            }
        }
        public class FuncDynamicWrapper<R, T1, T2, T3, T4, T5, T6> : BaseDelegateDynamicWrapper
        {
            public R Call(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
            {
                return Target.Call(arg1, arg2, arg3, arg4, arg5, arg6).UnwrapReturnValues<R>();
            }
        }
        public class FuncDynamicWrapper<R, T1, T2, T3, T4, T5, T6, T7> : BaseDelegateDynamicWrapper
        {
            public R Call(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
            {
                return Target.Call(arg1, arg2, arg3, arg4, arg5, arg6, arg7).UnwrapReturnValues<R>();
            }
        }
        public class FuncDynamicWrapper<R, T1, T2, T3, T4, T5, T6, T7, T8> : BaseDelegateDynamicWrapper
        {
            public R Call(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
            {
                return Target.Call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8).UnwrapReturnValues<R>();
            }
        }
        public class FuncDynamicWrapper<R, T1, T2, T3, T4, T5, T6, T7, T8, T9> : BaseDelegateDynamicWrapper
        {
            public R Call(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
            {
                return Target.Call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9).UnwrapReturnValues<R>();
            }
        }

        private static Type[] ActionWrapperTypes = new[]
        {
            typeof(ActionDynamicWrapper),
            typeof(ActionDynamicWrapper<>),
            typeof(ActionDynamicWrapper<,>),
            typeof(ActionDynamicWrapper<,,>),
            typeof(ActionDynamicWrapper<,,,>),
            typeof(ActionDynamicWrapper<,,,,>),
            typeof(ActionDynamicWrapper<,,,,,>),
            typeof(ActionDynamicWrapper<,,,,,,>),
            typeof(ActionDynamicWrapper<,,,,,,,>),
            typeof(ActionDynamicWrapper<,,,,,,,,>),
        };
        private static Type[] FuncWrapperTypes = new[]
        {
            typeof(FuncDynamicWrapper<>),
            typeof(FuncDynamicWrapper<,>),
            typeof(FuncDynamicWrapper<,,>),
            typeof(FuncDynamicWrapper<,,,>),
            typeof(FuncDynamicWrapper<,,,,>),
            typeof(FuncDynamicWrapper<,,,,,>),
            typeof(FuncDynamicWrapper<,,,,,,>),
            typeof(FuncDynamicWrapper<,,,,,,,>),
            typeof(FuncDynamicWrapper<,,,,,,,,>),
            typeof(FuncDynamicWrapper<,,,,,,,,,>),
        };
        public static Type GetWrapperType(Type returnType, int argc)
        {
            if (returnType == typeof(void))
            {
                if (argc >= 0 && argc < ActionWrapperTypes.Length)
                {
                    return ActionWrapperTypes[argc];
                }
            }
            else
            {
                if (argc >= 0 && argc < FuncWrapperTypes.Length)
                {
                    return FuncWrapperTypes[argc];
                }
            }
            return null;
        }

        private static IDelegateDynamicWrapper CreateDelegateImp(Type t, BaseDynamic dyn)
        {
            if (t != null && t.IsSubclassOf(typeof(Delegate)) && !object.ReferenceEquals(dyn, null))
            {
                System.Reflection.MethodInfo invoke = t.GetMethod("Invoke");
                System.Reflection.ParameterInfo[] pars = invoke.GetParameters();
                if (invoke.ReturnType == typeof(void))
                {
                    Type wrapperType = null;
                    if (pars != null && pars.Length > 0)
                    {
                        if (pars.Length < ActionWrapperTypes.Length)
                        {
                            wrapperType = ActionWrapperTypes[pars.Length];
                            Type[] genericTypes = new Type[pars.Length];
                            for (int i = 0; i < pars.Length; ++i)
                            {
                                genericTypes[i] = pars[i].ParameterType;
                            }
                            wrapperType = wrapperType.MakeGenericType(genericTypes);
                        }
                    }
                    else
                    {
                        wrapperType = typeof(ActionDynamicWrapper);
                    }
                    if (wrapperType != null)
                    {
                        var wrapper = Activator.CreateInstance(wrapperType) as IDelegateDynamicWrapper;
                        wrapper.Target = dyn;
                        return wrapper;
                    }
                }
                else
                {
                    var gcnt = 0;
                    if (pars != null && pars.Length > 0)
                    {
                        gcnt = pars.Length;
                    }
                    Type wrapperType = null;
                    Type[] genericTypes = new Type[gcnt + 1];
                    genericTypes[0] = invoke.ReturnType;
                    if (gcnt < FuncWrapperTypes.Length)
                    {
                        wrapperType = FuncWrapperTypes[gcnt];
                        if (gcnt > 0)
                        {
                            for (int i = 0; i < gcnt; ++i)
                            {
                                genericTypes[i + 1] = pars[i].ParameterType;
                            }
                            wrapperType = wrapperType.MakeGenericType(genericTypes);
                        }
                    }
                    if (wrapperType != null)
                    {
                        var wrapper = Activator.CreateInstance(wrapperType) as IDelegateDynamicWrapper;
                        wrapper.Target = dyn;
                        return wrapper;
                    }
                }
            }
            return null;
        }
        private static Delegate CreateDelegateFromWrapper(Type deltype, IDelegateDynamicWrapper delwrapper)
        {
            return delwrapper.MakeDelegate(deltype);
        }
        public static Delegate CreateDelegate(Type t, BaseDynamic dyn)
        {
            if (dyn is BaseLua)
            {
                var dynlua = dyn as BaseLua;
                var l = dynlua.L;
                var refid = dynlua.Refid;
                if (l != IntPtr.Zero && refid != 0)
                {
                    using (var lr = new LuaStateRecover(l))
                    {
                        l.GetField(lua.LUA_REGISTRYINDEX, "___delreg");
                        if (!l.istable(-1))
                        {
                            l.pop(1);

                            l.newtable(); // reg
                            l.newtable(); // reg meta
                            l.PushString("k"); // reg meta 'k'
                            l.SetField(-2, "__mode"); // reg meta
                            l.setmetatable(-2); // reg
                            l.pushvalue(-1); // reg reg
                            l.SetField(lua.LUA_REGISTRYINDEX, "___delreg"); // reg
                        }

                        l.getref(refid); // reg func
                        l.gettable(-2); // reg finfo
                        if (!l.istable(-1))
                        {
                            l.pop(1); // reg
                            l.newtable(); // reg finfo
                            l.pushvalue(-2); // reg finfo reg
                            l.getref(refid); // reg finfo reg func
                            l.pushvalue(-3); // reg finfo reg func finfo
                            l.settable(-3); // reg info reg
                            l.pop(1); //reg info
                        }

                        l.PushLua(t); // reg finfo dtype
                        l.gettable(-2); // reg finfo del
                        IDelegateDynamicWrapper delwrapper = null;
                        if (l.isuserdata(-1))
                        {
                            var wr = l.GetLua(-1).UnwrapDynamic<WeakReference>();
                            if (wr != null)
                            {
                                delwrapper = wr.GetWeakReference<IDelegateDynamicWrapper>();
                                if (delwrapper == null)
                                {
                                    wr.ReturnToPool();
                                }
                            }
                        }
                        if (delwrapper == null)
                        {
                            delwrapper = CreateDelegateImp(t, dyn);
                            if (delwrapper != null)
                            {
                                var wr = ObjectPool.GetWeakReferenceFromPool(delwrapper);
                                l.pop(1); // reg finfo
                                l.PushLua(t); // reg finfo dtype
                                l.PushRawObj(wr); // reg finfo dtype del
                                l.settable(-3); // reg finfo
                            }
                        }
                        if (delwrapper != null)
                        {
                            return CreateDelegateFromWrapper(t, delwrapper);
                        }
                    }
                }
            }
            var wrapper = CreateDelegateImp(t, dyn);
            if (wrapper != null)
            {
                return CreateDelegateFromWrapper(t, wrapper);
            }
            return null;
        }
        public static T CreateDelegate<T>(BaseDynamic dyn) where T : class
        {
            return CreateDelegate(typeof(T), dyn) as T;
        }
    }
}
