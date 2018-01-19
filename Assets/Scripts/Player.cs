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

public class Player : MonoBehaviour {
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
    private Vector3 velocity;
    private bool alive = true;

    [Header("Stamina")]
    public float initialStamina = 1;
    public float staminaReplenishDelay = 2;
    public float staminaReplenishRate = 0.3f;
    public float staminaFullReplenishDelay = 2;
    public float staminaFullReplenishRate = 0.3f;
    public float staminaRedDeplenishRate = 0.5f;
    public float staminaWheelFadeRate = 1;
    public CanvasGroup staminaWheelCanvasGroup;
    public Image staminaWheelGreen;
    public Image staminaWheelRed;
    public Image staminaWheelYellow;
    private float stamina;
    private float staminaRed;
    private float staminaYellow;
    private float staminaReplenishDelayTime;
    private float staminaFullReplenishDelayTime;
    private bool staminaFullReplenishing;

    [Header("Health")]
    public float initialHealth = 100;
    public float maxHealth = 100;
    public Image healthBar;
    public Image healthBarLag;
    public float healthLagRate = 30;
    private float healthLag;
    private float health;

    [Header("Mana")]
    public float initialMana = 100;
    public float maxMana = 100;
    public Image manaBar;
    public Image manaBarLag;
    public float manaLagRate = 30;
    private float manaLag;
    private float mana;

    [Header("Spell Bar")]
    public Spell[] spells;
    public RectTransform spellBar;
    public RectTransform spellBarSpell;
    public RectTransform spellBarHighlight;
    public float offsetX = 100;
    private int currentSpell;

    [Header("Melee")]
    public float meleeCooldown = 0.2f;
    public float meleeStopDuration = 0.7f;
    public float meleeStaminaCost = 0.1f; // Per melee attack
    public Transform meleeAttackArea;
    public TrailRenderer meleeFistTrail;
    public float meleeFistTrailDuration = 0.5f;
    public float meleeDamage = 10;
    private float meleeCooldownTime;
    private float meleeStopDurationTime;
    private float meleeFistTrailDurationTime;

    [Header("Basic Attack")]
    public BasicSpell basicSpell;
    public float basicSpellCooldown = 0.5f;
    public Transform basicSpellEmitPosition;
    public float basicSpellManaCost = 10;
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

    private void Start()
    {
        health = initialHealth;
        mana = initialMana;
        stamina = initialStamina;

        // Setup spell bar
        for (int i = 0; i < spells.Length; i++)
        {
            RectTransform newSpellBarSpell = Instantiate(spellBarSpell, spellBar);
            newSpellBarSpell.anchoredPosition = new Vector2(i * offsetX, 0);
            newSpellBarSpell.GetComponent<SpellBarSpell>().text.text = spells[i].displayName;
            newSpellBarSpell.GetComponent<SpellBarSpell>().image.sprite = spells[i].icon;
        }

        // Find original values
        originalFov = playerCamera.GetComponent<Camera>().fieldOfView;
        originalPositionalOffset = playerCamera.GetComponent<CameraOrbit>().cameraOffset;
    }

    private void Update()
    {
        Walk();
        Jump();
        ZoomCrosshair();
        SwitchSpell();
        Attack();
        UpdatePhysics();
        UpdateStamina();
        UpdateHealth();
        UpdateMana();
    }

    private void UpdateMana()
    {
        mana = Mathf.Clamp(mana, 0, maxMana);

        if (manaLag > mana)
        {
            manaLag -= manaLagRate * Time.deltaTime;
        }
        else
        {
            manaLag = mana;
        }

        manaBar.fillAmount = mana / maxMana;
        manaBarLag.fillAmount = manaLag / maxMana;
    }

    private void UpdateHealth()
    {
        health = Mathf.Clamp(health, 0, maxHealth);

        if (healthLag > health)
        {
            healthLag -= healthLagRate * Time.deltaTime;
        }
        else
        {
            healthLag = health;
        }

        healthBar.fillAmount = health / maxHealth;
        healthBarLag.fillAmount = healthLag / maxHealth;
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

    private void SwitchSpell()
    {
        if (Input.GetButtonDown("Next Spell"))
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
        }

        // Highlight selected spell
        spellBarHighlight.anchoredPosition = new Vector2(offsetX * currentSpell, 0);
    }

    private void Attack()
    {
        meleeCooldownTime -= Time.deltaTime;
        basicSpellCooldownTime -= Time.deltaTime;

        meleeFistTrailDurationTime -= Time.deltaTime;
        if (meleeFistTrailDurationTime <= 0)
        {
            meleeFistTrail.enabled = false;
        }

        if (Input.GetButtonDown("Attack"))
        {
            switch (spells[currentSpell].name)
            {
                case "Melee":
                    if (meleeCooldownTime <= 0 && stamina > 0)
                    {
                        animator.SetTrigger("Melee");
                        meleeStopDurationTime = meleeStopDuration;
                        meleeCooldownTime = meleeCooldown;
                        UseStamina(meleeStaminaCost);

                        // Enable trail
                        meleeFistTrail.enabled = true;
                        meleeFistTrailDurationTime = meleeFistTrailDuration;

                        // Hurt hurtables
                        Collider[] collidersInAttackArea = Physics.OverlapBox(meleeAttackArea.position, meleeAttackArea.localScale / 2, meleeAttackArea.rotation);
                        foreach (Collider colliderInAttackArea in collidersInAttackArea)
                        {
                            IHurtable hurtable = colliderInAttackArea.GetComponent<IHurtable>();

                            // If it's hurtable
                            if (hurtable != null)
                            {
                                hurtable.Hurt(meleeDamage);
                            }
                        }
                    }
                    break;

                case "Basic":
                    if (basicSpellCooldownTime <= 0 && mana > 0 && zoomCrosshairState == ZoomCrosshairState.Zoomed)
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
                        mana -= basicSpellManaCost;
                    }
                    break;
            }
        }
    }

    private void UpdateStamina()
    {
        if (staminaFullReplenishing)
        {
            // Replenish stamina fully
            // Don't change stamina until completely full to prevent player from using stamina when full-replenishing
            // Think of stamina as "spendable stamina"
            staminaFullReplenishDelayTime += Time.deltaTime;
            if (staminaFullReplenishDelayTime >= staminaFullReplenishDelay)
            {
                staminaYellow += staminaFullReplenishRate * Time.deltaTime;
            }

            // Check if stamina is full again
            if (staminaYellow >= 1)
            {
                staminaFullReplenishing = false;
                staminaYellow = 0;
                stamina = 1;
            }
        }
        else
        {
            // Replenish stamina
            staminaReplenishDelayTime += Time.deltaTime;
            if (staminaReplenishDelayTime >= staminaReplenishDelay)
            {
                stamina += staminaReplenishRate * Time.deltaTime;
            }

            stamina = Mathf.Clamp(stamina, 0, 1);

            // Check if stamina ran out
            if (stamina <= 0)
            {
                staminaFullReplenishing = true;
                staminaFullReplenishDelayTime = 0;
            }
        }

        // Change stamina red to the actual stamina value slowly
        if (staminaRed > stamina)
        {
            staminaRed -= staminaRedDeplenishRate * Time.deltaTime;
        }
        else
        {
            staminaRed = stamina;
        }

        // Render stamina
        staminaWheelGreen.fillAmount = stamina;
        staminaWheelRed.fillAmount = staminaRed;
        staminaWheelYellow.fillAmount = staminaYellow;

        // Fade out stamina if stamina = 1
        // Else, fade in
        float staminaWheelAlpha = staminaWheelCanvasGroup.alpha;
        if (stamina == 1)
        {
            staminaWheelAlpha -= staminaWheelFadeRate * Time.deltaTime;
        }
        else
        {
            staminaWheelAlpha += staminaWheelFadeRate * Time.deltaTime;
        }
        staminaWheelAlpha = Mathf.Clamp(staminaWheelAlpha, 0, 1);
        staminaWheelCanvasGroup.alpha = staminaWheelAlpha;
    }

    private void UseStamina(float cost)
    {
        stamina -= cost;
        staminaReplenishDelayTime = 0;
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
        if (runInput && stamina > 0)
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
        if (inputDirection == Vector2.zero || meleeStopDurationTime > 0)
        {
            animator.SetBool("Walking", false);
            
            // Set velocity
            velocity.x = 0;
            velocity.z = 0;
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
                    UseStamina(runningStaminaCost * Time.deltaTime);
                    break;

                case 2:
                    speed = sneakingSpeed;
                    break;

                default:
                    speed = walkingSpeed;
                    break;
            }
            Vector2 displacement = direction.normalized * Mathf.Clamp(inputDirection.magnitude, 0, 1) * speed;
            velocity.x = displacement.x;
            velocity.z = displacement.y;

            // Turn player towards moving direction
            Vector3 direction3d = new Vector3(direction.x, 0, direction.y);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction3d, Vector3.up), turningSpeed);
        }
    }

    private void Jump()
    {
        // Get input
        bool jumpInput = Input.GetButtonDown("Jump");

        // Set velocity
        if (jumpInput && characterController.isGrounded && stamina > 0 && meleeStopDurationTime <= 0)
        {
            velocity.y += jumpingSpeed;
            UseStamina(jumpingStaminaCost);
        }
    }

    private void UpdatePhysics()
    {
        // Update gravity if above ground
        if (!characterController.isGrounded)
        {
            velocity.y += Physics.gravity.y * Time.deltaTime;
        }
        // Stop velocity if on ground but still going down
        else
        {
            if (velocity.y < 0)
            {
                velocity.y = -0.1f;
            }
        }

        // Move player based on velocity
        characterController.Move(velocity * Time.deltaTime);
    }

    public void Die()
    {
        // Set not alive
        alive = false;

        // Ragdoll
        ragdoll.SetEnabled(true);
    }
}