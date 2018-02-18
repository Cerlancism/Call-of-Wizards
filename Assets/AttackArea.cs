using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour {
    public float amount;
    public bool createsMana;
    public bool backstabPossible;
    public Transform backstabSender;
    public bool active;
    public bool destroysIce;
    public CameraShake cameraShake;
    public float cameraShakeAmount;
    public HitPause hitPause;
    public float hitPauseAmount;
    public float otherHitPauseAmount;
    public AudioSource hitSource;
    public AudioClip[] hitSounds;

    private void Update()
    {
        //cooldownTime -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (active)
        {
            if (amount > 0)
            {
                IHurtable hurtable = other.GetComponent<IHurtable>();
                if (hurtable != null)
                {
                    hurtable.Hurt(amount, createsMana, (backstabPossible) ? backstabSender : null);

                    // Other hit pause
                    if (otherHitPauseAmount > 0)
                    {
                        HitPause otherHitPause = other.GetComponent<HitPause>();
                        if (otherHitPause != null)
                        {
                            otherHitPause.Pause(otherHitPauseAmount);
                        }
                    }

                    // Subject hit pause
                    if (hitPauseAmount > 0)
                    {
                        hitPause.Pause(hitPauseAmount);
                    }

                    // Camera shake
                    if (cameraShakeAmount > 0)
                    {
                        cameraShake.Shake(cameraShakeAmount);
                    }

                    // Sound
                    if (hitSource != null)
                    {
                        hitSource.PlayOneShot(hitSounds[UnityEngine.Random.Range(0, hitSounds.Length)]);
                    }
                }
            }

            IceChunk iceChunk = other.GetComponent<IceChunk>();
            if (iceChunk != null)
            {
                iceChunk.DestroyIce();
            }
        }
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
}
