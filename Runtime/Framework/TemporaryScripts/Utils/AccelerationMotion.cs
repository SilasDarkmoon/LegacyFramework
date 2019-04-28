using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationMotion : MonoBehaviour
{
    [Range(0, 1)]
    public float alpha = 0.8f;

    private float maxDeltaX;
    private float maxDeltaY;
    private Vector3 acceleration;
    void Start()
    {
        var rect = GetComponent<RectTransform>();
        maxDeltaX = Mathf.Floor(rect.rect.width * (transform.localScale.x - 1) * 0.5f);
        maxDeltaY = Mathf.Floor(rect.rect.height * (transform.localScale.y - 1) * 0.5f);
        acceleration = Input.acceleration.normalized;
    }
    void Update() {
        acceleration = alpha * acceleration + (1 - alpha) * Input.acceleration.normalized;
        transform.localPosition = new Vector3(-acceleration.x * maxDeltaX, -acceleration.y * maxDeltaY, 0);
    }
}
