using UnityEngine;
using XLua;

[LuaCallCSharp]
public static class UnityGameObjectExtension
{
    public static void FastSetActive(this GameObject obj, bool isActive)
    {
        if (obj != null && obj.activeSelf != isActive)
        {
            obj.SetActive(isActive);
        }
    }
}