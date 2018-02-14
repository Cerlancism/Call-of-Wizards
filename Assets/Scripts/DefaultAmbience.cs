using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultAmbience : MonoBehaviour {
    private Ambience[] ambienceZones;
    private bool previousNoAmbience = false;
    public AudioSource ambience;

    private void Start()
    {
        ambienceZones = FindObjectsOfType<Ambience>() as Ambience[];
    }

    private void Update()
    {
        bool noOtherAmbienceIsPlaying = true;
        foreach (Ambience ambienceZone in ambienceZones)
        {
            if (ambienceZone.IsActive)
            {
                noOtherAmbienceIsPlaying = false;
            }
        }

        // If changed
        if (previousNoAmbience != noOtherAmbienceIsPlaying)
        {
            if (noOtherAmbienceIsPlaying)
            {
                ambience.Play();
            }
            else
            {
                ambience.Pause();
            }
        }

        previousNoAmbience = noOtherAmbienceIsPlaying;
    }
}
