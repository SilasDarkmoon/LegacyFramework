using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaBehaviourBase : MonoBehaviour
{
    public LuaBehaviour luaBehaviour;
    protected bool isInitialized = false;

#if UNITY_EDITOR
    /// <summary>
    /// 默认选择自己或与自己最近父节点的LuaBehavior
    /// </summary>
    protected void Reset()
    {
        Transform trans = this.transform;
        while (trans != null)
        {
            var luaBehaviComp = trans.GetComponent<LuaBehaviour>();
            if (luaBehaviComp)
            {
                this.luaBehaviour = luaBehaviComp;
                break;
            }
            else
            {
                trans = trans.parent;
            }
        }
    }
#endif
}
