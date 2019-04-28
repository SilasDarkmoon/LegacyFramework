using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class UIAtlasUtility
{
    public static string[] textureExtensions = { ".png", ".jpg", ".bytes" };
    public static readonly string alphaTextureSuffix = "_Alpha";
    public static readonly string rgbAndAlphaAtlasExtension = ".png";
    public static readonly string rgbAtlasExtension = ".jpg";

#if UNITY_EDITOR
    private static Dictionary<string, List<string>> texturePathsCache = new Dictionary<string, List<string>>();

    public static List<string> GetTexturePathsInDirectory(string directoryPath)
    {
        if (texturePathsCache.ContainsKey(directoryPath))
        {
            return texturePathsCache[directoryPath];
        }

        DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

        if (!directoryInfo.Exists)
        {
            if (GLog.IsLogWarningEnabled) GLog.LogWarning(directoryPath + " doesn't exist, please check directory path.");
            return null;
        }

        var fileInfos = directoryInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);
        List<string> texturePaths = new List<string>();

        foreach (var fileInfo in fileInfos)
        {
            if (textureExtensions.Contains(fileInfo.Extension))
            {
                string filePath = fileInfo.FullName;
                texturePaths.Add(filePath.Substring(Application.dataPath.Length - "Assets".Length)
                    .Replace('\\', '/'));
            }
        }
        texturePathsCache[directoryPath] = texturePaths;

        return texturePaths;
    }

    public static string GetAtlasPathBySprite(string spritePath)
    {
#if !USE_CLIENT_RES_MANAGER
        spritePath = Capstones.UnityFramework.ResManager.GetDistributeAssetName(spritePath);
#endif
        string spriteName = Path.GetFileName(spritePath);
        spritePath = GetAssetPathWithoutUITextures(spritePath);
        string directoryPath = Path.GetDirectoryName(spritePath);
        var texturePaths = GetTexturePathsInDirectory(directoryPath);

        if (texturePaths == null)
        {
            return null;
        }

        for (int i = 0, length = texturePaths.Count; i < length; i++)
        {
            var texturePath = texturePaths[i];
            string textureName = Path.GetFileNameWithoutExtension(texturePath);

            if (textureName == null || textureName.EndsWith(alphaTextureSuffix))
            {
                continue;
            }

            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;

            if (importer == null)
            {
                continue;
            }

            if (importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                continue;
            }

            var spriteSheet = importer.spritesheet;

            for (int j = 0, sheetLength = spriteSheet.Length; j < sheetLength; j++)
            {
                var spriteMetaData = spriteSheet[j];

                if (spriteMetaData.name == spriteName)
                {
                    return texturePath;
                }
            }
        }

        return null;
    }
#endif

    public static string GetAssetPathWithoutUITextures(string path)
    {
        if (path.StartsWith("Assets/UITextures/"))
        {
            path = "Assets/" + path.Substring("Assets/UITextures/".Length);
        }

        return path;
    }

    public static string GetAssetPathWithUITextures(string path)
    {
        if (!path.StartsWith("Assets/UITextures/"))
        {
            path = "Assets/UITextures/" + path.Substring("Assets/".Length);
        }

        return path;
    }
}