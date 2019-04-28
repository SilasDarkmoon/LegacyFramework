using UnityEngine;
using UnityEngine.UI;

namespace UIExt
{
    [XLua.LuaCallCSharp]
    [ExecuteInEditMode]
    public class BytesImageSource : MonoBehaviour, IMaterialModifier
    {
        public string Path;
        public TextAsset Source;
        public bool OnlyLoadWhenEmpty;
        private Texture2D _Loaded = null;
        private bool _IsPacked = false;
        private Texture2D _CreatedTex;
        private Sprite _CreatedSprite;
        public Image.Type ImageType;
        public bool PreserveAspect = false;
        public Image.FillMethod FillMethod;
        public int FillOrigin;
        public float FillAmount = 1;
        public bool FillClockwise = true;
        public bool FillCenter = true;
        public bool IsNativeSize = false;

#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
        private Image _TargetImage;
        private RawImage _TargetRawImage;
#endif

        private void DestroyCreatedTex()
        {
            if (_CreatedSprite)
            {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
                DestroyImmediate(_CreatedSprite);
#else
                Destroy(_CreatedSprite);
#endif
                _CreatedSprite = null;
            }
            if (_CreatedTex)
            {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
                DestroyImmediate(_CreatedTex);
#else
                Destroy(_CreatedTex);
#endif
                _CreatedTex = null;
            }
        }

        public void ApplySource()
        {
            DestroyDynamicChild();
            DestroyCreatedTex();
            _Loaded = null;
            if (Source)
            {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
                var path = UnityEditor.AssetDatabase.GetAssetPath(Source);
                if (path != null && path.StartsWith("Assets/CapstonesRes/", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    if (GLog.IsLogErrorEnabled) GLog.LogError("Only 'Path' is allowed in BytesImageSource. Donot use 'Source'(TextAsset).\n" + path + "\n" + gameObject.name);
                }
#endif
                var tex = Capstones.UnityFramework.ResManager.LoadTexFromBytes(Source);
                if (tex)
                {
                    _CreatedTex = tex;
                    _Loaded = tex;

                    var image = GetComponent<Image>();
                    if (image)
                    {
                        var source = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                        source.name = tex.name;
                        _CreatedSprite = (Sprite)source;
                        image.overrideSprite = (Sprite)source;
                        image.type = ImageType;
                        if (ImageType == Image.Type.Filled)
                        {
                            image.fillMethod = FillMethod;
                            image.fillOrigin = FillOrigin;
                            image.fillAmount = FillAmount;
                            image.fillClockwise = FillClockwise;
                        }
                        if (ImageType == Image.Type.Simple || ImageType == Image.Type.Filled)
                        {
                            image.preserveAspect = PreserveAspect;
                        }
                        else
                        {
                            image.fillCenter = FillCenter;
                        }

                        if(IsNativeSize && image.type == Image.Type.Simple)
                        {
                            var rtf = image.transform as RectTransform;
                            rtf.sizeDelta = new Vector2(tex.width, tex.height);
                        }
                    }
                    var rawimage = GetComponent<RawImage>();
                    if (rawimage)
                    {
                        rawimage.texture = tex;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(Path))
            {
                var sprite = Capstones.UnityFramework.ResManager.LoadRes(Path) as Sprite;
                if (sprite && sprite.texture)
                {
                    _Loaded = sprite.texture;
                    _IsPacked = sprite.name.Contains("?");
                    Material matPacked = null;
                    if (_IsPacked)
                    {
                        matPacked = Capstones.UnityFramework.ResManager.LoadRes("Assets/CapstonesRes/Common/Materials/PackedSpritesDynamic.mat") as Material;
                    }

                    var image = GetComponent<Image>();
                    if (image)
                    {
                        image.overrideSprite = sprite;
                        image.type = ImageType;
                        if (ImageType == Image.Type.Filled)
                        {
                            image.fillMethod = FillMethod;
                            image.fillOrigin = FillOrigin;
                            image.fillAmount = FillAmount;
                            image.fillClockwise = FillClockwise;
                        }
                        if (ImageType == Image.Type.Simple || ImageType == Image.Type.Filled)
                        {
                            image.preserveAspect = PreserveAspect;
                        }
                        else
                        {
                            image.fillCenter = FillCenter;
                        }
                        if (_IsPacked)
                        {
                            if (image.material == null || image.material == image.defaultMaterial)
                            {
                                image.material = matPacked;
                            }
                        }

                        if (IsNativeSize && image.type == Image.Type.Simple)
                        {
                            var rtf = image.transform as RectTransform;
                            rtf.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
                        }
                    }
                    var rawimage = GetComponent<RawImage>();
                    if (rawimage)
                    {
                        rawimage.texture = _Loaded;
                        if (_IsPacked)
                        {
                            if (rawimage.material == null || rawimage.material == rawimage.defaultMaterial)
                            {
                                rawimage.material = matPacked;
                            }
                        }
                    }
                }
            }
        }

        public void DestroyDynamicChild()
        {
            var image = GetComponent<Image>();
            if (image)
            {
                image.overrideSprite = null;
                if (image.material && image.material.shader.name == "Custom/UI/Packed Sprites")
                {
                    image.material = null;
                }
            }
            var rawimage = GetComponent<RawImage>();
            if (rawimage)
            {
                rawimage.texture = null;
                if (rawimage.material && rawimage.material.shader.name == "Custom/UI/Packed Sprites")
                {
                    rawimage.material = null;
                }
            }
        }

        public void SetMaterialDirty()
        {
            var image = GetComponent<Image>();
            if (image)
            {
                image.SetMaterialDirty();
            }
            var rawimage = GetComponent<RawImage>();
            if (rawimage)
            {
                rawimage.SetMaterialDirty();
            }
        }

        public bool IsImageEmpty()
        {
            var image = GetComponent<Image>();
            if (image)
            {
                if (image.overrideSprite)
                {
                    return false;
                }
            }
            var rawimage = GetComponent<RawImage>();
            if (rawimage)
            {
                if (rawimage.texture)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsImagePacked()
        {
            var image = GetComponent<Image>();
            if (image)
            {
                if (image.overrideSprite)
                {
                    var name = image.overrideSprite.name;
                    if (name.Contains("?"))
                    {
                        return true;
                    }
                }
            }
            var rawimage = GetComponent<RawImage>();
            if (rawimage)
            {
                if (rawimage.texture)
                {
                    var name = rawimage.texture.name;
                    if (name.Contains("?"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsImageDirty()
        {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            var image = _TargetImage;
            if (!Application.isPlaying)
            {
                image = GetComponent<Image>();
            }
            if (image)
            {
                if (!image.overrideSprite && _Loaded)
                {
                    return true;
                }
                else if (image.overrideSprite && image.overrideSprite.texture != _Loaded)
                {
                    return true;
                }
            }
            var rawimage = _TargetRawImage;
            if (!Application.isPlaying)
            {
                rawimage = GetComponent<RawImage>();
            }
            if (rawimage)
            {
                if (rawimage.texture != _Loaded)
                {
                    return true;
                }
            }
#endif
            return false;
        }
        void Update()
        {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            if (IsImageDirty())
            {
                if (Application.isEditor && !Application.isPlaying || !OnlyLoadWhenEmpty || IsImageEmpty())
                {
                    ApplySource();
                }
            }
            else
            {
                var prefab = UnityEditor.PrefabUtility.GetPrefabParent(gameObject) as GameObject;
                if (prefab)
                {
                    var path = UnityEditor.AssetDatabase.GetAssetPath(prefab);
                    if (!string.IsNullOrEmpty(path))
                    {
                        var rawimage = prefab.GetComponent<RawImage>();
                        if (rawimage)
                        {
                            if (rawimage.texture)
                            {
                                SavePrefabWithoutChild();
                                ApplySource();
                            }
                        }
                    }
                }
            }
#endif
        }

        public void SavePrefabWithoutChild()
        {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            DestroyDynamicChild();
            var prefab = UnityEditor.PrefabUtility.GetPrefabParent(gameObject);
            if (prefab)
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(prefab);
                if (!string.IsNullOrEmpty(path))
                {
                    var root = UnityEditor.PrefabUtility.FindRootGameObjectWithSameParentPrefab(gameObject);
                    UnityEditor.PrefabUtility.CreatePrefab(path, root, UnityEditor.ReplacePrefabOptions.ConnectToPrefab);
                }
            }
#endif
        }
        public void RebuildImage()
        {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            var image = GetComponent<Image>();
            if (image)
            {
                image.enabled = false;
                image.enabled = true;
            }
#endif
        }

        void OnDisable()
        {
            ClearCachedMaterial();
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            if (!Application.isPlaying)
            {
                DestroyDynamicChild();
            }
#endif
        }
        void OnEnable()
        {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            if (!Application.isPlaying)
            {
                if (IsImageDirty() || !OnlyLoadWhenEmpty || IsImageEmpty())
                {
                    ApplySource();
                }
            }
            else
            {
                if ((!_Loaded && !OnlyLoadWhenEmpty) || (IsImageEmpty() && OnlyLoadWhenEmpty))
                {
                    ApplySource();
                }
            }
#else
            if ((!_Loaded && !OnlyLoadWhenEmpty) || (IsImageEmpty() && OnlyLoadWhenEmpty))
            {
                ApplySource();
            }
#endif
        }

        void Awake()
        {
            var image = GetComponent<Image>();
            if (image)
            {
                if (ImageType != image.type)
                {
                    ImageType = image.type;
                }
                if (ImageType == Image.Type.Simple || ImageType == Image.Type.Filled)
                {
                    if (PreserveAspect != image.preserveAspect)
                    {
                        PreserveAspect = image.preserveAspect;
                    }
                }
                else
                {
                    if (FillCenter != image.fillCenter)
                    {
                        FillCenter = image.fillCenter;
                    }
                }
                if (ImageType == Image.Type.Filled)
                {
                    if (FillMethod != image.fillMethod)
                    {
                        FillMethod = image.fillMethod;
                    }
                    if (FillOrigin != image.fillOrigin)
                    {
                        FillOrigin = image.fillOrigin;
                    }
                    if (FillAmount != image.fillAmount)
                    {
                        FillAmount = image.fillAmount;
                    }
                    if (FillClockwise != image.fillClockwise)
                    {
                        FillClockwise = image.fillClockwise;
                    }
                }
            }
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            _TargetRawImage = null;
            _TargetImage = null;
            if (Application.isPlaying)
            {
                _TargetRawImage = GetComponent<RawImage>();
                _TargetImage = image;
            }
#endif
        }

        void Start()
        {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            var rawimage = GetComponent<RawImage>();
            if (rawimage)
            {
                if (rawimage.texture)
                {
                    SavePrefabWithoutChild();
                }
            }
#endif
            if (!Application.isEditor || Application.isPlaying)
            {
                if ((!_Loaded && !OnlyLoadWhenEmpty) || (IsImageEmpty() && OnlyLoadWhenEmpty))
                {
                    ApplySource();
                }
                else
                {
                    _IsPacked = IsImagePacked();
                    SetMaterialDirty();
                }
            }
            else
            {
                ApplySource();
            }
        }

        void OnDestroy()
        {
            DestroyCreatedTex();
        }

        private Material m_RenderMaterial;
        private void ClearCachedMaterial()
        {
            if (this.m_RenderMaterial != null)
            {
#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
                DestroyImmediate(this.m_RenderMaterial);
#else
                Destroy(this.m_RenderMaterial);
#endif
            }
            this.m_RenderMaterial = null;
        }
        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (this.IsActive())
            {
                if (baseMaterial != null)
                {
                    if (_IsPacked) // should we check ispacked again here? (becuse the tex may be changed by code after start)
                    {
                        if (baseMaterial.HasProperty("_DynamicLoad") && baseMaterial.HasProperty("_PackedSprites"))
                        {
                            var _dyn = baseMaterial.GetFloat("_DynamicLoad");
                            var _pack = baseMaterial.GetFloat("_PackedSprites");
                            if (_dyn == 0 || _pack == 0)
                            {
                                ClearCachedMaterial();
                                Material material = new Material(baseMaterial)
                                {
                                    name = "DynPacked  (" + baseMaterial.name + ")",
                                    hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.NotEditable
                                };
                                material.SetFloat("_DynamicLoad", 1);
                                material.SetFloat("_PackedSprites", 1);
                                this.m_RenderMaterial = material;
                                return this.m_RenderMaterial;
                            }
                        }
                    }
                    else
                    {
                        if (baseMaterial.HasProperty("_PackedSprites"))
                        {
                            var _pack = baseMaterial.GetFloat("_PackedSprites");
                            if (_pack != 0)
                            {
                                ClearCachedMaterial();
                                Material material = new Material(baseMaterial)
                                {
                                    name = "DynPacked  (" + baseMaterial.name + ")",
                                    hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.NotEditable
                                };
                                material.SetFloat("_PackedSprites", 0);
                                this.m_RenderMaterial = material;
                                return this.m_RenderMaterial;
                            }
                        }
                    }
                }
            }
            return baseMaterial;
        }

        public bool IsActive()
        {
            return enabled && isActiveAndEnabled && gameObject.activeInHierarchy;
        }

        public void SetNativeSize()
        {
            var image = GetComponent<Image>();
            if (image)
            {
                if (ImageType == Image.Type.Simple || ImageType == Image.Type.Filled)
                {
                    image.SetNativeSize();
                }
            }
        }
    }
}