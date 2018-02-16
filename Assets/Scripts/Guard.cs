using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Guard : Enemy, IHurtable, IFlammable
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
    public Player player;
    public Glow glow;
    [ColorUsage(false, true, 0f, 8f, 0.125f, 3f)] public Color hurtGlow;
    public float hurtGlowDuration = 0.5f;
    private float hurtGlowTime;

    [Header("Senses (Vision)")]
    public float activeViewAngle = 90;
    public float peripheralViewAngle = 180;
    public Transform eye;
    public Transform playerFocus;
    public LayerMask opaqueLayers;
    private float longestDistance = 50;

    [Header("Thought")]
    public ThoughtState initialThoughtState = ThoughtState.Patrol;
    private ThoughtState thoughtState;
    private ThoughtState nextThoughtState;
    public enum ThoughtState { Patrol, Alert, Combat };
    public bool oblivious = false;

    [Header("Patrol settings")]
    public GuardPath guardPath;
    private int currentStep;
    private PatrolStep[] patrolSteps;

    [Header("Pathfinding")]
    public float pathRecalculationInterval = 1;
    private float pathRecalculationTime;
    private NavMeshPath path;
    public float cornerReachRadius = 0.2f;
    private int currentCorner = 0;

    [Header("Alert")]
    private Vector3 alertTarget;

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

    public bool Alive
    {
        get
        {
            return alive;
        }
    }

    private void Start()
    {
        // IMPORTANT
        enemyManager.RegisterEnemy(this);

        // Cheat so no need to redo every one
        if (guardPath == null)
        {
            GuardPath temp = transform.parent.GetComponent<GuardPath>();
            if (temp != null)
            {
                guardPath = temp;
                Debug.LogWarning("Set guard path for " + transform.parent.name + "! Not recommended, it's a hack!");
            }
        }

        patrolSteps = guardPath.patrolSteps;

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

                    if (EnableAlert()) ChangeThoughtState(ThoughtState.Alert);
                    if (EnableCombat()) ChangeThoughtState(ThoughtState.Combat);
                    break;

                case ThoughtState.Alert:
                    {
                        RecalculatePath(alertTarget);
                        bool success, reached;
                        direction = MoveOnPath(out success, out reached);

                        if (EnableCombat()) ChangeThoughtState(ThoughtState.Combat);
                    }
                    break;

                case ThoughtState.Combat:
                    if (player.Alive)
                    {
                        Vector3 displacement = player.transform.position - transform.position;
                        float distance = displacement.magnitude;

                        // Run
                        RecalculatePath(player.transform.position);
                        bool success, reached;
                        direction = MoveOnPath(out success, out reached);

                        // Fallback method in case of pathfinding failure AND
                        // Walk towards player even if pathfinding is done so it can react to his movements at close range
                        /*if (!success || reached)
                        {
                            direction = new Vector2(displacement.x, displacement.z);
                        }*/

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

        hurtGlowTime -= Time.deltaTime;
        if (hurtGlowTime < 0) hurtGlowTime = 0;
        //glow.SetGlow(hurtGlow * hurtGlowTime / hurtGlowDuration);
    }

    private void RecalculatePath(Vector3 target)
    {
        pathRecalculationTime -= Time.deltaTime;
        if (pathRecalculationTime <= 0)
        {
            NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);
            currentCorner = 0;
            pathRecalculationTime = pathRecalculationInterval;
        }
    }

    private Vector2 MoveOnPath(out bool success, out bool reached)
    {
        if (path.corners != null && path.corners.Length > 0 && path.status == NavMeshPathStatus.PathComplete)
        {
            Vector3 displacement = path.corners[currentCorner] - transform.position;
            float distance = displacement.magnitude;
            if (distance > cornerReachRadius)
            {
                Vector3 direction = displacement.normalized;
                Vector2 gamepadDirection = new Vector2(direction.x, direction.z);

                success = true;
                reached = false;
                return gamepadDirection;
            }
            else
            {
                if (currentCorner + 1 == path.corners.Length)
                {
                    // Reached
                    success = true;
                    reached = true;
                    return Vector2.zero;
                }
                else
                {
                    // Next corner
                    currentCorner++;
                    return MoveOnPath(out success, out reached);
                }
            }
        }

        success = false;
        reached = false;
        return Vector2.zero;
    }

    private bool EnableAlert()
    {
        return CanHearPlayer() && !oblivious;
    }

    private bool CanHearPlayer()
    {
        Vector3 displacement = player.transform.position - transform.position;
        float distance = displacement.magnitude;
        return distance < player.ProducedAudibleRange;
    }

    private bool EnableCombat()
    {
        return PlayerInActiveView() && PlayerCanBeSeen() && !oblivious;
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
        if (distance > longestDistance) return false;
        return !Physics.Raycast(eye.position, direction, distance, opaqueLayers, QueryTriggerInteraction.Ignore);
    }

    public void StartCombat()
    {
        ChangeThoughtState(ThoughtState.Combat);
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
                enemyManager.SetMetaEnemyState(this, MetaEnemyState.Idle);
                break;

            case ThoughtState.Alert:
                Debug.Log("Who's there?");
                alertTarget = player.transform.position;
                path = new NavMeshPath();
                pathRecalculationTime = 0;
                break;

            case ThoughtState.Combat:
                path = new NavMeshPath();
                pathRecalculationTime = 0; // To immediately calculate path
                enemyManager.SetMetaEnemyState(this, MetaEnemyState.Combat);
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

    public void Hurt(float amount, bool createsMana = false, Transform sender = null)
    {
        animator.SetTrigger("Impact");
        hurtGlowTime = hurtGlowDuration;
        impactDurationTime = impactDuration;
        meleeDamageDelayActivated = false;

        bool stealthKill = false;

        if (sender != null)
        {
            Vector3 displacement = sender.position - transform.position;
            float angle = Vector3.Angle(transform.forward, displacement);
            if (angle > 90)
            {
                stealthKill = true;
            }
        }

        if (stealthKill)
        {
            health.Hurt(9999999);
        }
        else
        {
            health.Hurt(amount);
            ChangeThoughtState(ThoughtState.Combat);
        }

        if (health.Dead)
        {
            Die(createsMana);
        }
    }

    private void Die(bool createsMana = false)
    {
        enemyManager.SetMetaEnemyState(this, MetaEnemyState.Dead);
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
        //DebugExtension.DrawCone(eye.position, eye.forward, Color.yellow, peripheralViewAngle / 2);
    }

    public void Kill(bool createsMana = false, Transform sender = null)
    {
        Hurt(9999999, createsMana);
    }

    public void Flame(float amount)
    {
        Hurt(amount);
    }
}
