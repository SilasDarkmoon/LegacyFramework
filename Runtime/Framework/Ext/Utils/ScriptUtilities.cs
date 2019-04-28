using System.Collections.Generic;
using UnityEngine;

namespace Capstones.UnityFramework
{
    public static class ScriptUtilities
    {
        public static T AddMissingComponent<T>(this GameObject go) where T : Component
        {
            if (go != null)
            {
                var component = go.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
                component = go.AddComponent<T>();
                return component;
            }
            return null;
        }

        public static Component AddMissingComponent(this GameObject go, System.Type compType)
        {
            if (go != null && compType != null && typeof(Component).IsAssignableFrom(compType))
            {
                var component = go.GetComponent(compType);
                if (component != null)
                {
                    return component;
                }
                component = go.AddComponent(compType);
                return component;
            }
            return null;
        }

        public static T[] GetComponentsInChildrenExplicit<T>(this GameObject par) where T : Component
        {
            return GetComponentsInChildrenExplicit<T>(par.transform);
        }
        public static T[] GetComponentsInChildrenExplicit<T>(this GameObject par, bool includeInactive) where T : Component
        {
            return GetComponentsInChildrenExplicit<T>(par.transform, includeInactive);
        }
        public static Component[] GetComponentsInChildrenExplicit(this GameObject par, System.Type type)
        {
            return GetComponentsInChildrenExplicit(par.transform, type);
        }
        public static Component[] GetComponentsInChildrenExplicit(this GameObject par, System.Type type, bool includeInactive)
        {
            return GetComponentsInChildrenExplicit(par.transform, type, includeInactive);
        }
        public static T[] GetComponentsInChildrenExplicit<T>(this Component par) where T : Component
        {
            return GetComponentsInChildrenExplicit<T>(par.transform);
        }
        public static T[] GetComponentsInChildrenExplicit<T>(this Component par, bool includeInactive) where T : Component
        {
            return GetComponentsInChildrenExplicit<T>(par.transform, includeInactive);
        }
        public static Component[] GetComponentsInChildrenExplicit(this Component par, System.Type type)
        {
            return GetComponentsInChildrenExplicit(par.transform, type);
        }
        public static Component[] GetComponentsInChildrenExplicit(this Component par, System.Type type, bool includeInactive)
        {
            return GetComponentsInChildrenExplicit(par.transform, type, includeInactive);
        }
        public static T[] GetComponentsInChildrenExplicit<T>(this Transform par) where T : Component
        {
            List<T> rv = new List<T>();
            int cc = par.childCount;
            for (int i = 0; i < cc; ++i)
            {
                var c = par.GetChild(i);
                c.GetComponentsInChildren<T>(rv);
            }
            return rv.ToArray();
        }
        public static T[] GetComponentsInChildrenExplicit<T>(this Transform par, bool includeInactive) where T : Component
        {
            List<T> rv = new List<T>();
            int cc = par.childCount;
            for (int i = 0; i < cc; ++i)
            {
                var c = par.GetChild(i);
                c.GetComponentsInChildren<T>(includeInactive, rv);
            }
            return rv.ToArray();
        }
        public static Component[] GetComponentsInChildrenExplicit(this Transform par, System.Type type)
        {
            List<Component> rv = new List<Component>();
            int cc = par.childCount;
            for (int i = 0; i < cc; ++i)
            {
                var c = par.GetChild(i);
                rv.AddRange(c.GetComponentsInChildren(type));
            }
            return rv.ToArray();
        }
        public static Component[] GetComponentsInChildrenExplicit(this Transform par, System.Type type, bool includeInactive)
        {
            List<Component> rv = new List<Component>();
            int cc = par.childCount;
            for (int i = 0; i < cc; ++i)
            {
                var c = par.GetChild(i);
                rv.AddRange(c.GetComponentsInChildren(type, includeInactive));
            }
            return rv.ToArray();
        }
    }
}