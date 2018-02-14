using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbitTarget : MonoBehaviour {
    public Transform target;
    public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;
    void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, smoothTime);
    }
}
