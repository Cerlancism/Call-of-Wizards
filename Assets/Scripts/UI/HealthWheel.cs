using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthWheel : MonoBehaviour {
    public Health health;
    public Image healthBar;
    public Image healthBarLag;
    public float healthLagRate = 30;
    private float healthLag;

    private void Update()
    {
        if (healthLag > health.Amount)
        {
            healthLag -= healthLagRate * Time.deltaTime;
        }
        else
        {
            healthLag = health.Amount;
        }

        healthBar.fillAmount = health.Amount / health.maxHealth;
        healthBarLag.fillAmount = healthLag / health.maxHealth;
    }
}
