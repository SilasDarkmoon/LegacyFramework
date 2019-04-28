using UnityEngine;
using System.Collections;

/// <summary>
/// Author:LHW+ 
/// Time:2016/10/31
/// Des:特效延迟播放脚本,为了特效方便使用
/// </summary>
public class EffectDelay : MonoBehaviour {

    public GameObject delayChild = null;
    public float delayTime = 0.5f;
    bool mBeginTimer = false;
    float mAccTimer = 0;

    public bool EditorMode=true;

    public void Play()
    {
        //mBeginTimer = true;
        this.enabled = true;
    }

    public void Reset()
    {
        this.enabled = false;
        //ClearData();
    }

    void Awake()
    {
        if (delayChild != null)
        {
            delayChild.FastSetActive(false);
        }
    }

    // Use this for initialization
    void Start ()
    {
        mBeginTimer = true;
       // if (EditorMode)
       // mBeginTimer = true;
    }

    // Update is called once per frame
    void Update ()
    {

        if (mBeginTimer)
        {
            mAccTimer += Time.deltaTime;//TODO:暂停

            if (mAccTimer >= delayTime)
            {
                mBeginTimer = false;

                if (delayChild != null)
                {
                     delayChild.FastSetActive(true);
                }

            }
        }
    }

    void OnEnable()
    {
        mBeginTimer = true;
    }

    void OnDisable()
    {
        ClearData();
    }

    void ClearData()
    {
        mBeginTimer = false;
        mAccTimer = 0;
        if (delayChild != null)
        {
            delayChild.FastSetActive(false);
        }
    }
}
