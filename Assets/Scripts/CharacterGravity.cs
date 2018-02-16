using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGravity : MonoBehaviour {
    public CharacterController characterController;
    public CharacterPhysics characterPhysics;

    [Header("Fall Damage")]
    public float minimumDamageSpeed = -8;
    public float damagePerSpeed = 2;
    private IHurtable hurtable;
    public bool enableFallDamage = false;

    private void Start()
    {
        hurtable = GetComponent<IHurtable>();
    }

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
                if (hurtable != null && enableFallDamage)
                {
                    // Fall damage
                    float fallenHeight = (characterPhysics.velocity.y * characterPhysics.velocity.y) / (2 * -Physics.gravity.y);
                    float fallingDamage = (fallenHeight / 2 - 1.5f) * 5; // Based on Minecraft fall damage
                    Debug.Log("Fallen height: " + fallenHeight + " Damage: " + fallingDamage);

                    if (fallingDamage > 0)
                    {
                        hurtable.Hurt(fallingDamage);
                    }
                }

                characterPhysics.velocity.y = 0;
            }
        }
    }
}
