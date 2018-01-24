using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaParticleSink : MonoBehaviour {
    public float radius = 2;
    public float strength = 1; // m/s velocity when particle is infinitely close to sink
    public float falloff = 1; // Falloff is measured in m

    private void FixedUpdate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider collider in colliders)
        {
            ManaParticle particle = collider.GetComponent<ManaParticle>();

            if (particle != null)
            {
                if (particle.armingTime <= 0)
                {
                    Rigidbody rb = particle.GetComponent<Rigidbody>();

                    Vector3 direction = transform.position - particle.transform.position;
                    float speed = LogFalloff(direction.magnitude, falloff) * strength;
                    Vector3 velocity = direction.normalized * speed;

                    rb.velocity = velocity;
                }
            }
        }
    }

    private float LogFalloff(float x, float n)
    {
        return Mathf.Pow(1 / (x + 1), n);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
