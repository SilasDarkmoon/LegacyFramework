using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaBehaviourLateUpdate : LuaBehaviourBase
{
    private LuaBehaviourFunc luaLateUpdate;

    void CheckInitialize()
    {
        if (!isInitialized)
        {
            if (luaBehaviour != null)
            {
                luaBehaviour.lua.Get("lateUpdate", out luaLateUpdate);
            }
            isInitialized = true;
        }
    }

    void LateUpdate()
    {
        CheckInitialize();
        if (luaBehaviour != null && luaLateUpdate != null)
        {
            luaLateUpdate(luaBehaviour.lua);
        }
    }

    protected void OnDestroy()
    {
        luaLateUpdate = null;
    }
}
