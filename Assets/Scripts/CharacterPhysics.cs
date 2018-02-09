using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPhysics : MonoBehaviour {
    public CharacterController characterController;
    public Vector3 velocity;

    public float stickToGroundDistance = 0.1f;

    private void Update()
    {
        // Move player based on velocity
        characterController.Move(velocity * Time.deltaTime);
    }

    private void LateUpdate()
    {
        // Stick to ground
        RaycastHit hit;
        Vector3 bottom = transform.position - Vector3.down * characterController.height / 2;

        if (Physics.Raycast(bottom, Vector3.down, out hit, stickToGroundDistance))
        {
            characterController.Move(Vector3.down * hit.distance);
        }
    }
}
