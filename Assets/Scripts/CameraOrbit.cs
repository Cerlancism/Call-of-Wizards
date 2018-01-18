using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour {
    public Transform target;

    // For orbit cam
    [Header("Orbit Camera Settings")]
    public float speed = 360;
    public float distance = -0;
    public Vector3 targetOffset;
    public Vector3 cameraOffset;
    public float lowestAngle = -45;
    public float highestAngle = 45;
    public Vector2 initialOrbit = new Vector2();
    private Vector2 currentOrbit;

	// Use this for initialization
	void Start () {
        currentOrbit = initialOrbit;
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 finalTargetPosition = target.position + targetOffset;
        // Calculate orbit
        currentOrbit.x += Input.GetAxis("Mouse X") * Time.deltaTime * speed;
        currentOrbit.y += Input.GetAxis("Mouse Y") * Time.deltaTime * speed;
        currentOrbit.y = Mathf.Clamp(currentOrbit.y, lowestAngle, highestAngle);
        // Calculate offset caused by orbit
        Vector3 currentOffset = Quaternion.Euler(currentOrbit.y, currentOrbit.x, 0) * new Vector3(0, 0, distance);
        // Change position to orbited position
        transform.position = finalTargetPosition + currentOffset;
        // Look at target
        transform.LookAt(finalTargetPosition);
        // Positionally offset, basically look to the side etc
        transform.Translate(cameraOffset);
    }
}
