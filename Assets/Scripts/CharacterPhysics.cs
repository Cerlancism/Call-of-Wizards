using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPhysics : MonoBehaviour {
    public CharacterController characterController;
    public Vector3 velocity;

    public float lenientGroundedDistance = 0.1f;

    [Header("Sliding")]
    private Vector3 hitNormal;
    public float slideFriction = 0.3f;

    private bool isGrounded;

    private void Update()
    {
        // Move player based on velocity
        characterController.Move(velocity * Time.deltaTime);

        // Slide on slides
        /*float slope = Vector3.Angle(Vector3.up, hitNormal);
        if (slope >= characterController.slopeLimit)
        {
            Debug.Log("Slding" + Time.time);
            characterPhysics.velocity.x += (1f - hitNormal.y) * hitNormal.x * (1f - slideFriction);
            characterPhysics.velocity.z += (1f - hitNormal.y) * hitNormal.z * (1f - slideFriction);
        }*/
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
            return Physics.Raycast(bottom, Vector3.down, lenientGroundedDistance, ~0, QueryTriggerInteraction.Ignore);
        }
    }

    public Transform GroundLenient
    {
        get
        {
            RaycastHit hit;
            Vector3 bottom = transform.position + characterController.center + (Vector3.down * characterController.height / 2);
            Physics.Raycast(bottom, Vector3.down, out hit, lenientGroundedDistance, ~0, QueryTriggerInteraction.Ignore);
            return hit.transform;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
    }
}
