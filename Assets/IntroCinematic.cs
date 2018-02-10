using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class IntroCinematic : MonoBehaviour {
    public PlayableDirector playableDirector;
    public GameObject target;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == target)
        {
            playableDirector.Play();
        }
    }
}
