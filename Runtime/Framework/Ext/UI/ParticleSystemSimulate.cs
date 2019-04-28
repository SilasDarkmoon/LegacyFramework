using UnityEngine;
using System.Collections;

public class ParticleSystemSimulate : MonoBehaviour {

    private ParticleSystem[] particleSystems;

    void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    void Update()
    {
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem childParticleSystem = particleSystems[i];
            if (childParticleSystem)
            {
                childParticleSystem.Simulate(Time.unscaledDeltaTime, false, false);
            }
        }
    }
}
