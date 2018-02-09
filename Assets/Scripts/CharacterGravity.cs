using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGravity : MonoBehaviour {
    public CharacterController characterController;
    public CharacterPhysics characterPhysics;

    private void Update()
    {
        // Update gravity if above ground
        if (!characterController.isGrounded)
        {
            characterPhysics.velocity.y += Physics.gravity.y * Time.deltaTime;
        }

        // Stop velocity if on ground but still going down
        else
        {
            if (characterPhysics.velocity.y < 0)
            {
                characterPhysics.velocity.y = 0;
            }
        }
    }
}
