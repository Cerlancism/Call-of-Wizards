using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellCollected : MonoBehaviour {
    public Text text;
    public AudioClip collectSound;
    public float collectSoundVolume = 1;
    public AudioSource audioSource;
    public CanvasGroup canvasGroup;
    public float appearTime = 3;
    public float disappearTime = 2;
    private float disappearSpeed;
    private float disappearCurrentTime;

    private void Start()
    {
        disappearSpeed = 1 / disappearTime;
    }

    public void ShowMessage(string message)
    {
        text.text = message;
        audioSource.PlayOneShot(collectSound, collectSoundVolume);
        canvasGroup.alpha = 1;

        disappearCurrentTime = disappearTime;
    }

    private void Update()
    {
        disappearCurrentTime -= Time.deltaTime;
        if (disappearCurrentTime < 0)
        {
            canvasGroup.alpha -= disappearSpeed * Time.deltaTime;
        }
    }
}
