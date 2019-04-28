using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
/// <summary>
/// 海鸣不骑猪
///此类主要是解决缓存时候各种组件设置的缓存 防止出现组件被异常操作
/// </summary>
public class CapsGameObjectCache : MonoBehaviour
{
    private GameObject _obj = null;
    private List<int> _keys = null;
    private List<bool> _values = null;
    private void Awake()
    {
        _obj = gameObject;
        _keys = new List<int>();
        _values = new List<bool>();
    }

    public void SetEnabled(bool enabled)
    {
        if (_obj == null) return;

        if (enabled == false)
        {
            _keys.Clear();
            _values.Clear();
        }

        Behaviour[] objes = _obj.GetComponentsInChildren<Animator>(true);
        SetItemsEnabled(objes, enabled);

        objes = _obj.GetComponentsInChildren<EventSystem>(true);
        SetItemsEnabled(objes, enabled);

        objes = _obj.GetComponentsInChildren<GraphicRaycaster>(true);
        SetItemsEnabled(objes, enabled);

        objes = _obj.GetComponentsInChildren<LuaBehaviourBase>(true);
        SetItemsEnabled(objes, enabled);

        objes = _obj.GetComponentsInChildren<LuaBehaviour>(true);
        SetItemsEnabled(objes, enabled);

        objes = _obj.GetComponentsInChildren<Camera>(true);
        SetItemsEnabled(objes, enabled);

#if UNITY_EDITOR
        objes = _obj.GetComponentsInChildren<Canvas>(true);
        SetItemsEnabled(objes, enabled);
#endif

        var videoPlayerAttr = _obj.GetComponentsInChildren<VideoPlayer>(true);
        foreach (var videoPlayer in videoPlayerAttr) videoPlayer.playbackSpeed *= enabled ? 1000 : 0.001f;
    }

    private void SetItemsEnabled(Behaviour[] objes, bool enabled)
    {
        if (objes == null) return;
        int hashCode = -1, index = -1;
        bool currEnabled = false, newEnabled = false;
        foreach (var obj in objes)
        {
            hashCode = obj.GetHashCode();
            currEnabled = obj.enabled;
            if (!enabled)
            {
                _keys.Add(hashCode);
                _values.Add(currEnabled);
                if (currEnabled != enabled) obj.enabled = enabled;
                continue;
            }
            index = _keys.IndexOf(hashCode);
            newEnabled = enabled;
            if (index != -1) newEnabled = _values[index];
            if (currEnabled != newEnabled) obj.enabled = enabled;
        }
    }
}
