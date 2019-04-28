using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class DissolveAnim : MonoBehaviour
{

    public float animTime;
    private float allAnimTime;
    public Material material;
    private Material _material;
    private Image _img;

    void CreateMaterials()
    {
        _material = new Material(material);
        _material.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.NotEditable;
        _img.material = _material;
    }

    void Awake()
    {
        _img = this.GetComponent<Image>();
    }

    void Start()
    {
        if (!material)
        {
            enabled = false;
            return;
        }
        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }
        CreateMaterials();
        if (!_material.shader || !_material.shader.isSupported)
        {
            enabled = false;
            return;
        }
        allAnimTime = animTime;
        _material.SetFloat("_Amount", 1.0f);
    }

    void Update()
    {
        if (animTime > 0)
        {
            animTime -= Time.deltaTime;
            if (!_material)
            {
                CreateMaterials();
            }
            _material.SetFloat("_Amount", animTime / allAnimTime);
        }
    }

    void OnDisable()
    {
        if (_material)
        {
            DestroyImmediate(_material);
        }
    }
}
