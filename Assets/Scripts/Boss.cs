using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour, IHurtable, IFlammable {
    public enum Stage { Idle, WalkBeforeSlash, Slash, AimBeforeDash, Dash, NextPhase, Dead };
    public Stage initialStage = Stage.WalkBeforeSlash;
    [SerializeField] private Stage currentStage;
    private Stage nextStage;

    public Stage[] stagesPhase1;
    public Stage[] stagesPhase2;

    public Player player;
    public Health health;
    public Animator animator;
    public float walkSpeed = 5;
    public float dashSpeed = 10;
    public float turnSpeed = 4;
    public float slashAngleOffset;
    public float slashDistance = 10;
    public float[] slashDurations;
    public int slashTypesPhase1 = 3;
    public int slashTypesPhase2 = 4;
    public float slashAngle = 90;
    public float idleDuration = 2;
    public float idleDurationPhase2 = 1;
    public float aimDashTurnSpeed = 6;
    public float dashTurnSpeed = 1;
    public float aimDashDuration = 1;
    public float dashDuration = 3;
    public float dashHitDamage = 60;
    public float dashHitCooldown = 1;
    public float nextPhaseDuration = 3.167f;
    public AttackArea swordAttackArea;
    public AttackArea iceBreaker;
    public MeleeWeaponTrail swordEffect;
    private float dashHitCooldownTime;
    private float slashBeginAngle;
    private bool dangerBody = false;
    public CharacterController characterController;

    [SerializeField] private int phase = 1;
    public float phase2Health = 100;

    public BossManager bossManager;

    [Header("Mana")]
    public Transform manaParticleSpawn;
    public ManaParticle manaParticle;
    public int manaParticlesAmount;
    public float manaParticleExplosionForce;

    private void Start()
    {
        currentStage = initialStage;
        nextStage = initialStage;
        OnStageEnable();
    }

    private void Update()
    {
        animator.SetBool("Walking", false);
        animator.SetBool("Running", false);

        switch (currentStage)
        {
            case Stage.Idle:
                {
                    // Turn towards player
                    Vector3 displacement = player.transform.position - transform.position;
                    Vector3 direction3d = new Vector3(displacement.x, 0, displacement.z);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction3d, Vector3.up), turnSpeed * Time.deltaTime);
                }
                break;

            case Stage.WalkBeforeSlash:
                {
                    // Turn towards player
                    Vector3 displacement = player.transform.position - transform.position;
                    Vector3 direction3d = new Vector3(displacement.x, 0, displacement.z);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction3d, Vector3.up), turnSpeed * Time.deltaTime);

                    // Walk
                    characterController.Move(transform.forward * walkSpeed * Time.deltaTime);
                    animator.SetBool("Walking", true);

                    // Slash
                    float distance = displacement.magnitude;
                    float angle = Vector3.Angle(transform.forward, displacement);
                    if (distance <= slashDistance)
                    {
                        SetStage(Stage.Slash);
                    }

                    if (EnableNextPhase()) SetStage(Stage.NextPhase);
                }
                break;

            case Stage.Slash:
                {
                    // Turn to ideal angle
                    // (DONE IN ANIMATION)
                }
                break;

            case Stage.AimBeforeDash:
                {
                    // Turn towards player
                    Vector3 displacement = player.transform.position - transform.position;
                    Vector3 direction3d = new Vector3(displacement.x, 0, displacement.z);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction3d, Vector3.up), aimDashTurnSpeed * Time.deltaTime);

                    if (EnableNextPhase()) SetStage(Stage.NextPhase);
                }
                break;

            case Stage.Dash:
                {
                    dashHitCooldownTime -= Time.deltaTime;

                    // Turn towards player
                    Vector3 displacement = player.transform.position - transform.position;
                    Vector3 direction3d = new Vector3(displacement.x, 0, displacement.z);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction3d, Vector3.up), dashTurnSpeed * Time.deltaTime);

                    // Walk
                    characterController.Move(transform.forward * dashSpeed * Time.deltaTime);
                    animator.SetBool("Walking", true);
                    animator.SetBool("Running", true);
                }
                break;
        }

        CheckStageChange();
    }

    private void OnStageEnable()
    {
        switch (currentStage)
        {
            case Stage.Idle:
                if (EnableNextPhase())
                {
                    SetStage(Stage.NextPhase);
                }
                else
                {
                    StartCoroutine(DelayedNextStage((phase == 1) ? idleDuration : idleDurationPhase2));
                }
                break;

            case Stage.Slash:
                slashBeginAngle = transform.rotation.eulerAngles.y;

                int randomSlashIndex = Random.Range(0, (phase == 1) ? slashTypesPhase1 : slashTypesPhase2);

                animator.SetInteger("Melee Type", randomSlashIndex);
                animator.SetTrigger("Melee");
                StartCoroutine(DelayedStage(slashDurations[randomSlashIndex], Stage.Idle));
                break;

            case Stage.AimBeforeDash:
                animator.SetTrigger("Pound Chest");
                StartCoroutine(DelayedStage(aimDashDuration, Stage.Dash));
                break;

            case Stage.Dash:
                StartCoroutine(DelayedStage(dashDuration, Stage.Idle));
                SetDangerBody(true);
                break;

            case Stage.NextPhase:
                phase = 2;
                animator.SetTrigger("Power Up");
                StartCoroutine(DelayedStage(nextPhaseDuration, Stage.Idle));
                break;
        }
    }

    private bool EnableNextPhase()
    {
        return phase == 1 && health.Amount <= phase2Health;
    }

    private IEnumerator DelayedNextStage(float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector3 displacement = player.transform.position - transform.position;
        float distance = displacement.magnitude;

        //if (distance < )
        if (phase == 1)
        {
            foreach (Stage stage in stagesPhase1)
            {
                SetStage(stagesPhase1[Random.Range(0, stagesPhase1.Length)]);
            }
        }
        else
        {
            foreach (Stage stage in stagesPhase1)
            {
                SetStage(stagesPhase2[Random.Range(0, stagesPhase2.Length)]);
            }
        }
    }

    private IEnumerator DelayedStage(float delay, Stage stage)
    {
        yield return new WaitForSeconds(delay);
        SetStage(stage);
    }

    private void OnStageDisable()
    {
        switch (currentStage)
        {
            case Stage.Slash:
                SetSword(false);
                break;

            case Stage.Dash:
                SetDangerBody(false);
                break;
        }
    }

    public void EnableSword()
    {
        SetSword(true);
    }

    public void DisableSword()
    {
        SetSword(false);
    }

    public void SetSword(bool value)
    {
        swordAttackArea.active = value;
        swordEffect.Emit = value;
    }

    public void EnableDangerBody()
    {
        SetDangerBody(true);
    }

    public void DisableDangerBody()
    {
        SetDangerBody(false);
    }

    public void SetDangerBody(bool value)
    {
        dangerBody = value;
        iceBreaker.active = value;
    }

    private void SetStage(Stage stage)
    {
        nextStage = stage;
    }

    private void CheckStageChange()
    {
        if (nextStage != currentStage)
        {
            OnStageDisable();
            currentStage = nextStage;
            OnStageEnable();
        }
    }

    public void Hurt(float amount, bool createsMana = false, Transform sender = null)
    {
        if (!health.Dead)
        {
            health.Hurt(amount);
            if (createsMana)
            {
                for (int i = 0; i < manaParticlesAmount; i++)
                {
                    ManaParticle newParticle = Instantiate(manaParticle, manaParticleSpawn.position, Quaternion.identity);
                    newParticle.Explode(manaParticleExplosionForce);
                }
            }

            if (health.Dead)
            {
                currentStage = Stage.Dead;
                animator.SetTrigger("Death");
                StartCoroutine(DelayedDeath());
            }
        }
    }

    private IEnumerator DelayedDeath()
    {
        yield return new WaitForSeconds(6.117f);
        bossManager.StopBoss();
    }

    public void Kill(bool createsMana = false, Transform sender = null)
    {
        Hurt(9999999, createsMana, sender);
    }

    private void OnControllerColliderHit(ControllerColliderHit collision)
    {
        if (dangerBody) {
            IHurtable hurtable = collision.transform.GetComponent<IHurtable>();
            if (hurtable != null)
            {
                if (dashHitCooldownTime <= 0)
                {
                    hurtable.Hurt(dashHitDamage);
                    dashHitCooldownTime = dashHitCooldown;
                }
            }
        }
    }

    public void Flame(float amount)
    {
        Hurt(amount);
    }
}
