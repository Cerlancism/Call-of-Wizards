﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBeamParticles : MonoBehaviour {
    private ParticleSystem ps;
    public IceChunk iceChunkPrefab;

    public float particlesPerIceChunk = 20;
    private float particlesNextIceChunk = 0;

    public float iceHeight = 4.1f;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnParticleTrigger()
    {
        List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
        int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);

        // iterate
        for (int i = 0; i < numEnter; i++)
        {
            ParticleSystem.Particle p = enter[i];
            particlesNextIceChunk++;
            if (particlesNextIceChunk >= particlesPerIceChunk)
            {
                Instantiate(iceChunkPrefab, new Vector3(p.position.x, iceHeight + Random.Range(0, 0.1f), p.position.z), Quaternion.Euler(0, Random.Range(0f, 360f), 0), null);
                particlesNextIceChunk = 0;
            }
        }
    }
}
