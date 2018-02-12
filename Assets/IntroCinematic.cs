using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class IntroCinematic : MonoBehaviour {
    public PlayableDirector playableDirector;
    public Player player;
    public AudioSource music;

    public Behaviour[] disableDuringCinematic;

    public CanvasGroup blackScreen;
    public float blackScreenTime = 2;
    private float blackScreenSpeed;
    public float blackScreenOutTime = 2;
    private float blackScreenOutSpeed;

    private void Start()
    {
        blackScreenSpeed = 1 / blackScreenTime;
        blackScreenOutSpeed = 1 / blackScreenOutTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player.gameObject)
        {
            player.Freeze();
            foreach (Behaviour behavior in disableDuringCinematic)
            {
                behavior.enabled = false;
            }

            StartCoroutine(FadeInBlack());
        }
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
        StartCoroutine(FadeOutBlack());
    }

    private IEnumerator FadeOutBlack()
    {
        yield return new WaitForSeconds((float) playableDirector.duration);

        // Set to black
        blackScreen.alpha = 1;

        // Prepare for gameplay
        foreach (Behaviour behavior in disableDuringCinematic)
        {
            behavior.enabled = true;
        }
        player.Unfreeze();

        // Fade out black
        while (blackScreen.alpha > 0)
        {
            blackScreen.alpha -= blackScreenOutSpeed * Time.deltaTime;
            yield return new WaitForSeconds(0);
        }
    }
}
