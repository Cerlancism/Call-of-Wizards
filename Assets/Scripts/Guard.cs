using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour, IHurtable {
    [Header("Movement")]
    public float playerDetectionRadius = 10;
    public float speed = 1;
    public float turningSpeed = 0.1f;
    public CharacterController characterController;
    public Animator animator;
    public Ragdoll ragdoll;
    private bool alive = true;
    private Vector3 velocity;
    private GameObject player;

    [Header("Attack")]
    public float playerAttackRadius = 1;
    public float meleeCooldown = 0.2f;
    public float meleeStopDuration = 0.7f;
    public Transform meleeAttackArea;
    public float meleeDamage = 10;
    public float meleeDamageDelay = 0.15f;
    private float meleeCooldownTime;
    private float meleeStopDurationTime;
    private float meleeDamageDelayTime;
    private bool meleeDamageDelayActivated;

    [Header("Health")]
    public float health = 40;

    [Header("Mana")]
    public Transform manaParticleSpawn;
    public ManaParticle manaParticle;
    public int manaParticlesAmount;
    public float manaParticleExplosionForce;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        if (alive)
        {
            Vector3 displacement = player.transform.position - transform.position;
            float distance = displacement.magnitude;

            if (meleeDamageDelayActivated)
            {
                meleeDamageDelayTime -= Time.deltaTime;
                if (meleeDamageDelayTime <= 0)
                {
                    // Hurt hurtables
                    Collider[] collidersInAttackArea = Physics.OverlapBox(meleeAttackArea.position, meleeAttackArea.localScale / 2, meleeAttackArea.rotation);
                    foreach (Collider colliderInAttackArea in collidersInAttackArea)
                    {
                        IHurtable hurtable = colliderInAttackArea.GetComponent<IHurtable>();

                        // If it's hurtable
                        // Also, don't hurt ourselves
                        if (hurtable != null && colliderInAttackArea.gameObject != gameObject)
                        {
                            hurtable.Hurt(meleeDamage);
                        }
                    }

                    meleeDamageDelayActivated = false;
                }
            }

            meleeCooldownTime -= Time.deltaTime;
            if (distance <= playerAttackRadius && meleeCooldownTime <= 0)
            {
                animator.SetTrigger("Melee");

                meleeStopDurationTime = meleeStopDuration;
                meleeCooldownTime = meleeCooldown;

                // Damage after some time
                meleeDamageDelayActivated = true;
                meleeDamageDelayTime = meleeDamageDelay;
            }

            meleeStopDurationTime -= Time.deltaTime;
            if (distance <= playerDetectionRadius && meleeStopDurationTime <= 0)
            {
                // Walk
                animator.SetBool("Walking", true);

                Vector2 direction = new Vector2(displacement.x, displacement.z).normalized;
                velocity.x = direction.x * speed;
                velocity.z = direction.y * speed;

                characterController.Move(velocity * Time.deltaTime);

                // Turn guard towards movement direction
                Vector3 direction3d = new Vector3(direction.x, 0, direction.y);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction3d, Vector3.up), turningSpeed * Time.deltaTime);
            }
            else
            {
                animator.SetBool("Walking", false);
            }
        }
    }

    public void Hurt(float amount, bool createsMana)
    {
        animator.SetTrigger("Impact");
        health -= amount;

        if (health <= 0)
        {
            Die(createsMana);
        }
    }

    private void Die(bool createsMana)
    {
        alive = false;

        if (createsMana)
        {
            for (int i = 0; i < manaParticlesAmount; i++)
            {
                ManaParticle newParticle = Instantiate(manaParticle, manaParticleSpawn.position, Quaternion.identity);
                newParticle.Explode(manaParticleExplosionForce);
            }
        }

        ragdoll.SetEnabled(true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerAttackRadius);
    }
}
