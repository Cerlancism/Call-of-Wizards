using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Ragdoll))]
public class RagdollClassEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(5);
        GUILayout.Label("Debug Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Enable Ragdoll", EditorStyles.miniButton))
        {
            ((Ragdoll)target).SetEnabled(true);
        }

        if (GUILayout.Button("Disable Ragdoll", EditorStyles.miniButton))
        {
            ((Ragdoll)target).SetEnabled(false);
        }
    }
}
#endif

public class Ragdoll : MonoBehaviour {
    public CharacterController characterController;
    public Animator animator;

    private void Start()
    {
        SetEnabled(false);
    }

    public void SetEnabled(bool enabled)
    {
        Rigidbody[] ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        Collider[] ragdollColliders = GetComponentsInChildren<Collider>();

        foreach (Rigidbody ragdollRigidbody in ragdollRigidbodies)
        {
            ragdollRigidbody.isKinematic = !enabled;
        }

        foreach (Collider ragdollCollider in ragdollColliders)
        {
            ragdollCollider.enabled = enabled;
        }

        characterController.enabled = !enabled;
        animator.enabled = !enabled;
    }
}
