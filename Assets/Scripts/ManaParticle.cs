using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaParticle : MonoBehaviour {
    public float armingTime = 1;
    public float manaAmount = 2;
    public float absorbRadius = 0.1f;
    public float despawnTime = 20;
    public Rigidbody rb;

    private void Start()
    {
        Destroy(gameObject, despawnTime);
    }

    private void Update()
    {
        armingTime -= Time.deltaTime;

        // Test if absorb
        if (armingTime <= 0)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, absorbRadius);
            foreach (Collider collider in colliders)
            {
                IManaAbsorber manaAbsorber = collider.GetComponent<IManaAbsorber>();
                if (manaAbsorber != null && manaAbsorber.CanAbsorb())
                {
                    manaAbsorber.AbsorbMana(manaAmount);
                    Destroy(gameObject);
                }
            }
        }
    }

    public void Explode(float force)
    {
        float randomizedForce = Random.value * force;
        Vector3 randomizedDirection = new Vector3(Random.value * 2 - 1, Random.value, Random.value * 2 - 1).normalized;
        rb.AddForce(randomizedDirection * randomizedForce);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, absorbRadius);
    }
}
