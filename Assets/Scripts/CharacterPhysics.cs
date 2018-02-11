using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPhysics : MonoBehaviour {
    public CharacterController characterController;
    public Vector3 velocity;

    public float lenientGroundedDistance = 0.1f;
    public LayerMask lenientGroundedLayerMask;

    private void Update()
    {
        // Move player based on velocity
        characterController.Move(velocity * Time.deltaTime);
    }

    public bool IsGroundedLenient
    {
        get
        {
            if (characterController.isGrounded)
            {
                return true;
            }

            Vector3 bottom = transform.position + characterController.center + (Vector3.down * characterController.height / 2);
            return Physics.Raycast(bottom, Vector3.down, lenientGroundedDistance, lenientGroundedLayerMask, QueryTriggerInteraction.Ignore);
        }
    }

    public Transform GroundLenient
    {
        get
        {
            RaycastHit hit;
            Vector3 bottom = transform.position + characterController.center + (Vector3.down * characterController.height / 2);
            Physics.Raycast(bottom, Vector3.down, out hit, lenientGroundedDistance, lenientGroundedLayerMask, QueryTriggerInteraction.Ignore);
            return hit.transform;
        }
    }
}
