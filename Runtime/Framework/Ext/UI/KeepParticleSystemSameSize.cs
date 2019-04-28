using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class KeepParticleSystemSameSize : MonoBehaviour {

    void Start () {
        CanvasScaler scaler = GetComponentInParent<CanvasScaler>();
        Vector2 referenceResolution = scaler.referenceResolution;
        float scaleFactor = (referenceResolution.y * Screen.width) / (referenceResolution.x * Screen.height);
        if (scaleFactor < 1)
        {
            ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0, len = systems.Length; i < len; i++)
            {
                ParticleSystem system = systems[i];
                system.startSize *= scaleFactor;
            }
        }
    }
}
