using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Cinematic))]
public class CinematicEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(5);
        GUILayout.Label("Debug Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Start", EditorStyles.miniButton))
        {
            ((Cinematic)target).StartCinematic();
        }
    }
}
#endif

public class Cinematic : MonoBehaviour {
    public PlayableDirector playableDirector;
    public Player player;
    public AudioSource music;
    public CheckpointManager checkpointManager;

    public Behaviour[] disableDuringCinematic;

    public CanvasGroup blackScreen;
    public float blackScreenTime = 2;
    private float blackScreenSpeed;

    public bool enabledSceneChange = false;
    public string destinationScene;

    public float blackScreenOutDelay = 2;
    public float blackScreenOutTime = 2;
    private float blackScreenOutSpeed;

    public bool triggered = false;

    private bool canSkip = false;
    public float skipTime = 1;
    private float skipSpeed;
    private float skipCurrent = 0;
    public GameObject skipGroup;
    public Image skipImage;
    public CanvasGroup[] skipCleanup;

    private IEnumerator endAfterCinematicCoroutine;

    public bool enabledCheckpoint = false;
    public string checkpoint;

    private void Start()
    {
        blackScreenSpeed = 1 / blackScreenTime;
        blackScreenOutSpeed = 1 / blackScreenOutTime;
        skipSpeed = 1 / skipTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player.gameObject && !triggered)
        {
            StartCinematic();
        }
    }

    public void StartCinematic()
    {
        triggered = true;
        player.Freeze();
        player.invincible = true;
        foreach (Behaviour behavior in disableDuringCinematic)
        {
            behavior.enabled = false;
        }

        StartCoroutine(FadeInBlack());
    }

    private IEnumerator FadeInBlack()
    {
        while (blackScreen.alpha < 1)
        {
            blackScreen.alpha += blackScreenSpeed * Time.deltaTime;
            yield return new WaitForSeconds(0);
        }

        music.Play();
        playableDirector.Play();
        canSkip = true;
        skipGroup.SetActive(true);
        endAfterCinematicCoroutine = EndAfterCinematic();
        StartCoroutine(endAfterCinematicCoroutine);
    }

    private void Update()
    {
        if (canSkip)
        {
            if (Input.GetButton("Skip Cinematic"))
            {
                skipCurrent += skipSpeed * Time.deltaTime;
            }
            else
            {
                skipCurrent -= skipSpeed * Time.deltaTime;
            }

            skipCurrent = Mathf.Clamp01(skipCurrent);
            skipImage.fillAmount = skipCurrent;

            if (skipCurrent >= 1)
            {
                // Skip cinematic
                playableDirector.time = playableDirector.duration;
                music.Stop(); // Cause right now the end music is just earrape for skipping
                //music.time = (float)playableDirector.duration;

                // Clean up Unity not extrapolating custom playable (fade)
                foreach (CanvasGroup stuff in skipCleanup)
                {
                    stuff.alpha = 0;
                }

                // Cancel waiting coroutine and skip right to the end
                StopCoroutine(endAfterCinematicCoroutine);
                StartCoroutine(End());
            }
        }
    }

    private IEnumerator EndAfterCinematic()
    {
        yield return new WaitForSeconds((float)playableDirector.duration);
        yield return StartCoroutine(End());
    }

    private IEnumerator End()
    {
        // No longer skip
        canSkip = false;
        skipGroup.SetActive(false);

        // Set to black
        blackScreen.alpha = 1;

        if (enabledSceneChange)
        {
            SceneManager.LoadSceneAsync(destinationScene);
        }
        else
        {
            // Prepare for gameplay
            foreach (Behaviour behavior in disableDuringCinematic)
            {
                behavior.enabled = true;
            }
            player.Unfreeze();
            player.invincible = false;

            // Fade out black
            yield return new WaitForSeconds(blackScreenOutDelay);

            if (enabledCheckpoint)
            {
                checkpointManager.SetSpawn(checkpoint);
            }

            while (blackScreen.alpha > 0)
            {
                blackScreen.alpha -= blackScreenOutSpeed * Time.deltaTime;
                yield return new WaitForSeconds(0);
            }
        }
    }
}
