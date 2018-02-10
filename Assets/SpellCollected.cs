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

    private void Start()
    {
        disappearSpeed = 1 / disappearTime;
    }

    public void CollectSpell(Spell spell)
    {
        text.text = spell.displayName + " spell found!";
        audioSource.PlayOneShot(collectSound, collectSoundVolume);
        canvasGroup.alpha = 1;

        StartCoroutine(DisappearAfterDelay());
    }

    private IEnumerator DisappearAfterDelay()
    {
        yield return new WaitForSeconds(appearTime);

        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= disappearSpeed * Time.deltaTime;
            yield return new WaitForSeconds(0);
        }
    }
}
