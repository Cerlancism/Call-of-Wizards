using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowCamera : MonoBehaviour {
    public ParticleSystem snow;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<NoSnow>() != null)
        {
            snow.Stop();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<NoSnow>() != null)
        {
            snow.Play();
        }
    }
}
