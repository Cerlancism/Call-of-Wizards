using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Guard : MonoBehaviour, IHurtable, IFlammable
{
    [Header("Movement")]
    public float speed = 1;
    public float runningSpeed = 5;
    public float turningSpeed = 0.1f;
    public CharacterController characterController;
    public Animator animator;
    public Ragdoll ragdoll;
    public Health health;
    private bool alive = true;
    private Vector3 velocity;
    private GameObject player;

    [Header("Senses (Vision)")]
    public float activeViewAngle = 90;
    public float peripheralViewAngle = 180;
    public Transform eye;
    public Transform playerFocus;
    public LayerMask opaqueLayers;

    [Header("Thought")]
    public ThoughtState initialThoughtState = ThoughtState.Patrol;
    private ThoughtState thoughtState;
    private ThoughtState nextThoughtState;
    public enum ThoughtState { Patrol, Combat };

    [Header("Patrol settings")]
    public PatrolStep[] patrolSteps;
    private int currentStep;

    [Header("Pathfinding")]
    public float pathRecalculationInterval = 1;
    private float pathRecalculationTime;
    private NavMeshPath path;
    public float cornerReachRadius = 0.2f;
    private int currentCorner = 0;

    [Header("Combat")]
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
    public LayerMask meleeLayerMask;

    [Header("Impact")]
    public float impactDuration = 1;
    private float impactDurationTime;

    [Header("Mana")]
    public Transform manaParticleSpawn;
    public ManaParticle manaParticle;
    public int manaParticlesAmount;
    public float manaParticleExplosionForce;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        thoughtState = initialThoughtState;
        nextThoughtState = initialThoughtState;
        OnThoughtStageEnable();
    }

    private void Update()
    {
        meleeCooldownTime -= Time.deltaTime;
        meleeStopDurationTime -= Time.deltaTime;
        impactDurationTime -= Time.deltaTime;

        if (alive)
        {
            Vector2 direction = Vector2.zero;
            bool running = false;

            if (meleeDamageDelayActivated)
            {
                meleeDamageDelayTime -= Time.deltaTime;
                if (meleeDamageDelayTime <= 0)
                {
                    // Hurt hurtables
                    Collider[] collidersInAttackArea = Physics.OverlapBox(meleeAttackArea.position, meleeAttackArea.localScale / 2, meleeAttackArea.rotation, meleeLayerMask);
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

            switch (thoughtState)
            {
                case ThoughtState.Patrol:
                    PatrolStep patrolStep = patrolSteps[currentStep];
                    direction = patrolStep.Direction();

                    // Next step
                    if (patrolStep.Next())
                    {
                        // Disable
                        patrolStep.enabled = false;

                        // Next and loop
                        currentStep++;
                        if (currentStep >= patrolSteps.Length)
                        {
                            currentStep = 0;
                        }

                        // Enable
                        patrolStep = patrolSteps[currentStep];
                        patrolStep.enabled = true;
                    }

                    if (EnableCombat()) ChangeThoughtState(ThoughtState.Combat);
                    break;
                case ThoughtState.Combat:
                    Vector3 displacement = player.transform.position - transform.position;
                    float distance = displacement.magnitude;

                    // Run
                    RecalculatePath();
                    direction = MoveOnPath();
                    running = true;

                    // Fight
                    if (distance <= playerAttackRadius && meleeCooldownTime <= 0)
                    {
                        animator.SetTrigger("Melee");

                        meleeStopDurationTime = meleeStopDuration;
                        meleeCooldownTime = meleeCooldown;

                        // Damage after some time
                        meleeDamageDelayActivated = true;
                        meleeDamageDelayTime = meleeDamageDelay;
                    }
                    break;
            }

            direction.Normalize();
            if (meleeStopDurationTime <= 0 && impactDurationTime <= 0 && direction != Vector2.zero)
            {
                // Walk
                animator.SetBool("Walking", true);
                animator.SetBool("Running", running);

                float effectiveSpeed = running ? runningSpeed : speed;

                velocity.x = direction.x * effectiveSpeed;
                velocity.z = direction.y * effectiveSpeed;

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

        CheckThoughtStateChanged();
    }

    private void RecalculatePath()
    {
        pathRecalculationTime -= Time.deltaTime;
        if (pathRecalculationTime <= 0)
        {
            NavMesh.CalculatePath(transform.position, player.transform.position, NavMesh.AllAreas, path);
            currentCorner = 0;
            pathRecalculationTime = pathRecalculationInterval;
        }
    }

    private Vector2 MoveOnPath()
    {
        if (path.corners != null && path.corners.Length > 0 && path.status == NavMeshPathStatus.PathComplete)
        {
            Vector3 displacement = path.corners[currentCorner] - transform.position;
            float distance = displacement.magnitude;
            if (distance > cornerReachRadius)
            {
                Vector3 direction = displacement.normalized;
                Vector2 gamepadDirection = new Vector2(direction.x, direction.z);

                return gamepadDirection;
            }
            else
            {
                if (currentCorner + 1 == path.corners.Length)
                {
                    // Reached
                    return Vector2.zero;
                }
                else
                {
                    // Next corner
                    currentCorner++;
                    return MoveOnPath();
                }
            }
        }

        return Vector2.zero;
    }

    private bool EnableCombat()
    {
        return PlayerInActiveView() && PlayerCanBeSeen();
    }

    private bool PlayerInActiveView()
    {
        return PlayerInFrontView(activeViewAngle);
    }

    private bool PlayerInPeripheralView()
    {
        return PlayerInFrontView(peripheralViewAngle);
    }

    private bool PlayerInFrontView(float maxAngle)
    {
        Vector3 displacement = playerFocus.position - eye.position;
        float playerAngle = Vector3.Angle(eye.forward, displacement);
        return playerAngle <= maxAngle / 2;
    }

    private bool PlayerCanBeSeen()
    {
        Vector3 displacement = playerFocus.position - eye.position;
        Vector3 direction = displacement.normalized;
        float distance = displacement.magnitude;
        return !Physics.Raycast(eye.position, direction, distance, opaqueLayers, QueryTriggerInteraction.Ignore);
    }

    private void OnThoughtStageEnable()
    {
        switch (thoughtState)
        {
            case ThoughtState.Patrol:
                // Disable all patrol steps by default
                foreach (PatrolStep patrolStep in patrolSteps)
                {
                    patrolStep.enabled = false;
                }
                break;

            case ThoughtState.Combat:
                path = new NavMeshPath();
                pathRecalculationTime = 0; // To immediately calculate path
                break;
        }
    }

    private void OnThoughtStageDisable()
    {
        switch (thoughtState)
        {
            case ThoughtState.Patrol:
                // Disable all patrol steps to save CPU
                foreach (PatrolStep patrolStep in patrolSteps)
                {
                    patrolStep.enabled = false;
                }
                break;
        }
    }

    private void ChangeThoughtState(ThoughtState next)
    {
        nextThoughtState = next;
    }

    private void CheckThoughtStateChanged()
    {
        if (nextThoughtState != thoughtState)
        {
            OnThoughtStageDisable();
            thoughtState = nextThoughtState;
            OnThoughtStageEnable();
        }
    }

    public void Hurt(float amount, bool createsMana = false)
    {
        animator.SetTrigger("Impact");
        impactDurationTime = impactDuration;
        meleeDamageDelayActivated = false;
        health.Hurt(amount);

        ChangeThoughtState(ThoughtState.Combat);

        if (health.Dead)
        {
            Die(createsMana);
        }
    }

    private void Die(bool createsMana = false)
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
        // Vision cones
        DebugExtension.DrawCone(eye.position, eye.forward, Color.red, activeViewAngle / 2);
        DebugExtension.DrawCone(eye.position, eye.forward, Color.yellow, peripheralViewAngle / 2);
    }

    public void Kill(bool createsMana = false)
    {
        Hurt(9999999, createsMana);
    }

    public void Flame(float amount)
    {
        Hurt(amount);
    }
}
