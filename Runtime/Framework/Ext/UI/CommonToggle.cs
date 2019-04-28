using UnityEngine;
using UnityEngine.UI;
using XLua;
using System;
using System.Collections;
using System.Collections.Generic;

public enum ButtonGroupType
{
    Static,
    Dynamic
}

[LuaCallCSharp]
public class CommonToggle : ToggleGroup
{
    public ButtonGroupType ButtonGroupType;
    public GameObject ToggleObj;
    public string TogglePrefab;
    public int TogglesCount;
    public List<GameObject> Toggles;
    public bool AllowReselect;
    private bool isTriggerLuaListener = true;
    private System.Action<LuaTable, LuaTable, int> onToggleCreated = null;
    private System.Action<LuaTable, LuaTable, int> onToggleSelected = null;
    private System.Action<LuaTable, LuaTable, int> onToggleDeselected = null;
    private System.Action<LuaTable, int> onTagSwitched = null;
    private int m_cache_tag;

    protected override void Awake()
    {
        base.Awake();

        LuaTable lua = gameObject.GetComponent<LuaBehaviour>().lua;
        lua.Get("onToggleSelected", out onToggleSelected);
        lua.Get("onToggleDeselected", out onToggleDeselected);
        lua.Get("onToggleCreated", out onToggleCreated);
        lua.Get("onTagSwitched", out onTagSwitched);
        if (ButtonGroupType == ButtonGroupType.Dynamic)
        {
            Toggles = new List<GameObject>();

            GameObject prefabObj = null;
            if (!string.IsNullOrEmpty(TogglePrefab))
            {
                prefabObj = Capstones.UnityFramework.ResManager.LoadRes(TogglePrefab) as GameObject;
            }

            for (int i = 0; i < TogglesCount; i++)
            {
                var obj = GameObject.Instantiate(prefabObj != null ? prefabObj : ToggleObj, transform);

                if (obj)
                {
                    if (!obj.activeSelf)
                    {
                        if (GLog.IsLogWarningEnabled) GLog.LogWarning("CommonToggle.Awake: obj is not active !!!");
                        obj.FastSetActive(true);
                    }

                    Toggles.Add(obj);
                }
            }

        }
        for (int i = 0; i < Toggles.Count; i++)
        {
            int tag = i + 1;
            GameObject obj = Toggles[i];
            Debug.Assert(obj.GetComponent<UnityEngine.UI.Toggle>(), "Toggle object must has a toggle compoment");
            LuaTable btnLua = obj.GetComponent<LuaBehaviour>().lua;
            if (btnLua == null)
            {
                if (GLog.IsLogWarningEnabled) GLog.LogWarning("CommonToggle.Awake: btnLua is null !!!");
            }
            if (onToggleCreated != null)
            {
                onToggleCreated(lua, btnLua, tag);
            }
            Toggle t = obj.GetComponent<Toggle>();
            t.group = this;
            t.onValueChanged = new Toggle.ToggleEvent();
            t.onValueChanged.AddListener((bool active) =>
            {
                if (isTriggerLuaListener)
                {
                    LuaTable btnLua2 = Toggles[tag - 1].GetComponent<LuaBehaviour>().lua;
                    if (active)
                    {
                        if (AllowReselect)
                            m_cache_tag = tag;
                        else if (tag == m_cache_tag)
                            return;
                        else
                            m_cache_tag = tag;

                        if (onToggleSelected != null)
                        {
                            onToggleSelected(lua, btnLua2, tag);
                        }
                        if (onTagSwitched != null)
                        {
                            onTagSwitched(lua, tag);
                        }
                    }
                    else
                    {
                        if (onToggleDeselected != null)
                        {
                            onToggleDeselected(lua, btnLua2, tag);
                        }
                    }
                }
            });
        }
    }

    public void SelectDefaultToggle(int tag)
    {
        m_cache_tag = tag;
        isTriggerLuaListener = false;
        LuaTable lua = gameObject.GetComponent<LuaBehaviour>().lua;
        for (int i = 0; i < Toggles.Count; i++)
        {
            int toggleTag = i + 1;
            GameObject obj = Toggles[i];
            LuaTable btnLua = obj.GetComponent<LuaBehaviour>().lua;
            if (btnLua == null)
            {
                if (GLog.IsLogWarningEnabled) GLog.LogWarning("CommonToggle.SelectDefaultToggle: btnLua is null !!!");
            }
            if (toggleTag == tag)
            {
                obj.GetComponent<Toggle>().isOn = true;
                if (onToggleSelected != null)
                {
                    onToggleSelected(lua, btnLua, tag);
                }
                if (onTagSwitched != null)
                {
                    onTagSwitched(lua, tag);
                }
            }
            else
            {
                obj.GetComponent<Toggle>().isOn = false;
                if (onToggleDeselected != null)
                {
                    onToggleDeselected(lua, btnLua, tag);
                }
            }
        }
        isTriggerLuaListener = true;
    }

    protected override void OnDestroy()
    {
        onToggleCreated = null;
        onToggleSelected = null;
        onToggleDeselected = null;
        onTagSwitched = null;
        base.OnDestroy();
    }
}
