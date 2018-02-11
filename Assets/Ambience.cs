using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ambience : MonoBehaviour {
    public AudioSource ambience;
    public AudioListener targetListener;
    private bool isActive = false;

    private void OnTriggerEnter(Collider other)
    {
        AudioListener audioListener = other.GetComponent<AudioListener>();
        if (audioListener == targetListener)
        {
            ambience.Play();
            isActive = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        AudioListener audioListener = other.GetComponent<AudioListener>();
        if (audioListener == targetListener)
        {
            ambience.Pause();
            isActive = false;
        }
    }

    public bool IsActive
    {
        get
        {
            return isActive;
        }
    }
}
