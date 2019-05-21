using System;
using System.IO;
using Capstones.UnityEngineEx;
using Capstones.UnityFramework;

#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
using UnityEditor;
#endif

namespace UnityEngine.UI
{
    [XLua.LuaCallCSharp]
    [AddComponentMenu("CapstoneUI/CapstoneImage", 11)]
    public class CapstoneImage : Image
    {
        private static string s_CapstoneETC1MaterialPath = "Assets/CapstonesRes/Common/Materials/UIETC1.mat";
        protected static Material s_CapstoneETC1Material;
        [SerializeField]
        private Texture2D m_AlphaTexture;
        [NonSerialized]
        private Texture2D m_OverrideAlphaTexture;

        protected CapstoneImage()
        {
#if UNITY_EDITOR
            this.raycastTarget = false;
#endif
        }

        private Texture2D activeAlphaTexture
        {
            get
            {
                return this.m_OverrideAlphaTexture != null ? this.m_OverrideAlphaTexture : this.m_AlphaTexture;
            }
        }

        public new Sprite sprite
        {
            get
            {
                return base.sprite;
            }
            set
            {
                if (value == null)
                {
                    this.m_AlphaTexture = null;
                }

                base.sprite = value;
            }
        }

        public new Sprite overrideSprite
        {
            get
            {
                return base.overrideSprite;
            }
            set
            {
                if (value == null)
                {
                    this.m_OverrideAlphaTexture = null;
                }

                base.overrideSprite = value;
            }
        }

        public static Material capstoneETC1Material
        {
            get
            {
                if (CapstoneImage.s_CapstoneETC1Material == null)
                    CapstoneImage.s_CapstoneETC1Material = (Material)ResManager.LoadRes(CapstoneImage.s_CapstoneETC1MaterialPath, typeof(Material));
                return CapstoneImage.s_CapstoneETC1Material;
            }
        }

        protected override void UpdateMaterial()
        {
            base.UpdateMaterial();

            if (this.activeAlphaTexture != null)
            {
                this.canvasRenderer.SetAlphaTexture(this.activeAlphaTexture);
            }
        }

        public void SetAtlasSprite(string spritePath)
        {
            Sprite newSprite;
            Texture2D alphaTexture;
            LoadSpriteAndAlphaTexture(spritePath, out newSprite, out alphaTexture);

            if (newSprite != null)
            {
                SetAtlasSprite(newSprite, alphaTexture);
            }
        }

        public void SetAtlasSprite(Sprite newSprite, Texture2D alphaTexture)
        {
            this.sprite = newSprite;
            this.m_AlphaTexture = alphaTexture;
            ResetMaterial(alphaTexture != null);
        }

        public void SetAtlasOverrideSprite(string spritePath)
        {
            Sprite newSprite;
            Texture2D alphaTexture;
            CapstoneImage.LoadSpriteAndAlphaTexture(spritePath, out newSprite, out alphaTexture);

            if (newSprite != null)
            {
                this.SetAtlasOverrideSprite(newSprite, alphaTexture);
            }
        }

        public void SetAtlasOverrideSprite(Sprite newSprite, Texture2D alphaTexture)
        {
            this.overrideSprite = newSprite;
            this.m_OverrideAlphaTexture = alphaTexture;
            this.ResetMaterial(alphaTexture != null);
        }

        private void ResetMaterial(bool isRgbAndAlphaAtlas)
        {
            if (isRgbAndAlphaAtlas)
            {
                if (this.material == this.defaultMaterial || this.material == CapstoneImage.defaultETC1GraphicMaterial)
                {
                    this.material = CapstoneImage.capstoneETC1Material;
                }
            }
            else
            {
                if (this.material == CapstoneImage.capstoneETC1Material)
                {
                    this.material = null;
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.sprite = null;
            this.overrideSprite = null;
            this.material = null;
        }

        public static void LoadSpriteAndAlphaTexture(string spritePath, out Sprite sprite, out Texture2D alphaTexture)
        {
            sprite = null;
            alphaTexture = null;

#if UNITY_EDITOR && !USE_CLIENT_RES_MANAGER
            string atlasPath = UIAtlasUtility.GetAtlasPathBySprite(spritePath);

            TextureImporter importer = AssetImporter.GetAtPath(atlasPath) as TextureImporter;

            if (importer == null)
            {
                return;
            }

            var spriteSheet = importer.spritesheet;
            string spriteName = Path.GetFileName(spritePath);

            for (int j = 0, sheetLength = spriteSheet.Length; j < sheetLength; j++)
            {
                var spriteMetaData = spriteSheet[j];

                if (spriteMetaData.name == spriteName)
                {
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
                    sprite = Sprite.Create(texture, spriteMetaData.rect, spriteMetaData.pivot, importer.spritePixelsPerUnit, 1, SpriteMeshType.FullRect, spriteMetaData.border);
                    sprite.name = spriteMetaData.name;
                    string directoryPath = Path.GetDirectoryName(atlasPath);
                    string atlasName = Path.GetFileNameWithoutExtension(atlasPath);
                    string extension = Path.GetExtension(atlasPath);
                    string alphaTexturePath = UnityPath.Combine(directoryPath, atlasName + UIAtlasUtility.alphaTextureSuffix + extension);
                    alphaTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(alphaTexturePath);
                    return;
                }
            }
#else
            sprite = (Sprite)ResManager.LoadRes(spritePath, typeof(Sprite));

            if (sprite != null)
            {
                string directoryPath = Path.GetDirectoryName(spritePath);
                string extension = Path.GetExtension(spritePath);
                string alphaTexturePath = UnityPath.Combine(directoryPath, sprite.texture.name + UIAtlasUtility.alphaTextureSuffix + extension);
                alphaTexture = (Texture2D)ResManager.LoadRes(alphaTexturePath, typeof(Texture2D));
            }
#endif
        }
    }
}