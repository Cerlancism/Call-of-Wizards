using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {
    public float initialHealth = 100;
    public float maxHealth = 100;
    private float health;

    public float Amount
    {
        get
        {
            return health;
        }
    }

    public bool Dead
    {
        get
        {
            return health <= 0;
        }
    }

    private void Start ()
    {
        health = initialHealth;
    }

    private void Update () {
        health = Mathf.Clamp(health, 0, maxHealth);
    }

    public void Hurt(float amount)
    {
        health -= amount;
    }

    public void Heal(float amount)
    {
        health += amount;
    }
}
