using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSpell : MonoBehaviour {
    public Vector3 direction;
    public GameObject shooter;
    public float speed = 20;
    public float damage = 20;
    public float despawnTime = 10;

    private void Start()
    {
        Destroy(gameObject, despawnTime);
    }

    private void Update () {
        transform.position += direction.normalized * speed * Time.deltaTime;
	}

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject != shooter)
        {
            // Hurt if hurtable
            IHurtable hurtable = collider.GetComponent<IHurtable>();
            if (hurtable != null)
            {
                hurtable.Hurt(damage);
            }

            Destroy(gameObject);
        }
    }
}
