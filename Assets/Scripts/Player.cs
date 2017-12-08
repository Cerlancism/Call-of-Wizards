using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public float MovementSpeed = 1;
    public float turnspeed = 10;
	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKey(KeyCode.W))
        {
            GetComponent<CharacterController>().Move(new Vector3(0, 0, MovementSpeed));
        }
        if (Input.GetKey(KeyCode.S))
        {
            GetComponent<CharacterController>().Move(new Vector3(0, 0, -MovementSpeed));
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0, -MovementSpeed * turnspeed, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0, MovementSpeed * turnspeed, 0);
        }
    }
}
