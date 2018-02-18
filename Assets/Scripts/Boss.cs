using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour, IHurtable {
    public enum Stage { Idle, WalkBeforeSlash, Slash, AimBeforeDash, Dash };
    public Stage initialStage = Stage.WalkBeforeSlash;
    private Stage currentStage;
    private Stage nextStage;

    public Stage[] stagesPhase1;

    public Player player;
    public Health health;
    public Animator animator;
    public float walkSpeed = 5;
    public float dashSpeed = 10;
    public float turnSpeed = 4;
    public float slashAngleOffset;
    public float slashDistance = 10;
    public float slashDuration = 3;
    public float slashAngle = 90;
    public float idleDuration = 2;
    public float aimDashTurnSpeed = 6;
    public float dashTurnSpeed = 1;
    public float aimDashDuration = 1;
    public float dashDuration = 3;
    public float dashHitDamage = 60;
    public float dashHitCooldown = 1;
    public AttackArea swordAttackArea;
    public MeleeWeaponTrail swordEffect;
    private float dashHitCooldownTime;
    private float slashBeginAngle;
    private bool dangerBody = false;

    public Image healthBar;
    public BossManager bossManager;

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
                    transform.Translate(Vector3.forward * walkSpeed * Time.deltaTime);
                    animator.SetBool("Walking", true);

                    // Slash
                    float distance = displacement.magnitude;
                    float angle = Vector3.Angle(transform.forward, displacement);
                    if (distance <= slashDistance && angle <= slashAngle)
                    {
                        SetStage(Stage.Slash);
                    }
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
                    transform.Translate(Vector3.forward * dashSpeed * Time.deltaTime);
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
                StartCoroutine(DelayedNextStage(idleDuration));
                break;

            case Stage.Slash:
                slashBeginAngle = transform.rotation.eulerAngles.y;
                animator.SetTrigger("Melee");
                StartCoroutine(DelayedStage(slashDuration, Stage.Idle));
                break;

            case Stage.AimBeforeDash:
                animator.SetTrigger("Pound Chest");
                StartCoroutine(DelayedStage(aimDashDuration, Stage.Dash));
                break;

            case Stage.Dash:
                StartCoroutine(DelayedStage(dashDuration, Stage.Idle));
                SetDangerBody(true);
                break;
        }
    }

    private IEnumerator DelayedNextStage(float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (Stage stage in stagesPhase1)
        {
            SetStage(stagesPhase1[Random.Range(0, stagesPhase1.Length)]);
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

    public void SetDangerBody(bool value)
    {
        dangerBody = value;
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

            if (health.Dead)
            {
                bossManager.StopBoss();
                Destroy(gameObject);
            }
        }
    }

    public void Kill(bool createsMana = false, Transform sender = null)
    {
        Hurt(9999999, createsMana, sender);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (dangerBody) {
            IHurtable hurtable = collider.GetComponent<IHurtable>();
            if (hurtable != null)
            {
                if (dashHitCooldownTime <= 0)
                {
                    hurtable.Hurt(dashHitDamage);
                    dashHitCooldownTime = dashHitCooldown;
                }
            }

            IceChunk iceChunk = collider.GetComponent<IceChunk>();
            if (iceChunk != null)
            {
                iceChunk.DestroyIce();
            }
        }
    }
}
