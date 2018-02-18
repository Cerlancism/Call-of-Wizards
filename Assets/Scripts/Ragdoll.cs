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
    public GameObject[] ignoreObjects;

    private void Start()
    {
        SetEnabled(false);
    }

    private bool Contains(GameObject[] array, GameObject item)
    {
        bool result = false;
        foreach (GameObject arrayItem in array)
        {
            if (arrayItem == item)
            {
                result = true;
            }
        }
        return result;
    }

    public void SetEnabled(bool enabled)
    {
        Rigidbody[] ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        Collider[] ragdollColliders = GetComponentsInChildren<Collider>();

        foreach (Rigidbody ragdollRigidbody in ragdollRigidbodies)
        {
            if (!Contains(ignoreObjects, ragdollRigidbody.gameObject))
            {
                ragdollRigidbody.isKinematic = !enabled;
            }
        }

        foreach (Collider ragdollCollider in ragdollColliders)
        {
            if (!Contains(ignoreObjects, ragdollCollider.gameObject))
            {
                ragdollCollider.enabled = enabled;
            }
        }

        characterController.enabled = !enabled;
        animator.enabled = !enabled;
    }
}
