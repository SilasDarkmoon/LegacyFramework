using UnityEngine;
using System.Collections;

public class UnscaledTimeParticleLegancy : MonoBehaviour
{
	float lastTime;
#if !UNITY_2018_1_OR_NEWER
    ParticleEmitter particle;
	void Awake()
	{
        particle = GetComponent<ParticleEmitter>();
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
        particle.Simulate(time);
		lastTime = Time.realtimeSinceStartup;
	}
#else
    Object particle;
#endif
}