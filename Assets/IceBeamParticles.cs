using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBeamParticles : MonoBehaviour {
    private ParticleSystem ps;
    public IceChunk iceChunkPrefab;

    public float iceLimitInterval = 0.3f;
    private float iceLimitTime;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        iceLimitTime -= Time.deltaTime;
    }

    private void OnParticleTrigger()
    {
        List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
        int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);

        // iterate
        for (int i = 0; i < numEnter; i++)
        {
            ParticleSystem.Particle p = enter[i];
            if (iceLimitTime < 0)
            {
                Instantiate(iceChunkPrefab, p.position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), null);
                iceLimitTime = iceLimitInterval;
            }
        }
    }
}
