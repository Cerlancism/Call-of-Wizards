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
    public string displayName; 
    public Transform spawnpoint;
}

public class CheckpointManager : MonoBehaviour
{
    public Checkpoint[] checkpoints;
    public string initialCheckpointName;
    public Player player;
    public Message spellCollected;

    public string playerPrefKey = "Checkpoint";
    public string CurrentCheckpointName
    {
        get
        {
            return PlayerPrefs.GetString(playerPrefKey);
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

    private bool isFirstStart
    {
        get
        {
            return !PlayerPrefs.HasKey(playerPrefKey);
        }
    }

    [Header("Time Travel Fixers")]
    public Cinematic introCinematic;
    public BasicTutorial basicTutorial;
    public Teleport teleport;

    [Header("Spells")]
    public Spellbook basicSpellbook;
    public Spellbook healingSpellbook;
    public Spellbook fireSpellbook;
    public Spellbook freezeSpellbook;

    [Header("First Start stuff")]
    public Cinematic startCinematic;

    private void Start()
    {
        if (isFirstStart)
        {
            // First start, play cinematic and set first checkpoint
            SetSilentSpawn(initialCheckpointName);
            startCinematic.StartCinematic();
        }

        Spawn();
    }

    private void Spawn()
    {
        Checkpoint currentCheckpoint = GetCheckpoint(CurrentCheckpointName);
        player.transform.position = currentCheckpoint.spawnpoint.position;
        player.transform.rotation = currentCheckpoint.spawnpoint.rotation;

        // Time fixers
        switch (CurrentCheckpointName)
        {
            case "Start":
                // Mute player for safe space to walk
                player.muted = true;
                break;

            case "Level 3":
                teleport.triggered = true;
                introCinematic.triggered = true;
                basicTutorial.done = true;
                break;

            case "Level 2":
                introCinematic.triggered = true;
                basicTutorial.done = true;
                break;
        }

        // Spell fixers
        switch (CurrentCheckpointName)
        {
            case "Test":
                player.AddSpell(healingSpellbook.spell);
                player.AddSpell(basicSpellbook.spell);
                player.AddSpell(fireSpellbook.spell);
                player.AddSpell(freezeSpellbook.spell);
                break;

            case "Level 3":
                player.AddSpell(healingSpellbook.spell);
                player.AddSpell(basicSpellbook.spell);
                player.AddSpell(fireSpellbook.spell);
                break;

            case "Level 2":
                player.AddSpell(healingSpellbook.spell);
                player.AddSpell(basicSpellbook.spell);
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
        spellCollected.ShowMessage(GetCheckpoint(newCheckpointName).displayName);
    }

    public void ResetProgress()
    {
        currentCheckpointName = null;
    }
}