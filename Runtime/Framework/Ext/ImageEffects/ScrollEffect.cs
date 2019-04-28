using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Image))]
public class ScrollEffect : MonoBehaviour {

    [Range(0, 1)]
    public float xScrollSpeed;
    public Shader shader;
    private Material material;
    private Image img;
    private float moveTime;
    private float loopTime;

    void FindShaders()
    {
        if (!shader)
        {
            shader = Shader.Find("Custom/UI/Scroll Packed Sprites");
        }
    }

    void CreateMaterials()
    {
        if (!material)
        {
            material = new Material(shader);
            material.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.NotEditable;
            img.material = material;
        }
    }

    void Start()
    {
        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }
        FindShaders();
        if (!shader || !shader.isSupported)
        {
            enabled = false;
            return;
        }
        img = this.GetComponent<Image>();
        CreateMaterials();
        loopTime = 1 / xScrollSpeed;
    }

    void Update()
    {
        if (!material)
        {
            CreateMaterials();
        }
        moveTime += Time.unscaledDeltaTime;
        if (moveTime >= loopTime)
        {
            moveTime = 0;
        }
        material.SetFloat("_XScrollValue", moveTime * xScrollSpeed);
    }

    void OnDisable()
    {
        if (material)
        {
            DestroyImmediate(material);
        }
    }
}
