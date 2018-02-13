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
                // Fall damage
                if (hurtable != null && enableFallDamage)
                {
                    if (characterPhysics.velocity.y < minimumDamageSpeed)
                    {
                        float speed = -characterPhysics.velocity.y;
                        hurtable.Hurt(speed * damagePerSpeed);
                    }
                }

                characterPhysics.velocity.y = 0;
            }
        }
    }
}
