using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public Transform preview;
}

// ANIMATOR INTEGER PARAMETERS
//
// Walk Mode - walk (0), run (1), crouch (2)
// Crouch = sneak

public class Player : MonoBehaviour, IHurtable, IManaAbsorber, IColdable
{
    [Header("Movement")]
    public float walkingSpeed = 5;
    public float runningSpeed = 10;
    public float sneakingSpeed = 3;
    public float jumpingSpeed = 10;
    public float turningSpeed = 0.1f;
    public float runningStaminaCost = 0.1f; // Per second
    public float jumpingStaminaCost = 0.1f; // Per jump
    //public float swimTriggerDepth = 1;
    public float swimTargetDepth = 0.1f;
    public Transform playerCamera;
    public CharacterController characterController;
    public Animator animator;
    public Ragdoll ragdoll;
    public Glow glow;
    [ColorUsage(false, true, 0f, 8f, 0.125f, 3f)] public Color hurtGlow;
    public float hurtGlowDuration = 0.5f;
    public float jumpSlopeLimit = 45;
    private float hurtGlowTime;
    private bool alive = true;
    private bool freeze = false;
    private bool swimming = false;
    private SwimmingMedium swimmingMedium;

    public void Freeze()
    {
        freeze = true;
        animator.SetBool("Walking", false);
        characterPhysics.velocity = Vector3.zero;
    }

    public void Unfreeze()
    {
        freeze = false;
    }

    public bool Alive
    {
        get
        {
            return alive;
        }
    }

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
    public Image spellWheel;
    public Transform spellPreview;
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
    public float meleeLockTurnSpeed = 12;
    public float meleeLockScanRadius = 3;
    private float meleeCooldownTime;
    private float meleeStopDurationTime;
    private float meleeFistTrailDurationTime;
    private float meleeDamageDelayTime;
    private bool meleeDamageDelayActivated;
    private Vector2 meleeLockDirection;
    public AudioSource punchSoundSource;
    public AudioClip[] punchSounds;

    [Header("Healing")]
    public float healingCooldown = 2;
    public float healingManaCost = 30;
    public float healingHealthAmount = 60;
    public float healingStopDuration = 2;
    private float healingStopDurationTime;
    private float healingCooldownTime;
    public AudioClip healSound;
    [ColorUsage(false, true, 0f, 8f, 0.125f, 3f)] public Color healGlow;
    public float healGlowDuration = 0.5f;
    private float healGlowTime;

    [Header("Basic Attack")]
    public BasicSpell basicSpell;
    public float basicSpellCooldown = 0.5f;
    public Transform basicSpellEmitPosition;
    public float basicSpellManaCost = 5;
    private float basicSpellCooldownTime;
    public AudioSource wandSoundSource;
    public AudioClip basicSound;

    [Header("Fire")]
    public Flamethrower flamethrower;
    public float fireSpellManaCost = 20; // Per second
    public AudioSource flamethrowerSound;

    [Header("Freezing")]
    public Flamethrower icethrower;
    public float iceSpellManaCost = 10; // Per second
    public AudioSource icethrowerSound;

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
    public AudioSource deathMusic;
    public float deathTimeScale = 0.3f;
    public GameObject[] deathHiddenUIs;
    public Image deathimage;
    public Text deathtext;
    public bool loadingScreen = false;

    [Header("Cold")]
    public float initialCold = 0;
    public float maxCold = 100;
    public float coldDamageInterval = 0.5f;
    public float coldDamagePerInterval = 5;
    public float warmOffPerSecond = 60;
    [ColorUsage(false, true, 0f, 8f, 0.125f, 3f)] public Color coldGlow;
    private bool isCold = false;
    private float coldPerSecond;
    private float cold;
    private float coldDamageTime;

    [Header("Audiotory")]
    private float producedAudibleRange = 0;
    public float runningAudibleRange = 7;
    public float walkingAudibleRange = 5;
    public bool muted = false;
    public float ProducedAudibleRange
    {
        get
        {
            return producedAudibleRange;
        }
    }

    private void Start()
    {
        // Setup spell bar
        UpdateSpellUI();

        // Find original values
        originalFov = playerCamera.GetComponent<Camera>().fieldOfView;
        originalPositionalOffset = playerCamera.GetComponent<CameraOrbit>().cameraOffset;

        //death image set to transparent at start
        deathimage.canvasRenderer.SetAlpha(0.0f);
        deathtext.enabled = false;

        cold = initialCold;
    }

    private void Update()
    {
        producedAudibleRange = 0;

        if (alive & !freeze)
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

            if (Input.GetButtonDown("Respawn"))
            {
                SceneManager.LoadSceneAsync(loadingScreen ? "Loading" : "Game");
            }
        }

        // Cold

        if (isCold)
        {
            cold += coldPerSecond * Time.deltaTime;
        }
        else
        {
            cold -= warmOffPerSecond * Time.deltaTime;
        }

        cold = Mathf.Clamp(cold, 0, maxCold);

        coldDamageTime -= Time.deltaTime;
        if (cold >= 100)
        {
            if (coldDamageTime < 0)
            {
                // Damage
                Hurt(coldDamagePerInterval);
                coldDamageTime = coldDamageInterval;
            }
        }

        // Glow

        hurtGlowTime -= Time.deltaTime;
        if (hurtGlowTime < 0) hurtGlowTime = 0;

        if (healingCooldownTime <= 0) healGlowTime -= Time.deltaTime;
        if (healGlowTime < 0) healGlowTime = 0;

        Color hurtTotalGlow = hurtGlow * hurtGlowTime / hurtGlowDuration;
        Color healTotalGlow = healGlow * healGlowTime / healGlowDuration;
        Color coldTotalGlow = coldGlow * cold / maxCold;
        glow.SetGlow(hurtTotalGlow + healTotalGlow + coldTotalGlow);

        // Mute

        if (muted)
        {
            producedAudibleRange = 0;
        }

        // Swim

        if (swimming)
        {
            characterPhysics.velocity.y = 0;

            float swimTargetY = swimmingMedium.SurfaceHeight - swimTargetDepth;
            if (transform.position.y < swimTargetY)
            {
                transform.position = new Vector3(transform.position.x, swimTargetY, transform.position.z);
            }
        }
    }

    private bool Contains(string[] array, string item)
    {
        bool result = false;
        foreach (string arrayItem in array)
        {
            if (arrayItem == item)
            {
                result = true;
            }
        }
        return result;
    }

    private void ZoomCrosshair()
    {
        switch (zoomCrosshairState)
        {
            case ZoomCrosshairState.Unzoomed:
                if (Input.GetButton("Zoom Crosshair") && !Contains(crosshairDontZoomSpells, spells[currentSpell].name))
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
                if (!Input.GetButton("Zoom Crosshair") || Contains(crosshairDontZoomSpells, spells[currentSpell].name))
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
            if (currentSpell >= spells.Count)
            {
                currentSpell = 0;
            }
        }
        if (Input.GetButtonDown("Previous Spell"))
        {
            currentSpell--;
            if (currentSpell < 0)
            {
                currentSpell = spells.Count - 1;
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

        float previousSpell = currentSpell;

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
            currentSpell += (int)Input.GetAxisRaw("Mouse ScrollWheel");
        }

        // Clamp spell
        currentSpell = Mathf.Clamp(currentSpell, 0, spells.Count - 1);

        // Highlight selected spell
        if (spellBarActivated)
        {
            spellBar.anchoredPosition = new Vector2(-offsetX * currentSpell, 0);
        }

        // Preview spell in spellwheel
        spellWheel.sprite = spells[currentSpell].icon;

        // Put spell preview
        if (mana.HasMana)
        {
            spellPreview.gameObject.SetActive(true);
        }
        else
        {
            spellPreview.gameObject.SetActive(false);
        }

        if (previousSpell != currentSpell)
        {
            foreach (Transform child in spellPreview)
            {
                Destroy(child.gameObject);
            }

            if (spells[currentSpell].preview != null)
            {
                Instantiate(spells[currentSpell].preview, spellPreview.position, Quaternion.identity, spellPreview);
            }
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
            // Lock onto hurtable
            if (meleeLockDirection != Vector2.zero)
            {
                Vector3 direction3d = new Vector3(meleeLockDirection.x, 0, meleeLockDirection.y);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction3d, Vector3.up), meleeLockTurnSpeed * Time.deltaTime);
            }

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
                        hurtable.Hurt(meleeDamage, true, transform);
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
                    punchSoundSource.PlayOneShot(punchSounds[UnityEngine.Random.Range(0, punchSounds.Length - 1)]);
                }
            }
        }

        if (Input.GetButtonDown("Attack"))
        {
            switch (spells[currentSpell].name)
            {
                case "Melee":
                    if (meleeCooldownTime <= 0)
                    {
                        animator.SetTrigger("Melee");
                        meleeStopDurationTime = meleeStopDuration;
                        meleeCooldownTime = meleeCooldown;
                        //stamina.UseStamina(meleeStaminaCost);

                        // Enable trail
                        meleeFistTrail.enabled = true;
                        meleeFistTrailDurationTime = meleeFistTrailDuration;

                        // Damage after some time
                        meleeDamageDelayActivated = true;
                        meleeDamageDelayTime = meleeDamageDelay;

                        // Lock onto hurtable
                        Collider[] possibleHurtables = Physics.OverlapSphere(transform.position, meleeLockScanRadius);
                        float shortestDistance = float.MaxValue;
                        Transform shortestHurtable = null;
                        foreach (Collider possibleHurtable in possibleHurtables)
                        {
                            float distance = (possibleHurtable.transform.position - transform.position).magnitude;
                            if (distance < shortestDistance && possibleHurtable.gameObject != gameObject)
                            {
                                IHurtable hurtable = possibleHurtable.GetComponent<IHurtable>();
                                if (hurtable != null)
                                {
                                    shortestHurtable = possibleHurtable.transform;
                                }
                            }
                        }
                        if (shortestHurtable != null)
                        {
                            Vector3 displacement = shortestHurtable.position - transform.position;
                            meleeLockDirection = new Vector2(displacement.x, displacement.z);
                        }
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

                        wandSoundSource.PlayOneShot(healSound);

                        healGlowTime = healGlowDuration;
                    }
                    break;

                case "Basic":
                    if (basicSpellCooldownTime <= 0 && mana.HasMana && zoomCrosshairState == ZoomCrosshairState.Zoomed)
                    {
                        basicSpellCooldownTime = basicSpellCooldown;

                        // Calculate projectile direction
                        Vector3 originalPosition = basicSpellEmitPosition.position;
                        Vector3 direction = GetTargetUnderCrosshair() - originalPosition;

                        // Create projectile
                        BasicSpell newBasicSpell = Instantiate(basicSpell, originalPosition, Quaternion.identity);
                        newBasicSpell.direction = direction;
                        newBasicSpell.shooter = gameObject;
                        newBasicSpell.cameraShake = cameraShake;

                        // Use up mana
                        mana.UseMana(basicSpellManaCost);

                        wandSoundSource.PlayOneShot(basicSound);
                    }
                    break;
            }
        }

        bool fireOn = false;
        bool iceOn = false;
        if (Input.GetButton("Attack"))
        {
            switch (spells[currentSpell].name)
            {
                case "Fire":
                    if (zoomCrosshairState == ZoomCrosshairState.Zoomed)
                    {
                        fireOn = true;
                    }
                    break;

                case "Freezing":
                    if (zoomCrosshairState == ZoomCrosshairState.Zoomed)
                    {
                        iceOn = true;
                    }
                    break;
            }
        }

        if (fireOn && mana.HasMana)
        {
            flamethrower.transform.LookAt(GetTargetUnderCrosshair());
            flamethrower.Play();
            flamethrowerSound.mute = false;
            mana.UseMana(fireSpellManaCost * Time.deltaTime);
        }
        else
        {
            flamethrower.Stop();
            flamethrowerSound.mute = true;
        }

        if (iceOn && mana.HasMana)
        {
            icethrower.transform.LookAt(GetTargetUnderCrosshair());
            icethrower.Play();
            icethrowerSound.mute = false;
            mana.UseMana(iceSpellManaCost * Time.deltaTime);
        }
        else
        {
            icethrower.Stop();
            icethrowerSound.mute = true;
        }
    }

    private Vector3 GetTargetUnderCrosshair()
    {
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

        return target;
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
                    producedAudibleRange = runningAudibleRange;
                    stamina.UseStamina(runningStaminaCost * Time.deltaTime);
                    break;

                case 2:
                    speed = sneakingSpeed;
                    break;

                default:
                    speed = walkingSpeed;
                    producedAudibleRange = walkingAudibleRange;
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
        if (jumpInput && characterPhysics.IsGroundedLenient && meleeStopDurationTime <= 0)
        {
            //characterPhysics.velocity += jumpingSpeed * characterPhysics.GroundLenientRaycast.normal.normalized;
            if (characterPhysics.GroundLenientSlope <= jumpSlopeLimit)
            {
                characterPhysics.velocity.y += jumpingSpeed;
            }
            //stamina.UseStamina(jumpingStaminaCost);
        }
    }

    public void Die()
    {
        alive = false;
        ragdoll.SetEnabled(true);

        // Play effect
        deathMusic.Play();
        foreach (GameObject deathHiddenUI in deathHiddenUIs)
        {
            deathHiddenUI.SetActive(false);
        }

        deathimage.CrossFadeAlpha(1.0f, 2f, false);
        deathtext.enabled = true;
        manaParticleSink.enabled = false;
    }

    public void AbsorbMana(float amount)
    {
        mana.ReplenishMana(amount);
    }

    public bool CanAbsorb()
    {
        return true;
        return !mana.MaxMana;
    }

    public void Hurt(float amount, bool createsMana = false, Transform sender = null)
    {
        if (!health.Dead)
        {
            health.Hurt(amount);
            hurtGlowTime = hurtGlowDuration;

            if (health.Dead)
            {
                Die();
            }
        }
    }

    public void Kill(bool createsMana = false, Transform sender = null)
    {
        Hurt(999999, createsMana);
    }

    public void EnterCold(float amount)
    {
        isCold = true;
        coldPerSecond = amount;
    }

    public void ExitCold()
    {
        isCold = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        SwimmingMedium swimmingMedium = other.GetComponent<SwimmingMedium>();
        if (swimmingMedium != null)
        {
            swimming = true;
            characterGravity.enabled = false;
            this.swimmingMedium = swimmingMedium;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SwimmingMedium swimmingMedium = other.GetComponent<SwimmingMedium>();
        if (swimmingMedium != null)
        {
            swimming = false;
            characterGravity.enabled = true;
        }
    }
}