using Capstones.UnityFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Shear : MonoBehaviour
{
    [SerializeField]
    private float _K;
    [SerializeField]
    private Transform _Level0;
    [SerializeField]
    private Transform _Level1;

    public float K
    {
        get { return _K; }
        set
        {
            if (_K != value)
            {
                _K = value;
                ApplyShear();
#if UNITY_EDITOR
                _K_Old = value;
#endif
            }
        }
    }
    public Transform Level0
    {
        get { return _Level0; }
        set
        {
            if (_Level0 != value)
            {
                _Level0 = value;
                ApplyShear();
#if UNITY_EDITOR
                _Level0_Old = value;
#endif
            }
        }
    }
    public Transform Level1
    {
        get { return _Level1; }
        set
        {
            if (_Level1 != value)
            {
                _Level1 = value;
                ApplyShear();
#if UNITY_EDITOR
                _Level1_Old = value;
#endif
            }
        }
    }

    public void ApplyShear()
    {
        if (_Level0 && _Level1)
        {
            float tanm = (_K + Mathf.Sqrt(_K * _K + 4)) / 2;
            float ctgm = 1 / tanm;
            float m = Mathf.Atan(tanm);
            float g = Mathf.PI / 2 - m;
            _Level0.localScale = new Vector3(ctgm, tanm, 1);
            _Level1.localRotation = Quaternion.Euler(0, 0, g * Mathf.Rad2Deg);
            _Level0.localRotation = Quaternion.Euler(0, 0, -m * Mathf.Rad2Deg);
        }
    }

    private float _K_Old;
    private Transform _Level0_Old;
    private Transform _Level1_Old;
#if UNITY_EDITOR
    private void Update()
    {
        if (_K_Old != _K || _Level0 != _Level0_Old || _Level1 != _Level1_Old)
        {
            ApplyShear();
            _K_Old = _K;
            _Level0_Old = _Level0;
            _Level1_Old = _Level1;
        }
    }
#endif
}
