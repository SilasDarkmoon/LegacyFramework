using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XLuaAnimationEvent : MonoBehaviour
{
    public LuaBehaviour luaBehaviour;
    private AnimationEventDelegate luaHandler;

    void Start()
    {
        if (luaBehaviour != null)
        {
            luaBehaviour.lua.Get("animationEventCallback", out luaHandler);
        }
    }

    public void AnimationEventCallback(string strParam)
    {
        if (luaBehaviour != null && luaHandler != null)
        {
            luaHandler(luaBehaviour.lua, strParam);
        }
    }

    void OnDestroy()
    {
        luaHandler = null;
    }
}
