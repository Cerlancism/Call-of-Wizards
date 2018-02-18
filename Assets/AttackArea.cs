using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour {
    public float amount;
    public bool createsMana;
    public bool backstabPossible;
    public Transform backstabSender;
    public GameObject[] ignoreHurtables;
    public bool active;
    public float cooldown;
    private float cooldownTime;
    public bool destroysIce;

    private void Update()
    {
        //cooldownTime -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (active)
        {
            IHurtable hurtable = other.GetComponent<IHurtable>();
            if (hurtable != null)
            {
                //if (!Contains(ignoreHurtables, other.gameObject))
                //{
                //if (cooldownTime <= 0)
                //{
                    hurtable.Hurt(amount, createsMana, (backstabPossible) ? backstabSender : null);
                    //cooldownTime = cooldown;
                //}
                //}
            }

            IceChunk iceChunk = GetComponent<Collider>().GetComponent<IceChunk>();
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
