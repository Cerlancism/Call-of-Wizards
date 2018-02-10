using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Player))]
public class RagdollEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(5);
        GUILayout.Label("Debug Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Die", EditorStyles.miniButton))
        {
            ((Player)target).Die();
        }
    }
}
#endif

[Serializable]
public class Spell
{
    public string name;
    public string displayName;
    public Sprite icon;
}

// ANIMATOR INTEGER PARAMETERS
//
// Walk Mode - walk (0), run (1), crouch (2)
// Crouch = sneak

public class Player : MonoBehaviour, IHurtable, IManaAbsorber {
    [Header("Movement")]
    public float walkingSpeed = 5;
    public float runningSpeed = 10;
    public float sneakingSpeed = 3;
    public float jumpingSpeed = 10;
    public float turningSpeed = 0.1f;
    public float runningStaminaCost = 0.1f; // Per second
    public float jumpingStaminaCost = 0.1f; // Per jump
    public Transform playerCamera;
    public CharacterController characterController;
    public Animator animator;
    public Ragdoll ragdoll;
    private bool alive = true;

    public Stamina stamina;
    public Health health;
    public Mana mana;
    public CharacterPhysics characterPhysics;
    public CameraShake cameraShake;
    public HitPause hitPause;
    public TimeManager timeManager;
    public CharacterGravity characterGravity;
    public ManaParticleSink manaParticleSink;

    [Header("Spell Bar")]
    public List<Spell> spells;
    public RectTransform spellBar;
    public RectTransform spellBarSpell;
    public float offsetX = 100;
    public CanvasGroup spellBarCanvasGroup;
    public float spellBarTimescale = 0.1f;
    private int currentSpell;
    private bool spellBarActivated;

    [Header("Melee")]
    public float meleeCooldown = 0.2f;
    public float meleeStopDuration = 0.7f;
    public float meleeStaminaCost = 0.1f; // Per melee attack
    public Transform meleeAttackArea;
    public TrailRenderer meleeFistTrail;
    public float meleeFistTrailDuration = 0.5f;
    public float meleeDamage = 10;
    public float meleeDamageDelay = 0.15f;
    public float meleeHitPauseTime = 0.1f;
    private float meleeCooldownTime;
    private float meleeStopDurationTime;
    private float meleeFistTrailDurationTime;
    private float meleeDamageDelayTime;
    private bool meleeDamageDelayActivated;

    [Header("Healing")]
    public float healingCooldown = 2;
    public float healingManaCost = 30;
    public float healingHealthAmount = 60;
    public float healingStopDuration = 2;
    private float healingStopDurationTime;
    private float healingCooldownTime;

    [Header("Basic Attack")]
    public BasicSpell basicSpell;
    public float basicSpellCooldown = 0.5f;
    public Transform basicSpellEmitPosition;
    public float basicSpellManaCost = 5;
    private float basicSpellCooldownTime;

    [Header("Zoom Crosshair")]
    public CanvasGroup crosshairCanvasGroup;
    public float crosshairFov = 30;
    public Vector3 crosshairPositionalOffset;
    public float crosshairZoomDuration = 0.2f;
    public string[] crosshairDontZoomSpells;
    private float crosshairZoomTime;
    private float originalFov;
    private Vector3 originalPositionalOffset;
    private enum ZoomCrosshairState { Unzoomed, ZoomingIn, Zoomed, ZoomingOut };
    private ZoomCrosshairState zoomCrosshairState = ZoomCrosshairState.Unzoomed;

    [Header("Crosshair")]
    public float crosshairFallbackDistanceFromCamera = 100;

    [Header("Death")]
    public AudioClip deathMusic;
    public AudioSource backgroundMusicSource;
    public float deathTimeScale = 0.3f;

    private void Start()
    {
        // Setup spell bar
        UpdateSpellUI();

        // Find original values
        originalFov = playerCamera.GetComponent<Camera>().fieldOfView;
        originalPositionalOffset = playerCamera.GetComponent<CameraOrbit>().cameraOffset;
    }

    private void Update()
    {
        if (alive)
        {
            Walk();
            Jump();
            Attack();
        }
        ZoomCrosshair();
        SwitchSpell();

        if (!alive)
        {
            timeManager.timeScale = deathTimeScale;
        }
    }

    private void ZoomCrosshair()
    {
        switch (zoomCrosshairState)
        {
            case ZoomCrosshairState.Unzoomed:
                if (Input.GetButton("Zoom Crosshair") && !ArrayUtility.Contains(crosshairDontZoomSpells, spells[currentSpell].name))
                {
                    zoomCrosshairState = ZoomCrosshairState.ZoomingIn;
                    crosshairZoomTime = 0;
                }
                break;

            case ZoomCrosshairState.ZoomingIn:
                {
                    crosshairZoomTime += Time.deltaTime;

                    float t = crosshairZoomTime / crosshairZoomDuration;
                    t = Mathf.Clamp(t, 0, 1);

                    playerCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(originalFov, crosshairFov, Mathf.SmoothStep(0, 1, t));
                    playerCamera.GetComponent<CameraOrbit>().cameraOffset = Vector3.Lerp(originalPositionalOffset, crosshairPositionalOffset, Mathf.SmoothStep(0, 1, t));
                    crosshairCanvasGroup.alpha = Mathf.Lerp(0, 1, Mathf.SmoothStep(0, 1, t));

                    if (crosshairZoomTime >= crosshairZoomDuration)
                    {
                        zoomCrosshairState = ZoomCrosshairState.Zoomed;
                    }
                    break;
                }

            case ZoomCrosshairState.Zoomed:
                if (!Input.GetButton("Zoom Crosshair") || ArrayUtility.Contains(crosshairDontZoomSpells, spells[currentSpell].name))
                {
                    zoomCrosshairState = ZoomCrosshairState.ZoomingOut;
                    crosshairZoomTime = 0;
                }
                break;

            case ZoomCrosshairState.ZoomingOut:
                {
                    crosshairZoomTime += Time.deltaTime;

                    float t = crosshairZoomTime / crosshairZoomDuration;
                    t = Mathf.Clamp(t, 0, 1);

                    playerCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(crosshairFov, originalFov, Mathf.SmoothStep(0, 1, t));
                    playerCamera.GetComponent<CameraOrbit>().cameraOffset = Vector3.Lerp(crosshairPositionalOffset, originalPositionalOffset, Mathf.SmoothStep(0, 1, t));
                    crosshairCanvasGroup.alpha = Mathf.Lerp(1, 0, Mathf.SmoothStep(0, 1, t * 2));

                    if (crosshairZoomTime >= crosshairZoomDuration)
                    {
                        zoomCrosshairState = ZoomCrosshairState.Unzoomed;
                    }
                    break;
                }
        }
    }

    private void UpdateSpellUI()
    {
        // Delete spell UI
        foreach (RectTransform child in spellBar)
        {
            Destroy(child.gameObject);
        }

        // Construct spell UI
        for (int i = 0; i < spells.Count; i++)
        {
            RectTransform newSpellBarSpell = Instantiate(spellBarSpell, spellBar);
            newSpellBarSpell.anchoredPosition = new Vector2(i * offsetX, 0);
            newSpellBarSpell.GetComponent<SpellBarSpell>().text.text = spells[i].displayName;
            newSpellBarSpell.GetComponent<SpellBarSpell>().image.sprite = spells[i].icon;
        }
    }

    public void AddSpell(Spell spell)
    {
        spells.Add(spell);
        UpdateSpellUI();
    }

    private void SwitchSpell()
    {
        /*if (Input.GetButtonDown("Next Spell"))
        {
            currentSpell++;
            if (currentSpell >= spells.Length)
            {
                currentSpell = 0;
            }
        }
        if (Input.GetButtonDown("Previous Spell"))
        {
            currentSpell--;
            if (currentSpell < 0)
            {
                currentSpell = spells.Length - 1;
            }
        }*/

        // Spell bar activation
        if (Input.GetButton("Switch Spells"))
        {
            spellBarActivated = true;
            timeManager.timeScale = spellBarTimescale;
            spellBarCanvasGroup.alpha = 1;
        }
        else
        {
            spellBarActivated = false;
            timeManager.timeScale = 1;
            spellBarCanvasGroup.alpha = 0;
        }

        if (spellBarActivated)
        {
            // Numbered navigation
            for (int i = 0; i < spells.Count && i < 10; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    currentSpell = i;
                }
            }

            // Scrollwheel navigation
            currentSpell += (int) Input.GetAxisRaw("Mouse ScrollWheel");
        }

        // Clamp spell
        currentSpell = Mathf.Clamp(currentSpell, 0, spells.Count - 1);

        // Highlight selected spell
        if (spellBarActivated)
        {
            spellBar.anchoredPosition = new Vector2(-offsetX * currentSpell, 0);
        }
    }

    private void Attack()
    {
        meleeCooldownTime -= Time.deltaTime;
        healingCooldownTime -= Time.deltaTime;
        basicSpellCooldownTime -= Time.deltaTime;

        meleeFistTrailDurationTime -= Time.deltaTime;
        if (meleeFistTrailDurationTime <= 0)
        {
            meleeFistTrail.enabled = false;
        }

        if (meleeDamageDelayActivated)
        {
            meleeDamageDelayTime -= Time.deltaTime;
            if (meleeDamageDelayTime <= 0)
            {
                bool didHitHurtable = false;

                // Hurt hurtables
                Collider[] collidersInAttackArea = Physics.OverlapBox(meleeAttackArea.position, meleeAttackArea.localScale / 2, meleeAttackArea.rotation);
                foreach (Collider colliderInAttackArea in collidersInAttackArea)
                {
                    IHurtable hurtable = colliderInAttackArea.GetComponent<IHurtable>();

                    // If it's hurtable
                    // Also, don't hurt ourselves
                    if (hurtable != null && colliderInAttackArea.gameObject != gameObject)
                    {
                        hurtable.Hurt(meleeDamage, true);
                        didHitHurtable = true;

                        // Hit effects
                        HitPause otherHitPause = colliderInAttackArea.GetComponent<HitPause>();
                        if (otherHitPause)
                        {
                            otherHitPause.Pause(meleeHitPauseTime);
                        }
                    }
                }

                meleeDamageDelayActivated = false;

                if (didHitHurtable)
                {
                    // Hit effects
                    cameraShake.Shake(0.3f);
                    hitPause.Pause(meleeHitPauseTime);
                }
            }
        }

        if (Input.GetButtonDown("Attack"))
        {
            switch (spells[currentSpell].name)
            {
                case "Melee":
                    if (meleeCooldownTime <= 0 && stamina.HasStamina)
                    {
                        animator.SetTrigger("Melee");
                        meleeStopDurationTime = meleeStopDuration;
                        meleeCooldownTime = meleeCooldown;
                        stamina.UseStamina(meleeStaminaCost);

                        // Enable trail
                        meleeFistTrail.enabled = true;
                        meleeFistTrailDurationTime = meleeFistTrailDuration;

                        // Damage after some time
                        meleeDamageDelayActivated = true;
                        meleeDamageDelayTime = meleeDamageDelay;
                    }
                    break;

                case "Healing":
                    if (healingCooldownTime <= 0 && mana.HasMana)
                    {
                        healingCooldownTime = healingCooldown;
                        healingStopDurationTime = healingStopDuration;
                        animator.SetTrigger("Healing");

                        // Use up mana
                        mana.UseMana(healingManaCost);

                        // Replenish health
                        health.Heal(healingHealthAmount);
                    }
                    break;

                case "Basic":
                    if (basicSpellCooldownTime <= 0 && mana.HasMana && zoomCrosshairState == ZoomCrosshairState.Zoomed)
                    {
                        basicSpellCooldownTime = basicSpellCooldown;

                        // Get target by crosshair
                        Vector3 target;
                        RaycastHit hit;
                        if (Physics.Raycast(playerCamera.GetComponent<Camera>().ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit))
                        {
                            target = hit.point;
                        }
                        else
                        {
                            target = playerCamera.position + playerCamera.forward * crosshairFallbackDistanceFromCamera;
                        }

                        // Calculate projectile direction
                        Vector3 originalPosition = basicSpellEmitPosition.position;
                        Vector3 direction = target - originalPosition;

                        // Create projectile
                        BasicSpell newBasicSpell = Instantiate(basicSpell, originalPosition, Quaternion.identity);
                        newBasicSpell.direction = direction;
                        newBasicSpell.shooter = gameObject;

                        // Use up mana
                        mana.UseMana(basicSpellManaCost);
                    }
                    break;
            }
        }
    }

    private void Walk()
    {
        // Get input direction
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        bool runInput = Input.GetButton("Run");
        bool sneakInput = Input.GetButton("Crouch");
        Vector2 inputDirection = new Vector2(horizontalInput, verticalInput);

        // Find walk mode
        int walkMode;
        if (runInput && stamina.HasStamina)
        {
            walkMode = 1;
        }
        else if (sneakInput)
        {
            walkMode = 2;
        }
        else
        {
            walkMode = 0;
        }
        animator.SetInteger("Walk Mode", walkMode);

        // Check if moving
        meleeStopDurationTime -= Time.deltaTime;
        healingStopDurationTime -= Time.deltaTime;
        if (inputDirection == Vector2.zero || meleeStopDurationTime > 0 || healingStopDurationTime > 0)
        {
            animator.SetBool("Walking", false);

            // Set velocity
            characterPhysics.velocity.x = 0;
            characterPhysics.velocity.z = 0;
        }
        else
        {
            animator.SetBool("Walking", true);

            // Make relative to camera
            float cameraRotation = playerCamera.transform.eulerAngles.y;
            // Rotate inputDirection by cameraRotation
            Vector2 direction = Quaternion.AngleAxis(cameraRotation, -Vector3.forward) * inputDirection;

            // Set velocity
            float speed;
            switch (walkMode)
            {
                case 1:
                    speed = runningSpeed;
                    stamina.UseStamina(runningStaminaCost * Time.deltaTime);
                    break;

                case 2:
                    speed = sneakingSpeed;
                    break;

                default:
                    speed = walkingSpeed;
                    break;
            }
            Vector2 displacement = direction.normalized * Mathf.Clamp(inputDirection.magnitude, 0, 1) * speed;
            characterPhysics.velocity.x = displacement.x;
            characterPhysics.velocity.z = displacement.y;

            // Turn player towards moving direction
            Vector3 direction3d = new Vector3(direction.x, 0, direction.y);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction3d, Vector3.up), turningSpeed * Time.deltaTime);
        }
    }

    private void Jump()
    {
        // Get input
        bool jumpInput = Input.GetButtonDown("Jump");

        // Set velocity
        if (jumpInput && characterPhysics.IsGroundedLenient && stamina.HasStamina && meleeStopDurationTime <= 0)
        {
            characterPhysics.velocity.y += jumpingSpeed;
            stamina.UseStamina(jumpingStaminaCost);
        }
    }

    public void Die()
    {
        alive = false;
        ragdoll.SetEnabled(true);

        // Play effect
        backgroundMusicSource.PlayOneShot(deathMusic);

        // Release mana
        manaParticleSink.enabled = false;
    }

    public void AbsorbMana(float amount)
    {
        mana.ReplenishMana(amount);
    }

    public bool CanAbsorb()
    {
        return !mana.MaxMana;
    }

    public void Hurt(float amount, bool createsMana = false)
    {
        health.Hurt(amount);
        if (health.Dead)
        {
            Die();
        }
    }

    public void Kill(bool createsMana = false)
    {
        Hurt(999999, createsMana);
    }
}