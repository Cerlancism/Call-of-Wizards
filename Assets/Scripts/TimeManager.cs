using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour {
    public float timeScale = 1;
    public float fixedDeltaTime = 0.02f;

    private void Update()
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = timeScale * fixedDeltaTime;
    }
}
