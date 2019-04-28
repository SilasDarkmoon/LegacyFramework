using System;
using System.Collections;
using System.Collections.Generic;
using Capstones.PlatExt;

namespace Capstones.Dynamic
{
    public class ClrObjectWrapper : BaseDynamic
    {
#if ENABLE_OBJ_POOL
        [ThreadStatic] protected internal static ObjectPool.GenericInstancePool<ClrObjectWrapper> _Pool;
        protected internal static ObjectPool.GenericInstancePool<ClrObjectWrapper> Pool
        {
            get
            {
                if (_Pool == null)
                {
                    _Pool = new ObjectPool.GenericInstancePool<ClrObjectWrapper>();
                }
                return _Pool;
            }
        }
#endif
        public static ClrObjectWrapper GetFromPool(object obj, Type type)
        {
#if ENABLE_OBJ_POOL
            return Pool.GetFromPool(() => new ClrObjectWrapper(obj, type), wrapper => wrapper.Init(obj, type));
#else
            return new ClrObjectWrapper(obj, type);
#endif
        }
        public override void ReturnToPool()
        {
#if ENABLE_OBJ_POOL
            Pool.ReturnToPool(this);
#endif
        }

        public override object Binding
        {
            get
            {
                return _Binding;
            }
            internal set
            {
                _Binding = value;
            }
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
        public ClrObjectWrapper(object obj)
        {
            Init(obj);
        }
        public ClrObjectWrapper(object obj, Type type)
        {
            Init(obj, type);
        }
        public void Init(object obj)
        {
            _Binding = null;
            BindingCore = null;
            if (obj != null)
            {
                BindingCore = ClrTypeCore.GetTypeCore(obj.GetType());
            }
            Binding = obj;
        }
        public void Init(object obj, Type type)
        {
            _Binding = null;
            BindingCore = null;
            if (type == null)
            {
                if (obj != null)
                {
                    BindingCore = ClrTypeCore.GetTypeCore(obj.GetType());
                }
                Binding = obj;
            }
            else
            {
                BindingCore = ClrTypeCore.GetTypeCore(type);
                if (type.IsInstanceOfType(obj))
                {
                    Binding = obj;
                }
            }
        }

        protected internal override object GetFieldImp(object key)
        {
            if (BindingCore != null)
            {
                var rv = BindingCore.GetFieldFor(Binding, key);
                if (rv is ClrCallable)
                {
                    ((ClrCallable)rv).Target = _Binding;
                }
                return rv;
            }
            return null;
        }
        protected internal override bool SetFieldImp(object key, object val)
        {
            if (BindingCore != null)
            {
                return BindingCore.SetFieldFor(Binding, key, val);
            }
            return false;
        }

#region operators
        public override object BinaryOp(string op, object other)
        {
            bool shouldCheckRightHand = true;
            switch (op)
            {
                // +
                case "+":
                    {
                        var func = BindingCore.GetFieldFor(null, "op_Addition").WrapDynamic();
                        if (object.ReferenceEquals(func, null))
                        {
                            break;
                        }
                        else
                        {
                            var rv = func.Call(Binding, other);
                            if (object.ReferenceEquals(rv, null) || rv.Length < 1)
                            {
                                if (shouldCheckRightHand)
                                    goto case "+2";
                            }
                            else
                            {
                                return rv[0];
                            }
                        }
                    }
                    break;
                case "+1":
                    shouldCheckRightHand = false;
                    goto case "+";
                case "+2":
                    {
                        if (!object.ReferenceEquals(other, null))
                        {
                            var wrapper = other.WrapDynamic();
                            if (!object.ReferenceEquals(wrapper, null))
                            {
                                return wrapper.BinaryOp("+1", Binding);
                            }
                        }
                    }
                    break;
                // *
                case "*":
                    {
                        var func = BindingCore.GetFieldFor(null, "op_Multiply").WrapDynamic();
                        if (object.ReferenceEquals(func, null))
                        {
                            break;
                        }
                        else
                        {
                            var rv = func.Call(Binding, other);
                            if (object.ReferenceEquals(rv, null) || rv.Length < 1)
                            {
                                if (shouldCheckRightHand)
                                    goto case "*2";
                            }
                            else
                            {
                                return rv[0];
                            }
                        }
                    }
                    break;
                case "*1":
                    shouldCheckRightHand = false;
                    goto case "*";
                case "*2":
                    {
                        if (!object.ReferenceEquals(other, null))
                        {
                            var wrapper = other.WrapDynamic();
                            if (!object.ReferenceEquals(wrapper, null))
                            {
                                return wrapper.BinaryOp("*1", Binding);
                            }
                        }
                    }
                    break;
                // -
                case "-":
                    {
                        var func = BindingCore.GetFieldFor(null, "op_Subtraction").WrapDynamic();
                        if (object.ReferenceEquals(func, null))
                        {
                            break;
                        }
                        else
                        {
                            var rv = func.Call(Binding, other);
                            if (object.ReferenceEquals(rv, null) || rv.Length < 1)
                            {
                                if (shouldCheckRightHand)
                                    goto case "-2";
                            }
                            else
                            {
                                return rv[0];
                            }
                        }
                    }
                    break;
                case "-1":
                    shouldCheckRightHand = false;
                    goto case "-";
                case "-2":
                    {
                        if (!object.ReferenceEquals(other, null))
                        {
                            var wrapper = other.WrapDynamic();
                            if (!object.ReferenceEquals(wrapper, null))
                            {
                                return wrapper.BinaryOp("-1", Binding);
                            }
                        }
                    }
                    break;
                // /
                case "/":
                    {
                        var func = BindingCore.GetFieldFor(null, "op_Division").WrapDynamic();
                        if (object.ReferenceEquals(func, null))
                        {
                            break;
                        }
                        else
                        {
                            var rv = func.Call(Binding, other);
                            if (object.ReferenceEquals(rv, null) || rv.Length < 1)
                            {
                                if (shouldCheckRightHand)
                                    goto case "/2";
                            }
                            else
                            {
                                return rv[0];
                            }
                        }
                    }
                    break;
                case "/1":
                    shouldCheckRightHand = false;
                    goto case "/";
                case "/2":
                    {
                        if (!object.ReferenceEquals(other, null))
                        {
                            var wrapper = other.WrapDynamic();
                            if (!object.ReferenceEquals(wrapper, null))
                            {
                                return wrapper.BinaryOp("/1", Binding);
                            }
                        }
                    }
                    break;
                // TODO: mod% pow^ concat.. lt< le<=
                case "==":
                default:
                    break;
            }
            return base.BinaryOp(op, other);
        }

        public override object UnaryOp(string op)
        {
            switch(op)
            {
                case "-":
                    {
                        var func = BindingCore.GetFieldFor(null, "op_UnaryNegation").WrapDynamic();
                        if (object.ReferenceEquals(func, null))
                        {
                            break;
                        }
                        else
                        {
                            var rv = func.Call(Binding);
                            if (object.ReferenceEquals(rv, null) || rv.Length < 1)
                            {
                                //break;
                            }
                            else
                            {
                                return rv[0];
                            }
                        }
                    }
                    break;
                // TODO: len#
                case "+":
                default:
                    break;
            }
            return base.UnaryOp(op);
        }
#endregion
    }
}