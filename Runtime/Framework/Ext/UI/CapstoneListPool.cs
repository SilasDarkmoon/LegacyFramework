using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    internal static class CapstoneListPool<T>
    {
        private static readonly CapstoneObjectPool<List<T>> s_ListPool = new CapstoneObjectPool<List<T>>((UnityAction<List<T>>)null, (UnityAction<List<T>>)(l => l.Clear()));

        public static List<T> Get()
        {
            return CapstoneListPool<T>.s_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            CapstoneListPool<T>.s_ListPool.Release(toRelease);
        }
    }
}