using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CheckpointManager))]
public class CheckpointManagerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(5);
        GUILayout.Label("Debug Controls", EditorStyles.boldLabel);

        GUILayout.Label("Checkpoint: " + ((CheckpointManager)target).CurrentCheckpointName);
        if (GUILayout.Button("Reset Progress", EditorStyles.miniButton))
        {
            ((CheckpointManager)target).ResetProgress();
        }

        EditorUtility.SetDirty(target);
    }
}
#endif

[Serializable]
public class Checkpoint
{
    public string name;
    public Transform spawnpoint;
}

public class CheckpointManager : MonoBehaviour
{
    public Checkpoint[] checkpoints;
    public string initialCheckpointName;
    public Player player;

    public string playerPrefKey = "Checkpoint";
    public string CurrentCheckpointName
    {
        get
        {
            return PlayerPrefs.GetString(playerPrefKey, initialCheckpointName);
        }
    }
    private string currentCheckpointName
    {
        set
        {
            if (value == null)
            {
                PlayerPrefs.DeleteKey(playerPrefKey);
            }
            else
            {
                PlayerPrefs.SetString(playerPrefKey, value);
            }
        }
    }

    [Header("Time Travel Fixers")]
    public IntroCinematic introCinematic;

    private void Start()
    {
        Spawn();
    }

    private void Spawn()
    {
        Checkpoint currentCheckpoint = GetCheckpoint(CurrentCheckpointName);
        player.transform.position = currentCheckpoint.spawnpoint.position;
        player.transform.rotation = currentCheckpoint.spawnpoint.rotation;

        switch (CurrentCheckpointName)
        {
            case "Level 2":
            case "Level 3":
                introCinematic.triggered = true;
                break;
        }
    }

    private Checkpoint GetCheckpoint(string name)
    {
        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (checkpoint.name == name)
            {
                return checkpoint;
            }
        }

        return null;
    }

    public void SetSilentSpawn(string newCheckpointName)
    {
        currentCheckpointName = newCheckpointName;
    }

    public void SetSpawn(string newCheckpointName)
    {
        SetSilentSpawn(newCheckpointName);
    }

    public void ResetProgress()
    {
        currentCheckpointName = null;
    }
}