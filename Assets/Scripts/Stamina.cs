using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stamina : MonoBehaviour {
    public float initialStamina = 1;
    public float replenishDelay = 2;
    public float replenishRate = 0.3f;
    public float fullReplenishDelay = 2;
    public float fullReplenishRate = 0.3f;
    private float stamina;
    private float replenishDelayTimer;
    private float fullReplenish;
    private float fullReplenishDelayTimer;
    private bool isFullReplenishing;

    public float Amount
    {
        get
        {
            return stamina;
        }
    }

    public float FullReplenishAmount
    {
        get
        {
            return fullReplenish;
        }
    }

    public bool HasStamina
    {
        get
        {
            return stamina > 0;
        }
    }

    private void Start()
    {
        stamina = initialStamina;
    }

    private void Update()
    {
        if (isFullReplenishing)
        {
            UpdateFullReplenish();
        }
        else
        {
            UpdateReplenish();
        }
    }

    private void UpdateFullReplenish()
    {
        // Replenish stamina fully
        // Don't change stamina until completely full to prevent player from using stamina when full-replenishing
        // Think of stamina as "spendable stamina"
        fullReplenishDelayTimer += Time.deltaTime;
        if (fullReplenishDelayTimer >= fullReplenishDelay)
        {
            fullReplenish += fullReplenishRate * Time.deltaTime;
        }

        // Check if stamina is full again
        if (fullReplenish >= 1)
        {
            isFullReplenishing = false;
            fullReplenish = 0;
            stamina = 1;
        }
    }

    private void UpdateReplenish()
    {
        // Replenish stamina
        replenishDelayTimer += Time.deltaTime;
        if (replenishDelayTimer >= replenishDelay)
        {
            stamina += replenishRate * Time.deltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0, 1);

        // Check if stamina ran out
        if (stamina <= 0)
        {
            isFullReplenishing = true;
            fullReplenishDelayTimer = 0;
        }
    }

    public void UseStamina(float cost)
    {
        stamina -= cost;
        replenishDelayTimer = 0;
    }
}
