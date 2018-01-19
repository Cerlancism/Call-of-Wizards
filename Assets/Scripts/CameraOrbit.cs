using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour {
    public Transform target;

    // For orbit cam
    [Header("Orbit Camera Settings")]
    public float speed = 360;
    public float distance = 10;
    public float minDistance;
    public Vector3 targetOffset;
    public Vector3 cameraOffset;
    public float lowestAngle = -45;
    public float highestAngle = 45;
    public Vector2 initialOrbit = new Vector2();
    public float wallBufferSize = 0.5f;
    private Vector2 currentOrbit;
    private LayerMask wallLayerMask;

	// Use this for initialization
	void Start () {
        currentOrbit = initialOrbit;
        wallLayerMask = LayerMask.GetMask("Camera Wall");
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 finalTargetPosition = target.position + targetOffset;
        // Calculate orbit
        currentOrbit.x += Input.GetAxis("Mouse X") * speed;
        currentOrbit.y += Input.GetAxis("Mouse Y") * speed;
        currentOrbit.y = Mathf.Clamp(currentOrbit.y, lowestAngle, highestAngle);
        // Calculate offset direction caused by orbit
        Vector3 currentOffsetDirection = Quaternion.Euler(currentOrbit.y, currentOrbit.x, 0) * new Vector3(0, 0, 1);
        // Shortern orbit distance if required to avoid going through walls
        float currentDistance = distance;
        RaycastHit hit;
        if (Physics.Raycast(finalTargetPosition, currentOffsetDirection, out hit, distance + wallBufferSize, wallLayerMask))
            // Hit wall, shortern distance
            currentDistance = hit.distance - wallBufferSize;
        currentDistance = Mathf.Max(currentDistance, minDistance);
        Vector3 currentOffset = currentOffsetDirection * currentDistance;
        // Change position to orbited position
        transform.position = finalTargetPosition + currentOffset;
        // Look at target
        transform.LookAt(finalTargetPosition);
        // Positionally offset, basically look to the side etc
        transform.Translate(cameraOffset);
    }
}
