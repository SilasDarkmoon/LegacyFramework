using Capstones.UnityEngineEx;
using UnityEngine;

public class BytesImagePartInfo : ScriptableObject
{
    public string FileName;
    public BytesImageSpriteInfo MainSprite;
    public System.Collections.Generic.List<BytesImageSpriteInfo> SpriteInfos;
}

[System.Serializable]
public class BytesImageSpriteInfo
{
    public string SpriteName;
    public string PackPath;
    public Rect Region;
    public float PPU;
    public Vector4 Border;

    public Sprite CreateSprite()
    {
        var tex = ResManager.LoadRes(PackPath, typeof(Texture2D)) as Texture2D;
        if (tex == null)
        {
            return null;
        }
        var sprite = Sprite.Create(tex, Region, new Vector2(0.5f, 0.5f), PPU, 0, SpriteMeshType.Tight, Border);
        sprite.name = SpriteName;
        return sprite;
    }
}
