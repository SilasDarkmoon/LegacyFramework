using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaBehaviourUpdate : LuaBehaviourBase
{
    private LuaBehaviourFunc luaUpdate;

    void CheckInitialize()
    {
        if (!isInitialized)
        {
            if (luaBehaviour != null)
            {
                luaBehaviour.lua.Get("update", out luaUpdate);
            }
            isInitialized = true;
        }
    }

    void Update()
    {
        CheckInitialize();
        if (luaBehaviour != null && luaUpdate != null)
        {
            luaUpdate(luaBehaviour.lua);
        }
    }

    protected void OnDestroy()
    {
        luaUpdate = null;
    }
}
