using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextAdjustLanguageLineSpacing : Text
{
    [System.Serializable]
    public class LanguageLineSpacing
    {
        public string language = "";
        public float lineSpacing = 1f;
    }
    const string prefixFilter = "lang_";
    const float defaultLineSpacing = 1;

    public LanguageLineSpacing[] languageLineSpacingList = new LanguageLineSpacing[] { };

    private Dictionary<string, float> _languageLineSpacingMap = null;
    private Dictionary<string, float> LanguageLineSpacingMap
    {
        get
        {
            if (_languageLineSpacingMap == null)
            {
                _languageLineSpacingMap = new Dictionary<string, float>();
                for (int i = 0; i < languageLineSpacingList.Length; i++)
                {
                    _languageLineSpacingMap.Add(languageLineSpacingList[i].language, languageLineSpacingList[i].lineSpacing);
                }
            }
            return _languageLineSpacingMap;
        }
    }

    private string GetValidLanguage()
    {
        string[] allFlags = Capstones.UnityFramework.ResManager.GetDistributeFlags();

        for (int i = allFlags.Length - 1; i >= 0; i--)
        {
            if (allFlags[i].StartsWith(prefixFilter))
            {
                return allFlags[i];
            }
        }

        return null;
    }

    private float GetLineSpacingWithLanguage(string language)
    {
        if (string.IsNullOrEmpty(language)) return defaultLineSpacing;
        float lineSpacing = 1;
        if (LanguageLineSpacingMap.TryGetValue(language, out lineSpacing))
        {
            return lineSpacing;
        }

        return defaultLineSpacing;
    }

    public TextGenerationSettings GetGenerationSettingsPlus(Vector2 extents)
    {
        var settings = base.GetGenerationSettings(extents);

        settings.lineSpacing = GetLineSpacingWithLanguage(GetValidLanguage());

        return settings;
    }

    readonly UIVertex[] m_TempVertsPlus = new UIVertex[4];
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (font == null)
            return;

        // We don't care if we the font Texture changes while we are doing our Update.
        // The end result of cachedTextGenerator will be valid for this instance.
        // Otherwise we can get issues like Case 619238.
        m_DisableFontTextureRebuiltCallback = true;

        Vector2 extents = rectTransform.rect.size;

        var settings = GetGenerationSettingsPlus(extents);
        cachedTextGenerator.PopulateWithErrors(text, settings, gameObject);

        // Apply the offset to the vertices
        IList<UIVertex> verts = cachedTextGenerator.verts;
        float unitsPerPixel = 1 / pixelsPerUnit;
        //Last 4 verts are always a new line... (\n)
        int vertCount = verts.Count - 4;

        Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
        roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
        toFill.Clear();
        if (roundingOffset != Vector2.zero)
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVertsPlus[tempVertsIndex] = verts[i];
                m_TempVertsPlus[tempVertsIndex].position *= unitsPerPixel;
                m_TempVertsPlus[tempVertsIndex].position.x += roundingOffset.x;
                m_TempVertsPlus[tempVertsIndex].position.y += roundingOffset.y;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVertsPlus);
            }
        }
        else
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVertsPlus[tempVertsIndex] = verts[i];
                m_TempVertsPlus[tempVertsIndex].position *= unitsPerPixel;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVertsPlus);
            }
        }

        m_DisableFontTextureRebuiltCallback = false;
    }
#if UNITY_EDITOR
    protected override void Reset()
    {
        this.font = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/CapstonesRes/Common/Fonts/CapstonesPlaceHolder.otf");
        this.color = Color.white;
        this.raycastTarget = false;
        this.languageLineSpacingList = new LanguageLineSpacing[] { };
    }
#endif
}
