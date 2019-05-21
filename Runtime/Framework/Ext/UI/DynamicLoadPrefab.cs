using UnityEngine;
using UnityEngine.UI;
using Capstones.UnityFramework;
using Capstones.UnityEngineEx;

namespace UIExt
{
    [ExecuteInEditMode]
    public class DynamicLoadPrefab : MonoBehaviour
    {
        public GameObject Source;
        public string Path;
        [SerializeField]
        protected internal GameObject _Child;

        public GameObject Child
        {
            get { return _Child; }
        }

#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
        public enum Lifetime
        {
            Empty = 0,
            Loaded,
            Frozen,
        }
        [System.NonSerialized]
        public Lifetime _Lifetime;
        private enum DelayedWork
        {
            None = 0,
            OnEnable,
            OnDisable,
        }
        private DelayedWork _DelayedWork;

        public static void MarkChildrenFrozen(Component comp)
        {
            if (comp)
            {
                var ccnt = comp.transform.childCount;
                for (int i = 0; i < ccnt; ++i)
                {
                    var child = comp.transform.GetChild(i);
                    var ccomps = child.GetComponentsInChildren<DynamicLoadPrefab>(true);
                    if (ccomps != null)
                    {
                        foreach (var ccomp in ccomps)
                        {
                            ccomp._Lifetime = Lifetime.Frozen;
                        }
                    }
                }
            }
        }
        public static void MarkFrozen(Component comp)
        {
            if (comp)
            {
                var ccomps = comp.GetComponentsInChildren<DynamicLoadPrefab>(true);
                if (ccomps != null)
                {
                    foreach (var ccomp in ccomps)
                    {
                        ccomp._Lifetime = Lifetime.Frozen;
                    }
                }
            }
        }

        public void MarkChildrenFrozen()
        {
            MarkChildrenFrozen(this);
        }
        public void MarkFrozen()
        {
            MarkFrozen(this);
        }

        void Start()
        {
            //EditorBridge.OnPlayModeChanged += OnPlayModeChanged;
            //CheckAndSavePrefab();
            ApplySource();
        }
        void OnDestroy()
        {
            //EditorBridge.OnPlayModeChanged -= OnPlayModeChanged;
            MarkFrozen();
        }
        public void DestroyDynamicChildren()
        {
            if (this && _Lifetime != Lifetime.Frozen)
            {
                DestroyDynamicChildrenImp();
            }
        }
        private void DestroyDynamicChildrenImp()
        {
            var children = this.GetComponentsInChildrenExplicit<DynamicLoadPrefabChild>(true);
            foreach (var child in children)
            {
                if (child)
                {
                    MarkFrozen(child);
                    DestroyImmediate(child.gameObject);
                }
            }
            _Child = null;
            _Lifetime = Lifetime.Empty;
        }
        private void OnPlayModeChanged()
        {
            if (this) // when parent destroy child, the child's event is removed in OnDestroy, but this will be triged once.
            {
                var isPlaying = UnityEngine.Application.isPlaying;
                var isToPlay = UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
                if (!isPlaying && isToPlay || isPlaying && !isToPlay)
                { // pre changing.
                    DestroyDynamicChildrenImp();
                    MarkFrozen();
                }
                else
                { // after changing
                    if (_Lifetime == Lifetime.Frozen)
                    { // this component is frozen by last pre-changing
                        _Lifetime = Lifetime.Empty;
                    }
                }
            }
        }
        public void ApplySource()
        {
            if (_Lifetime == Lifetime.Empty)
            {
                DestroyDynamicChildren();
            
                var source = Source;
                if (!source && !string.IsNullOrEmpty(Path))
                {
                    source = ResManager.LoadRes(Path, typeof(GameObject)) as GameObject;
                }
                if (source)
                {
                    var go = UnityEditor.PrefabUtility.InstantiatePrefab(source) as GameObject;
                    if (go)
                    {
                        go.transform.SetParent(transform, false);
                        go.name = source.name;
                        _Child = go;

                        if (!Application.isPlaying)
                        {
                            foreach (var canvas in go.GetComponentsInChildren<Canvas>())
                            {
                                if (canvas)
                                {
                                    if (canvas.isActiveAndEnabled)
                                    {
                                        if (!canvas.GetComponent<DynamicLoadPrefabCanvas>())
                                        {
                                            canvas.gameObject.AddComponent<DynamicLoadPrefabCanvas>();
                                        }
                                    }
                                }
                            }
                        }

                        if (Application.isPlaying)
                        {
                            var selfLuaBehav = this.GetComponent<LuaBehaviour>();
                            var childLuaBehav = go.GetComponent<LuaBehaviour>();
                            if (selfLuaBehav && childLuaBehav)
                            {
                                childLuaBehav.CallLuaFunc("OnDynamicLoad", selfLuaBehav.lua);
                            }
                        }
                        else
                        {
                            if (!go.GetComponent<DynamicLoadPrefabChild>())
                            {
                                go.AddComponent<DynamicLoadPrefabChild>();
                            }
                        }
                        _Lifetime = Lifetime.Loaded;
                    }
                }
            }
        }

        void Update()
        {
            CheckAndSavePrefab();
            if (_Lifetime == Lifetime.Loaded && !_Child)
            {
                _Lifetime = Lifetime.Empty;
            }
            ApplySource();
        }
        void OnEnable()
        {
            UnregDelayedWork();
            if (_Lifetime != Lifetime.Frozen && !Application.isPlaying)
            {
                //EditorBridge.OnDelayedCallOnce += OnEnableDelayed;
                _DelayedWork = DelayedWork.OnEnable;
            }
        }
        private void OnEnableDelayed()
        {
            if (this)
            {
                Update();
            }
        }
        void OnDisable()
        {
            UnregDelayedWork();
            if (_Lifetime != Lifetime.Frozen && !Application.isPlaying)
            {
                //EditorBridge.OnDelayedCallOnce += OnDisableDelayed;
                _DelayedWork = DelayedWork.OnDisable;
            }
        }
        private void OnDisableDelayed()
        {
            if (this)
            {
                DestroyDynamicChildren();
            }
        }
        private void UnregDelayedWork()
        {
            if (_DelayedWork == DelayedWork.OnEnable)
            {
                //EditorBridge.OnDelayedCallOnce -= OnEnableDelayed;
                _DelayedWork = DelayedWork.None;
            }
            else if (_DelayedWork == DelayedWork.OnDisable)
            {
                //EditorBridge.OnDelayedCallOnce -= OnDisableDelayed;
                _DelayedWork = DelayedWork.None;
            }
        }

        public void CheckAndSavePrefab()
        {
            if (_Child)
            {
                if (_Lifetime == Lifetime.Loaded)
                {
                    var cprefab = UnityEditor.PrefabUtility.FindRootGameObjectWithSameParentPrefab(_Child);
                    if (cprefab)
                    {
                        cprefab = UnityEditor.PrefabUtility.GetPrefabParent(cprefab) as GameObject;
                        var cpath = UnityEditor.AssetDatabase.GetAssetPath(cprefab);

                        bool isChildSavedInParent = false;
                        var prefab = UnityEditor.PrefabUtility.FindRootGameObjectWithSameParentPrefab(gameObject);
                        if (prefab)
                        {
                            prefab = UnityEditor.PrefabUtility.GetPrefabParent(prefab) as GameObject;
                            var path = UnityEditor.AssetDatabase.GetAssetPath(prefab);
                            if (!string.IsNullOrEmpty(path))
                            {
                                if (path == cpath)
                                {
                                    isChildSavedInParent = true;
                                }
                            }
                        }

                        if (isChildSavedInParent)
                        {
                            SavePrefabWithoutChild();
                        }
                        else
                        {
                            if (cprefab != null && cprefab.GetComponent<DynamicLoadPrefabChild>())
                            {
                                SavePrefabChild();
                            }
                        }
                    }
                }
                else if (_Lifetime == Lifetime.Empty)
                {
                    var prefab = UnityEditor.PrefabUtility.GetPrefabParent(this) as DynamicLoadPrefab;
                    if (prefab != null && prefab._Child)
                    {
                        SavePrefabWithoutChild();
                    }
                }
            }
        }
        public void SavePrefabWithoutChild()
        {
            var prefab = UnityEditor.PrefabUtility.GetPrefabParent(gameObject);
            if (prefab)
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(prefab);
                if (!string.IsNullOrEmpty(path))
                {
                    var root = UnityEditor.PrefabUtility.FindRootGameObjectWithSameParentPrefab(gameObject);

                    var rccomp = root.GetComponent<DynamicLoadPrefabChild>();
                    bool brccomp = rccomp;
                    if (brccomp)
                    {
                        DestroyImmediate(rccomp);
                    }

                    var comps = root.GetComponentsInChildren<DynamicLoadPrefab>(true);
                    foreach (var comp in comps)
                    {
                        if (comp)
                        {
                            comp.DestroyDynamicChildren();
                        }
                    }

                    UnityEditor.PrefabUtility.CreatePrefab(path, root, UnityEditor.ReplacePrefabOptions.ConnectToPrefab);
                    UnityEditor.AssetDatabase.SaveAssets();

                    if (brccomp)
                    {
                        root.AddComponent<DynamicLoadPrefabChild>();
                    }
                }
            }
        }
        public void SavePrefabChild()
        {
            var cprefab = UnityEditor.PrefabUtility.FindRootGameObjectWithSameParentPrefab(_Child);
            if (cprefab && cprefab == _Child)
            {
                cprefab = UnityEditor.PrefabUtility.GetPrefabParent(cprefab) as GameObject;

                if (cprefab)
                {
                    var cpath = UnityEditor.AssetDatabase.GetAssetPath(cprefab);
                    if (!string.IsNullOrEmpty(cpath))
                    {
                        var ccomp = _Child.GetComponent<DynamicLoadPrefabChild>();
                        if (ccomp)
                        {
                            DestroyImmediate(ccomp);

                            var comps = _Child.GetComponentsInChildren<DynamicLoadPrefab>(true);
                            foreach (var comp in comps)
                            {
                                if (comp)
                                {
                                    comp.DestroyDynamicChildren();
                                }
                            }

                            UnityEditor.PrefabUtility.CreatePrefab(cpath, _Child, UnityEditor.ReplacePrefabOptions.ConnectToPrefab);
                            UnityEditor.AssetDatabase.SaveAssets();

                            _Child.AddComponent<DynamicLoadPrefabChild>();
                        }
                    }
                }
            }
        }
#else
        void Start()
        {
            ApplySource();
        }
        public void ApplySource()
        {
            if (_Child)
            {
                return;
            }

            var source = Source;
            if (!source && !string.IsNullOrEmpty(Path))
            {
                source = Capstones.UnityFramework.ResManager.LoadRes(Path, typeof(GameObject)) as GameObject;
            }
            if (source)
            {
                var go = Instantiate(source) as GameObject;
                if (go)
                {
                    go.transform.SetParent(transform, false);
                    go.name = source.name;
                    _Child = go;

                    var selfLuaBehav = this.GetComponent<LuaBehaviour>();
                    var childLuaBehav = go.GetComponent<LuaBehaviour>();
                    if (selfLuaBehav && childLuaBehav)
                    {
                        childLuaBehav.CallLuaFunc("OnDynamicLoad", selfLuaBehav.lua);
                    }
                }
            }
        }
#endif
    }
}