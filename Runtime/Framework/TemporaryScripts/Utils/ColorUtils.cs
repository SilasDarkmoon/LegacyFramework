using System;
using System.Collections.Generic;
using UnityEngine;
[XLua.LuaCallCSharp]
public static class ColorUtils
{
    public static Color ParseColorString(string colorString)
    {
        if (string.IsNullOrEmpty(colorString))
        {
            if (GLog.IsLogWarningEnabled) GLog.LogWarning("colorString is null or empty");
            return Color.black;
        }

        string[] rgba = colorString.Split(',');

        if (rgba.Length == 3)
        {
            return new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]));
        }
        else if (rgba.Length == 4)
        {
            return new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), float.Parse(rgba[3]));
        }

        throw new Exception("Illegal color string length!");
    }

    public static float CalcColorDiffValue(Color color1, Color color2)
    {
        var diff = Mathf.Abs(GetHsvFromRgb(color1)["hue"] - GetHsvFromRgb(color2)["hue"]);
        return diff > 180 ? 360 - diff : diff;
    }

    public static IDictionary<string, float> GetHsvFromRgb(Color color)
    {
        IDictionary<string, float> hsv = new Dictionary<string, float>();

        var maxV = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
        var minV = Mathf.Min(Mathf.Min(color.r, color.g), color.b);

        hsv["saturation"] = Mathf.Abs(maxV) < Constants.TOLERANCE ? 0 : 1 - minV / maxV;
        hsv["value"] = maxV;

        if (Mathf.Abs(maxV - minV) < Constants.TOLERANCE)
        {
            hsv["hue"] = 0;
        }
        else if (Mathf.Abs(maxV - color.r) < Constants.TOLERANCE && color.g >= color.b)
        {
            hsv["hue"] = 60 * (color.g - color.b) / (maxV - minV);
        }
        else if (Mathf.Abs(maxV - color.r) < Constants.TOLERANCE && color.g < color.b)
        {
            hsv["hue"] = 60 * (color.g - color.b) / (maxV - minV) + 360;
        }
        else if (Mathf.Abs(maxV - color.g) < Constants.TOLERANCE)
        {
            hsv["hue"] = 60 * (color.b - color.r) / (maxV - minV) + 120;
        }
        else
        {
            hsv["hue"] = 60 * (color.r - color.g) / (maxV - minV) + 240;
        }

        return hsv;
    }

    public static bool IsCloseColor(Color color1, Color color2)
    {
        return 1 - Mathf.Abs(color1.r - color2.r) * 0.297 - Mathf.Abs(color1.g - color2.g) * 0.593 - Mathf.Abs(color1.b - color2.b) * 0.11 > 0.804;
    }

    public static bool IsCloseColorNew(Color color1, Color color2)
    {
        float h1, s1, v1;
        Color.RGBToHSV(color1, out h1, out s1, out v1);

        float h2, s2, v2;
        Color.RGBToHSV(color2, out h2, out s2, out v2);

        if (Mathf.Abs(h1 - h2) >= 0.08f && Mathf.Abs(h1 - h2) <= 0.95f)
        {
            return false;
        }

        if (Mathf.Abs(s1 - s2) >= 0.75f || Mathf.Abs(v1 - v2) >= 0.75f)
        {
            return false;
        }

        if (Mathf.Abs(s1 - s2) >= 0.5f && Mathf.Abs(v1 - v2) >= 0.5f)
        {
            return false;
        }

        return true;
    }
}
