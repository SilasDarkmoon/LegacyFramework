using UnityEngine;
using System.Collections;

public class UnscaledTimeParticle : MonoBehaviour
{
    float lastTime;
    ParticleSystem particle;
    void Awake()
    {
        particle = GetComponent<ParticleSystem>();
    }

    void Start()
    {
        lastTime = Time.realtimeSinceStartup;
    }

    void OnEnable()
    {
        lastTime = Time.realtimeSinceStartup;
    }

    // Update is called once per frame
    void Update()
    {
        float time = Time.realtimeSinceStartup - lastTime;
        particle.Simulate(time, true, false);
        lastTime = Time.realtimeSinceStartup;
    }
}