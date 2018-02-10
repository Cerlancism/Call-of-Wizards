using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour {
    public float exponential = 2;
    public float seed = 20102025;
    public float magnitude = 10;
    public float frequency = 1;
    public float decreaseRate = 1;
    private float shake;

    private void LateUpdate()
    {
        float shakeCurrentMagnitude = Mathf.Pow(shake, exponential) * magnitude;

        float pitch = shakeCurrentMagnitude * (Mathf.PerlinNoise(seed, Time.time * frequency) - 0.5f);
        float yaw = shakeCurrentMagnitude * (Mathf.PerlinNoise(seed + 1, Time.time * frequency) - 0.5f);
        float roll = shakeCurrentMagnitude * (Mathf.PerlinNoise(seed + 2, Time.time * frequency) - 0.5f);

        transform.Rotate(pitch, yaw, roll);

        shake -= decreaseRate * Time.deltaTime;
        shake = Mathf.Clamp01(shake);
    }

    public void Shake(float shakeAmount)
    {
        shake = shakeAmount;
    }
}
