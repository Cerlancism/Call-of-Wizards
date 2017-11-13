using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour {
    public Transform target;

    // For orbit cam
    [Header("Orbit Camera Settings")]
    public float orbitSpeed = 360;
    public float orbitDistance = 10;
    public Vector3 orbitTargetOffset;
    public Vector3 orbitCameraOffset;
    public float orbitYLowestAngle = -45;
    public float orbitYHighestAngle = 45;
    private Vector2 orbitCurrentOrbit = new Vector2();

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        Vector3 finalTargetPosition = target.position + orbitTargetOffset;
        // Calculate orbit
        orbitCurrentOrbit.x += Input.GetAxis("Mouse X") * Time.deltaTime * orbitSpeed;
        orbitCurrentOrbit.y += -Input.GetAxis("Mouse Y") * Time.deltaTime * orbitSpeed;
        orbitCurrentOrbit.y = Mathf.Clamp(orbitCurrentOrbit.y, orbitYLowestAngle, orbitYHighestAngle);
        // Calculate offset caused by orbit
        Vector3 currentOffset = Quaternion.Euler(orbitCurrentOrbit.y, orbitCurrentOrbit.x, 0) * new Vector3(0, 0, orbitDistance);
        // Change position to orbited position
        transform.position = finalTargetPosition + currentOffset;
        // Look at target
        transform.LookAt(finalTargetPosition);
        // Positionally offset, basically look to the side etc
        transform.Translate(orbitCameraOffset);
    }
}
