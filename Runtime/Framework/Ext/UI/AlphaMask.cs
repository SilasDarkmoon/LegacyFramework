using UnityEngine;
using UnityEngine.UI;

namespace UIExt
{
    [ExecuteInEditMode]
    public class AlphaMask : MonoBehaviour//, IMaterialModifier
    {
//        public string Path;
//        public TextAsset Source;
//        public Sprite Sprite;
//        public Vector4 MaskTillingAndOffset = new Vector4(1, 1, 0, 0);

//        private Sprite _RealSprite;
//        private bool _IsPacked = false;
//        private Sprite _CreatedSprite;

//        private void DestroyCreatedTex()
//        {
//            if (_CreatedSprite)
//            {
//                var tex = _CreatedSprite.texture;
//#if UNITY_EDITOR
//                DestroyImmediate(tex);
//                DestroyImmediate(_CreatedSprite);
//#else
//                Destroy(tex);
//                Destroy(_CreatedSprite);
//#endif
//                _CreatedSprite = null;
//            }
//        }
//        public void ApplySource()
//        {
//            DestroyCreatedTex();
//            _RealSprite = null;
//            _IsPacked = false;
//            if (Sprite)
//            {
//                _RealSprite = Sprite;
//            }
//            else if (Source)
//            {
//                var tex = ResManager.LoadTexFromBytes(Source);
//                if (tex)
//                {
//                    var source = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
//                    source.name = tex.name;
//                    _RealSprite = source;
//                    _CreatedSprite = _RealSprite;
//                }
//            }
//            if (!_RealSprite && !string.IsNullOrEmpty(Path))
//            {
//                var sprite = ResManager.LoadRes(Path) as Sprite;
//                _RealSprite = sprite;
//            }

//            if (_RealSprite && _RealSprite.texture)
//            {
//                _IsPacked = _RealSprite.name.Contains("?");

//                Material matPacked = ResManager.LoadRes("Assets/CapstonesRes/Common/Materials/UIAlphaMaskPacked.mat") as Material;
//                var image = GetComponent<Image>();
//                if (image)
//                {
//                    if (image.material == null || image.material == image.defaultMaterial || image.material.shader.name == "Custom/UI/Packed Sprites")
//                    {
//                        image.material = matPacked;
//                    }
//                }
//                var rawimage = GetComponent<RawImage>();
//                if (rawimage)
//                {
//                    if (rawimage.material == null || rawimage.material == rawimage.defaultMaterial || rawimage.material.shader.name == "Custom/UI/Packed Sprites")
//                    {
//                        rawimage.material = matPacked;
//                    }
//                }
//            }
//#if UNITY_EDITOR
//            _Saved_Path = Path;
//            _Saved_Source = Source;
//            _Saved_Sprite = Sprite;
//            _Saved_MaskTillingAndOffset = MaskTillingAndOffset;
//#endif
//        }

//        public void SetMaterialDirty()
//        {
//            var image = GetComponent<Image>();
//            if (image)
//            {
//                image.SetMaterialDirty();
//            }
//            var rawimage = GetComponent<RawImage>();
//            if (rawimage)
//            {
//                rawimage.SetMaterialDirty();
//            }
//        }

//#if UNITY_EDITOR
//        private string _Saved_Path;
//        private TextAsset _Saved_Source;
//        private Sprite _Saved_Sprite;
//        private Vector4 _Saved_MaskTillingAndOffset = new Vector4(1, 1, 0, 0);

//        void Update()
//        {
//            if (_Saved_Path != Path || _Saved_Source != Source || _Saved_Sprite != Sprite)
//            {
//                ApplySource();
//            }
//            else if (_Saved_MaskTillingAndOffset != MaskTillingAndOffset)
//            {
//                SetMaterialDirty();
//            }
//        }
//        void OnDisable()
//        {
//            ClearCachedMaterial();
//            SetMaterialDirty();
//        }
//        void OnEnable()
//        {
//            ApplySource();
//        }
//#endif

//        void Start()
//        {
//            ApplySource();
//        }

//        void OnDestroy()
//        {
//            DestroyCreatedTex();
//        }

//        private Material m_RenderMaterial;
//        private void ClearCachedMaterial()
//        {
//            if (this.m_RenderMaterial != null)
//            {
//#if UNITY_EDITOR
//                DestroyImmediate(this.m_RenderMaterial);
//#else
//                Destroy(this.m_RenderMaterial);
//#endif
//            }
//            this.m_RenderMaterial = null;
//        }
//        public Material GetModifiedMaterial(Material baseMaterial)
//        {
//            if (baseMaterial != null)
//            {
//                if (_IsPacked)
//                {
//                    if (baseMaterial.HasProperty("_DynamicLoadMask") && baseMaterial.HasProperty("_PackedSpritesMask"))
//                    {
//                        var _dyn = baseMaterial.GetFloat("_DynamicLoadMask");
//                        var _pack = baseMaterial.GetFloat("_PackedSpritesMask");
//                        if (_dyn == 1 || _pack == 0)
//                        {
//                            ClearCachedMaterial();
//                            Material material = new Material(baseMaterial)
//                            {
//                                name = "DynPacked  (" + baseMaterial.name + ")",
//                                hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.NotEditable
//                            };
//                            material.SetFloat("_DynamicLoadMask", 0);
//                            material.SetFloat("_PackedSpritesMask", 1);
//                            if (_RealSprite)
//                            {
//                                material.SetTexture("_MaskTex", _RealSprite.texture);
//                                material.SetTextureScale("_MaskTex", new Vector2(MaskTillingAndOffset.x, MaskTillingAndOffset.y));
//                                material.SetTextureOffset("_MaskTex", new Vector2(MaskTillingAndOffset.z, MaskTillingAndOffset.w));
//                                material.SetVector("_MaskUV", UnityEngine.Sprites.DataUtility.GetOuterUV(_RealSprite));
//                            }
//                            this.m_RenderMaterial = material;
//                            return this.m_RenderMaterial;
//                        }
//                    }
//                }
//                else
//                {
//                    if (baseMaterial.HasProperty("_PackedSpritesMask"))
//                    {
//                        var _pack = baseMaterial.GetFloat("_PackedSpritesMask");
//                        if (_pack != 0)
//                        {
//                            ClearCachedMaterial();
//                            Material material = new Material(baseMaterial)
//                            {
//                                name = "DynPacked  (" + baseMaterial.name + ")",
//                                hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.NotEditable
//                            };
//                            material.SetFloat("_PackedSpritesMask", 0);
//                            if (_RealSprite)
//                            {
//                                material.SetTexture("_MaskTex", _RealSprite.texture);
//                                material.SetTextureScale("_MaskTex", new Vector2(MaskTillingAndOffset.x, MaskTillingAndOffset.y));
//                                material.SetTextureOffset("_MaskTex", new Vector2(MaskTillingAndOffset.z, MaskTillingAndOffset.w));
//                                material.SetVector("_MaskUV", UnityEngine.Sprites.DataUtility.GetOuterUV(_RealSprite));
//                            }
//                            this.m_RenderMaterial = material;
//                            return this.m_RenderMaterial;
//                        }
//                    }
//                }
//            }
//            return baseMaterial;
//        }
    }
}