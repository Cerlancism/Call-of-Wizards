using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ambience : MonoBehaviour {
    public AudioSource ambience;
    public AudioListener targetListener;

    private void OnTriggerEnter(Collider other)
    {
        AudioListener audioListener = other.GetComponent<AudioListener>();
        if (audioListener == targetListener)
        {
            ambience.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        AudioListener audioListener = other.GetComponent<AudioListener>();
        if (audioListener == targetListener)
        {
            ambience.Stop();
        }
    }
}
