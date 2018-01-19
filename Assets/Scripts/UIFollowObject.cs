using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFollowObject : MonoBehaviour {
    public Transform target;
    public Vector2 offset;
    public Camera targetCamera;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 screenPosition = targetCamera.WorldToScreenPoint(target.position);
        transform.position = screenPosition + offset;
	}
}
