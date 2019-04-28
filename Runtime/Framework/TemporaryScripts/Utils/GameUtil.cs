using UnityEngine;
using XLua;
/*
 * FileName:    GameUtil
 * Author:      海鸣不骑猪
 * Description: 游戏通用 util 接口
*/
[LuaCallCSharp]
public static class GameUtil
{
    #region 清理显示对象下面所有物体
    public static void ClearChildren(Object obj, float t = 0)
    {
        ClearChildrenBase(obj, t);
    }

    public static void ClearChildrenImmediate(Object obj)
    {
        ClearChildrenBase(obj, 0f, true);
    }

    private static void ClearChildrenBase(Object obj, float t = 0, bool isImmediate = false)
    {
        if (obj == null) return;
        Transform tra = null;
        if (obj is Transform)
        {
            tra = obj as Transform;
        }
        else if (obj is GameObject)
        {
            tra = (obj as GameObject).transform;
        }
        else if (obj is Component)
        {
            tra = (obj as Component).transform;
        }
        if (tra == null) return;
        var count = tra.childCount;
        for (var i = 1; i <= count; i++)
        {
            if (isImmediate)
            {
                DestroyImmediate(tra.GetChild(i - 1));
                continue;
            }
            Destroy(tra.GetChild(i - 1), t);
        }
    }
    #endregion
    #region 删除Object物体
    public static void Destroy(Object obj, float t = 0)
    {
        DestroyBase(obj, t, false);
    }

    public static void DestroyImmediate(Object obj)
    {
        DestroyBase(obj, 0f, true);
    }

    private static void DestroyBase(Object obj, float t = 0f, bool isImmediate = false)
    {
        if (obj == null) return;
        if (obj is GameObject)
        {
            if (isImmediate)
            {
                GameObject.DestroyImmediate(obj as GameObject);
                return;
            }
            GameObject.Destroy(obj as GameObject, t);
            return;
        }
        if (obj is Transform)
        {
            if (isImmediate)
            {
                GameObject.DestroyImmediate((obj as Transform).gameObject);
                return;
            }
            GameObject.Destroy((obj as Transform).gameObject, t);
            return;
        }
        if (isImmediate)
        {
            GameObject.DestroyImmediate(obj);
            return;
        }
        GameObject.Destroy(obj, t);
    }
    #endregion

}
