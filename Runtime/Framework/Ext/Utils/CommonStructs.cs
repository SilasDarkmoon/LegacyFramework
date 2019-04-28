using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Capstones.UnityFramework
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct BOOL
    {
        public BOOL(bool v)
        {
            val = v ? 1 : 0;
        }
        public BOOL(int v)
        {
            val = v;
        }

        public int val;
        public static implicit operator bool(BOOL v)
        {
            return v.val != 0;
        }
        public static implicit operator BOOL(bool v)
        {
            var v2 = new BOOL();
            v2.val = v ? 1 : 0;
            return v2;
        }
        public static bool operator==(BOOL v1, BOOL v2)
        {
            return (v1.val == 0) == (v2.val == 0);
        }
        public static bool operator!=(BOOL v1, BOOL v2)
        {
            return (v1.val == 0) != (v2.val == 0);
        }
        public static bool operator==(BOOL v1, bool v2)
        {
            return (v1.val != 0) == v2;
        }
        public static bool operator!=(BOOL v1, bool v2)
        {
            return (v1.val != 0) != v2;
        }
        public static bool operator ==(bool v1, BOOL v2)
        {
            return v1 == (v2.val != 0);
        }
        public static bool operator !=(bool v1, BOOL v2)
        {
            return v1 != (v2.val != 0);
        }

        public override bool Equals(object obj)
        {
            bool v1 = (val != 0);
            if (obj is bool)
            {
                return v1 == (bool)obj;
            }
            else if (obj is BOOL)
            {
                return v1 == (((BOOL)obj).val != 0);
            }
            return false;
        }
        public override int GetHashCode()
        {
            bool v1 = (val != 0);
            return v1.GetHashCode();
        }
        public override string ToString()
        {
            bool v1 = (val != 0);
            return v1.ToString();
        }
    }

    public struct Pack<T1, T2>
    {
        public T1 t1;
        public T2 t2;

        public Pack(T1 p1, T2 p2)
        {
            t1 = p1;
            t2 = p2;
        }
    }
    public struct Pack<T1, T2, T3>
    {
        public T1 t1;
        public T2 t2;
        public T3 t3;

        public Pack(T1 p1, T2 p2, T3 p3)
        {
            t1 = p1;
            t2 = p2;
            t3 = p3;
        }
    }
    public struct Pack<T1, T2, T3, T4>
    {
        public T1 t1;
        public T2 t2;
        public T3 t3;
        public T4 t4;

        public Pack(T1 p1, T2 p2, T3 p3, T4 p4)
        {
            t1 = p1;
            t2 = p2;
            t3 = p3;
            t4 = p4;
        }
    }
    public struct Pack<T1, T2, T3, T4, T5>
    {
        public T1 t1;
        public T2 t2;
        public T3 t3;
        public T4 t4;
        public T5 t5;

        public Pack(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        {
            t1 = p1;
            t2 = p2;
            t3 = p3;
            t4 = p4;
            t5 = p5;
        }
    }
    public struct Pack<T1, T2, T3, T4, T5, T6>
    {
        public T1 t1;
        public T2 t2;
        public T3 t3;
        public T4 t4;
        public T5 t5;
        public T6 t6;

        public Pack(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
        {
            t1 = p1;
            t2 = p2;
            t3 = p3;
            t4 = p4;
            t5 = p5;
            t6 = p6;
        }
    }
    public struct Pack<T1, T2, T3, T4, T5, T6, T7>
    {
        public T1 t1;
        public T2 t2;
        public T3 t3;
        public T4 t4;
        public T5 t5;
        public T6 t6;
        public T7 t7;

        public Pack(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7)
        {
            t1 = p1;
            t2 = p2;
            t3 = p3;
            t4 = p4;
            t5 = p5;
            t6 = p6;
            t7 = p7;
        }
    }
    public struct Pack<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        public T1 t1;
        public T2 t2;
        public T3 t3;
        public T4 t4;
        public T5 t5;
        public T6 t6;
        public T7 t7;
        public T8 t8;

        public Pack(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8)
        {
            t1 = p1;
            t2 = p2;
            t3 = p3;
            t4 = p4;
            t5 = p5;
            t6 = p6;
            t7 = p7;
            t8 = p8;
        }
    }
    public struct Pack<T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        public T1 t1;
        public T2 t2;
        public T3 t3;
        public T4 t4;
        public T5 t5;
        public T6 t6;
        public T7 t7;
        public T8 t8;
        public T9 t9;

        public Pack(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9)
        {
            t1 = p1;
            t2 = p2;
            t3 = p3;
            t4 = p4;
            t5 = p5;
            t6 = p6;
            t7 = p7;
            t8 = p8;
            t9 = p9;
        }
    }
    public struct Pack<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
    {
        public T1 t1;
        public T2 t2;
        public T3 t3;
        public T4 t4;
        public T5 t5;
        public T6 t6;
        public T7 t7;
        public T8 t8;
        public T9 t9;
        public T10 t10;

        public Pack(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10)
        {
            t1 = p1;
            t2 = p2;
            t3 = p3;
            t4 = p4;
            t5 = p5;
            t6 = p6;
            t7 = p7;
            t8 = p8;
            t9 = p9;
            t10 = p10;
        }
    }

    public struct ValueList<T> : IList<T>
    {
        private T t0;
        private T t1;
        private T t2;
        private T t3;
        private T t4;
        private T t5;
        private T t6;
        private T t7;
        private T t8;
        private T t9;
        private List<T> tx;

        private int _cnt;

        #region static funcs for set and get
        private delegate T GetTDel(ref ValueList<T> list);
        private delegate void SetTDel(ref ValueList<T> list, T val);

        private static T GetT0(ref ValueList<T> list) { return list.t0; }
        private static T GetT1(ref ValueList<T> list) { return list.t1; }
        private static T GetT2(ref ValueList<T> list) { return list.t2; }
        private static T GetT3(ref ValueList<T> list) { return list.t3; }
        private static T GetT4(ref ValueList<T> list) { return list.t4; }
        private static T GetT5(ref ValueList<T> list) { return list.t5; }
        private static T GetT6(ref ValueList<T> list) { return list.t6; }
        private static T GetT7(ref ValueList<T> list) { return list.t7; }
        private static T GetT8(ref ValueList<T> list) { return list.t8; }
        private static T GetT9(ref ValueList<T> list) { return list.t9; }

        private static void SetT0(ref ValueList<T> list, T val) { list.t0 = val; }
        private static void SetT1(ref ValueList<T> list, T val) { list.t1 = val; }
        private static void SetT2(ref ValueList<T> list, T val) { list.t2 = val; }
        private static void SetT3(ref ValueList<T> list, T val) { list.t3 = val; }
        private static void SetT4(ref ValueList<T> list, T val) { list.t4 = val; }
        private static void SetT5(ref ValueList<T> list, T val) { list.t5 = val; }
        private static void SetT6(ref ValueList<T> list, T val) { list.t6 = val; }
        private static void SetT7(ref ValueList<T> list, T val) { list.t7 = val; }
        private static void SetT8(ref ValueList<T> list, T val) { list.t8 = val; }
        private static void SetT9(ref ValueList<T> list, T val) { list.t9 = val; }

        private static GetTDel[] GetTFuncs = new GetTDel[]
        {
            GetT0,
            GetT1,
            GetT2,
            GetT3,
            GetT4,
            GetT5,
            GetT6,
            GetT7,
            GetT8,
            GetT9,
        };
        private static SetTDel[] SetTFuncs = new SetTDel[]
        {
            SetT0,
            SetT1,
            SetT2,
            SetT3,
            SetT4,
            SetT5,
            SetT6,
            SetT7,
            SetT8,
            SetT9,
        };
        #endregion

        #region IList<T>
        public int IndexOf(T item)
        {
            for (int i = 0; i < _cnt; ++i)
            {
                if (object.Equals(this[i], item))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            if (index >= 0 && index <= _cnt)
            {
                this.Add(default(T));
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
                this[_cnt - 1] = default(T);
                --_cnt;
            }
        }

        public T this[int index]
        {
            get
            {
                if (index >= 0 && index < _cnt)
                {
                    if (index < GetTFuncs.Length)
                    {
                        return GetTFuncs[index](ref this);
                    }
                    else
                    {
                        if (tx != null)
                        {
                            var pindex = index - GetTFuncs.Length;
                            if (pindex < tx.Count)
                            {
                                return tx[pindex];
                            }
                        }
                    }
                }
                return default(T);
            }
            set
            {
                if (index >= 0 && index < _cnt)
                {
                    if (index < SetTFuncs.Length)
                    {
                        SetTFuncs[index](ref this, value);
                    }
                    else
                    {
                        if (tx != null)
                        {
                            var pindex = index - SetTFuncs.Length;
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

        public void Add(T item)
        {
            if (_cnt < SetTFuncs.Length)
            {
                this[_cnt++] = item;
            }
            else
            {
                ++_cnt;
                if (tx == null)
                {
                    tx = new List<T>(8);
                }
                tx.Add(item);
            }
        }

        public void Clear()
        {
            _cnt = 0;
            t0 = default(T);
            t1 = default(T);
            t2 = default(T);
            t3 = default(T);
            t4 = default(T);
            t5 = default(T);
            t6 = default(T);
            t7 = default(T);
            t8 = default(T);
            t9 = default(T);
            tx = null;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
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

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index >= 0 && index < _cnt)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _cnt; ++i)
            {
                yield return this[i];
            }
        }
        #endregion

        public override bool Equals(object obj)
        {
            if (obj is ValueList<T>)
            {
                ValueList<T> types2 = (ValueList<T>)obj;
                if (types2._cnt == _cnt)
                {
                    for (int i = 0; i < _cnt; ++i)
                    {
                        if (!object.Equals(this[i], types2[i]))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        internal static bool OpEquals(ValueList<T> source, ValueList<T> other)
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
        public static bool operator ==(ValueList<T> source, ValueList<T> other)
        {
            return OpEquals(source, other);
        }
        public static bool operator !=(ValueList<T> source, ValueList<T> other)
        {
            return !OpEquals(source, other);
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
}
