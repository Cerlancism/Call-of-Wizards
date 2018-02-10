using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPause : MonoBehaviour {
    public Animator animator;
    private float timeRemaining;

    private void Update()
    {
        timeRemaining -= Time.deltaTime;

        if (timeRemaining > 0)
        {
            // Pause
            animator.speed = 0;
        }
        else
        {
            // Resume
            animator.speed = 1;
        }
    }

    public void Pause(float time)
    {
        timeRemaining = time;
    }
}
