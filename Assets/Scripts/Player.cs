using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ANIMATOR INTEGER PARAMETERS
//
// Walk Mode - walk (0), run (1), sneak (2)

public class Player : MonoBehaviour {
    [Header("Movement")]
    public float walkingSpeed = 5;
    public float runningSpeed = 10;
    public float sneakingSpeed = 3;
    public float jumpingSpeed = 10;
    public float turningSpeed = 0.1f;
    public Transform playerCamera;
    public CharacterController characterController;
    public Animator animator;
    private Vector3 velocity;
    private bool alive = true;

    [Header("Stamina")]
    public float initialStamina = 1;
    public float runningStaminaCost = 0.1f; // Per second
    public float jumpingStaminaCost = 0.2f; // Per jump
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

    private void Start()
    {
        stamina = initialStamina;

        // Too lazy to do it manually
        SetRagdollEnabled(false);
    }

    private void Update()
    {
        Walk();
        Jump();
        UpdatePhysics();
        UpdateStamina();
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
        bool sneakInput = Input.GetButton("Sneak");
        Vector2 inputDirection = new Vector2(horizontalInput, verticalInput);

        // Check if moving
        if (inputDirection == Vector2.zero)
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
            if (runInput && stamina > 0)
            {
                speed = runningSpeed;
                UseStamina(runningStaminaCost * Time.deltaTime);
                animator.SetInteger("Walk Mode", 1);
            }
            else if (sneakInput)
            {
                speed = sneakingSpeed;
                animator.SetInteger("Walk Mode", 2);
            }
            else
            {
                speed = walkingSpeed;
                animator.SetInteger("Walk Mode", 0);
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
        if (jumpInput && characterController.isGrounded && stamina > 0)
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
                velocity.y = 0;
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
        SetRagdollEnabled(true);
    }

    private void SetRagdollEnabled(bool enabled)
    {
        Rigidbody[] ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody ragdollRigidbody in ragdollRigidbodies)
        {
            ragdollRigidbody.isKinematic = !enabled;
        }

        characterController.enabled = !enabled;
        animator.enabled = !enabled;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Player))]
public class ScreenshotManagerEditor : UnityEditor.Editor
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