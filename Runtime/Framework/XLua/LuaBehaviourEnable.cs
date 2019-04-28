using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaBehaviourEnable : LuaBehaviourBase
{
    private LuaBehaviourFunc luaOnEnable;
    private LuaBehaviourFunc luaOnDisable;

    void CheckInitialize()
    {
        if (!isInitialized)
        {
            if (luaBehaviour != null)
            {
                luaBehaviour.lua.Get("onEnable", out luaOnEnable);
                luaBehaviour.lua.Get("onDisable", out luaOnDisable);
            }
            isInitialized = true;
        }
    }

    void OnEnable()
    {
        CheckInitialize();
        if (luaBehaviour != null && luaOnEnable != null)
        {
            luaOnEnable(luaBehaviour.lua);
        }
    }

    void OnDisable()
    {
        CheckInitialize();
        if (luaBehaviour != null && luaOnDisable != null)
        {
            luaOnDisable(luaBehaviour.lua);
        }
    }

    protected void OnDestroy()
    {
        luaOnEnable = null;
        luaOnDisable = null;
    }
}
