﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSpell : MonoBehaviour {
    public Vector3 direction;
    public GameObject shooter;
    public CameraShake cameraShake;
    public float cameraShakeAmount = 0.4f;
    public float hitPauseTime = 0.1f;
    public float speed = 20;
    public float damage = 20;
    public float despawnTime = 10;
    public Transform explosion;

    private void Start()
    {
        Destroy(gameObject, despawnTime);
    }

    private void Update () {
        transform.position += direction.normalized * speed * Time.deltaTime;
	}

    private void OnTriggerEnter(Collider collider)
    {
        if (!collider.isTrigger && collider.gameObject != shooter)
        {
            // Hurt if hurtable
            IHurtable hurtable = collider.GetComponent<IHurtable>();
            if (hurtable != null)
            {
                hurtable.Hurt(damage);

                // Hit effects
                HitPause otherHitPause = collider.GetComponent<HitPause>();
                if (otherHitPause)
                {
                    otherHitPause.Pause(hitPauseTime);
                }
            }

            Instantiate(explosion, transform.position, Quaternion.identity);
            cameraShake.Shake(cameraShakeAmount);
            Destroy(gameObject);
        }
    }
}
